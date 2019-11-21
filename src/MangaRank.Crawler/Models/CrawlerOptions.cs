namespace MangaRank.Models
{
    public class CrawlerOptions
    {
        public string PaApiAccessKeyId { get; set; }

        public string PaApiSecretKey { get; set; }

        public string PaApiAssociateTag { get; set; }

        public string NetlifyWebhookUrl { get; set; }

        public string CloudStorageBackupBucketName { get; set; }

        public string CloudStorageDataBucketName { get; set; }

        public string GoogleApplicationCredentialsFile { get; set; }

        public string GoogleCloudProjectId { get; set; }
    }
}
