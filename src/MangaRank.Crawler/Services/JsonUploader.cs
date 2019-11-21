using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using MangaRank.Data;
using MangaRank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MangaRank.Services
{
    public class JsonUploader
    {
        const int MaxItemCount = 1000;

        readonly CrawlerOptions options;

        readonly ILogger<JsonUploader> logger;

        readonly ApplicationDbContextFactory dbContextFactory;

        static readonly Encoding UTF8 = new UTF8Encoding(false);

        readonly JsonSerializer serializer;

        readonly StorageClient storageClient;

        public JsonUploader(
            CrawlerOptions options,
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.dbContextFactory = dbContextFactory;
            logger = loggerFactory.CreateLogger<JsonUploader>();
            storageClient = StorageClient.Create();
            serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            });
        }

        public async Task UploadAsync(CancellationToken cancellationToken)
        {
            try
            {
                var items = await UploadItemsAsync(cancellationToken);
                await UploadTagsAsync(items, cancellationToken);
                await UploadEntriesAsync(items, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Google Cloud Storage へのアップロード中に例外が発生しました。");
                throw;
            }
        }

        async Task<Google.Apis.Storage.v1.Data.Object> UploadCoreAsync(string fileName, Stream source, CancellationToken cancellationToken)
        {
            return await storageClient.UploadObjectAsync(
                bucket: options.CloudStorageDataBucketName,
                objectName: fileName,
                contentType: "application/json",
                source: source,
                options: new UploadObjectOptions
                {
                    PredefinedAcl = PredefinedObjectAcl.PublicRead,
                },
                cancellationToken: cancellationToken);
        }

        async Task<IReadOnlyList<Item>> UploadItemsAsync(CancellationToken cancellationToken)
        {
            using (var context = dbContextFactory.CreateDbContext())
            {
                var items = await context.Items
                    .Where(i => i.IsComic == true)
                    .Where(i => i.Row != null)
                    .OrderBy(i => i.Row)
                    .Take(MaxItemCount)
                    .Include(i => i.ItemTags)
                    .ThenInclude(it => it.Tag)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var itemList = items.Select(i => ToItemDictionary(i)).ToList();

                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream, UTF8))
                {
                    serializer.Serialize(writer, itemList);

                    await writer.FlushAsync();
                    stream.Seek(0, SeekOrigin.Begin);

                    var obj = await UploadCoreAsync("items.json", stream, cancellationToken);
                    logger.LogInformation($"{obj.Name} を作成しました。");
                }

                return items;
            }
        }

        async Task UploadTagsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken)
        {
            var tags = from i in items
                       from it in i.ItemTags
                       group it by it.TagId;

            var tagList = tags.Select(it =>
            {
                var tagDict = new Dictionary<string, object>
                {
                    ["id"] = it.Key.ToString(),
                    ["name"] = it.Select(x => x.Tag.Name).FirstOrDefault(),
                    ["count"] = it.Count(),
                };
                return tagDict;
            }).ToList();

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, UTF8))
            {
                serializer.Serialize(writer, tagList);

                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                var obj = await UploadCoreAsync("tags.json", stream, cancellationToken);
                logger.LogInformation($"{obj.Name} を作成しました。");
            }
        }

        async Task UploadEntriesAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken)
        {
            var first = true;

            using (var context = dbContextFactory.CreateDbContext())
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, UTF8))
            {
                await writer.WriteLineAsync("[");

                foreach (var item in items)
                {
                    var entries = await (
                        from e in context.Entries
                        join ei in context.EntryItems on e.Id equals ei.EntryId
                        where ei.ItemId == item.Id
                        orderby e.PublishedAt descending
                        select e
                        ).Take(100)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

                    foreach (var e in entries)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            await writer.WriteLineAsync(",");
                        }
                        var entryDict = ToEntryDictionary(item, e);
                        serializer.Serialize(writer, entryDict);
                    }
                }

                await writer.WriteLineAsync("]");

                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                var obj = await UploadCoreAsync("entries.json", stream, cancellationToken);
                logger.LogInformation($"{obj.Name} を作成しました。");
            }
        }

        static Dictionary<string, object> ToItemDictionary(Item item)
        {
            var tagList = item.ItemTags
                .OrderBy(x => x.Tag.Count)
                .Select(x => new Dictionary<string, object>
                {
                    ["id"] = x.Tag.Id.ToString(),
                    ["name"] = x.Tag.Name,
                    ["count"] = x.Tag.Count,
                });

            var itemDict = new Dictionary<string, object>()
            {
                ["id"] = item.Id.ToString(),
                ["asin"] = item.Asin,
                ["title"] = item.Title,
                ["detailPageUrl"] = item.DetailPageUrl,
                ["imageUrl"] = item.ImageUrl,
                ["author"] = item.Author,
                ["publisher"] = item.Publisher,
                ["description"] = item.Description,
                ["score"] = item.Score.Value,
                ["tags"] = tagList,
            };

            if (item.PublishedOn.HasValue)
            {
                itemDict["publishedOn"] = item.PublishedOn.Value.ToUniversalTime();
            }

            return itemDict;
        }

        static Dictionary<string, object> ToEntryDictionary(Item item, Entry entry)
        {
            return new Dictionary<string, object>
            {
                ["asin"] = item.Asin,
                ["id"] = $"{entry.Id}_{item.Id}",
                ["url"] = entry.Url,
                ["title"] = entry.Title,
                ["publishedAt"] = entry.PublishedAt.ToUniversalTime(),
            };
        }
    }
}
