using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using MangaRank.Models;
using Microsoft.Extensions.Logging;

namespace MangaRank.Services
{
    class BackupCleaner
    {
        readonly CrawlerOptions options;

        readonly ILogger<BackupCleaner> logger;

        readonly StorageClient storageClient;

        public BackupCleaner(CrawlerOptions options, ILoggerFactory loggerFactory)
        {
            this.options = options;
            logger = loggerFactory.CreateLogger<BackupCleaner>();
            storageClient = StorageClient.Create();
        }

        public async Task CleanAsync(CancellationToken cancellationToken)
        {
            var deadlineAt = DateTime.UtcNow.AddDays(-7);
            var result = storageClient.ListObjectsAsync(options.CloudStorageBackupBucketName);
            var enumerator = result.GetEnumerator();
            while (await enumerator.MoveNext(cancellationToken))
            {
                // 1週間より前のバックアップは削除する
                var obj = enumerator.Current;
                if (obj.TimeCreated < deadlineAt)
                {
                    await storageClient.DeleteObjectAsync(obj, cancellationToken: cancellationToken);
                    logger.LogInformation($"{obj.Name} を削除しました。");
                }
            }
        }
    }
}
