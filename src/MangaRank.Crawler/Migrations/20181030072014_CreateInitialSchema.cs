using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MangaRank.Migrations
{
    public partial class CreateInitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blogs",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    asin = table.Column<string>(nullable: false),
                    title = table.Column<string>(nullable: false),
                    is_comic = table.Column<bool>(nullable: true),
                    image_url = table.Column<string>(nullable: true),
                    author = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    publisher = table.Column<string>(nullable: true),
                    published_on = table.Column<DateTime>(nullable: true),
                    score = table.Column<int>(nullable: true),
                    rank = table.Column<int>(nullable: true),
                    row = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(nullable: false),
                    count = table.Column<int>(nullable: false, defaultValue: 0)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false),
                    published_at = table.Column<DateTime>(nullable: false),
                    is_crawled = table.Column<bool>(nullable: false),
                    blog_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_entries_blogs_blog_id",
                        column: x => x.blog_id,
                        principalTable: "blogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_tags",
                columns: table => new
                {
                    item_id = table.Column<int>(nullable: false),
                    tag_id = table.Column<int>(nullable: false),
                    rank = table.Column<int>(nullable: true),
                    row = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_tags", x => new { x.tag_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_item_tags_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entry_items",
                columns: table => new
                {
                    entry_id = table.Column<int>(nullable: false),
                    item_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entry_items", x => new { x.entry_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_entry_items_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entry_items_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blogs_url",
                table: "blogs",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entries_blog_id",
                table: "entries",
                column: "blog_id");

            migrationBuilder.CreateIndex(
                name: "IX_entries_url",
                table: "entries",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entry_items_item_id",
                table: "entry_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_tags_item_id",
                table: "item_tags",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_tags_tag_id_rank",
                table: "item_tags",
                columns: new[] { "tag_id", "rank" });

            migrationBuilder.CreateIndex(
                name: "IX_item_tags_tag_id_row",
                table: "item_tags",
                columns: new[] { "tag_id", "row" });

            migrationBuilder.CreateIndex(
                name: "IX_items_asin",
                table: "items",
                column: "asin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_rank",
                table: "items",
                column: "rank");

            migrationBuilder.CreateIndex(
                name: "IX_items_row",
                table: "items",
                column: "row");

            migrationBuilder.CreateIndex(
                name: "IX_tags_count",
                table: "tags",
                column: "count");

            migrationBuilder.CreateIndex(
                name: "IX_tags_name",
                table: "tags",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entry_items");

            migrationBuilder.DropTable(
                name: "item_tags");

            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "blogs");
        }
    }
}
