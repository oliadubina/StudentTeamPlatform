using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;

namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        private string? GetUserId()
        {
            var Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Id;
        }
        [HttpGet("my-createdprojects")]
        [Authorize]
        public async Task<IActionResult> GetMyCreatedProjectsAsync()
        {
            var authorId = GetUserId();
            if (authorId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(authorId, out int parsedAuthorId)) return Unauthorized("Некоректний токен");
            var myProjectsList = await _projectService.GetAllMyProjectsAsync(parsedAuthorId);
            return Ok(myProjectsList);
        }
        [HttpGet("my-joinedprojects")]
        [Authorize]
        public async Task<IActionResult> GetMyJoinedProjectsAsync()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            var myJoinedProjects = await _projectService.GetJoinedProjectsAsync(parsedUserId);
            return Ok(myJoinedProjects);
        }
        [HttpGet("{projectId}")]
        [Authorize]
        public async Task<IActionResult> GetProjectByIdAsync(int projectId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            var project = await _projectService.GetProjectByIdAsync(projectId, parsedUserId);
            if (project == null) return NotFound();
            return Ok(project);
        }
        [HttpPost("create-project")]
        [Authorize]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectDTO createProjectDTO)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            bool isCreated = await _projectService.CreateProjectAsync(createProjectDTO, parsedUserId);
            if (!isCreated)
            {
                return BadRequest("Не вдалося створити проєкт");
            }
            return Ok(createProjectDTO);
        }
        [HttpPut("update-project")]
        [Authorize]
        public async Task<IActionResult> UpdateProjectAsync(UpdateProjectDTO updateProjectDTO)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            bool isUpdated = await _projectService.UpdateProjectAsync(updateProjectDTO, parsedUserId);
            if (!isUpdated)
            {
                return BadRequest("Не вдалося редагувати проєкт");
            }
            return Ok(updateProjectDTO);
        }
        [HttpDelete("{projectId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProjectAsync(int projectId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            bool IsDeleted = await _projectService.DeleteProjectAsync(projectId, parsedUserId);
            if (!IsDeleted)
            {
                return BadRequest("Не вдалося видалити проєкт (можливо, його не існує, ви не автор, або в ньому є учасники)");
            }
            return NoContent();
        }
        [HttpPost("search")]
        [Authorize]
        public async Task<IActionResult> SearchProjectsAsync(ProjectFilterDTO projectFilterDTO)
        {
            var projects = await _projectService.SearchProjectsAsync(projectFilterDTO);
            return Ok(projects);
        }
        [HttpGet("recommended-projects")]
        [Authorize]
        public async Task<IActionResult> GetRecommendedProjectsAsync()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            var recomendedProjects = await _projectService.GetRecommendedProjectsAsync(parsedUserId);
            return Ok(recomendedProjects);
        }
        [HttpGet("chats")]
        [Authorize]
        public async Task<IActionResult> GetChatProjectsAsync()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Не знайдено Id");
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");

            var chats = await _projectService.GetChatProjectsAsync(parsedUserId);
            return Ok(chats);
        }
        // Додаємо ендпоінт для видалення учасника / виходу з команди
        [HttpDelete("{projectId}/contributors/{studentId}")]
        [Authorize]
        public async Task<IActionResult> RemoveContributorAsync(int projectId, int studentId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Не знайдено Id у токені");
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");

            // Викликаємо метод сервісу
            bool isRemoved = await _projectService.RemoveContributorAsync(projectId, studentId, parsedUserId);

            if (!isRemoved)
            {
                return BadRequest("Не вдалося виконати дію. Перевірте права доступу або наявність користувача в проєкті.");
            }

            return Ok(new { message = "Учасник успішно покинув проєкт (або був видалений), вільні місця відновлено!" });
        }
        [HttpGet("{projectId}/chat-history")]
        [Authorize]
        public async Task<IActionResult> GetChatHistory(int projectId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Не знайдено Id");
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");

            var project = await _projectService.GetProjectByIdAsync(projectId, parsedUserId);
            if (project == null) return NotFound();
            if (!project.IsAuthor && !project.IsContributor) return Forbid();

            // Витягуємо всі повідомлення для конкретного проєкту, сортуємо за датою (старіші зверху)
            var messages = await _projectService.GetChatHistoryAsync(projectId);
            return Ok(messages);
        }
    }
}
