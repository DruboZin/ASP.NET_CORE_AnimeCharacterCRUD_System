using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWorld.Migrations
{
    /// <inheritdoc />
    public partial class sp_2nd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_InsertAnimeCharacter
    @AnimeCharacterName NVARCHAR(50),
    @DateOfBirth DATE,
    @BankBalance DECIMAL(18,2),
    @IsAlive BIT,
    @CharacterPicture NVARCHAR(200),
    @Address NVARCHAR(200),
    @GenresId INT,
    @NewAnimeCharacterId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AnimeCharacters
    (
        AnimeCharacterName, 
        DateOfBirth, 
        BankBalance, 
        IsAlive, 
        CharacterPicture, 
        Address, 
        GenresId
    )
    VALUES
    (
        @AnimeCharacterName, 
        @DateOfBirth, 
        @BankBalance, 
        @IsAlive, 
        @CharacterPicture, 
        @Address, 
        @GenresId
    );

    SET @NewAnimeCharacterId = SCOPE_IDENTITY();
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
