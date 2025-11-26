using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWorld.Migrations
{
    /// <inheritdoc />
    public partial class scriptsZZ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genress",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenresName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genress", x => x.GenresId);
                });

            migrationBuilder.CreateTable(
                name: "AnimeCharacters",
                columns: table => new
                {
                    AnimeCharacterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimeCharacterName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    BankBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    CharacterPicture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimeCharacters", x => x.AnimeCharacterId);
                    table.ForeignKey(
                        name: "FK_AnimeCharacters_Genress_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genress",
                        principalColumn: "GenresId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimeNames",
                columns: table => new
                {
                    AnimeNameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalEp = table.Column<int>(type: "int", nullable: false),
                    OnGoing = table.Column<bool>(type: "bit", nullable: false),
                    AnimeCharacterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimeNames", x => x.AnimeNameId);
                    table.ForeignKey(
                        name: "FK_AnimeNames_AnimeCharacters_AnimeCharacterId",
                        column: x => x.AnimeCharacterId,
                        principalTable: "AnimeCharacters",
                        principalColumn: "AnimeCharacterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimeCharacters_GenresId",
                table: "AnimeCharacters",
                column: "GenresId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimeNames_AnimeCharacterId",
                table: "AnimeNames",
                column: "AnimeCharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimeNames");

            migrationBuilder.DropTable(
                name: "AnimeCharacters");

            migrationBuilder.DropTable(
                name: "Genress");
        }
    }
}
