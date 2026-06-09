using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.DTO
{
    public class ProjectFilterDTO
    {
        public List<int> TechnologyIds { get; set; } = new List<int>();
        public ProjectType ProjectType { get; set; }
        public WorkFormat WorkFormat { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
