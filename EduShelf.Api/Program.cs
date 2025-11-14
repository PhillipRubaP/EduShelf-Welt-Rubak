using EduShelf.Api.Data;
using EduShelf.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.AI;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
 
 var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = new Microsoft.AspNetCore.Server.Kestrel.Core.MinDataRate(100, TimeSpan.FromSeconds(30));
});
 
 // Add Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOllamaChatCompletion(
    modelId: builder.Configuration["AIService:ChatModel"]!,
    endpoint: new Uri(builder.Configuration["AIService:Endpoint"]!))
    .Services.AddHttpClient("Ollama", c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["AIService:Endpoint"]!);
        c.Timeout = TimeSpan.FromMinutes(5);
    });
#pragma warning disable SKEXP0070
kernelBuilder.AddOllamaTextEmbeddingGeneration(
    modelId: builder.Configuration["AIService:EmbeddingModel"]!,
    endpoint: new Uri(builder.Configuration["AIService:Endpoint"]!))
    .Services.AddHttpClient("Ollama", c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["AIService:Endpoint"]!);
        c.Timeout = TimeSpan.FromMinutes(5);
    });

builder.Services.AddScoped<IndexingService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<IntentDetectionService>();
builder.Services.AddScoped<RetrievalService>();
builder.Services.AddScoped<PromptGenerationService>();
builder.Services.AddScoped<IRAGService, RAGService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EduShelf API", Version = "v1" });
    c.TagActionsBy(api => new[] { api.GroupName });
    c.DocInclusionPredicate((name, api) => true);

    c.AddServer(new OpenApiServer { Url = "http://localhost:49152", Description = "Local Docker" });

    // JWT Bearer Token hinzufügen
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            Array.Empty<string>()
        }
    });
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

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello from EduShelf.Api!");

app.MapControllers();

app.Run();

