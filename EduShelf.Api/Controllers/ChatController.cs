using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var response = await _chatService.GetResponseAsync(request.Message, int.Parse(userId));
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                // Log the exception details here if you have a logger
                return StatusCode(500, new { response = "An error occurred while processing your request." });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}