using IntegratingWithSwagger.Models;

namespace IntegratingWithSwagger.Services;

// Registered as a singleton, so every request shares one instance and all
// access has to be locked.
public sealed class InMemoryBlogStore : IBlogStore
{
    private readonly Lock _gate = new();
    private readonly List<Blog> _blogs;
    private int _nextId;

    public InMemoryBlogStore()
    {
        var seededAt = DateTime.UtcNow;

        _blogs =
        [
            new Blog { Id = 1, Title = "First Blog", Content = "This is the first blog post.", CreatedAtUtc = seededAt },
            new Blog { Id = 2, Title = "Second Blog", Content = "This is the second blog post.", CreatedAtUtc = seededAt }
        ];

        _nextId = 3;
    }

    public IReadOnlyList<Blog> GetAll()
    {
        lock (_gate)
        {
            // Snapshot: the serializer must not enumerate the live list.
            return _blogs.ToArray();
        }
    }

    public Blog? GetById(int id)
    {
        lock (_gate)
        {
            return _blogs.FirstOrDefault(blog => blog.Id == id);
        }
    }

    public Blog Add(string title, string content)
    {
        lock (_gate)
        {
            var blog = new Blog
            {
                Id = _nextId++,
                Title = title.Trim(),
                Content = content.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _blogs.Add(blog);
            return blog;
        }
    }

    public Blog? Update(int id, string title, string content)
    {
        lock (_gate)
        {
            var blog = _blogs.FirstOrDefault(existing => existing.Id == id);

            if (blog is null)
            {
                return null;
            }

            blog.Title = title.Trim();
            blog.Content = content.Trim();
            blog.UpdatedAtUtc = DateTime.UtcNow;
            return blog;
        }
    }

    public bool Delete(int id)
    {
        lock (_gate)
        {
            var blog = _blogs.FirstOrDefault(existing => existing.Id == id);
            return blog is not null && _blogs.Remove(blog);
        }
    }
}
