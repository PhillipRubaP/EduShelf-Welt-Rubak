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
using FluentValidation.AspNetCore;
using EduShelf.Api.Extensions;

 var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = new Microsoft.AspNetCore.Server.Kestrel.Core.MinDataRate(100, TimeSpan.FromSeconds(30));
});
 
// Add Application Services
builder.Services.AddApplicationServices(builder.Configuration);

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
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
 
 builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
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


app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello from EduShelf.Api!");

app.MapControllers();

app.Run();

