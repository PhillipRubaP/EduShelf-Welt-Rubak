using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EduShelf.Api.Services
{
    public class ImageProcessingService
    {
        private readonly Kernel _kernel;

        public ImageProcessingService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> ProcessImageAsync(Stream imageStream, string prompt)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            // Convert the image stream to a byte array
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await imageStream.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // Create the chat history with the prompt and the image
            var chatHistory = new ChatHistory();
            var promptMessage = new ChatMessageContentItemCollection
            {
                new TextContent(prompt),
                new ImageContent(imageBytes, "image/jpeg") // Assuming JPEG, adjust if needed
            };
            chatHistory.AddUserMessage(promptMessage);

            // Get the response from the multimodal model
            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

            return result.Content;
        }
    }
}