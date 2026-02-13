using EduShelf.Api.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IQuizService
    {
        Task<PagedResult<QuizDto>> GetQuizzesAsync(int page, int pageSize);
        Task<QuizDto> GetQuizAsync(int id);
        Task<QuizDto> CreateQuizAsync(QuizCreateDto quizDto);
        Task<QuizDto> UpdateQuizAsync(int id, QuizUpdateDto quizUpdateDto);
        Task PatchQuizAsync(int id, QuizUpdateDto quizUpdateDto);
        Task DeleteQuizAsync(int id);
        Task<QuizDto> GenerateQuizAsync(GenerateQuizRequest request, int userId);
    }
}
