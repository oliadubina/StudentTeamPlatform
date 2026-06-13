using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Controllers;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Controllers;

public class ProjectControllerTests
{
    [Fact]
    public async Task GetProjectByIdAsync_ProjectExists_ReturnsOkWithProject()
    {
        // Arrange
        var project = new ProjectResponseDetailsDTO
        {
            Id = 5,
            Title = "Project"
        };
        var controller = new ProjectController(new FakeProjectService { ProjectDetails = project });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.GetProjectByIdAsync(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(project, okResult.Value);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ProjectMissing_ReturnsNotFound()
    {
        // Arrange
        var controller = new ProjectController(new FakeProjectService { ProjectDetails = null });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.GetProjectByIdAsync(5);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateProjectAsync_ServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ProjectController(new FakeProjectService { CreateResult = false });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.CreateProjectAsync(new CreateProjectDTO());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CompleteProjectAsync_ServiceReturnsTrue_ReturnsOk()
    {
        // Arrange
        var service = new FakeProjectService { CompleteResult = true };
        var controller = new ProjectController(service);
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.CompleteProjectAsync(5);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(5, service.LastProjectId);
        Assert.Equal(7, service.LastUserId);
    }

    [Fact]
    public async Task GetChatHistory_UserHasNoAccess_ReturnsForbid()
    {
        // Arrange
        var controller = new ProjectController(new FakeProjectService
        {
            ProjectDetails = new ProjectResponseDetailsDTO
            {
                Id = 5,
                IsAuthor = false,
                IsContributor = false
            }
        });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.GetChatHistory(5);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    private sealed class FakeProjectService : IProjectService
    {
        public ProjectResponseDetailsDTO? ProjectDetails { get; set; }
        public bool CreateResult { get; set; } = true;
        public bool UpdateResult { get; set; } = true;
        public bool CompleteResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public bool RemoveContributorResult { get; set; } = true;
        public int? LastProjectId { get; private set; }
        public int? LastUserId { get; private set; }

        public Task<List<ProjectResponseDTO>> GetAllMyProjectsAsync(int authorId)
        {
            LastUserId = authorId;
            return Task.FromResult(new List<ProjectResponseDTO>());
        }

        public Task<List<ProjectResponseDTO>> GetJoinedProjectsAsync(int userId)
        {
            LastUserId = userId;
            return Task.FromResult(new List<ProjectResponseDTO>());
        }

        public Task<ProjectResponseDetailsDTO> GetProjectByIdAsync(int projectId, int currentUserId)
        {
            LastProjectId = projectId;
            LastUserId = currentUserId;
            return Task.FromResult(ProjectDetails!);
        }

        public Task<bool> CreateProjectAsync(CreateProjectDTO createProjectDTO, int authorId)
        {
            LastUserId = authorId;
            return Task.FromResult(CreateResult);
        }

        public Task<bool> UpdateProjectAsync(UpdateProjectDTO updateProjectDTO, int authorId)
        {
            LastUserId = authorId;
            return Task.FromResult(UpdateResult);
        }

        public Task<bool> CompleteProjectAsync(int projectId, int authorId)
        {
            LastProjectId = projectId;
            LastUserId = authorId;
            return Task.FromResult(CompleteResult);
        }

        public Task<bool> DeleteProjectAsync(int projectId, int authorId)
        {
            LastProjectId = projectId;
            LastUserId = authorId;
            return Task.FromResult(DeleteResult);
        }

        public Task<List<ProjectResponseDTO>> SearchProjectsAsync(ProjectFilterDTO projectFilterDTO)
        {
            return Task.FromResult(new List<ProjectResponseDTO>());
        }

        public Task<List<ProjectResponseDTO>> GetRecommendedProjectsAsync(int userId)
        {
            LastUserId = userId;
            return Task.FromResult(new List<ProjectResponseDTO>());
        }

        public Task<bool> RemoveContributorAsync(int projectId, int studentId, int currentUserId)
        {
            LastProjectId = projectId;
            LastUserId = currentUserId;
            return Task.FromResult(RemoveContributorResult);
        }

        public Task<List<ChatMessageDTO>> GetChatHistoryAsync(int projectId)
        {
            LastProjectId = projectId;
            return Task.FromResult(new List<ChatMessageDTO>());
        }

        public Task<List<ChatProjectDTO>> GetChatProjectsAsync(int userId)
        {
            LastUserId = userId;
            return Task.FromResult(new List<ChatProjectDTO>());
        }
    }
}
