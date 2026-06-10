using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.DTO
{
    public class StudentSpaceRequestDTO
    {
        public int RequestId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
