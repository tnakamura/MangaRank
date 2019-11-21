using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MangaRank.Data;
using MangaRank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MangaRank.Services
{
    public class JsonExporter
    {
        readonly ApplicationDbContextFactory dbContextFactory;

        readonly ILogger<JsonExporter> logger;

        readonly string basePath;

        static readonly Encoding UTF8 = new UTF8Encoding(false);

        readonly JsonSerializer serializer;

        const int MaxItemCount = 1000;

        public JsonExporter(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            string basePath)
        {
            this.dbContextFactory = dbContextFactory;
            this.basePath = basePath;
            logger = loggerFactory.CreateLogger<JsonExporter>();
            serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            });
        }

        public async Task ExportAsync(CancellationToken cancellationToken)
        {
            var items = await ExportItemsAsync(cancellationToken);

            ExportTags(items, cancellationToken);

            await ExportEntriesAsync(items, cancellationToken);
        }

        async Task<IReadOnlyList<Item>> ExportItemsAsync(CancellationToken cancellationToken)
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

                var path = Path.Combine(basePath, "items.json");
                using (var writer = new StreamWriter(path, false, UTF8))
                {
                    serializer.Serialize(writer, itemList);
                }

                logger.LogInformation($"{path} を作成しました。");
                return items;
            }
        }

        void ExportTags(IReadOnlyList<Item> items, CancellationToken cancellationToken)
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

            var path = Path.Combine(basePath, "tags.json");
            using (var writer = new StreamWriter(path, false, UTF8))
            {
                serializer.Serialize(writer, tagList);
            }

            logger.LogInformation($"{path} を作成しました。");
        }

        async Task ExportEntriesAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken)
        {
            var path = Path.Combine(basePath, "entries.json");
            var first = true;

            using (var context = dbContextFactory.CreateDbContext())
            using (var writer = new StreamWriter(path, false, UTF8))
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
            }

            logger.LogInformation($"{path} を作成しました。");
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
