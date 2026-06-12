using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.DTO
{
    public class JoinRequestDTO
    {
        public int Id { get; set; } 
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty; 
        public int ProjectRoleId { get; set; }
        public string ProjectRoleName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
