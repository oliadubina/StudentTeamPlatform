using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.Hubs
{
    // Hub - це базовий клас SignalR
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // 1. Коли користувач відкриває сторінку проєкту, він приєднується до "кімнати" цього проєкту
        public async Task JoinProjectChat(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
        }

        // 2. Коли хтось відправляє повідомлення
        public async Task SendMessage(int projectId, int senderId, string text)
        {
            // Зберігаємо повідомлення в базу даних
            var message = new ChatMessage
            {
                ProjectId = projectId,
                SenderId = senderId,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Дістаємо ім'я відправника, щоб гарно показати його в чаті
            var sender = await _context.Users.FindAsync(senderId);

            // Формуємо об'єкт для відправки
            var messageDto = new
            {
                id = message.Id,
                projectId = projectId,
                senderId = senderId,
                senderName = sender?.FullName ?? "Unknown",
                text = text,
                sentAt = message.SentAt
            };

            // РОЗСИЛАЄМО всім учасникам групи (тобто тим, хто зараз на сторінці цього проєкту)
            await Clients.Group(projectId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }
    }
}