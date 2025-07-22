using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using EduShelf.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOllamaChatCompletion(
    modelId: builder.Configuration["AIService:ChatModel"]!,
    endpoint: new Uri(builder.Configuration["AIService:Endpoint"]!));
kernelBuilder.AddOllamaTextEmbeddingGeneration(
    modelId: builder.Configuration["AIService:EmbeddingModel"]!,
    endpoint: new Uri(builder.Configuration["AIService:Endpoint"]!));

builder.Services.AddScoped<IndexingService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseVector()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

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

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello from EduShelf.Api!");

app.MapControllers();

app.Run();
