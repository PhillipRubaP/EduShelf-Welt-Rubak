using EduShelf.Api.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IQuizService
    {
        Task<IEnumerable<QuizDto>> GetQuizzesAsync();
        Task<QuizDto> GetQuizAsync(int id);
        Task<QuizDto> CreateQuizAsync(QuizCreateDto quizDto);
        Task<QuizDto> UpdateQuizAsync(int id, QuizUpdateDto quizUpdateDto);
        Task PatchQuizAsync(int id, QuizUpdateDto quizUpdateDto);
        Task DeleteQuizAsync(int id);
    }
}
