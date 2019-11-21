namespace MangaRank.Models
{
    /// <summary>
    /// 商品に付けられたタグを表します。
    /// </summary>
    public class ItemTag
    {
        protected ItemTag() { }

        public ItemTag(Item item, Tag tag)
        {
            Item = item;
            Tag = tag;
            ItemId = item.Id;
            TagId = tag.Id;
        }

        /// <summary>
        /// 商品 ID を取得します。
        /// </summary>
        public int ItemId { get; private set; }

        /// <summary>
        /// タグ ID を取得します。
        /// </summary>
        public int TagId { get; private set; }

        /// <summary>
        /// タグ内での順位を取得します。
        /// </summary>
        public int? Rank { get; private set; }

        /// <summary>
        /// タグ内での行番号を取得します。
        /// </summary>
        public int? Row { get; private set; }

        /// <summary>
        /// 商品を取得します。
        /// </summary>
        public Item Item { get; private set; }

        /// <summary>
        /// タグを取得します。
        /// </summary>
        public Tag Tag { get; private set; }

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
