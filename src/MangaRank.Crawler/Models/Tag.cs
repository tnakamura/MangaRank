using System.Collections.Generic;

namespace MangaRank.Models
{
    public class Tag
    {
        protected Tag()
        {
        }

        public Tag(string name)
        {
            Name = name;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        /// <summary>
        /// タグが付いた商品の件数を取得します。
        /// </summary>
        public int Count { get; private set; }

        public IList<ItemTag> ItemTags { get; private set; } = new List<ItemTag>();
    }
}
