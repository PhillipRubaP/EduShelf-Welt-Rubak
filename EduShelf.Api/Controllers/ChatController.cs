using EduShelf.Api.Exceptions;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> PostMessage([FromBody] ChatRequest request)
        {
            var userId = GetUserId();
            var response = await _chatService.GetResponseAsync(request.Message, userId, request.ChatSessionId);
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

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var userId = GetUserId();
            try
            {
                var messages = await _chatService.GetMessagesForSessionAsync(userId, sessionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { response = ex.Message });
            }
        }

        [HttpGet("sessions/{sessionId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(int sessionId, int messageId)
        {
            var userId = GetUserId();
            try
            {
                var message = await _chatService.GetMessageAsync(userId, sessionId, messageId);
                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { response = ex.Message });
            }
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
        public string Message { get; set; }
        public int ChatSessionId { get; set; }
    }

    public class CreateSessionRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
    }
}