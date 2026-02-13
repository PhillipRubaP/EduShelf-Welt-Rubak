using EduShelf.Api.Services;
using EduShelf.Api.Services.FileStorage;
using EduShelf.Api.Services.Background;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

using EduShelf.Api.Extensions;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Http;

namespace EduShelf.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Semantic Kernel
        var kernelBuilder = services.AddKernel();
        

        // Define Polly Policies
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        // HTTP Client for ImageProcessing
        services.AddHttpClient<IImageProcessingService, ImageProcessingService>(client =>
        {
            client.BaseAddress = new Uri(configuration["AIService:Endpoint"]!);
            client.Timeout = TimeSpan.FromMinutes(10);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        // Ollama Client with Resilience
        // We manually create the handler pipeline because Semantic Kernel accepts an HttpClient instance
        var ollamaHandler = new PolicyHttpMessageHandler(retryPolicy.WrapAsync(circuitBreakerPolicy));
        ollamaHandler.InnerHandler = new HttpClientHandler();

        var ollamaClient = new HttpClient(ollamaHandler)
        {
            BaseAddress = new Uri(configuration["AIService:Endpoint"]!),
            Timeout = TimeSpan.FromMinutes(10)
        };

        kernelBuilder.AddOllamaChatCompletion(
            modelId: configuration["AIService:ChatModel"]!,
            httpClient: ollamaClient);

        #pragma warning disable SKEXP0070
        kernelBuilder.AddOllamaEmbeddingGenerator(
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
        services.AddScoped<IFlashcardService, FlashcardService>();
        services.AddScoped<IFileParsingService, FileParsingService>();
        
        // File Storage
        services.AddSingleton<IFileStorageService, MinioStorageService>();
        
        services.AddHttpContextAccessor();

        // Background Job Processing
        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<BackgroundJobService>();

        return services;
    }
}
