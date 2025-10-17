using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using System.IO;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EduShelf.Api.Services
{
    public class ChatService
    {
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;
        private readonly ILogger<ChatService> _logger;
        private readonly IntentDetectionService _intentDetectionService;
        private readonly RetrievalService _retrievalService;
        private readonly PromptGenerationService _promptGenerationService;

        public ChatService(
            ApiDbContext context,
            Kernel kernel,
            ILogger<ChatService> logger,
            IntentDetectionService intentDetectionService,
            RetrievalService retrievalService,
            PromptGenerationService promptGenerationService)
        {
            _context = context;
            _kernel = kernel;
            _logger = logger;
            _intentDetectionService = intentDetectionService;
            _retrievalService = retrievalService;
            _promptGenerationService = promptGenerationService;
        }

        public async Task<string> GetResponseAsync(string userInput, int userId, int chatSessionId)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                throw new BadRequestException("User input cannot be empty.");
            }

            if (userInput.Length > 2000)
            {
                throw new BadRequestException("User input cannot exceed 2000 characters.");
            }

            try
            {
                var chatSession = await _context.ChatSessions
                    .Include(cs => cs.ChatMessages)
                    .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

                if (chatSession == null)
                {
                    throw new Exception("Chat session not found.");
                }

                var intent = await _intentDetectionService.GetIntentAsync(userInput);
                var relevantChunks = await _retrievalService.GetRelevantChunksAsync(userInput, userId, intent);
                var chatHistory = _promptGenerationService.BuildChatHistory(chatSession, relevantChunks, userInput);

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                var responseContent = result.Content ?? "I'm sorry, I couldn't generate a response.";

                var chatMessage = new ChatMessage
                {
                    ChatSessionId = chatSessionId,
                    Message = userInput,
                    Response = responseContent,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response from ChatService.");
                return "An error occurred while processing your request.";
            }
        }

        public async Task<ChatSession> CreateChatSessionAsync(int userId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new BadRequestException("Title cannot be empty.");
            }

            if (title.Length > 100)
            {
                throw new BadRequestException("Title cannot exceed 100 characters.");
            }

            var chatSession = new ChatSession
            {
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync();

            await _context.Entry(chatSession).Reference(cs => cs.User).LoadAsync();

            return chatSession;
        }

        public async Task<List<ChatSession>> GetChatSessionsAsync(int userId)
        {
            return await _context.ChatSessions
                .Include(cs => cs.User)
                .Where(cs => cs.UserId == userId)
                .OrderByDescending(cs => cs.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesForSessionAsync(int userId, int chatSessionId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

            if (session == null)
            {
                throw new Exception("Chat session not found.");
            }

            return await _context.ChatMessages
                .Where(cm => cm.ChatSessionId == chatSessionId)
                .OrderBy(cm => cm.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> GetMessageAsync(int userId, int chatSessionId, int messageId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

            if (session == null)
            {
                throw new Exception("Chat session not found.");
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == messageId && cm.ChatSessionId == chatSessionId);

            if (message == null)
            {
                throw new Exception("Message not found.");
            }

            return message;
        }

    }
}