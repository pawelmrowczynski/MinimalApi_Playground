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

    public Task<Book?> GetByIsbnAsync(string isbn)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Book book)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string isbn)
    {
        throw new NotImplementedException();
    }
}