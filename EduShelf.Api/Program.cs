using EduShelf.Api.Data;
using EduShelf.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.AI;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
 
 var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = new Microsoft.AspNetCore.Server.Kestrel.Core.MinDataRate(100, TimeSpan.FromSeconds(30));
});
 
 // Add Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();
builder.Services.AddHttpClient();
// Create a custom HttpClient with a long timeout for Ollama
var ollamaClient = new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["AIService:Endpoint"]!),
    Timeout = TimeSpan.FromMinutes(10)
};

kernelBuilder.AddOllamaChatCompletion(
    modelId: builder.Configuration["AIService:ChatModel"]!,
    httpClient: ollamaClient);

#pragma warning disable SKEXP0070
kernelBuilder.AddOllamaTextEmbeddingGeneration(
    modelId: builder.Configuration["AIService:EmbeddingModel"]!,
    httpClient: ollamaClient);

builder.Services.AddScoped<IndexingService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<IntentDetectionService>();
builder.Services.AddScoped<RetrievalService>();
builder.Services.AddScoped<PromptGenerationService>();
builder.Services.AddScoped<IRAGService, RAGService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddHttpContextAccessor();

// This is a temporary workaround to bridge ITextEmbeddingGenerationService to IEmbeddingGenerator
// This should be replaced if a better adapter or direct registration becomes available.
#pragma warning disable SKEXP0001
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    var textEmbeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    return new TextEmbeddingGenerationServiceAdapter(textEmbeddingService);
});
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    return new TextEmbeddingGenerationServiceAdapter(embeddingService);
});

builder.Services.AddDbContext<ApiDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseVector());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax; // Ensure cookie is sent on cross-site requests
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EduShelf API", Version = "v1" });
    c.TagActionsBy(api => new[] { api.GroupName });
    c.DocInclusionPredicate((name, api) => true);

    c.AddServer(new OpenApiServer { Url = "http://localhost:49152", Description = "Local Docker" });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
 
 builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173", "http://localhost:5174")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello from EduShelf.Api!");

app.MapControllers();

app.Run();

