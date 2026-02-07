using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IFlashcardService
    {
        Task<IEnumerable<FlashcardDto>> GetFlashcardsAsync(int userId, bool isAdmin);
        Task<FlashcardDto> GetFlashcardAsync(int id, int userId, bool isAdmin);
        Task<IEnumerable<FlashcardDto>> GetFlashcardsByTagAsync(int tagId, int userId, bool isAdmin);
        Task<FlashcardDto> CreateFlashcardAsync(FlashcardCreateDto flashcardDto, int userId);
        Task<FlashcardDto> UpdateFlashcardAsync(int id, FlashcardCreateDto flashcardDto, int userId, bool isAdmin);
        Task PatchFlashcardAsync(int id, FlashcardUpdateDto flashcardUpdate, int userId, bool isAdmin);
        Task DeleteFlashcardAsync(int id, int userId, bool isAdmin);
        Task<List<FlashcardDto>> GenerateFlashcardsAsync(GenerateFlashcardsRequest request, int userId);
    }
}
