using elmanassa.DTOs;
using elmanassa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace elmanassa.Controllers
{
    [ApiController]
    [Route("api/v1/ai")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        }

        /// <summary>
        /// Create a new AI conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ApiResponse<AiConversationDTO>>> CreateConversation(
            [FromBody] AiConversationCreateDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(
                        "Invalid conversation data", "VALIDATION_ERROR", false));

                var userId = GetUserId();
                var conversation = await _aiService.CreateConversationAsync(userId, model.Title);

                if (conversation == null)
                    return BadRequest(new ApiResponse<object>(
                        "Failed to create conversation", "CONVERSATION_CREATION_FAILED", false));

                return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id },
                    new ApiResponse<AiConversationDTO>(conversation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Get conversation by ID
        /// </summary>
        [HttpGet("conversations/{id}")]
        public async Task<ActionResult<ApiResponse<AiConversationDTO>>> GetConversation(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var conversation = await _aiService.GetConversationAsync(id, userId);

                if (conversation == null)
                    return NotFound(new ApiResponse<object>(
                        "Conversation not found", "CONVERSATION_NOT_FOUND", false));

                return Ok(new ApiResponse<AiConversationDTO>(conversation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversation");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Get all user conversations
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<ApiResponse<List<AiConversationDTO>>>> GetConversations()
        {
            try
            {
                var userId = GetUserId();
                var conversations = await _aiService.GetUserConversationsAsync(userId);

                return Ok(new ApiResponse<List<AiConversationDTO>>(conversations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversations");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Send message to AI conversation
        /// </summary>
        [HttpPost("conversations/{id}/messages")]
        public async Task<ActionResult<ApiResponse<AiMessageDTO>>> SendMessage(
            Guid id,
            [FromBody] AiMessageCreateDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(
                        "Invalid message data", "VALIDATION_ERROR", false));

                var userId = GetUserId();
                var message = await _aiService.SendMessageAsync(id, userId, model.Message);

                if (message == null)
                    return BadRequest(new ApiResponse<object>(
                        "Failed to send message", "MESSAGE_SEND_FAILED", false));

                return CreatedAtAction(nameof(GetConversation), new { id = id },
                    new ApiResponse<AiMessageDTO>(message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Delete conversation
        /// </summary>
        [HttpDelete("conversations/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteConversation(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var success = await _aiService.DeleteConversationAsync(id, userId);

                if (!success)
                    return NotFound(new ApiResponse<object>(
                        "Conversation not found", "CONVERSATION_NOT_FOUND", false));

                return Ok(new ApiResponse<object>("Conversation deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }
    }
}
