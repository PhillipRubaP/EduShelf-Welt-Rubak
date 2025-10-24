using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace EduShelf.Api.Services
{
    public class ChatService
    {
        private readonly ApiDbContext _context;
        private readonly IRAGService _ragService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            ApiDbContext context,
            IRAGService ragService,
            ILogger<ChatService> logger)
        {
            _context = context;
            _ragService = ragService;
            _logger = logger;
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
                    .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

                if (chatSession == null)
                {
                    throw new NotFoundException("Chat session not found.");
                }

                var responseContent = await _ragService.GetResponseAsync(userInput, userId);

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
                throw new NotFoundException("Chat session not found.");
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
                throw new NotFoundException("Chat session not found.");
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == messageId && cm.ChatSessionId == chatSessionId);

            if (message == null)
            {
                throw new NotFoundException("Message not found.");
            }

            return message;
        }

        public async Task DeleteChatSessionAsync(int userId, int chatSessionId)
        {
            var chatSession = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

            if (chatSession == null)
            {
                throw new NotFoundException("Chat session not found.");
            }

            _context.ChatSessions.Remove(chatSession);
            await _context.SaveChangesAsync();
        }

        public async Task<ChatSession> UpdateChatSessionAsync(int userId, int sessionId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new BadRequestException("Title cannot be empty.");
            }

            if (title.Length > 100)
            {
                throw new BadRequestException("Title cannot exceed 100 characters.");
            }

            var chatSession = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == sessionId && cs.UserId == userId);

            if (chatSession == null)
            {
                throw new NotFoundException("Chat session not found.");
            }

            chatSession.Title = title;
            await _context.SaveChangesAsync();

            return chatSession;
        }
    }
}