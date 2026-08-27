using IntegratingWithSwagger.Middleware;
using IntegratingWithSwagger.Models;
using IntegratingWithSwagger.Services;
using IntegratingWithSwagger.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IntegratingWithSwagger.Endpoints;

public static class BlogEndpoints
{
    public static RouteGroupBuilder MapBlogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/blogs")
            .WithTags("Blogs");

        group.MapGet("/", (IBlogStore store) => TypedResults.Ok(store.GetAll()))
            .WithName("GetBlogs")
            .WithSummary("List all blogs")
            .WithDescription("Returns every blog post. No API key required.");

        group.MapGet("/{id:int}", Results<Ok<Blog>, NotFound> (int id, IBlogStore store) =>
            {
                var blog = store.GetById(id);
                return blog is null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(blog);
            })
            .WithName("GetBlogById")
            .WithSummary("Get a blog by id")
            .WithDescription("Returns 404 if no post with that id exists. No API key required.");

        group.MapPost("/", (CreateBlogRequest request, IBlogStore store) =>
            {
                var blog = store.Add(request.Title, request.Content);
                return TypedResults.Created($"/blogs/{blog.Id}", blog);
            })
            .AddEndpointFilter<ValidationFilter<CreateBlogRequest>>()
            .WithName("CreateBlog")
            .WithSummary("Create a blog")
            .WithDescription(
                $"The id and timestamps are assigned by the server. Requires an '{ApiKeyAuthenticationMiddleware.HeaderName}' header.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:int}", Results<Ok<Blog>, NotFound> (int id, UpdateBlogRequest request, IBlogStore store) =>
            {
                var blog = store.Update(id, request.Title, request.Content);
                return blog is null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(blog);
            })
            .AddEndpointFilter<ValidationFilter<UpdateBlogRequest>>()
            .WithName("UpdateBlog")
            .WithSummary("Replace a blog")
            .WithDescription(
                $"A full replacement, so title and content are both required. Requires an '{ApiKeyAuthenticationMiddleware.HeaderName}' header.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:int}", Results<NoContent, NotFound> (int id, IBlogStore store) =>
                store.Delete(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound())
            .WithName("DeleteBlog")
            .WithSummary("Delete a blog")
            .WithDescription(
                $"Returns 204 on success, 404 if it did not exist. Requires an '{ApiKeyAuthenticationMiddleware.HeaderName}' header.")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }
}
