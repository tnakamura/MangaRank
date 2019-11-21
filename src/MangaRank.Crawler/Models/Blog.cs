using System.Collections.Generic;

namespace MangaRank.Models
{
    public class Blog
    {
        protected Blog()
        {
        }

        public Blog(string title, string url)
        {
            Title = title;
            Url = url;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Url { get; private set; }

        public IReadOnlyList<Entry> Entries { get; private set; } = new List<Entry>();

        public override string ToString() => $"{Title}\t{Url}";

        public void UpdateFrom(Blog blog)
        {
            Title = blog.Title;
            Url = blog.Url;
        }
    }
}
