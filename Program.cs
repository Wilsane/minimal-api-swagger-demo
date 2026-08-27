using IntegratingWithSwagger.Endpoints;
using IntegratingWithSwagger.Middleware;
using IntegratingWithSwagger.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton<IBlogStore, InMemoryBlogStore>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// Reads are public; writes need the API key.
app.UseWhen(
    context => HttpMethods.IsPost(context.Request.Method)
               || HttpMethods.IsPut(context.Request.Method)
               || HttpMethods.IsDelete(context.Request.Method),
    branch => branch.UseMiddleware<ApiKeyAuthenticationMiddleware>());

app.MapGet("/", () => Results.Ok(new
    {
        message = "Hello World! I am root.",
        api = "Blogs API",
        docs = "/swagger"
    }))
    .ExcludeFromDescription();

app.MapBlogEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/throw", void () => throw new InvalidOperationException("Deliberate test exception."))
        .ExcludeFromDescription();
}

app.Run();

// Lets WebApplicationFactory<Program> reach this class from a test project.
public partial class Program;
