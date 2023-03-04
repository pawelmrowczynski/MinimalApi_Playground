using Dapper;
using Minimal.Api.Data;

namespace Minimal.Api.Services;

class BookService : IBookService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Book book)
    {
        // var existingBook = await GetByIsbnAsync(book.Isbn);
        //
        // if (existingBook is not null)
        // {
        //     return false;
        // }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            $@"insert into Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
                values (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)",
                book);
        return result == 1;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var book = await connection.QuerySingleOrDefaultAsync<Book>(@"Select * FROM Books WHERE Isbn = @isbn", new {Isbn = isbn});
        return book;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        return await connection.QueryAsync<Book>(@"Select * FROM Books");
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var books =  await connection.QueryAsync<Book>("Select * FROM Books WHERE Title LIKE '%' || @searchTerm || '%'", new { searchTerm});
        return books;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var existingBook = await this.GetByIsbnAsync(book.Isbn);
        if (existingBook is null)
        {
            return false;
        }
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"Update Books set Title = @Title, Author = @Author, PageCount = @PageCount WHERE Isbn = @Isbn", book);
        return result == 1;
    }

    public Task<bool> DeleteAsync(string isbn)
    {
        throw new NotImplementedException();
    }
}