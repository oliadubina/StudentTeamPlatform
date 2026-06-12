namespace StudentTeamPlatform.Api.DTO
{
    public class ChatProjectDTO
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsAuthor { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
