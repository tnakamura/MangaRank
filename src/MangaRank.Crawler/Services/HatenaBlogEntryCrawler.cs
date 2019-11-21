using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using MangaRank.Data;
using MangaRank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaRank.Services
{
    class HatenaBlogEntryCrawler
    {
        readonly HttpClient client;

        readonly HtmlParser parser;

        readonly ApplicationDbContextFactory dbContextFactory;

        readonly ILogger<HatenaBlogEntryCrawler> logger;

        public HatenaBlogEntryCrawler(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            this.dbContextFactory = dbContextFactory;
            logger = loggerFactory.CreateLogger<HatenaBlogEntryCrawler>();
            client = new HttpClient(new RetryHandler());
            parser = new HtmlParser();
        }

        public async Task CrawlAsync(CancellationToken cancellationToken)
        {
            var crawlContext = new CrawlContext();
            int? lastEntryId = null;
            for (; ; )
            {
                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    var hasEntries = false;

                    var entries = await FindUncrawledEntriesAsync(
                        dbContext,
                        lastEntryId,
                        cancellationToken);

                    foreach (var entry in entries)
                    {
                        hasEntries = true;

                        await CrawlEntryAsync(
                            dbContext,
                            crawlContext,
                            entry,
                            cancellationToken);

                        lastEntryId = entry.Id;
                    }

                    if (!hasEntries)
                    {
                        break;
                    }
                }
            }
        }

        async Task<IReadOnlyList<Entry>> FindUncrawledEntriesAsync(
            ApplicationDbContext dbContext,
            int? lastEntryId,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Entries
                .Where(e => e.IsCrawled == false);

            if (lastEntryId != null)
            {
                query = query.Where(e => e.Id > lastEntryId);
            }

            query = query.OrderBy(e => e.Id)
                .Include(e => e.EntryItems)
                .Take(100);

            return await query.ToListAsync(cancellationToken);
        }


        async Task CrawlEntryAsync(
            ApplicationDbContext dbContext,
            CrawlContext crawlContext,
            Entry entry,
            CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(entry.Url, cancellationToken);

            await Task.WhenAll(
                RegisterItemsAsync(dbContext, crawlContext, entry, response, cancellationToken),
                Task.Delay(1000));
        }

        async Task RegisterItemsAsync(
            ApplicationDbContext dbContext,
            CrawlContext crawlContext,
            Entry entry,
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Found)
            {
                goto END_CRAWL;
            }
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning($"{entry.Url} の取得に失敗しました。({response.StatusCode})");
                return;
            }

            var html = await response.Content.ReadAsStringAsync();
            var document = await parser.ParseAsync(html, cancellationToken);
            var newItems = ExtractItems(document);

            foreach (var newItem in newItems)
            {
                var existedItem = await FindByAsinAsync(
                    dbContext,
                    newItem.Asin,
                    cancellationToken);

                Item item;
                if (existedItem != null)
                {
                    item = existedItem;
                }
                else
                {
                    // １つのエントリで同じ商品を複数回紹介することがあるので、
                    // 新しい商品は即時登録しておく。
                    dbContext.Items.Add(newItem);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    item = newItem;
                }

                if (!entry.HasItem(item))
                {
                    entry.AddItem(item);
                }

                crawlContext.AddCount();

                logger.LogInformation($"{crawlContext.Count}\t{newItem.Title}\t{entry.Url}");
            }

            END_CRAWL:
            entry.MarkAsCrawled();
            dbContext.Entries.Update(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation($"{entry.Url} スクレイピング終了");
        }

        async Task<Item> FindByAsinAsync(ApplicationDbContext dbContext, string asin, CancellationToken cancellationToken)
        {
            return await dbContext.Items
                .Where(i => i.Asin == asin)
                .Include(i => i.ItemTags)
                .SingleOrDefaultAsync(cancellationToken);
        }

        static IEnumerable<Item> ExtractItems(IHtmlDocument document)
        {
            foreach (var item in ExtractItemsForHatena(document))
                yield return item;

            foreach (var item in ExtractItemsForYomereba(document))
                yield return item;

            foreach (var item in ExtractItemsForKaereba(document))
                yield return item;
        }

        static IEnumerable<Item> ExtractItemsForKaereba(IHtmlDocument document)
        {
            var elements = document.QuerySelectorAll(".kaerebalink-name > a");
            var anchors = elements.Cast<IHtmlAnchorElement>();
            foreach (var a in anchors)
            {
                var asin = ExtractAsin(a.Href);
                if (string.IsNullOrEmpty(asin))
                {
                    continue;
                }
                var title = a.TextContent;
                yield return new Item(asin, title);
            }
        }

        static IEnumerable<Item> ExtractItemsForYomereba(IHtmlDocument document)
        {
            var elements = document.QuerySelectorAll(".booklink-name > a");
            var anchors = elements.Cast<IHtmlAnchorElement>();
            foreach (var a in anchors)
            {
                var asin = ExtractAsin(a.Href);
                if (string.IsNullOrEmpty(asin))
                {
                    continue;
                }
                var title = a.TextContent;
                yield return new Item(asin, title);
            }
        }

        static IEnumerable<Item> ExtractItemsForHatena(IHtmlDocument document)
        {
            var elements = document.QuerySelectorAll(".hatena-asin-detail-title > a");
            var anchors = elements.Cast<IHtmlAnchorElement>();
            foreach (var a in anchors)
            {
                var asin = ExtractAsin(a.Href);
                if (string.IsNullOrEmpty(asin))
                {
                    continue;
                }
                var title = a.TextContent;
                yield return new Item(asin, title);
            }
        }

        static string ExtractAsin(string url)
        {
            const string ASIN = "ASIN/";

            var index = url.IndexOf(ASIN);
            if (index < 0)
            {
                return null;
            }

            var sub = url.Substring(index + ASIN.Length);
            var slash = sub.IndexOf("/");
            if (slash < 0)
            {
                return null;
            }

            var result = sub.Substring(0, slash);
            return result;
        }

        class CrawlContext
        {
            public long Count { get; private set; }

            public void AddCount(long add = 1) => Count += add;
        }
    }
}
