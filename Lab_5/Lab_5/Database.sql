IF DB_ID(N'BooksDB') IS NULL
BEGIN
    CREATE DATABASE BooksDB;
END
GO

USE BooksDB;
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        Id INT IDENTITY(1,1) CONSTRAINT PK_Books PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        [Text] NVARCHAR(MAX) NOT NULL,
        ImageData VARBINARY(MAX) NULL
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.AddBook
    @Title NVARCHAR(100),
    @Text NVARCHAR(MAX),
    @ImageData VARBINARY(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Books (Title, [Text], ImageData)
    VALUES (@Title, @Text, @ImageData);
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteBookById
    @Id INT,
    @DeletedRows INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Books
    WHERE Id = @Id;

    SET @DeletedRows = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE dbo.CheckBookById
    @Id INT,
    @Exists BIT OUTPUT,
    @Title NVARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Books WHERE Id = @Id)
    BEGIN
        SET @Exists = 1;

        SELECT @Title = Title
        FROM dbo.Books
        WHERE Id = @Id;
    END
    ELSE
    BEGIN
        SET @Exists = 0;
        SET @Title = NULL;
    END
END
GO
