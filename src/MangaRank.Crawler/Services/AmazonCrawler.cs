using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangaRank.Data;
using MangaRank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nager.AmazonProductAdvertising;
using Nager.AmazonProductAdvertising.Model;
using AmazonItem = Nager.AmazonProductAdvertising.Model.Item;

namespace MangaRank.Services
{
    class AmazonCrawler
    {
        readonly ApplicationDbContextFactory dbContextFactory;

        readonly IAmazonWrapper client;

        readonly ILogger<AmazonCrawler> logger;

        public AmazonCrawler(
            CrawlerOptions secrets,
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            this.dbContextFactory = dbContextFactory;
            logger = loggerFactory.CreateLogger<AmazonCrawler>();

            var authentication = new AmazonAuthentication()
            {
                AccessKey = secrets.PaApiAccessKeyId,
                SecretKey = secrets.PaApiSecretKey,
            };
            client = new AmazonWrapper(
                authentication,
                AmazonEndpoint.JP,
                secrets.PaApiAssociateTag);
        }

        public async Task CrawlAsync(CancellationToken cancellationToken)
        {

            int? lastItemId = null;
            for (; ; )
            {
                using (var context = dbContextFactory.CreateDbContext())
                {
                    var items = await FindUncrawledItemsAsync(
                        context,
                        lastItemId,
                        cancellationToken);

                    if (items.Count == 0)
                    {
                        break;
                    }

                    var itemDict = new Dictionary<string, Models.Item>();
                    foreach (var item in items)
                    {
                        lastItemId = item.Id;

                        if (item.Asin.Length != 10)
                        {
                            continue;
                        }

                        itemDict[item.Asin] = item;

                        if (10 <= itemDict.Count)
                        {
                            await CrawlItemsWithRetryAsync(context, itemDict, cancellationToken);
                            itemDict.Clear();
                        }
                    }

                    if (0 < itemDict.Count)
                    {
                        await CrawlItemsWithRetryAsync(context, itemDict, cancellationToken);
                        itemDict.Clear();
                    }
                }
            }
        }

        async Task<IReadOnlyList<Models.Item>> FindUncrawledItemsAsync(
            ApplicationDbContext context,
            int? lastItemId,
            CancellationToken cancellationToken)
        {
            var query = context.Items
                .Where(i => i.IsComic == null);

            if (lastItemId != null)
            {
                query = query.Where(i => i.Id > lastItemId);
            }

            var items = await query.OrderBy(i => i.Id)
                .Include(i => i.ItemTags)
                .Take(100)
                .ToListAsync(cancellationToken);

            return items;
        }

        const int MaxRetryCount = 3;

        async Task CrawlItemsWithRetryAsync(
            ApplicationDbContext context,
            IDictionary<string, Models.Item> items,
            CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();
            for (; ; )
            {
                try
                {
                    await CrawlItemsAsync(context, items, cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    if (MaxRetryCount < exceptions.Count)
                    {
                        var ae = new AggregateException(exceptions);
                        logger.LogError(
                            ae,
                            $"{string.Join(",", items.Keys)} の商品情報取得で例外が発生しました。");
                        throw ae;
                    }
                    await Task.Delay(200);
                }
            }
        }

        async Task CrawlItemsAsync(
            ApplicationDbContext context,
            IDictionary<string, Models.Item> items,
            CancellationToken cancellationToken)
        {
            var response = await client.LookupAsync(
                articleNumbers: items.Keys.ToList(),
                responseGroup: AmazonResponseGroup.Large);

            if (0 < response?.Items?.Request?.Errors?.Length)
            {
                var messages = string.Join(
                    Environment.NewLine,
                    response.Items.Request.Errors.Select(e => e.Message));
                logger.LogError(messages);
            }

            if (response?.Items?.Item == null)
            {
                return;
            }

            await Task.WhenAll(
                SaveItemsAsync(context, items, response, cancellationToken),
                Task.Delay(1000));
        }

        async Task SaveItemsAsync(
            ApplicationDbContext context,
            IDictionary<string, Models.Item> items,
            AmazonItemResponse response,
            CancellationToken cancellationToken)
        {
            foreach (var item in response.Items.Item)
            {
                if (items.TryGetValue(item.ASIN, out var targetItem))
                {
                    var isComic = IsComic(item);

                    targetItem.SetDetails(
                        isComic: isComic,
                        detailPageUrl: item.DetailPageURL,
                        imageUrl: item.MediumImage?.URL,
                        author: string.Join(",", item.ItemAttributes.Author ?? new string[0]),
                        publisher: item.ItemAttributes.Publisher,
                        description: string.Join(
                            Environment.NewLine,
                            item.EditorialReviews?.Select(e => e.Content) ?? new string[0])
                    );

                    if (isComic)
                    {
                        // タグを保存
                        var tags = GetTags(item);
                        var savedTags = await FindOrCreateTagsAsync(context, tags, cancellationToken);
                        foreach (var tag in savedTags)
                        {
                            if (!targetItem.HasTag(tag))
                            {
                                targetItem.AddTag(tag);
                            }
                        }
                    }

                    context.Items.Update(targetItem);
                    await context.SaveChangesAsync(cancellationToken);
                    logger.LogInformation($"Is {targetItem.Title} comic？: {targetItem.IsComic}");
                }
            }
        }

        async Task<IEnumerable<Tag>> FindOrCreateTagsAsync(
            ApplicationDbContext context,
            IEnumerable<Tag> tags,
            CancellationToken cancellationToken)
        {
            var newTags = new Dictionary<string, Tag>();
            var result = new List<Tag>();

            foreach (var tag in tags)
            {
                var existTag = await context.Tags
                    .Where(t => t.Name == tag.Name)
                    .SingleOrDefaultAsync(cancellationToken);

                if (existTag != null)
                {
                    result.Add(existTag);
                }
                else if (!newTags.ContainsKey(tag.Name))
                {
                    context.Tags.Add(tag);
                    newTags[tag.Name] = tag;
                    result.Add(tag);
                }
            }

            if (0 < newTags.Count)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        static HashSet<Tag> GetTags(AmazonItem item)
        {
            var tags = new HashSet<Tag>();

            if (item.ItemAttributes != null)
            {
                if (item.ItemAttributes.Author != null)
                {
                    foreach (var a in item.ItemAttributes.Author)
                    {
                        var tagName = a.Trim();
                        tags.Add(new Tag(tagName));
                    }
                }

                if (!string.IsNullOrEmpty(item.ItemAttributes.Publisher))
                {
                    tags.Add(new Tag(item.ItemAttributes.Publisher));
                }
            }

            if (item.BrowseNodes != null)
            {
                foreach (var node in item.BrowseNodes)
                {
                    foreach (var segment in node.Name.Split("/"))
                    {
                        var tagName = segment.Trim();
                        tags.Add(new Tag(tagName));
                    }
                }
            }

            return tags;
        }

        static bool IsComic(AmazonItem item)
        {
            if (item.BrowseNodes == null)
            {
                return false;
            }

            return item.BrowseNodes
                .Where(b => b.Name != null)
                    .Where(b => b.Name.Contains("コミック") || b.Name.Contains("マンガ"))
                    .Any();
        }
    }
}
