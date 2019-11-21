namespace MangaRank.Models
{
    public class EntryItem
    {
        protected EntryItem()
        {
        }

        public EntryItem(int entryId, int itemId)
        {
            ItemId = itemId;
            EntryId = entryId;
        }

        public int EntryId { get; private set; }

        public int ItemId { get; private set; }

        public Entry Entry { get; private set; }

        public Item Item { get; private set; }
    }
}
