using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.DTO
{
    public class ProjectResponseDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<TechnologyDTO> Technology { get; set; } = new List<TechnologyDTO>();
        public int MaxContributors { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsContributor { get; set; }
        public ProjectState ProjectState { get; set; }
        public ProjectType ProjectType { get; set; }
        public WorkFormat WorkFormat { get; set; }
        public string Language { get; set; } = string.Empty;
        public ICollection<JoinRequestDTO> PendingRequests { get; set; } = new List<JoinRequestDTO>();
        public ICollection<ProjectRoleDTO> ProjectRoles { get; set; } = new List<ProjectRoleDTO>();
        public ICollection<ContributorsDTO> Contributors { get; set; } = new List<ContributorsDTO>();
    }

}
