USE BooksDB;
GO

DECLARE @Exists BIT;
DECLARE @Title NVARCHAR(100);

EXEC dbo.CheckBookById
    @Id = 1,
    @Exists = @Exists OUTPUT,
    @Title = @Title OUTPUT;

SELECT @Exists AS Found, @Title AS BookTitle;