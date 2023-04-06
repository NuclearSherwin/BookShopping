using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShopping.Migrations
{
    public partial class addFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgPath",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_FileId",
                table: "Books",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Files_FileId",
                table: "Books",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Files_FileId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Books_FileId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Books");

            migrationBuilder.AddColumn<string>(
                name: "ImgPath",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
