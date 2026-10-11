USE BooksDB;
GO

DECLARE @DeletedRows INT;

EXEC dbo.AddBook
    @Title = N'Книга для видалення',
    @Text = N'Тестовий запис',
    @ImageData = NULL;

DECLARE @Id INT = (
    SELECT MAX(Id)
    FROM dbo.Books
    WHERE Title = N'Книга для видалення'
);

EXEC dbo.DeleteBookById
    @Id = @Id,
    @DeletedRows = @DeletedRows OUTPUT;

SELECT @DeletedRows AS DeletedRows;