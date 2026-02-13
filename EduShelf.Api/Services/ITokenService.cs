namespace EduShelf.Api.Services
{
    public interface ITokenService
    {
        int CountTokens(string text);
        string Truncate(string text, int maxTokens);
    }
}
