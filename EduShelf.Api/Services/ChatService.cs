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
using EduShelf.Api.Models.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EduShelf.Api.Services
{
    public class ChatService
    {
        private class Intent
        {
            public string Type { get; set; }
            public string DocumentName { get; set; }
        }
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;
        private readonly ILogger<ChatService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IndexingService _indexingService;

        public ChatService(ApiDbContext context, Kernel kernel, ILogger<ChatService> logger, IConfiguration configuration, IndexingService indexingService)
        {
            _context = context;
            _kernel = kernel;
            _logger = logger;
            _configuration = configuration;
            _indexingService = indexingService;
        }

        public async Task<string> GetResponseAsync(string userInput, int userId, int chatSessionId)
        {
            try
            {
                var chatSession = await _context.ChatSessions
                    .Include(cs => cs.ChatMessages)
                    .FirstOrDefaultAsync(cs => cs.Id == chatSessionId && cs.UserId == userId);

                if (chatSession == null)
                {
                    throw new Exception("Chat session not found.");
                }

                var contextText = new StringBuilder();
                List<DocumentChunk> relevantChunks;

                var intent = await GetIntentAsync(userInput);

                if (intent.Type == "summarize" && !string.IsNullOrEmpty(intent.DocumentName))
                {
                    var documentNameWithoutExtension = Path.GetFileNameWithoutExtension(intent.DocumentName);
                    relevantChunks = await _context.DocumentChunks
                        .Include(dc => dc.Document)
                        .Where(dc => dc.Document.UserId == userId && EF.Functions.ILike(dc.Document.Title, documentNameWithoutExtension))
                        .ToListAsync();
                }
                else
                {
                    var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                    var promptEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(userInput);
                    var k = GetDynamicK(userInput);
                    relevantChunks = await _context.DocumentChunks
                        .Include(dc => dc.Document)
                        .Where(dc => dc.Document.UserId == userId)
                        .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding)))
                        .Take(k)
                        .ToListAsync();
                }

                if (relevantChunks.Any())
                {
                    foreach (var chunk in relevantChunks)
                    {
                        contextText.AppendLine($"[{chunk.Document.Title}] {chunk.Content}");
                    }
                }

                var contextString = contextText.ToString();
                var contextLengthLimit = _configuration.GetValue<int>("AIService:ContextLengthLimit", 4096);
                if (contextString.Length > contextLengthLimit)
                {
                    contextString = TruncateToTokenLimit(contextString, contextLengthLimit);
                }

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(_configuration.GetValue<string>("AIService:Prompts:System"));

                foreach (var message in chatSession.ChatMessages.OrderBy(m => m.CreatedAt))
                {
                    chatHistory.AddUserMessage(message.Message);
                    if (!string.IsNullOrEmpty(message.Response))
                    {
                        chatHistory.AddAssistantMessage(message.Response);
                    }
                }

                chatHistory.AddSystemMessage($"Context:\n{contextString}");
                chatHistory.AddUserMessage(userInput);

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

        private async Task<Intent> GetIntentAsync(string userInput)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(_configuration.GetValue<string>("AIService:Prompts:Intent"));
            chatHistory.AddUserMessage(userInput);

            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
            var rawContent = result.Content ?? string.Empty;
            _logger.LogInformation("Intent detection raw response: {IntentResponse}", rawContent);

            var cleanedJson = rawContent.Trim();
            if (cleanedJson.StartsWith("```json"))
            {
                cleanedJson = cleanedJson.Substring(7);
            }
            if (cleanedJson.StartsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(3);
            }
            if (cleanedJson.EndsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(0, cleanedJson.Length - 3);
            }
            cleanedJson = cleanedJson.Trim();

            try
            {
                var intent = JsonSerializer.Deserialize<Intent>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Intent { Type = "question", DocumentName = null };
                _logger.LogInformation("Parsed intent: Type={IntentType}, DocumentName={DocumentName}", intent.Type, intent.DocumentName);
                return intent;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse intent JSON: {JsonContent}", result.Content);
                return new Intent { Type = "question", DocumentName = null };
            }
        }

        private int GetDynamicK(string userInput)
        {
            var wordCount = userInput.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount < 10) return 5;
            if (wordCount <= 30) return 10;
            return 20;
        }

        private string TruncateToTokenLimit(string text, int tokenLimit)
        {
            if (text.Length <= tokenLimit) return text;

            var sentences = text.Split(new[] { ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries);
            var truncatedText = new StringBuilder();
            var currentLength = 0;

            foreach (var sentence in sentences)
            {
                if (currentLength + sentence.Length + 2 > tokenLimit)
                {
                    break;
                }
                truncatedText.Append(sentence).Append(". ");
                currentLength += sentence.Length + 2;
            }

            return truncatedText.ToString().TrimEnd() + "...";
        }
    }
}