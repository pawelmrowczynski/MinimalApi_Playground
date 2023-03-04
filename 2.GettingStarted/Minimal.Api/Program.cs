using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Minimal.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PeopleService>();
builder.Services.AddSingleton<GuidGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware registration starts here
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("ok-result", () => Results.Ok(new
{
    Name = "Pawel M"
}));

app.MapGet("slow request", async () =>
{
    await Task.Delay(1000);
    return Results.Ok(new
    {
        Name = "Pawel M"
    });
});

app.MapPost("post", () => "This is a post");

app.MapMethods("all-the-methods", new[] { "PATCH" }, () => "Patch");

var handler = () => "handler";

app.MapGet("var", handler);


app.MapGet("get-params/{age:int}", (int age) => $"age provded was {age}");
app.MapGet("get-params2/{age:minlength(4)}", (string age) => $"age provded was {age}");
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0#route-constraints 


app.MapGet("people/search", (string? searchTerm, PeopleService peopleService) =>
{
    if (searchTerm is null)
    {
        return Results.NotFound();
    }

    var results = peopleService.Search(searchTerm);
    return Results.Ok(results);
});

app.MapGet("mix/{routeParam}", (
    [FromRoute] string routeParam, 
    [FromQuery(Name = "test")]int queryParam, 
    [FromServices]GuidGenerator guidGenerator) =>
{
    return Results.Ok($"{routeParam}, {queryParam}, {guidGenerator.NewGuid}");
});


app.MapPost("people", (Person person) =>
{
    return Results.Ok(person);
});

app.MapGet("httpcontext-1", async (context) =>
{
    await context.Response.WriteAsync("Hello from context 1");
});

app.MapGet("http", async (HttpRequest request, HttpResponse response) =>
{
    var queries = request.QueryString.Value;
    await response.WriteAsync($"Hello from HTTP Response. Queries were {queries}");
});


app.MapGet("claims", (ClaimsPrincipal user) =>
{
    return Results.Ok();
});


app.MapGet("map-point", (MapPoint point) =>
{

});


//Response types
app.MapGet("simple-string", () => "Hello");
app.MapGet("raw-json-obj", () => new { message = "hello"});
app.MapGet("ok-obj", () => Results.Ok("Hello"));
app.MapGet("ok-json-obj", () => Results.Ok(new { message = "hello"}));
app.MapGet("text-string", () => Results.Text("Hello"));

app.MapGet("stream-result", () =>
{
    var memoryStream = new MemoryStream();
    var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
    streamWriter.Write("Hello");
    streamWriter.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    return Results.Stream(memoryStream);

});

//Logging

app.MapGet("logging", (ILogger<Program> log) =>
{
    log.LogDebug("test");
    Results.Ok("ble");
});



// Middleware registration stops here
app.Run();