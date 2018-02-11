using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TwitterWebMVCv2.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagID",
                table: "TweetHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetID",
                table: "TweetHashtags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TweetHashtags",
                table: "TweetHashtags");

            migrationBuilder.DropIndex(
                name: "IX_TweetHashtags_TweetID",
                table: "TweetHashtags");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "TweetHashtags",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "TweetHashtags",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TweetHashtags",
                table: "TweetHashtags",
                column: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TweetHashtags",
                table: "TweetHashtags");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "TweetHashtags");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "TweetHashtags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TweetHashtags",
                table: "TweetHashtags",
                columns: new[] { "HashtagID", "TweetID" });

            migrationBuilder.CreateIndex(
                name: "IX_TweetHashtags_TweetID",
                table: "TweetHashtags",
                column: "TweetID");

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Hashtags_HashtagID",
                table: "TweetHashtags",
                column: "HashtagID",
                principalTable: "Hashtags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TweetHashtags_Tweets_TweetID",
                table: "TweetHashtags",
                column: "TweetID",
                principalTable: "Tweets",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
