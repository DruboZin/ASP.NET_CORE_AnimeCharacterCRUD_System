CREATE TYPE dbo.AnimeNameType AS TABLE
(
    AnimationName varchar(50),
    TotalEp int,
    OnGoing bit
)
GO
CREATE PROCEDURE sp_CreateAnimeCharacterWithAnimeNames
    @AnimeCharacterName varchar(50),
    @DateOfBirth date,
    @BankBalance decimal(18, 2),
    @IsAlive bit,
    @CharacterPicture varchar(255),
    @Address varchar(200),
    @GenresId int,
    @AnimeNameType dbo.AnimeNameType READONLY  -- Table-Valued Parameter
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert AnimeCharacter
    INSERT INTO AnimeCharacter (AnimeCharacterName, DateOfBirth, BankBalance, IsAlive, CharacterPicture, Address, GenresId)
    VALUES (@AnimeCharacterName, @DateOfBirth, @BankBalance, @IsAlive, @CharacterPicture, @Address, @GenresId)

    -- Get the last inserted AnimeCharacterId
    DECLARE @AnimeCharacterId INT = SCOPE_IDENTITY()

    -- Insert AnimeNames
    INSERT INTO AnimeName (AnimeCharacterId, AnimationName, TotalEp, OnGoing)
    SELECT @AnimeCharacterId, AnimationName, TotalEp, OnGoing FROM @AnimeNameType
END
GO
CREATE PROCEDURE sp_UpdateAnimeCharacterWithAnimeNames
    @AnimeCharacterId INT,
    @AnimeCharacterName varchar(50),
    @DateOfBirth date,
    @BankBalance decimal(18, 2),
    @IsAlive bit,
    @CharacterPicture varchar(255),
    @Address varchar(200),
    @GenresId int,
    @AnimeNameType dbo.AnimeNameType READONLY  -- Table-Valued Parameter
AS
BEGIN
    SET NOCOUNT ON;

    -- Update AnimeCharacter
    UPDATE AnimeCharacter
    SET AnimeCharacterName = @AnimeCharacterName, 
        DateOfBirth = @DateOfBirth, 
        BankBalance = @BankBalance,
        IsAlive = @IsAlive,
        CharacterPicture = @CharacterPicture,
        Address = @Address,
        GenresId = @GenresId
    WHERE AnimeCharacterId = @AnimeCharacterId

    -- Delete existing AnimeNames for this AnimeCharacter
    DELETE FROM AnimeName WHERE AnimeCharacterId = @AnimeCharacterId

    -- Insert new AnimeNames
    INSERT INTO AnimeName (AnimeCharacterId, AnimationName, TotalEp, OnGoing)
    SELECT @AnimeCharacterId, AnimationName, TotalEp, OnGoing FROM @AnimeNameType
END
GO
CREATE PROCEDURE sp_DeleteAnimeCharacter
    @AnimeCharacterId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete AnimeNames related to this AnimeCharacter
    DELETE FROM AnimeName WHERE AnimeCharacterId = @AnimeCharacterId

    -- Delete the AnimeCharacter
    DELETE FROM AnimeCharacter WHERE AnimeCharacterId = @AnimeCharacterId
END
GO
CREATE PROCEDURE sp_GetAllAnimeCharacters
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM AnimeCharacter
END
GO
CREATE PROCEDURE sp_GetAnimeCharacterById
    @AnimeCharacterId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Select AnimeCharacter
    SELECT * FROM AnimeCharacter WHERE AnimeCharacterId = @AnimeCharacterId;

    -- Select associated AnimeNames
    SELECT * FROM AnimeName WHERE AnimeCharacterId = @AnimeCharacterId;
END
GO
CREATE PROCEDURE sp_GetAnimeNamesByAnimeCharacterId
    @AnimeCharacterId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM AnimeName WHERE AnimeCharacterId = @AnimeCharacterId;
END
GO

CREATE PROCEDURE sp_InsertAnimeCharacter
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

    INSERT INTO AnimeCharacters (AnimeCharacterName, DateOfBirth, BankBalance, IsAlive, CharacterPicture, Address, GenresId)
    VALUES (@AnimeCharacterName, @DateOfBirth, @BankBalance, @IsAlive, @CharacterPicture, @Address, @GenresId);

    SET @NewAnimeCharacterId = SCOPE_IDENTITY();
END
GO
CREATE PROCEDURE sp_InsertAnimeName
    @AnimationName NVARCHAR(50),
    @TotalEp INT,
    @OnGoing BIT,
    @AnimeCharacterId INT
AS
BEGIN
    INSERT INTO AnimeNames (AnimationName, TotalEp, OnGoing, AnimeCharacterId)
    VALUES (@AnimationName, @TotalEp, @OnGoing, @AnimeCharacterId);
END
