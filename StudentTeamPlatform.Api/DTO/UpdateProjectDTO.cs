using StudentTeamPlatform.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class UpdateProjectDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public List<TechnologyDTO> Technologies { get; set; } = new List<TechnologyDTO>();
        [Required]
        public int MaxContributors { get; set; }
        [Required]
        public ProjectState ProjectState { get; set; }
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
