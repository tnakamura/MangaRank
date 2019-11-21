using System.Collections.Generic;
using System.Linq;
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
    class HatenaBlogGroupCrawler
    {
        // はてなブロググループの「マンガ」グループを対象にする。
        const string BaseUrl = "http://hatenablog.com/g/11696248318754550860/blogs";

        readonly HttpClient client;

        readonly HtmlParser parser;

        readonly ApplicationDbContextFactory dbContextFactory;

        readonly ILogger<HatenaBlogGroupCrawler> logger;

        public HatenaBlogGroupCrawler(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            client = new HttpClient();
            parser = new HtmlParser();
            logger = loggerFactory.CreateLogger<HatenaBlogGroupCrawler>();
            this.dbContextFactory = dbContextFactory;
        }

        public async Task CrawlAsync(CancellationToken cancellationToken)
        {
            var requestUrl = BaseUrl;
            for (; ; )
            {
                // はてなグループブログ一覧の HTML をダウンロードしてパースする。
                var response = await client.GetAsync(requestUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var task = RegiserBlogsAsync(response, cancellationToken);
                await Task.WhenAll(
                    task,
                    Task.Delay(1000));
                if (task.Result == null)
                {
                    return;
                }

                requestUrl = task.Result;
            }
        }

        async Task<string> RegiserBlogsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var body = await response.Content.ReadAsStringAsync();
            var document = await parser.ParseAsync(body);

            using (var context = dbContextFactory.CreateDbContext())
            {
                foreach (var blog in ExtractBlogs(document))
                {
                    if (await BlogExitesAsync(context, blog, cancellationToken))
                    {
                        await context.SaveChangesAsync(cancellationToken);
                        return null;
                    }

                    context.Blogs.Add(blog);
                    logger.LogInformation($"{blog.Title}");
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            // 次ページへのリンクがあればクロールを続ける。
            // 次ページが無ければ終了。
            if (TryGetNextPageUrl(document, out var nextUrl))
            {
                return nextUrl;
            }
            else
            {
                return null;
            }
        }

        async Task<bool> BlogExitesAsync(ApplicationDbContext context, Blog blog, CancellationToken cancellationToken)
        {
            return await context.Blogs.AnyAsync(b => b.Url == blog.Url, cancellationToken);
        }

        static bool TryGetNextPageUrl(IHtmlDocument document, out string result)
        {
            var next = document.QuerySelector(".more > a") as IHtmlAnchorElement;
            if (next != null)
            {
                result = BaseUrl + next.Search; // クエリ文字列の部分を足す
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        static IEnumerable<Blog> ExtractBlogs(IHtmlDocument document)
        {
            // ブログ一覧からブログタイトルとリンクを取り出す。
            // はてなブロググループのブログ一覧は class を cllass にタイポしてるので、
            // cllass 属性で検索する。
            var elements = document.QuerySelectorAll("[cllass=blog-list-content] > a");
            var anchors = elements.Cast<IHtmlAnchorElement>();

            foreach (var a in anchors)
            {
                var blog = new Blog(title: a.TextContent, url: a.Href);

                yield return blog;
            }
        }
    }
}
