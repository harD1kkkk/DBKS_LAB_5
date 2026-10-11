USE BooksDB;
GO

EXEC dbo.AddBook
    @Title = N'Тестова книга',
    @Text = N'Перевірка процедури додавання запису.',
    @ImageData = NULL;
GO

SELECT TOP 1 Id, Title, [Text]
FROM dbo.Books
ORDER BY Id DESC;