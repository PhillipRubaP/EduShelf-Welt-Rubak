using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var response = await _chatService.GetResponseAsync(request.Message);
            return Ok(new { response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}