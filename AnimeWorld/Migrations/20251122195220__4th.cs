using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimeWorld.Migrations
{
    /// <inheritdoc />
    public partial class _4th : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE sp_DeleteAnimeCharacterById
    @AnimeCharacterId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 🔹 Step 1: Delete related AnimeName entries (to avoid foreign key constraint issues)
        DELETE FROM AnimeNames
        WHERE AnimeCharacterId = @AnimeCharacterId;

        -- 🔹 Step 2: Delete the AnimeCharacter itself
        DELETE FROM AnimeCharacters
        WHERE AnimeCharacterId = @AnimeCharacterId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000),
                @ErrorSeverity INT,
                @ErrorState INT;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
