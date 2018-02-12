using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TwitterWebMVCv2.Migrations
{
    public partial class UnixTimeStamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tweets_DateTime",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "TweetHashtags");

            migrationBuilder.AddColumn<int>(
                name: "UnixTimeStamp",
                table: "Tweets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnixTimeStamp",
                table: "TweetHashtags",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_UnixTimeStamp",
                table: "Tweets",
                column: "UnixTimeStamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tweets_UnixTimeStamp",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "UnixTimeStamp",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "UnixTimeStamp",
                table: "TweetHashtags");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Tweets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "TweetHashtags",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_DateTime",
                table: "Tweets",
                column: "DateTime");
        }
    }
}
