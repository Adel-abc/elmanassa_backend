using elmanassa.ApplicationDbContext;
using elmanassa.DTOs;
using elmanassa.Models;
using Microsoft.EntityFrameworkCore;

namespace elmanassa.Services
{
    public interface IAiService
    {
        Task<AiConversationDTO?> CreateConversationAsync(Guid userId, string title);
        Task<AiConversationDTO?> GetConversationAsync(Guid conversationId, Guid userId);
        Task<List<AiConversationDTO>> GetUserConversationsAsync(Guid userId);
        Task<AiMessageDTO?> SendMessageAsync(Guid conversationId, Guid userId, string message);
        Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);
    }

    public class AiService : IAiService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AiService> _logger;

        public AiService(AppDbContext context, ILogger<AiService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AiConversationDTO?> CreateConversationAsync(Guid userId, string title)
        {
            try
            {
                var conversation = new AiConversation
                {
                    UserId = userId,
                    Title = title,
                    Messages = new List<AiMessage>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.AiConversations.Add(conversation);
                await _context.SaveChangesAsync();

                return new AiConversationDTO
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId,
                    Title = conversation.Title,
                    Messages = new List<AiMessageDTO>(),
                    CreatedAt = conversation.CreatedAt,
                    UpdatedAt = conversation.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return null;
            }
        }

        public async Task<AiConversationDTO?> GetConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.AiConversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (conversation == null)
                    return null;

                var messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new AiMessageDTO
                    {
                        Id = m.Id,
                        ConversationId = m.ConversationId,
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList();

                return new AiConversationDTO
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId,
                    Title = conversation.Title,
                    Messages = messages,
                    CreatedAt = conversation.CreatedAt,
                    UpdatedAt = conversation.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversation");
                return null;
            }
        }

        public async Task<List<AiConversationDTO>> GetUserConversationsAsync(Guid userId)
        {
            try
            {
                var conversations = await _context.AiConversations
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .Select(c => new AiConversationDTO
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        Title = c.Title,
                        Messages = new List<AiMessageDTO>(),
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return conversations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user conversations");
                return new List<AiConversationDTO>();
            }
        }

        public async Task<AiMessageDTO?> SendMessageAsync(Guid conversationId, Guid userId, string message)
        {
            try
            {
                var conversation = await _context.AiConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (conversation == null)
                    return null;

                // Save user message
                var userMessage = new AiMessage
                {
                    ConversationId = conversationId,
                    Role = "user",
                    Content = message,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AiMessages.Add(userMessage);

                // Generate AI response (simplified - in production would call LLM API)
                var aiResponse = await GenerateAiResponseAsync(message);

                var aiMessage = new AiMessage
                {
                    ConversationId = conversationId,
                    Role = "assistant",
                    Content = aiResponse,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AiMessages.Add(aiMessage);

                // Update conversation updated_at
                conversation.UpdatedAt = DateTime.UtcNow;
                _context.AiConversations.Update(conversation);

                await _context.SaveChangesAsync();

                return new AiMessageDTO
                {
                    Id = userMessage.Id,
                    ConversationId = userMessage.ConversationId,
                    Role = userMessage.Role,
                    Content = userMessage.Content,
                    CreatedAt = userMessage.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return null;
            }
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.AiConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (conversation == null)
                    return false;

                _context.AiConversations.Remove(conversation);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation");
                return false;
            }
        }

        private async Task<string> GenerateAiResponseAsync(string userMessage)
        {
            // This is a placeholder implementation
            // In production, this would call OpenAI, Claude, or another LLM API
            await Task.Delay(100); // Simulate API call delay

            return $"I received your message: '{userMessage}'. " +
                   "This is a placeholder AI response. In production, this would be generated by an LLM service.";
        }
    }
}
