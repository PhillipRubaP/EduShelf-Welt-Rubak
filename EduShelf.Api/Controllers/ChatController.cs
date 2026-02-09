using EduShelf.Api.Exceptions;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> PostMessage([FromForm] ChatRequest request)
        {
            var userId = GetUserId();
            var response = await _chatService.GetResponseAsync(request.Message, userId, request.ChatSessionId, request.Image);
            return Ok(new { response });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = GetUserId();
            var sessions = await _chatService.GetChatSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var userId = GetUserId();
            var session = await _chatService.CreateChatSessionAsync(userId, request.Title);
            return Ok(session);
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var userId = GetUserId();
            await _chatService.DeleteChatSessionAsync(userId, sessionId);
            return NoContent();
        }

        [HttpPut("sessions/{sessionId}")]
        public async Task<IActionResult> UpdateSession(int sessionId, [FromBody] UpdateSessionRequest request)
        {
            var userId = GetUserId();
            var session = await _chatService.UpdateChatSessionAsync(userId, sessionId, request.Title);
            return Ok(session);
        }
 
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var userId = GetUserId();
            var messages = await _chatService.GetMessagesForSessionAsync(userId, sessionId);
            return Ok(messages);
        }

        [HttpGet("sessions/{sessionId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(int sessionId, int messageId)
        {
            var userId = GetUserId();
            var message = await _chatService.GetMessageAsync(userId, sessionId, messageId);
            return Ok(message);
        }

        private int GetUserId()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");
            }
            return userId;
        }
    }

    public class ChatRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
        public int ChatSessionId { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class CreateSessionRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateSessionRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
    }
}