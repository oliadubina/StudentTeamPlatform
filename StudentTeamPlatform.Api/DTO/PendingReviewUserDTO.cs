namespace StudentTeamPlatform.Api.DTO
{
    public class PendingReviewUserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
