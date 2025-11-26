using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWorld.Migrations
{
    /// <inheritdoc />
    public partial class sp_world : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_GetAnimeCharacterById
    @AnimeCharacterId INT
AS
BEGIN
    -- Select Master Record
    SELECT ac.*, g.GenresName
    FROM AnimeCharacters ac
    LEFT JOIN Genress g ON ac.GenresId = g.GenresId
    WHERE ac.AnimeCharacterId = @AnimeCharacterId;

    -- Select Child Records
    SELECT *
    FROM AnimeNames
    WHERE AnimeCharacterId = @AnimeCharacterId;
END
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_InsertAnimeCharacter
(
    @AnimeCharacterName NVARCHAR(100),
    @DateOfBirth DATE,
    @BankBalance DECIMAL(18,2),
    @IsAlive BIT,
    @GenresId INT,
    @CharacterPicture NVARCHAR(200),
    @Address NVARCHAR(MAX),
    @AnimeCharacterId INT OUTPUT
)
AS
BEGIN
    INSERT INTO AnimeCharacters 
    (AnimeCharacterName, DateOfBirth, BankBalance, IsAlive, GenresId, CharacterPicture, Address)
    VALUES
    (@AnimeCharacterName, @DateOfBirth, @BankBalance, @IsAlive, @GenresId, @CharacterPicture, @Address)

    SET @AnimeCharacterId = SCOPE_IDENTITY()
END
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_InsertAnimeName
(
    @AnimationName NVARCHAR(50),
    @TotalEp INT,
    @OnGoing BIT,
    @AnimeCharacterId INT
)
AS
BEGIN
    INSERT INTO AnimeNames (AnimationName, TotalEp, OnGoing, AnimeCharacterId)
    VALUES (@AnimationName, @TotalEp, @OnGoing, @AnimeCharacterId)
END
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_UpdateAnimeCharacter
(
    @AnimeCharacterId INT,
    @AnimeCharacterName NVARCHAR(100),
    @DateOfBirth DATE,
    @BankBalance DECIMAL(18,2),
    @IsAlive BIT,
    @GenresId INT,
    @CharacterPicture NVARCHAR(200),
    @Address NVARCHAR(MAX)
)
AS
BEGIN
    UPDATE AnimeCharacters
    SET 
        AnimeCharacterName = @AnimeCharacterName,
        DateOfBirth = @DateOfBirth,
        BankBalance = @BankBalance,
        IsAlive = @IsAlive,
        GenresId = @GenresId,
        CharacterPicture = @CharacterPicture,
        Address = @Address
    WHERE AnimeCharacterId = @AnimeCharacterId;
END
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_DeleteAnimeCharacter
(
    @AnimeCharacterId INT
)
AS
BEGIN
    DELETE FROM AnimeNames WHERE AnimeCharacterId = @AnimeCharacterId;
    DELETE FROM AnimeCharacters WHERE AnimeCharacterId = @AnimeCharacterId;
END
");
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_DeleteAnimeName
(
    @AnimeNameId INT
)
AS
BEGIN
    DELETE FROM AnimeNames WHERE AnimeNameId = @AnimeNameId;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
