namespace EduShelf.Api.Services
{
    public interface IRAGService
    {
        Task<(string, List<string>)> GetContextAndSourcesAsync(string query, int userId);
    }
}