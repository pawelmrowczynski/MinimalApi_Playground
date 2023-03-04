using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Minimal.Api;
using Minimal.Api.Auth;
using Minimal.Api.Data;
using Minimal.Api.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName).AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(
    ApiKeySchemeConstants.SchemeName,
    _ => { });
builder.Services.AddAuthorization();



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

app.UseAuthorization();

app.MapGet("/books",
            [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
    (string? searchTerm, IBookService bookService) =>
{
    if (searchTerm is null && string.IsNullOrWhiteSpace(searchTerm))
    {
        return Results.Ok(bookService.GetAllAsync());
    }
    var results = bookService.SearchByTitleAsync(searchTerm);
    return Results.Ok(results);
});

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
{
    var book = await bookService.GetByIsbnAsync(isbn);

    return book is not null ? Results.Ok(book) : Results.NotFound();
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

app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator)  =>
{
    book.Isbn = isbn;
    var validationResult = await validator.ValidateAsync(book);

    if (validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    var updated = await bookService.UpdateAsync(book);

    return updated ? Results.Ok(book) : Results.NotFound();
});




app.MapDelete("books/{bookId}", (string bookId, BookRepository bookRepository) =>
{
    bookRepository.DeleteBook(bookId);
    return Results.Ok();
});


var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();