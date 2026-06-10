namespace StudentTeamPlatform.Api.Models
{
    public enum RequestStatus
    {
        Pending,  
        Accepted, 
        Rejected
    }
    public class JoinRequest
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int StudentId { get; set; }

        // ДОДАЄМО ЗВ'ЯЗОК З РОЛЛЮ:
        public int ProjectRoleId { get; set; }
        public ProjectRole ProjectRole { get; set; } = null!;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Project Project { get; set; } = null!;
        public User Student { get; set; } = null!;
    }
}
