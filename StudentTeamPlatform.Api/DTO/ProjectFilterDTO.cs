using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.DTO
{
    public class ProjectFilterDTO
    {
        public string? SearchKeyword { get; set; }
        public List<TechnologyDTO> Technologies { get; set; } = new List<TechnologyDTO>();
        public ProjectType? ProjectType { get; set; }
        public WorkFormat? WorkFormat { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
