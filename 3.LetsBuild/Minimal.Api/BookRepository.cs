namespace Minimal.Api;

public class BookRepository
{
    public List<Book> Books { get; set; }

    public BookRepository()
    {
        this.Books = new List<Book>
        {
            new Book
            {
                Isbn = "978-0137081073",
                Title = "The Clean Coder",
                Author = "Robert C. Martin",
                ShortDescription =
                    "In The Clean Coder: A Code of Conduct for Professional Programmers, legendary software expert Robert C. Martin introduces the disciplines, techniques, tools, and practices of true software craftsmanship",
                PageCount = 242,
                ReleaseDate = new DateTime(2011, 11, 11)
            }
        };
    }
    
    public Book GetByIsbn(string isbn)
    {
        return this.Books.Single(x => x.Isbn == isbn);
    }

    public IEnumerable<Book> GetBooks()
    {
        return this.Books;
    }

    public void AddBook(Book book)
    {
        this.Books.Add(book);
    }

    public IEnumerable<Book> Search(string query)
    {
        return this.Books.Where(x => x.ShortDescription.Contains(query) || x.Author.Contains(query));
    }

    public void DeleteBook(string bookId)
    {
        this.Books.RemoveAll(x => x.Isbn == bookId);
    }
}