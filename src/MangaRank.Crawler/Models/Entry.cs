using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace MangaRank.Models
{
    public class Entry
    {
        protected Entry()
        {
        }

        public Entry(int blogId, string title, string url, DateTime publishedAt)
        {
            Title = title;
            Url = url;
            PublishedAt = publishedAt;
            BlogId = blogId;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Url { get; private set; }

        public DateTime PublishedAt { get; private set; }

        public bool IsCrawled { get; private set; }

        public int BlogId { get; private set; }

        public Blog Blog { get; private set; }

        public IList<EntryItem> EntryItems { get; private set; } = new List<EntryItem>();

        public override string ToString() =>
            $"{PublishedAt.ToString("yyyy-MM-dd")}\t{Title}\t{Url}";

        public void MarkAsCrawled()
        {
            IsCrawled = true;
        }

        public bool HasItem(Item item)
        {
            return EntryItems.Any(e => e.ItemId == item.Id);
        }

        public void AddItem(Item item)
        {
            EntryItems.Add(new EntryItem(Id, item.Id));
        }

        public bool IsValidDateTime =>
            SqlDateTime.MinValue.Value <= PublishedAt &&
            PublishedAt <= SqlDateTime.MaxValue.Value;
    }
}
