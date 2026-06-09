using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.Services
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDTO>> GetAllMyProjectsAsync(int authorId);
        Task <List<ProjectResponseDTO>> GetJoinedProjectsAsync(int userId);
        Task<ProjectResponseDetailsDTO>GetProjectByIdAsync(int projectId, int currentUserId);

        Task<bool>CreateProjectAsync(CreateProjectDTO createProjectDTO, int authorId);
        Task<bool>UpdateProjectAsync(UpdateProjectDTO updateProjectDTO, int authorId);
        Task<bool> DeleteProjectAsync(int projectId, int authorId);

        Task<IList<ProjectResponseDTO>> SearchProjectsAsync(ProjectFilterDTO projectFilterDTO);
        Task<IList<ProjectResponseDTO>> GetRecommendedProjectsAsync(int userId);
        
    }
}
