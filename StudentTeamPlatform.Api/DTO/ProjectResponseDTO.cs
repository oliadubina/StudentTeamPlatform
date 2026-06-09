using StudentTeamPlatform.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class ProjectResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectState ProjectState { get; set; }
        public ProjectType ProjectType { get; set; }
        public WorkFormat WorkFormat { get; set; }
        public string Language { get; set; } = string.Empty;
    }
}
