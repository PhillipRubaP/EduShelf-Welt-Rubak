namespace EduShelf.Api.Services
{
    public interface IRAGService
    {
        Task<string> GetResponseAsync(string query, int userId);
    }
}