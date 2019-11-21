using Microsoft.EntityFrameworkCore.Migrations;

namespace MangaRank.Migrations
{
    public partial class AddDetailPageUrlColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "detail_page_url",
                table: "items",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "detail_page_url",
                table: "items");
        }
    }
}
