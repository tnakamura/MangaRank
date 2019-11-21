using System;
using System.Threading;
using System.Threading.Tasks;
using MangaRank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaRank.Services
{
    class ScoreCalculator
    {
        readonly ApplicationDbContextFactory dbContextFactory;

        readonly ILogger<ScoreCalculator> logger;

        public ScoreCalculator(
            ApplicationDbContextFactory dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            this.dbContextFactory = dbContextFactory;
            logger = loggerFactory.CreateLogger<ScoreCalculator>();
        }

        public async Task CalculateAsync(CancellationToken cancellationToken)
        {
            using (var context = dbContextFactory.CreateDbContext())
            using (var tx = await context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await CalculateScoreAsync(context, cancellationToken);
                    await CalculateTaggedItemCountAsync(context, cancellationToken);
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"スコアの集計中に例外が発生しました。");
                    tx.Rollback();
                    throw;
                }
            }
        }

        static DateTimeOffset JstNow => DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9));

        async Task CalculateScoreAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var beginAt = JstNow;
            logger.LogInformation($"スコア集計開始: {beginAt}");

            await context.Database.ExecuteSqlCommandAsync(@"
-- 一時テーブルを作成
CREATE TEMP TABLE item_scores (
  item_id INT NOT NULL PRIMARY KEY,
  score INT NOT NULL
);

-- 一時テーブルにアイテムのスコアを登録
INSERT INTO item_scores
(item_id, score)
SELECT tmp.item_id
     , COUNT(*) AS score
FROM (
    SELECT DISTINCT
           ei.item_id
         , e.blog_id
    FROM entry_items AS ei
    INNER JOIN entries AS e
            ON ei.entry_id = e.id
) AS tmp
GROUP BY tmp.item_id;

-- 一時テーブルをもとにスコアを更新
UPDATE items
SET score = (SELECT item_scores.score
             FROM item_scores
			 WHERE item_scores.item_id = items.id);
			 
-- 一時テーブルを削除
DROP TABLE item_scores;
", cancellationToken);

            var endAt = JstNow;
            logger.LogInformation($"スコア集計終了: {JstNow}\t所要時間: {endAt - beginAt}");
        }

        async Task CalculateTaggedItemCountAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var beginAt = JstNow;
            logger.LogInformation($"タグ別件数集計開始: {beginAt}");

            await context.Database.ExecuteSqlCommandAsync(@"
UPDATE tags
SET count = (
	SELECT COUNT(*)
    FROM item_tags
	WHERE tag_id = tags.id);
", cancellationToken);

            var endAt = JstNow;
            logger.LogInformation($"タグ別件数集計終了: {endAt}\t所要時間: {endAt - beginAt}");
        }
    }
}
