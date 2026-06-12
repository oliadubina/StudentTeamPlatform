using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.Models;
using System.Security.Claims;

namespace StudentTeamPlatform.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task JoinProjectChat(string projectId)
        {
            var userId = GetCurrentUserId();

            if (userId == null || !int.TryParse(projectId, out int parsedProjectId))
            {
                throw new HubException("Некоректні дані чату.");
            }

            bool hasAccess = await HasProjectChatAccessAsync(parsedProjectId, userId.Value);

            if (!hasAccess)
            {
                throw new HubException("Немає доступу до чату цього проєкту.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
        }

        public async Task SendMessage(int projectId, string text)
        {
            var senderId = GetCurrentUserId();

            if (senderId == null)
            {
                throw new HubException("Користувача не авторизовано.");
            }

            bool hasAccess = await HasProjectChatAccessAsync(projectId, senderId.Value);

            if (!hasAccess)
            {
                throw new HubException("Немає доступу до чату цього проєкту.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var message = new ChatMessage
            {
                ProjectId = projectId,
                SenderId = senderId.Value,
                Text = text.Trim(),
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId.Value);

            var messageDto = new
            {
                id = message.Id,
                projectId,
                senderId = senderId.Value,
                senderName = sender?.FullName ?? "Unknown",
                text = message.Text,
                sentAt = message.SentAt
            };

            await Clients.Group(projectId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }

        private int? GetCurrentUserId()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out int parsedUserId) ? parsedUserId : null;
        }

        private async Task<bool> HasProjectChatAccessAsync(int projectId, int userId)
        {
            return await _context.Projects.AnyAsync(project =>
                project.Id == projectId &&
                (project.AuthorId == userId || project.Contributors.Any(contributor => contributor.Id == userId)));
        }
    }
}
