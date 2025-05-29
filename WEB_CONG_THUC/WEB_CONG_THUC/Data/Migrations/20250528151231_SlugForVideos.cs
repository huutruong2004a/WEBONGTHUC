using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CONG_THUC.Data.Migrations
{
    /// <inheritdoc />
    public partial class SlugForVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Videos");
        }
    }
}
