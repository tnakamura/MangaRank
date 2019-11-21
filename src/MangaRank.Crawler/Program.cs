using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Diagnostics.AspNetCore;
using MangaRank.Data;
using MangaRank.Models;
using MangaRank.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MangaRank
{
    class Program
    {
        public static IConfiguration Configuration { get; }

        public static CrawlerOptions Options { get; } = new CrawlerOptions();

        static Program()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            Configuration.Bind("CrawlerOptions", Options);
        }

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            InitializeGoogleCloudSdk();

            Encoding.RegisterProvider(UTF8EncodingProvider.Default);

            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddGoogle(Options.GoogleCloudProjectId, LoggerOptions.Create(LogLevel.Warning));

            var dbContextFactory = new ApplicationDbContextFactory();

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                (a, b, c, d) => true);

            var source = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                source.Cancel();
            };

            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                await dbContext.Database.MigrateAsync(source.Token);
            }

            var subCommand = args.FirstOrDefault();
            for (; ; )
            {
                switch (subCommand)
                {
                    case "hatenabloggroup":
                    case "0":
                        await CrawlHatenaBlogGroupAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "hatenablog":
                    case "1":
                        await CrawlHatenaBlogAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "hatenablogentry":
                    case "2":
                        await CrawlHatenaBlogEntryAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "amazon":
                    case "3":
                        await CrawlAmazonAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "calculate":
                    case "4":
                        await CalculateScoreAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "exportjson":
                    case "5":
                        await ExportJsonAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "uploadjson":
                    case "6":
                        await UploadJsonAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    case "backup":
                    case "7":
                        await BackupDatabaseAsync(loggerFactory, source.Token);
                        return;
                    case "requestbuild":
                    case "8":
                        await RequestBuildAsync(loggerFactory, source.Token);
                        return;
                    case "cleanbackup":
                    case "9":
                        await CleanBackupAsync(loggerFactory, source.Token);
                        return;
                    case "executeall":
                    case "10":
                        await ExecuteAllAsync(dbContextFactory, loggerFactory, source.Token);
                        return;
                    default:
                        Console.WriteLine("[0] hatenabloggroup");
                        Console.WriteLine("[1] hatenablog");
                        Console.WriteLine("[2] hatenablogentry");
                        Console.WriteLine("[3] amazon");
                        Console.WriteLine("[4] calculate");
                        Console.WriteLine("[5] exportjson");
                        Console.WriteLine("[6] uploadjson");
                        Console.WriteLine("[7] backup");
                        Console.WriteLine("[8] requestbuild");
                        Console.WriteLine("[9] cleanbackup");
                        Console.WriteLine("[10] executeall");
                        subCommand = Console.ReadLine();
                        break;
                }
            }
        }

        static async Task ExecuteAllAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            await CrawlHatenaBlogGroupAsync(dbContextFactory, loggerFactory, cancellationToken);
            await CrawlHatenaBlogAsync(dbContextFactory, loggerFactory, cancellationToken);
            await CrawlHatenaBlogEntryAsync(dbContextFactory, loggerFactory, cancellationToken);
            await CrawlAmazonAsync(dbContextFactory, loggerFactory, cancellationToken);
            await CalculateScoreAsync(dbContextFactory, loggerFactory, cancellationToken);
            await UploadJsonAsync(dbContextFactory, loggerFactory, cancellationToken);
            await BackupDatabaseAsync(loggerFactory, cancellationToken);
            await CleanBackupAsync(loggerFactory, cancellationToken);
            await RequestBuildAsync(loggerFactory, cancellationToken);
        }

        static async Task CleanBackupAsync(
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var cleaner = new BackupCleaner(
                Options,
                loggerFactory);
            await cleaner.CleanAsync(cancellationToken);
        }

        static async Task CrawlAmazonAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var crawler = new AmazonCrawler(
                Options,
                dbContextFactory,
                loggerFactory);
            await crawler.CrawlAsync(cancellationToken);
        }

        static async Task CrawlHatenaBlogEntryAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var crawler = new HatenaBlogEntryCrawler(
                dbContextFactory,
                loggerFactory);
            await crawler.CrawlAsync(cancellationToken);
        }

        static async Task CrawlHatenaBlogAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var crawler = new HatenaBlogCrawler(
                dbContextFactory,
                loggerFactory);
            await crawler.CrawlAsync(cancellationToken);
        }

        static async Task CrawlHatenaBlogGroupAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var crawler = new HatenaBlogGroupCrawler(
                dbContextFactory,
                loggerFactory);
            await crawler.CrawlAsync(cancellationToken);
        }

        static async Task CalculateScoreAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var calculator = new ScoreCalculator(dbContextFactory, loggerFactory);
            await calculator.CalculateAsync(cancellationToken);
        }

        static async Task ExportJsonAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var exporter = new JsonExporter(
                dbContextFactory,
                loggerFactory,
                AppContext.BaseDirectory);
            await exporter.ExportAsync(cancellationToken);
        }

        static async Task UploadJsonAsync(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var uploader = new JsonUploader(
                Options,
                dbContextFactory,
                loggerFactory);
            await uploader.UploadAsync(cancellationToken);
        }

        static async Task BackupDatabaseAsync(
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var uploader = new DatabaseBackuper(
                Options,
                loggerFactory);
            await uploader.BackupAsync(cancellationToken);
        }

        static async Task RequestBuildAsync(
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var requester = new BuildRequester(
                Options,
                loggerFactory);
            await requester.RequestAsync(cancellationToken);
        }

        static void InitializeGoogleCloudSdk()
        {
            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                Path.Combine(
                    AppContext.BaseDirectory,
                    Options.GoogleApplicationCredentialsFile));
        }

        sealed class UTF8EncodingProvider : EncodingProvider
        {
            static readonly Encoding UTF8 = new UTF8Encoding(false);

            public static readonly EncodingProvider Default = new UTF8EncodingProvider();

            private UTF8EncodingProvider()
            {
            }

            public override Encoding GetEncoding(int codepage)
            {
                if (codepage == UTF8.CodePage)
                {
                    return UTF8;
                }
                else
                {
                    return null;
                }
            }

            public override Encoding GetEncoding(string name)
            {
                switch (name)
                {
                    case "utf8":
                        return UTF8;
                    default:
                        return null;
                }
            }
        }
    }
}
