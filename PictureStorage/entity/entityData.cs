using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EntityStorage.entity
{
    public partial class FirstMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PicturesDetails",
                columns: table => new
                {
                    PictureDetailsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PictureInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PicturesDetails", x => x.PictureDetailsId);
                });

            migrationBuilder.CreateTable(
                name: "PicturesInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    PictureDetailsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PicturesInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PicturesInfo_PicturesDetails_PictureDetailsId",
                        column: x => x.PictureDetailsId,
                        principalTable: "PicturesDetails",
                        principalColumn: "PictureDetailsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecognizedCategories",
                columns: table => new
                {
                    ObjectId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PictureInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    PictureInformationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizedCategories", x => x.ObjectId);
                    table.ForeignKey(
                        name: "FK_RecognizedCategories_PicturesInfo_PictureInformationId",
                        column: x => x.PictureInformationId,
                        principalTable: "PicturesInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PicturesInfo_PictureDetailsId",
                table: "PicturesInfo",
                column: "PictureDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedCategories_PictureInformationId",
                table: "RecognizedCategories",
                column: "PictureInformationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognizedCategories");

            migrationBuilder.DropTable(
                name: "PicturesInfo");

            migrationBuilder.DropTable(
                name: "PicturesDetails");
        }
    }
}
