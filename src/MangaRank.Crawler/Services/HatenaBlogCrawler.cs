using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
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
    class HatenaBlogCrawler
    {
        readonly HttpClient client;

        readonly ApplicationDbContextFactory dbContextFactory;

        readonly ILogger<HatenaBlogCrawler> logger;

        public HatenaBlogCrawler(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            this.dbContextFactory = dbContextFactory;
            logger = loggerFactory.CreateLogger<HatenaBlogCrawler>();
            client = new HttpClient();
        }

        public async Task CrawlAsync(CancellationToken cancellationToken)
        {
            var context = new Context();
            int? lastBlogId = null;
            for (; ; )
            {
                var blogs = await FindAllAsync(lastBlogId, cancellationToken);

                if (blogs.Count == 0)
                {
                    return;
                }

                foreach (var blog in blogs)
                {
                    await CrawlBlogAsync(context, blog, cancellationToken);
                    lastBlogId = blog.Id;
                }
            }
        }

        async Task<IReadOnlyList<Blog>> FindAllAsync(int? lastBlogId, CancellationToken cancellationToken)
        {
            using (var context = dbContextFactory.CreateDbContext())
            {
                var query = context.Blogs.AsQueryable();

                if (lastBlogId != null)
                {
                    query = query.Where(b => b.Id > lastBlogId);
                }

                query = query.OrderBy(b => b.Id)
                    .Take(100);

                return await query.ToListAsync(cancellationToken);
            }
        }

        async Task CrawlBlogAsync(Context context, Blog blog, CancellationToken cancellationToken)
        {
            // asin で検索した結果をスクレイピングすれば、
            // 書評記事だけ抽出できるかも
            var requestUrl = GetSearchPageUrl(blog);

            for (; ; )
            {
                try
                {
                    // エントリ一覧の HTML をダウンロードしてパースする。
                    var response = await client.GetAsync(requestUrl, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    var resultTask = SaveEntriesAsync(
                            context,
                            blog,
                            response,
                            cancellationToken);

                    await Task.WhenAll(
                        resultTask,
                        Task.Delay(1000));

                    if (resultTask.Result != null)
                    {
                        requestUrl = resultTask.Result;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (HttpRequestException ex) when (ex.InnerException is AuthenticationException ae)
                {
                    logger.LogWarning(ae, $"{requestUrl} の処理中に例外が発生しましたが続行します。");
                    break;
                }
                catch (HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.HostNotFound)
                {
                    logger.LogWarning(se, $"{requestUrl} の処理中に例外が発生しましたが続行します。");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{requestUrl} の処理中に例外発生。");
                    throw;
                }
            }
        }

        async Task<Uri> SaveEntriesAsync(
            Context crawlContext,
            Blog blog,
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var parser = new HtmlParser();
            var body = await response.Content.ReadAsStringAsync();
            var document = await parser.ParseAsync(body, cancellationToken);

            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                foreach (var entry in ExtractEntries(document, blog))
                {
                    if (await ExistsEntryAsync(dbContext, entry, cancellationToken))
                    {
                        logger.LogInformation($"{entry.Url} は既に存在します。");
                        await dbContext.SaveChangesAsync(cancellationToken);
                        return null;
                    }

                    crawlContext.AddCount();
                    dbContext.Entries.Add(entry);
                    logger.LogInformation($"{crawlContext.Count}\t{entry.Title}");
                }
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // 次ページへのリンクがあればクロールを続ける。
            // 次ページが無ければ終了。
            if (TryGetNextPageUri(document, out var nextUri))
            {
                return nextUri;
            }
            else
            {
                logger.LogInformation($"{blog.Title} の記事をすべてクロールしました。");
                return null;
            }
        }

        async Task<bool> ExistsEntryAsync(ApplicationDbContext context, Entry entry, CancellationToken cancellationToken)
        {
            return await context.Entries.AnyAsync(e => e.Url == entry.Url);
        }

        static Uri GetSearchPageUrl(Blog blog)
        {
            return new Uri(new Uri(blog.Url), "search?q=asin");
        }

        static IEnumerable<Entry> ExtractEntries(IHtmlDocument document, Blog blog)
        {
            var entryElements = document.QuerySelectorAll(".archive-entry");
            foreach (var entryElement in entryElements)
            {
                var time = entryElement.QuerySelector(".archive-date > a > time") as IHtmlTimeElement;
                if (time == null)
                {
                    continue;
                }
                if (!DateTime.TryParse(time.DateTime, out var publishedAt))
                {
                    continue;
                }

                var a = entryElement.QuerySelector("a.entry-title-link") as IHtmlAnchorElement;
                if (a == null)
                {
                    continue;
                }

                var entry = new Entry(
                    title: a.TextContent,
                    url: a.Href,
                    publishedAt: publishedAt,
                    blogId: blog.Id);

                if (!entry.IsValidDateTime)
                {
                    continue;
                }

                yield return entry;
            }
        }

        static bool TryGetNextPageUri(IHtmlDocument document, out Uri nextUri)
        {
            var next = document.QuerySelector(".pager-next > a") as IHtmlAnchorElement;
            if (next != null)
            {
                nextUri = new Uri(next.Href);
                return true;
            }
            else
            {
                nextUri = null;
                return false;
            }
        }

        class Context
        {
            public long Count { get; private set; }

            public void AddCount(long add = 1)
            {
                Count += add;
            }
        }
    }
}
