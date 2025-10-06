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

        public async Task<string> GetResponseAsync(string userInput, int userId)
        {
            try
            {
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

                    // If no chunks are found, try to re-index the document
                    if (!relevantChunks.Any())
                    {
                        _logger.LogWarning("No chunks found for document '{DocumentName}'. Attempting to re-index.", documentNameWithoutExtension);
                        var documentToIndex = await _context.Documents
                            .FirstOrDefaultAsync(d => d.UserId == userId && EF.Functions.ILike(d.Title, documentNameWithoutExtension));

                        if (documentToIndex != null)
                        {
                            await _indexingService.IndexDocumentAsync(documentToIndex.Id, documentToIndex.Path);
                            // Re-query for the chunks after indexing
                            relevantChunks = await _context.DocumentChunks
                                .Include(dc => dc.Document)
                                .Where(dc => dc.Document.UserId == userId && EF.Functions.ILike(dc.Document.Title, documentNameWithoutExtension))
                                .ToListAsync();
                            _logger.LogInformation("Re-indexing complete. Found {ChunkCount} chunks.", relevantChunks.Count);
                        }
                        else
                        {
                            _logger.LogError("Could not find document '{DocumentName}' to re-index.", documentNameWithoutExtension);
                        }
                    }
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

                if (!relevantChunks.Any())
                {
                    contextText.AppendLine("No relevant documents found.");
                }
                else
                {
                    foreach (var chunk in relevantChunks)
                    {
                        // Titel hinzufügen für besseren Kontext
                        contextText.AppendLine($"[{chunk.Document.Title}] {chunk.Content}");
                    }
                }

                // Promptgröße begrenzen (z. B. 4000 Zeichen)
                var contextString = contextText.ToString();
                var contextLengthLimit = _configuration.GetValue<int>("AIService:ContextLengthLimit", 4096);

                if (contextString.Length > contextLengthLimit)
                {
                    contextString = TruncateToTokenLimit(contextString, contextLengthLimit);
                }

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                // ChatHistory statt einfachem Prompt
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage("""
                    You are a learning assistant on the EduShelf platform. 
                    Your primary role is to help users study and learn from the documents they upload. 
                    - Always prioritize using the content of the provided documents. 
                    - If the answer is not in the documents, clearly say: "This is not in your documents," and then you may provide general knowledge if it is accurate and helpful. 
                    - When you mix document-based information and general knowledge, always separate them clearly. 
                    - Be neutral, concise, and explanatory — never inject your own opinions or judge the document content. 
                    - If asked to summarize or explain, always mention which document the content came from when possible.

                """);

                chatHistory.AddSystemMessage($"Context:\n{contextString}");
                chatHistory.AddUserMessage(userInput);

                var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

                return result.Content ?? "I'm sorry, I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response from ChatService.");
                return "An error occurred while processing your request.";
            }
        }

        private async Task<Intent> GetIntentAsync(string userInput)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("""
                You are an intent detection agent. Your job is to analyze the user's prompt and determine their intent and the document they are referring to.
                - The possible intents are: "summarize", "question".
                - If the user wants a general overview, a summary, or to know what a document is about, the intent is "summarize".
                - If the user is asking a specific question that can be answered from a small part of the document, the intent is "question".
                - Your output must be a single, valid JSON object with two properties: "Type" (string) and "DocumentName" (string or null).
                - Extract the filename, including its extension. Look for names in quotes, or phrases like "file named X", "document Y".
                - If no specific document is mentioned, "DocumentName" must be null.

                Examples:
                - User: "Summarize the file 'my_document.pdf'" -> {"Type": "summarize", "DocumentName": "my_document.pdf"}
                - User: "What is the capital of France according to the text?" -> {"Type": "question", "DocumentName": null}
                - User: "Tell me everything about 'chapter1.docx'" -> {"Type": "summarize", "DocumentName": "chapter1.docx"}
                - User: "Read the file named czech.txt and summarize its contents" -> {"Type": "summarize", "DocumentName": "czech.txt"}
                - User: "where is the name Czechia first mentioned" -> {"Type": "question", "DocumentName": null}
            """);
            chatHistory.AddUserMessage(userInput);

            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
            var rawContent = result.Content ?? string.Empty;
            _logger.LogInformation("Intent detection raw response: {IntentResponse}", rawContent);

            // Clean the response to remove markdown code blocks
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
                // If JSON parsing fails, default to a general question
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
            // Simple heuristic: 1 token ~= 4 characters
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