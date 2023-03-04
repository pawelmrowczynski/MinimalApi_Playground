using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Minimal.Api;
using Minimal.Api.Data;
using Minimal.Api.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>  new SqliteConnectionFactory("Data source=./library.db"));
builder.Services.AddSingleton<BookRepository>();
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Use swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/books", (string? searchTerm, BookRepository bookRepo) =>
{
    if (searchTerm is null)
    {
        return Results.Ok(bookRepo.GetBooks());
    }
    var results = bookRepo.Search(searchTerm);
    return Results.Ok(results);
});

//app.MapGet("books", (BookRepository bookRepo) => Results.Ok(bookRepo.GetBooks()));
app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator)  =>
{
    var validationResult = await validator.ValidateAsync(book);

    if (validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }
    
    var created = await bookService.CreateAsync(book);
    if (!created)
    {
        return Results.BadRequest(new List<ValidationFailure>
        {
            new ValidationFailure("Isbn", "A book with this ISBN already exists")
        });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapPatch("books/{bookId}", (string bookId, UpdateBookRequest updateRequest, BookRepository bookRepository) =>
{
    var book = bookRepository.GetByIsbn(bookId);
    book.ReleaseDate = updateRequest.ReleaseDate;
    book.Author = updateRequest.Author;
    book.Title = updateRequest.Title;
    book.PageCount = updateRequest.PageCount;

    return Results.Ok(book);
});

app.MapDelete("books/{bookId}", (string bookId, BookRepository bookRepository) =>
{
    bookRepository.DeleteBook(bookId);
    return Results.Ok();
});


var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();