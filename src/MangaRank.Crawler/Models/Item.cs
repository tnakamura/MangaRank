using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaRank.Models
{
    /// <summary>
    /// Amazon の商品を表します。
    /// </summary>
    public class Item
    {
        /// <summary>
        /// <see cref="Item"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public Item()
        {
        }

        /// <summary>
        /// <see cref="Item"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="asin">ASIN</param>
        /// <param name="title">タイトル</param>
        public Item(string asin, string title)
        {
            Asin = asin;
            Title = title;
        }

        /// <summary>
        /// ID を取得または設定します。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ASIN を取得または設定します。
        /// </summary>
        public string Asin { get; set; }

        /// <summary>
        /// 商品ページの URL を取得または設定します。
        /// </summary>
        public string DetailPageUrl { get; set; }

        /// <summary>
        /// タイトルを取得または設定します。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// コミックかどうか示す値を取得または設定します。
        /// </summary>
        public bool? IsComic { get; set; }

        /// <summary>
        /// 画像 URL を取得または設定します。
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 著者を取得または設定します。
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 説明を取得または設定します。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 出版社を取得または設定します。
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// 出版日を取得または設定します。
        /// </summary>
        public DateTime? PublishedOn { get; set; }

        /// <summary>
        /// スコアを取得または設定します。
        /// </summary>
        public int? Score { get; set; }

        /// <summary>
        /// 順位を取得または設定します。
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// 行番号を取得または設定します。
        /// </summary>
        public int? Row { get; set; }

        public IList<ItemTag> ItemTags { get; set; } = new List<ItemTag>();

        public bool HasTag(Tag tag)
        {
            return ItemTags.Where(x => x.ItemId == Id)
                .Where(x => x.TagId == tag.Id)
                .Any();
        }

        public void AddTag(Tag tag)
        {
            ItemTags.Add(new ItemTag(this, tag));
        }

        public override string ToString() =>
            $"{Asin}\t{Title}";

        public void SetDetails(
            bool isComic,
            string detailPageUrl,
            string imageUrl,
            string author,
            string description,
            string publisher)
        {
            DetailPageUrl = detailPageUrl;
            IsComic = isComic;
            ImageUrl = imageUrl;
            Author = author;
            Description = description;
            Publisher = publisher;
        }

        public void ChangeScore(int score)
        {
            Score = score;
        }

        /// <summary>
        /// 順位を変更します。
        /// </summary>
        /// <param name="rank">新しい順位</param>
        public void ChangeRank(int rank)
        {
            Rank = rank;
        }
    }
}
