namespace StudentTeamPlatform.Api.DTO
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int RevieweeId { get; set; }
        public string RevieweeName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
