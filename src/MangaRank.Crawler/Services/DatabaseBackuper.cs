using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using MangaRank.Models;
using Microsoft.Extensions.Logging;

namespace MangaRank.Services
{
    class DatabaseBackuper
    {
        readonly ILogger<DatabaseBackuper> logger;

        readonly StorageClient storageClient;

        readonly CrawlerOptions options;

        public DatabaseBackuper(
            CrawlerOptions options,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            logger = loggerFactory.CreateLogger<DatabaseBackuper>();
            storageClient = StorageClient.Create();
        }

        public async Task BackupAsync(CancellationToken cancellationToken)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "MangaRank.db");
            if (!File.Exists(path))
            {
                logger.LogWarning($"{path} が存在しません。");
                return;
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var objectName = $"MangaRank{suffix}.db";
                    var obj = await UploadCoreAsync(
                        objectName,
                        "application/x-sqlite3",
                        stream,
                        cancellationToken);
                    logger.LogInformation($"{obj.Name} を作成しました。");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"${path} のアップロード中に例外が発生しました。");
                throw;
            }
        }

        async Task<Google.Apis.Storage.v1.Data.Object> UploadCoreAsync(
            string fileName,
            string contentType,
            Stream source,
            CancellationToken cancellationToken)
        {
            return await storageClient.UploadObjectAsync(
                bucket: options.CloudStorageBackupBucketName,
                objectName: fileName,
                contentType: contentType,
                source: source,
                options: new UploadObjectOptions
                {
                    PredefinedAcl = PredefinedObjectAcl.BucketOwnerFullControl,
                },
                cancellationToken: cancellationToken);
        }
    }
}
