using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace StudentTeamPlatform.Api.Services
{
    public class ProjectService: IProjectService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IJoinRequestService _joinRequestService;
        public ProjectService(AppDbContext appDbContext, IJoinRequestService joinRequestService)
        {
            _appDbContext = appDbContext;
            _joinRequestService=joinRequestService;
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
                .Include(p=>p.ProjectRoles).Include(p=>p.Technologies).Include(p=>p.Author).FirstOrDefaultAsync(p=>p.Id==projectId);
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
            AuthorDTO authorName = new AuthorDTO()
            {
                Id=project.Author.Id,
                FullName=project.Author.FullName
            };
            var responseDTO = new ProjectResponseDetailsDTO
            {
                Id = projectId,
                AuthorName=authorName,
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
                var requests=_joinRequestService.GetProjectRequestsAsync(projectId, currentUserId);
                responseDTO.PendingRequests=await requests;
            }
            return responseDTO;
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
        public async Task<bool> UpdateProjectAsync(UpdateProjectDTO updateProjectDTO, int authorId)
        {
            if(updateProjectDTO == null || authorId==0)
            {
                return false;
            }
            var project =await _appDbContext.Projects.Where(p=> p.Id==updateProjectDTO.Id)
                .Include(p => p.ProjectRoles)
                .Include(p=>p.Contributors)
                .Include(p=>p.Technologies).FirstOrDefaultAsync();
            if (project == null|| project.AuthorId!=authorId)
            {
                return false;
            }
            project.Title = updateProjectDTO.Title;
            project.Description = updateProjectDTO.Description;
            project.ProjectState = updateProjectDTO.ProjectState;
            project.ProjectType = updateProjectDTO.ProjectType;
            project.WorkFormat = updateProjectDTO.WorkFormat;
            project.Language = updateProjectDTO.Language;
            var listTechnologies = updateProjectDTO.Technologies.Select(t => new Technology
            {
                Name = t.Name
            }).ToList();
            project.Technologies= listTechnologies;
            if(project.Contributors.Any()==false)
            {
                _appDbContext.ProjectRoles.RemoveRange(project.ProjectRoles);
                var newRoles=updateProjectDTO.Roles.Select(role=>new ProjectRole
                {
                    Name=role.Name,
                    SlotsCount=role.SlotsCount,
                    ProjectId=project.Id
                }).ToList();
                await _appDbContext.ProjectRoles.AddRangeAsync(newRoles);
                project.MaxContributors = updateProjectDTO.MaxContributors;
            }
            await _appDbContext.SaveChangesAsync();
            return true;

        }
        public async Task<bool> DeleteProjectAsync(int projectId, int authorId)
        {
            if (projectId==0 || authorId == 0)
                { return false; }
            var project = await _appDbContext.Projects.Include(p=>p.Contributors).FirstOrDefaultAsync(p => p.Id==projectId);
            if (project == null || project.AuthorId!=authorId)
                { return false; }
            if (project.Contributors.Any()) 
            { 
                return false; 
            }
            
            _appDbContext.Remove(project);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<List<ProjectResponseDTO>> SearchProjectsAsync(ProjectFilterDTO projectFilterDTO)
        {
            var projects = _appDbContext.Projects.Where(p => p.ProjectState==ProjectState.SearchTeam);
            if (!string.IsNullOrWhiteSpace(projectFilterDTO.SearchKeyword))
            {
                projects = projects.Where(p => p.Title.ToLower().Contains(projectFilterDTO.SearchKeyword.ToLower()) || p.Description.ToLower().Contains(projectFilterDTO.SearchKeyword.ToLower()));
            }
            if (projectFilterDTO.ProjectType!=null)
            {
                projects = projects.Where(p => p.ProjectType == projectFilterDTO.ProjectType);
            }
            if (projectFilterDTO.WorkFormat!=null)
            {
                projects=projects.Where(p => p.WorkFormat == projectFilterDTO.WorkFormat);
            }
            if (!string.IsNullOrWhiteSpace(projectFilterDTO.Language))
            {
                projects=projects.Where(p => p.Language == projectFilterDTO.Language);
            }
            if (projectFilterDTO.Technologies != null && projectFilterDTO.Technologies.Any())
            {
                var techNames = projectFilterDTO.Technologies.Select(t => t.Name.ToLower()).ToList();
                projects = projects.Where(p => p.Technologies.Any(pt => techNames.Contains(pt.Name.ToLower())));
            }
            if (projectFilterDTO.Role!=null)
            {
                projects=projects.Where(p=>p.ProjectRoles.Any(r=>r.Name==projectFilterDTO.Role && r.SlotsCount>0)).Include(p => p.Technologies);
            }
            var projectsList=await projects.ToListAsync();
            var listOfProjectResponseDTO = projectsList.Select(project => new ProjectResponseDTO
            {
                Id=project.Id,
                Description=project.Description,
                Language=project.Language,
                ProjectState=project.ProjectState,
                ProjectType=project.ProjectType,
                Title=project.Title,
                WorkFormat=project.WorkFormat
            }).ToList();
            return listOfProjectResponseDTO;
        }
        public async Task<List<ProjectResponseDTO>> GetRecommendedProjectsAsync(int userId)
        {
            var userProfile = await _appDbContext.Users.Include(u => u.Skills).FirstOrDefaultAsync(u => u.Id==userId);

            if (userProfile==null || userProfile.Skills.Any()==false)
            {
                return new List<ProjectResponseDTO>();
            }
            var projects = await _appDbContext.Projects.Where(p => p.ProjectState==ProjectState.SearchTeam && p.AuthorId!=userId)
                .Include(p => p.Technologies).Include(p => p.ProjectRoles).ToListAsync();
            ICollection<ProjectResponseDTO> recomendationList = new List<ProjectResponseDTO>();

            foreach (var project in projects)
            {
                // Робимо загальний бал дробовим числом, щоб математика працювала правильно
                double totalScore = 0;

                // --- БЛОК 1: НАВИЧКИ (Максимум 40 балів) ---
                if (!project.Technologies.Any())
                {
                    // Якщо проєкт не вимагає конкретних технологій — даємо одразу 40 балів
                    totalScore += 40;
                }
                else
                {
                    // Скільки технологій проєкту збігається з навичками користувача?
                    // LINQ робить те саме, що твої два foreach, але в 1 рядок:
                    int matchedTechs = 0;
                    foreach (var tech in project.Technologies)
                    {
                        if (userProfile.Skills.Any(s => s.Name.ToLower() == tech.Name.ToLower()))
                        {
                            matchedTechs++;
                        }
                    }

                    // Правильна математика з приведенням до double!
                    totalScore += ((double)matchedTechs / project.Technologies.Count) * 40;
                }

                // --- БЛОК 2: РОЛІ (Максимум 30 балів) ---
                // Тут ми дописуємо твій незавершений if(role.SlotsCount > 0 && ...)
                bool hasRoleMatch = false;
                foreach (var role in project.ProjectRoles)
                {
                    if (role.SlotsCount > 0)
                    {
                        // Перевіряємо, чи збігається назва ролі з категорією якоїсь навички студента
                        // (або з його спеціальністю, якщо ти її витягнеш)
                        if (userProfile.Skills.Any(s => s.Category.ToLower().Contains(role.Name.ToLower()) ||
                                                        role.Name.ToLower().Contains(s.Category.ToLower())))
                        {
                            hasRoleMatch = true;
                            break; // Знайшли хоча б одну підходящу роль - виходимо з циклу
                        }
                    }
                }

                if (hasRoleMatch)
                {
                    totalScore += 30;
                }

                // --- БЛОК 3: ФОРМАТ (15 балів) ---
                if (userProfile.WorkFormat == project.WorkFormat || userProfile.WorkFormat == null)
                {
                    totalScore += 15;
                }

                // --- БЛОК 4: МОВА (15 балів) ---
                if (string.Equals(userProfile.PreferredLanguage, project.Language, StringComparison.OrdinalIgnoreCase))
                {
                    totalScore += 15;
                }

                // 5. Записуємо результат (якщо бал достатньо високий, наприклад > 40)
                if (totalScore >= 40)
                {
                    recomendationList.Add(new ProjectResponseDTO
                    {
                        Id = project.Id,
                        Title = project.Title,
                        Description = project.Description,
                        ProjectState = project.ProjectState,
                        ProjectType = project.ProjectType,
                        WorkFormat = project.WorkFormat,
                        Language = project.Language,
                        MatchScore = (int)Math.Round(totalScore) // Округлюємо до цілого числа
                    });
                }
            }

            return recomendationList.OrderByDescending(p => p.MatchScore).Take(10).ToList();
        }
    }
}
