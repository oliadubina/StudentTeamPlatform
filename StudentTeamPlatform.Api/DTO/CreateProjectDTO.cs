using StudentTeamPlatform.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class CreateProjectDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public List<int> TechnologyIds { get; set; } = new List<int>();
        [Required]
        public int MaxContributors { get; set; }
        [Required]
        public ProjectType ProjectType { get; set; }
        [Required]
        public WorkFormat WorkFormat { get; set; }
        [Required]
        public string Language { get; set; } = string.Empty;
        [Required]
        public List<ProjectRoleDTO> Roles { get; set; } = new List<ProjectRoleDTO>();
    }
}
