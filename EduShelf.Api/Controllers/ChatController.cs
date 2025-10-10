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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _chatService.GetResponseAsync(request.Message, int.Parse(userId), request.ChatSessionId);
            return Ok(new { response });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var sessions = await _chatService.GetChatSessionsAsync(int.Parse(userId));
            return Ok(sessions);
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _chatService.CreateChatSessionAsync(int.Parse(userId), request.Title);
            return Ok(session);
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var messages = await _chatService.GetMessagesForSessionAsync(int.Parse(userId), sessionId);
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var message = await _chatService.GetMessageAsync(int.Parse(userId), sessionId, messageId);
                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { response = ex.Message });
            }
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