using EduShelf.Api.Models.Entities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace EduShelf.Api.Services
{
    public class IntentDetectionService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<IntentDetectionService> _logger;
        private readonly IConfiguration _configuration;

        public IntentDetectionService(Kernel kernel, ILogger<IntentDetectionService> logger, IConfiguration configuration)
        {
            _kernel = kernel;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Intent> GetIntentAsync(string userInput)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(_configuration.GetValue<string>("AIService:Prompts:Intent") ?? "Identify the intent.");
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
                if (intent.DocumentName != null) {
                   _logger.LogInformation("DocumentName raw value: '{DocumentNameRaw}'", intent.DocumentName);
                }
                return intent;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse intent JSON: {JsonContent}", result.Content);
                return new Intent { Type = "question", DocumentName = null };
            }
        }
    }
}