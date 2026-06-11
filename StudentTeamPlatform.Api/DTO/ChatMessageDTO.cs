namespace StudentTeamPlatform.Api.DTO
{
    public class ChatMessageDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty; // Фронтенду треба лише ім'я, а не весь об'єкт User
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}