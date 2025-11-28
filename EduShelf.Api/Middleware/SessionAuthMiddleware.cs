using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace EduShelf.Api.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            
            // If the endpoint allows anonymous access, skip the session check
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // Check if the UserId exists in the session
            if (!context.Session.Keys.Contains("UserId"))
            {
                // If not, return a 401 Unauthorized response
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("User not authenticated.");
                return;
            }

            // If the session is valid, continue to the next middleware
            await _next(context);
        }
    }
}
