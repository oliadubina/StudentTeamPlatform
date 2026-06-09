using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using System.Collections;


namespace StudentTeamPlatform.Api.Services
{
    public class ProjectService: IProjectService
    {
        private readonly AppDbContext _appDbContext;
        public ProjectService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<List<ProjectResponseDTO>> GetAllMyProjectsAsync(int authorId)
        {
            if (authorId == 0)
            {
                return new List<ProjectResponseDTO>();

            }
            var projectsList = await _appDbContext.Projects.Where(p => p.AuthorId==authorId).ToListAsync();
            var projects= projectsList.Select( project => new ProjectResponseDTO
            {
                Title = project.Title,
                Description = project.Description,
                Id = project.Id,
                Language=project.Language,
                ProjectState = project.ProjectState,
                ProjectType = project.ProjectType,
                WorkFormat = project.WorkFormat
            }).ToList();
            
            return projects; 
        }
        public async Task<List<ProjectResponseDTO>> GetJoinedProjectsAsync(int userId)
        {
            if (userId == 0)
            {
                return new List<ProjectResponseDTO>();
            }
            var projectsList = await _appDbContext.Projects.Where(p => p.Contributors.Any(c => c.Id== userId)).ToListAsync();
            var projects = projectsList.Select(project => new ProjectResponseDTO
            {
                Title = project.Title,
                Description = project.Description,
                Id = project.Id,
                Language=project.Language,
                ProjectState = project.ProjectState,
                ProjectType = project.ProjectType,
                WorkFormat = project.WorkFormat
            }).ToList();
            return projects;
        }
        public async Task<ProjectResponseDetailsDTO> GetProjectByIdAsync(int projectId, int currentUserId)
        {
            if (projectId == 0 || currentUserId == 0)
            {
                return null;
            }
            var project =  await _appDbContext.Projects.Include(p=> p.Contributors)
                .Include(p=>p.ProjectRoles).Include(p=>p.Technologies).FirstOrDefaultAsync(p=>p.Id==projectId);
            if (project == null) return null;
            bool IsAuthor=project.AuthorId==currentUserId;
            bool IsContributor=project.Contributors.Any(c=>c.Id==currentUserId);
            var listProjectRoles = project.ProjectRoles.Select(role =>new ProjectRoleDTO
            {
                Name= role.Name,
                SlotsCount= role.SlotsCount
            }).ToList();
            var listTechnologies = project.Technologies.Select(t => new TechnologyDTO
            {
                Name = t.Name
            }).ToList();
            var responseDTO = new ProjectResponseDetailsDTO
            {
                Id = projectId,
                Title = project.Title,
                Description = project.Description,
                IsAuthor = IsAuthor,
                IsContributor = IsContributor,
                Language = project.Language,
                ProjectState = project.ProjectState,
                ProjectType = project.ProjectType,
                WorkFormat = project.WorkFormat,
                MaxContributors = project.MaxContributors,
                ProjectRoles = listProjectRoles,
                Technology = listTechnologies
            };
            if(IsAuthor)
            {

            }
        }
        public async Task<bool> CreateProjectAsync(CreateProjectDTO createProjectDTO, int authorId)
        {
            if (createProjectDTO == null || authorId==0)
            {
                return false;
            }
            var technologies = await _appDbContext.Technologies.Where(t => createProjectDTO.TechnologyIds.Contains(t.Id)).ToListAsync();
            var projectRoles = createProjectDTO.Roles;
            List<ProjectRole> projectRolesList=new List<ProjectRole>();
            foreach (var role in projectRoles) 
            {
                ProjectRole projectRole = new ProjectRole
                {
                    Name=role.Name,
                    SlotsCount=role.SlotsCount
                };
                projectRolesList.Add(projectRole);
                
            }
            Project project = new Project
            {
                Title=createProjectDTO.Title,
                Description=createProjectDTO.Description,
                MaxContributors=createProjectDTO.MaxContributors,
                Technologies=technologies,
                ProjectState=ProjectState.SearchTeam,
                AuthorId=authorId,
                CreatedAt=DateTime.UtcNow,
                Language=createProjectDTO.Language,
                ProjectRoles=projectRolesList,
                ProjectType=createProjectDTO.ProjectType,
                WorkFormat=createProjectDTO.WorkFormat

            };
            await _appDbContext.AddAsync(project);
            await _appDbContext.SaveChangesAsync();
            return true;
           
        }
    }
}
