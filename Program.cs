using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var blogs = new List<Blog>
{
    new Blog { Id = 1, Title = "First Blog", Content = "This is the first blog post." },
    new Blog { Id = 2, Title = "Second Blog", Content = "This is the second blog post." }
};

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context,next) =>
{
    var startTime = DateTime.UtcNow;
    await next.Invoke();
    var endTime = DateTime.UtcNow;
    var duration = endTime - startTime;
    Console.WriteLine($"Request duration: {duration.TotalMilliseconds} ms");
});

/*
app.UseWhen(context => context.Request.Method != "GET", appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        Console.WriteLine($"Non-GET request: {context.Request.Method} {context.Request.Path}");

        var extractedPassword = context.Request.Headers["X-Api-Key"];
        if (extractedPassword == "thisIsAVeryBadPassword")
            await next.Invoke();
        else
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized. Invalid API Key.");
        }
    });
});
*/

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});


app.MapGet("/", () => "Hello World!\nI am root");

app.MapGet("/blogs", () =>
{
    return TypedResults.Ok(blogs);
});


app.MapGet("/blogs/{id:int}", Results<Ok<Blog>, NotFound> (int id) =>
{
    var blog = blogs.FirstOrDefault(b => b.Id == id);
    if (blog == null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(blog);
})
.WithDescription("Get a blog by its ID")
.WithSummary("This endpoint retrieves a blog post by its unique identifier. If the blog post does not exist, it returns a 404 Not Found response.");

app.MapPost("/blogs", (Blog newBlog) =>
{
    newBlog.Id = blogs.Max(b => b.Id) + 1;
    blogs.Add(newBlog);
    return TypedResults.Created($"/blogs/{newBlog.Id}", newBlog);
});


app.MapPut("/blogs/{id:int}",Results<Ok<Blog>, NotFound> (int id, Blog updatedBlog) =>
{
    var blog = blogs.FirstOrDefault(b => b.Id == id);
    if (blog == null)
    {
        return TypedResults.NotFound();
    }
    blog.Title = updatedBlog.Title;
    blog.Content = updatedBlog.Content;
    return TypedResults.Ok(blog);
});


app.MapDelete("/blogs/{id:int}", Results<NoContent, NotFound> (int id) =>
{
    var blog = blogs.FirstOrDefault(b => b.Id == id);
    if (blog == null)
    {
        return TypedResults.NotFound();
    }
    blogs.Remove(blog);
    return TypedResults.NoContent();
});

app.Run();

public class Blog
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}
