using System.Net;
using System.Text.Json;
using EduShelf.Api.Exceptions;

namespace EduShelf.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });

        switch (exception)
        {
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = notFoundException.Message });
                break;
            case BadRequestException badRequestException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = badRequestException.Message });
                break;
            case KernelServiceException kernelServiceException:
                code = HttpStatusCode.InternalServerError;
                result = JsonSerializer.Serialize(new { error = kernelServiceException.Message });
                break;
            case DatabaseException databaseException:
                code = HttpStatusCode.InternalServerError;
                result = JsonSerializer.Serialize(new { error = databaseException.Message });
                break;
            case AuthenticationException authenticationException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = authenticationException.Message });
                break;
            case AuthorizationException authorizationException:
                code = HttpStatusCode.Forbidden;
                result = JsonSerializer.Serialize(new { error = authorizationException.Message });
                break;
            case IndexingServiceException indexingServiceException:
                code = HttpStatusCode.InternalServerError;
                result = JsonSerializer.Serialize(new { error = indexingServiceException.Message });
                break;
            case FileProcessingException fileProcessingException:
                code = HttpStatusCode.InternalServerError;
                result = JsonSerializer.Serialize(new { error = fileProcessingException.Message });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}