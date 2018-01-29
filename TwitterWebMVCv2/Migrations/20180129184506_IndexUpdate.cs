using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TwitterWebMVCv2.Migrations
{
    public partial class IndexUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TweetHashtags_HashtagID",
                table: "TweetHashtags",
                column: "HashtagID");

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_ID",
                table: "Hashtags",
                column: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TweetHashtags_HashtagID",
                table: "TweetHashtags");

            migrationBuilder.DropIndex(
                name: "IX_Hashtags_ID",
                table: "Hashtags");
        }
    }
}
