using EduShelf.Api.Services;
using EduShelf.Api.Services.FileStorage;
using EduShelf.Api.Services.Background;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace EduShelf.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Semantic Kernel
        var kernelBuilder = services.AddKernel();
        
        // HTTP Client for ImageProcessing
        services.AddHttpClient<IImageProcessingService, ImageProcessingService>(client =>
        {
            client.BaseAddress = new Uri(configuration["AIService:Endpoint"]!);
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // Ollama Client
        var ollamaClient = new HttpClient
        {
            BaseAddress = new Uri(configuration["AIService:Endpoint"]!),
            Timeout = TimeSpan.FromMinutes(10)
        };

        kernelBuilder.AddOllamaChatCompletion(
            modelId: configuration["AIService:ChatModel"]!,
            httpClient: ollamaClient);

        #pragma warning disable SKEXP0070
        kernelBuilder.AddOllamaTextEmbeddingGeneration(
            modelId: configuration["AIService:EmbeddingModel"]!,
            httpClient: ollamaClient);

        // Core Services
        services.AddScoped<IndexingService>();
        services.AddScoped<ChatService>();
        services.AddScoped<IntentDetectionService>();
        services.AddScoped<RetrievalService>();
        services.AddScoped<PromptGenerationService>();
        services.AddScoped<IRAGService, RAGService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IDocumentService, DocumentService>();
        
        // File Storage
        services.AddSingleton<IFileStorageService, MinioStorageService>();
        
        services.AddHttpContextAccessor();

        // Background Job Processing
        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<BackgroundJobService>();

        // Embedding Generators (Workaround)
        #pragma warning disable SKEXP0001
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var textEmbeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            return new TextEmbeddingGenerationServiceAdapter(textEmbeddingService);
        });

        return services;
    }
}
