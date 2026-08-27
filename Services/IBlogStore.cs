using IntegratingWithSwagger.Models;

namespace IntegratingWithSwagger.Services;

public interface IBlogStore
{
    IReadOnlyList<Blog> GetAll();

    Blog? GetById(int id);

    Blog Add(string title, string content);

    Blog? Update(int id, string title, string content);

    bool Delete(int id);
}
