using MangaRank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaRank.Data
{
    public class ApplicationDbContext : DbContext
    {
        readonly string connectionString;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext(string connectionString)
            : base()
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connectionString != null)
            {
                optionsBuilder.UseSqlite(connectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BlogConfiguration());
            modelBuilder.ApplyConfiguration(new EntryConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new EntryItemConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new ItemTagConfiguration());
        }

        public DbSet<Blog> Blogs => Set<Blog>();

        public DbSet<Entry> Entries => Set<Entry>();

        public DbSet<Item> Items => Set<Item>();

        public DbSet<EntryItem> EntryItems => Set<EntryItem>();

        public DbSet<Tag> Tags => Set<Tag>();

        public DbSet<ItemTag> ItemTags => Set<ItemTag>();
    }

    class BlogConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Url)
                .HasColumnName("url")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired();

            builder.ToTable("blogs");

            builder.HasKey(e => e.Id);

            builder.HasMany(e => e.Entries)
                .WithOne(e => e.Blog)
                .HasForeignKey(e => e.BlogId);

            builder.HasIndex(e => e.Url)
                .IsUnique(true);
        }
    }

    class EntryConfiguration : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Url)
                .HasColumnName("url")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired();

            builder.Property(e => e.PublishedAt)
                .HasColumnName("published_at")
                .IsRequired();

            builder.Property(e => e.IsCrawled)
                .HasColumnName("is_crawled")
                .IsRequired();

            builder.Property(e => e.BlogId)
                .HasColumnName("blog_id")
                .IsRequired();

            builder.ToTable("entries");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.BlogId);

            builder.HasIndex(e => e.Url).IsUnique(true);

            builder.HasOne(e => e.Blog)
                .WithMany(e => e.Entries)
                .HasForeignKey(e => e.BlogId);

            builder.HasMany(e => e.EntryItems)
                .WithOne(e => e.Entry)
                .HasForeignKey(e => e.EntryId);
        }
    }

    class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Asin)
                .HasColumnName("asin")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired();

            builder.Property(e => e.DetailPageUrl)
                .HasColumnName("detail_page_url");

            builder.Property(e => e.ImageUrl)
                .HasColumnName("image_url");

            builder.Property(e => e.Author)
                .HasColumnName("author");

            builder.Property(e => e.Publisher)
                .HasColumnName("publisher");

            builder.Property(e => e.PublishedOn)
                .HasColumnName("published_on");

            builder.Property(e => e.Description)
                .HasColumnName("description");

            builder.Property(e => e.Score)
                .HasColumnName("score");

            builder.Property(e => e.Rank)
                .HasColumnName("rank");

            builder.Property(e => e.IsComic)
                .HasColumnName("is_comic");

            builder.Property(e => e.Row)
                .HasColumnName("row");

            builder.ToTable("items");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Asin)
                .IsUnique(true);

            builder.HasIndex(e => e.Rank);

            builder.HasIndex(e => e.Row);
        }
    }

    class EntryItemConfiguration : IEntityTypeConfiguration<EntryItem>
    {
        public void Configure(EntityTypeBuilder<EntryItem> builder)
        {
            builder.Property(e => e.EntryId)
                .HasColumnName("entry_id")
                .IsRequired();

            builder.Property(e => e.ItemId)
                .HasColumnName("item_id")
                .IsRequired();

            builder.ToTable("entry_items");

            builder.HasKey(e => new { e.EntryId, e.ItemId });

            builder.HasOne(e => e.Item);

            builder.HasOne(e => e.Entry);
        }
    }

    class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(e => e.Count)
                .HasColumnName("count")
                .IsRequired()
                .HasDefaultValue(0);

            builder.ToTable("tags");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Name)
                .IsUnique(true);

            builder.HasIndex(e => e.Count);

            builder.HasMany(e => e.ItemTags)
                .WithOne(e => e.Tag)
                .HasForeignKey(e => e.TagId);
        }
    }

    class ItemTagConfiguration : IEntityTypeConfiguration<ItemTag>
    {
        public void Configure(EntityTypeBuilder<ItemTag> builder)
        {
            builder.Property(e => e.TagId)
                .HasColumnName("tag_id")
                .IsRequired();

            builder.Property(e => e.ItemId)
                .HasColumnName("item_id")
                .IsRequired();

            builder.Property(e => e.Rank)
                .HasColumnName("rank");

            builder.Property(e => e.Row)
                .HasColumnName("row");

            builder.ToTable("item_tags");

            builder.HasKey(e => new { e.TagId, e.ItemId });

            builder.HasOne(e => e.Item)
                .WithMany(e => e.ItemTags)
                .HasForeignKey(e => e.ItemId);

            builder.HasOne(e => e.Tag)
                .WithMany(e => e.ItemTags)
                .HasForeignKey(e => e.TagId);

            builder.HasIndex(e => new { e.TagId, e.Rank });

            builder.HasIndex(e => new { e.TagId, e.Row });
        }
    }
}
