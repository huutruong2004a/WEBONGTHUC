using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WEB_CONG_THUC.Data.Migrations
{
    /// <inheritdoc />
    public partial class user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Recipes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RecipeReview",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Bữa sáng", null },
                    { 2, "Bữa trưa", null },
                    { 3, "Bữa tối", null },
                    { 4, "Tráng miệng", null },
                    { 5, "Món khai vị", null },
                    { 6, "Món chay", null },
                    { 7, "Đồ uống", null },
                    { 8, "Bánh ngọt", null },
                    { 9, "Món ăn nhanh", null },
                    { 10, "Món ăn truyền thống", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_UserId",
                table: "Recipes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeReview_UserId",
                table: "RecipeReview",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeReview_AspNetUsers_UserId",
                table: "RecipeReview",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_AspNetUsers_UserId",
                table: "Recipes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeReview_AspNetUsers_UserId",
                table: "RecipeReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_AspNetUsers_UserId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_UserId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_RecipeReview_UserId",
                table: "RecipeReview");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RecipeReview");
        }
    }
}
