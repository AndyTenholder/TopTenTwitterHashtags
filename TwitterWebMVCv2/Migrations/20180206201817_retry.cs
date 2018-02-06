using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TwitterWebMVCv2.Migrations
{
    public partial class retry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hashtags",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tweets",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateTime = table.Column<DateTime>(nullable: false),
                    LanguageID = table.Column<int>(nullable: false),
                    TweetIdString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweets", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tweets_Languages_LanguageID",
                        column: x => x.LanguageID,
                        principalTable: "Languages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetHashtags",
                columns: table => new
                {
                    HashtagID = table.Column<int>(nullable: false),
                    TweetID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetHashtags", x => new { x.HashtagID, x.TweetID });
                    table.ForeignKey(
                        name: "FK_TweetHashtags_Hashtags_HashtagID",
                        column: x => x.HashtagID,
                        principalTable: "Hashtags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TweetHashtags_Tweets_TweetID",
                        column: x => x.TweetID,
                        principalTable: "Tweets",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_ID",
                table: "Hashtags",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_TweetHashtags_HashtagID",
                table: "TweetHashtags",
                column: "HashtagID");

            migrationBuilder.CreateIndex(
                name: "IX_TweetHashtags_TweetID",
                table: "TweetHashtags",
                column: "TweetID");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_DateTime",
                table: "Tweets",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_LanguageID",
                table: "Tweets",
                column: "LanguageID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TweetHashtags");

            migrationBuilder.DropTable(
                name: "Hashtags");

            migrationBuilder.DropTable(
                name: "Tweets");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
