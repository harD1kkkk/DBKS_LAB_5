USE BooksDB;
GO

DECLARE @TestTitle NVARCHAR(100) = N'Тестова книга для процедур';
DECLARE @TestId INT;
DECLARE @Exists BIT;
DECLARE @Title NVARCHAR(100);
DECLARE @DeletedRows INT;

EXEC dbo.AddBook
    @Title = @TestTitle,
    @Text = N'Перевірка процедури додавання запису.',
    @ImageData = NULL;

SELECT TOP (1) @TestId = Id
FROM dbo.Books
WHERE Title = @TestTitle
ORDER BY Id DESC;

SELECT Id, Title, [Text]
FROM dbo.Books
WHERE Id = @TestId;

EXEC dbo.CheckBookById
    @Id = @TestId,
    @Exists = @Exists OUTPUT,
    @Title = @Title OUTPUT;

SELECT @TestId AS TestId, @Exists AS Found, @Title AS BookTitle;

EXEC dbo.DeleteBookById
    @Id = @TestId,
    @DeletedRows = @DeletedRows OUTPUT;

SELECT @DeletedRows AS DeletedRows;

EXEC dbo.CheckBookById
    @Id = 999999,
    @Exists = @Exists OUTPUT,
    @Title = @Title OUTPUT;

SELECT 999999 AS TestId, @Exists AS Found, @Title AS BookTitle;
GO
