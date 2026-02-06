using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace EduShelf.Api.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _visionModel;

        public ImageProcessingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _visionModel = configuration["AIService:VisionModel"] ?? "qwen3-vl:8b";
        }

        public async Task<string> ProcessImageAsync(byte[] imageData, string prompt)
        {
            var base64Image = Convert.ToBase64String(imageData);

            var requestBody = new
            {
                model = _visionModel,
                prompt = prompt,
                stream = false,
                images = new List<string> { base64Image }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseBody);
            return jsonResponse.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }
    }
}