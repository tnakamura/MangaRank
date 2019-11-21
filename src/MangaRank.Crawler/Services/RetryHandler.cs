using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MangaRank.Services
{
    class RetryHandler : DelegatingHandler
    {
        const int RETRY_COUNT = 3;

        public RetryHandler()
            : this(new HttpClientHandler())
        {
        }

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();
            var retryCount = 0;
            for (; ; )
            {
                try
                {
                    var response = await base.SendAsync(request, cancellationToken);
                    return response;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    if (CanRetry(retryCount))
                    {
                        retryCount++;
                    }
                    else
                    {
                        throw new AggregateException(exceptions);
                    }
                }
                await Task.Delay(500, cancellationToken);
            }
        }

        bool CanRetry(int retryCount)
        {
            return retryCount < RETRY_COUNT;
        }
    }
}
