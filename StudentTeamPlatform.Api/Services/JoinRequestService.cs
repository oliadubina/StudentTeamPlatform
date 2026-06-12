using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.Services
{
    public class JoinRequestService: IJoinRequestService
    {
        private readonly AppDbContext _appDbContext;
        public JoinRequestService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        // Додаємо параметр roleId
        public async Task<bool> ApplyForProjectAsync(int projectId, int studentId, int roleId)
        {
            if (projectId == 0 || studentId == 0 || roleId == 0) return false;

            var project = await _appDbContext.Projects
                .Include(p => p.Contributors)
                .Include(p => p.ProjectRoles) // Підтягуємо ролі
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.ProjectState != ProjectState.SearchTeam) return false;
            if (project.AuthorId == studentId) return false;
            if (project.Contributors.Any(c => c.Id == studentId)) return false;

            // ЗАКРИВАЄМО ПРОГАЛИНУ: Перевіряємо, чи існує така роль і чи є на ній вільні місця!
            var role = project.ProjectRoles.FirstOrDefault(r => r.Id == roleId);
            if (role == null || role.SlotsCount <= 0)
            {
                return false; // Місць на цю роль уже немає, заявка скасовується
            }

            bool isItNotFirstRequest = await _appDbContext.JoinRequests
                .AnyAsync(j => j.ProjectId == projectId && j.StudentId == studentId);
            if (isItNotFirstRequest) return false;

            JoinRequest joinRequest = new JoinRequest()
            {
                ProjectId = projectId,
                StudentId = studentId,
                ProjectRoleId = roleId, // Зберігаємо роль, на яку подався студент
                CreatedAt = DateTime.UtcNow,
                Status = RequestStatus.Pending
            };

            await _appDbContext.JoinRequests.AddAsync(joinRequest);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<ICollection<JoinRequestDTO>> GetProjectRequestsAsync(int projectId, int authorId)
        {
            if (projectId == 0 || authorId==0)
            {
                return new List<JoinRequestDTO>();
            }
            var requestList= await _appDbContext.JoinRequests
                .Include(r => r.Student)
                .Include(r => r.ProjectRole)
                .Where(r=> r.ProjectId==projectId && r.Project.AuthorId==authorId)
                .ToListAsync();
            
            var listofRequestsDTO=requestList.Select(request=>new JoinRequestDTO
            {
                Id = request.Id,
                StudentId=request.StudentId,
                StudentName = request.Student.FullName,
                ProjectRoleId=request.ProjectRoleId,
                ProjectRoleName=request.ProjectRole.Name,
                Status=request.Status,
                CreatedAt=request.CreatedAt
            }).ToList();
            return listofRequestsDTO;
        }
        public async Task<bool> RespondToRequestAsync(int requestId, int authorId, RequestStatus status)
        {
            if(requestId == 0 || authorId == 0) { return false; }
            var request= await _appDbContext.JoinRequests.FirstOrDefaultAsync(r=>r.Id==requestId);
            if(request==null) { return false; }
            var project = await _appDbContext.Projects
                .Include(p => p.Contributors)
                .Include(p => p.ProjectRoles)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.AuthorId == authorId);
            if (project!=null)
            {
                if (request.Status != RequestStatus.Pending) return false;
                if (status == RequestStatus.Accepted)
                {
                    if (project.Contributors.Count >= project.MaxContributors) return false;

                    // Шукаємо конкретну роль, на яку подавався студент
                    var role = await _appDbContext.ProjectRoles.FindAsync(request.ProjectRoleId);
                    if (role == null || role.SlotsCount <= 0) return false; // Місць на цю роль вже немає

                    // ЗМЕНШУЄМО КІЛЬКІСТЬ ВІЛЬНИХ МІСЦЬ НА РОЛЬ!
                    role.SlotsCount--;

                    var student = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == request.StudentId);
                    if (student != null)
                    {
                        project.Contributors.Add(student);
                    }
                    request.Status = status;

                    // АВТОМАТИЧНА ЗМІНА СТАТУСУ ПРОЄКТУ:
                    // Якщо після цього в базі не залишилося жодної ролі з вільними місцями (усі SlotsCount == 0)
                    if (!project.ProjectRoles.Any(r => r.SlotsCount > 0))
                    {
                        project.ProjectState = ProjectState.InProcess; // Закриваємо набір, переводимо в процес!
                    }
                }
                else
                {
                    request.Status = status;
                }

                await _appDbContext.SaveChangesAsync();
                return true;
            }
             return false; 
        }
        public async Task<List<StudentSpaceRequestDTO>> GetMySubmittedRequestsAsync(int studentId)
        {
            return await _appDbContext.JoinRequests
                .Include(r => r.Project)
                .Where(r => r.StudentId == studentId)
                .Select(r => new StudentSpaceRequestDTO
                {
                    RequestId = r.Id,
                    ProjectId = r.ProjectId,
                    ProjectTitle = r.Project.Title,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
        }
        public async Task<bool> CancelRequestAsync(int requestId, int studentId)
        {
            var request = await _appDbContext.JoinRequests.FindAsync(requestId);

            // Перевіряємо: чи існує запит, чи це запит цього студента, і чи він ще не розглянутий
            if (request == null || request.StudentId != studentId || request.Status != RequestStatus.Pending)
                return false;

            _appDbContext.JoinRequests.Remove(request);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
