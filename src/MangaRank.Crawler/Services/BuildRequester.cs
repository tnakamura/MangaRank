using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MangaRank.Models;
using Microsoft.Extensions.Logging;

namespace MangaRank.Services
{
    class BuildRequester
    {
        readonly HttpClient client;

        readonly ILogger<BuildRequester> logger;

        readonly CrawlerOptions options;

        public BuildRequester(CrawlerOptions options, ILoggerFactory loggerFactory)
        {
            this.options = options;
            client = new HttpClient();
            logger = loggerFactory.CreateLogger<BuildRequester>();
        }

        public async Task RequestAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await client.PostAsync(
                    options.NetlifyWebhookUrl,
                    new StringContent(string.Empty),
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                logger.LogInformation("Netlify にビルドを要求しました。");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Netlify へのビルド要求が失敗しました。");
                throw;
            }
        }
    }
}
