﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_CONG_THUC.Data.Migrations
{
    /// <inheritdoc />
    public partial class RatingForVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "VideoComments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "VideoComments");
        }
    }
}
