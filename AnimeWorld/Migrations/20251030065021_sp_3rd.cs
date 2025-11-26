using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWorld.Migrations
{
    /// <inheritdoc />
    public partial class sp_3rd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_DeleteAnimeNamesByCharacter
    @AnimeCharacterId INT
AS
BEGIN
    DELETE FROM AnimeNames WHERE AnimeCharacterId = @AnimeCharacterId;
END
GO
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_GetAnimeCharacterById
    @AnimeCharacterId INT
AS
BEGIN
    SELECT ac.*, g.GenresName
    FROM AnimeCharacters ac
    LEFT JOIN Genress g ON ac.GenresId = g.GenresId
    WHERE ac.AnimeCharacterId = @AnimeCharacterId;

    SELECT *
    FROM AnimeNames
    WHERE AnimeCharacterId = @AnimeCharacterId;
END
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
