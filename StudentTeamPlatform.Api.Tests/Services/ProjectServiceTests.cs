using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Services;

public class ProjectServiceTests
{
    [Fact]
    public async Task CreateProjectAsync_ValidDto_CreatesProjectWithRolesAndTechnologies()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var technology = new Technology { Name = "React" };
        dbContext.Users.Add(author);
        dbContext.Technologies.Add(technology);
        await dbContext.SaveChangesAsync();

        var service = new ProjectService(dbContext, new FakeJoinRequestService());
        var createDto = new CreateProjectDTO
        {
            Title = "New Project",
            Description = "Project description",
            MaxContributors = 2,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            TechnologyIds = new List<int> { technology.Id },
            Roles = new List<ProjectRoleDTO>
            {
                new() { Name = "Frontend", SlotsCount = 2 }
            }
        };

        // Act
        var result = await service.CreateProjectAsync(createDto, author.Id);

        // Assert
        Assert.True(result);

        var project = await dbContext.Projects
            .Include(p => p.Technologies)
            .Include(p => p.ProjectRoles)
            .SingleAsync();

        Assert.Equal("New Project", project.Title);
        Assert.Equal(author.Id, project.AuthorId);
        Assert.Equal(ProjectState.SearchTeam, project.ProjectState);
        Assert.Single(project.Technologies);
        Assert.Single(project.ProjectRoles);
        Assert.Equal("Frontend", project.ProjectRoles.Single().Name);
    }

    [Fact]
    public async Task SearchProjectsAsync_SearchKeywordAndRole_ReturnsMatchingSearchTeamProjects()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        dbContext.Users.Add(author);
        dbContext.Projects.AddRange(
            new Project
            {
                Title = "React Coursework",
                Description = "Frontend work",
                Author = author,
                ProjectState = ProjectState.SearchTeam,
                ProjectType = ProjectType.CourseWork,
                WorkFormat = WorkFormat.Online,
                Language = "Ukrainian",
                MaxContributors = 1,
                ProjectRoles = new List<ProjectRole>
                {
                    new() { Name = "Frontend", SlotsCount = 1 }
                }
            },
            new Project
            {
                Title = "Archived React Project",
                Description = "Should not be returned",
                Author = author,
                ProjectState = ProjectState.Done,
                ProjectType = ProjectType.CourseWork,
                WorkFormat = WorkFormat.Online,
                Language = "Ukrainian",
                MaxContributors = 1,
                ProjectRoles = new List<ProjectRole>
                {
                    new() { Name = "Frontend", SlotsCount = 1 }
                }
            },
            new Project
            {
                Title = "Backend Coursework",
                Description = "Different role",
                Author = author,
                ProjectState = ProjectState.SearchTeam,
                ProjectType = ProjectType.CourseWork,
                WorkFormat = WorkFormat.Online,
                Language = "Ukrainian",
                MaxContributors = 1,
                ProjectRoles = new List<ProjectRole>
                {
                    new() { Name = "Backend", SlotsCount = 1 }
                }
            });
        await dbContext.SaveChangesAsync();

        var service = new ProjectService(dbContext, new FakeJoinRequestService());

        // Act
        var result = await service.SearchProjectsAsync(new ProjectFilterDTO
        {
            SearchKeyword = "React",
            Role = "Frontend"
        });

        // Assert
        var project = Assert.Single(result);
        Assert.Equal("React Coursework", project.Title);
    }

    [Fact]
    public async Task CompleteProjectAsync_AuthorCompletesProject_SetsProjectStateToDone()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var project = new Project
        {
            Title = "Active Project",
            Description = "Description",
            Author = author,
            ProjectState = ProjectState.InProcess,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            MaxContributors = 1
        };
        dbContext.Users.Add(author);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = new ProjectService(dbContext, new FakeJoinRequestService());

        // Act
        var result = await service.CompleteProjectAsync(project.Id, author.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(ProjectState.Done, project.ProjectState);
    }

    [Fact]
    public async Task CompleteProjectAsync_NonAuthor_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var otherUser = new User { FullName = "Other", Email = "other@test.com" };
        var project = new Project
        {
            Title = "Active Project",
            Description = "Description",
            Author = author,
            ProjectState = ProjectState.InProcess,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            MaxContributors = 1
        };
        dbContext.Users.AddRange(author, otherUser);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = new ProjectService(dbContext, new FakeJoinRequestService());

        // Act
        var result = await service.CompleteProjectAsync(project.Id, otherUser.Id);

        // Assert
        Assert.False(result);
        Assert.Equal(ProjectState.InProcess, project.ProjectState);
    }

    [Fact]
    public async Task GetChatProjectsAsync_ProjectIsDone_ExcludesProjectFromActiveChats()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        dbContext.Users.Add(author);
        dbContext.Projects.AddRange(
            new Project
            {
                Title = "Active Chat",
                Description = "Description",
                Author = author,
                ProjectState = ProjectState.InProcess,
                ProjectType = ProjectType.CourseWork,
                WorkFormat = WorkFormat.Online,
                Language = "Ukrainian",
                MaxContributors = 1
            },
            new Project
            {
                Title = "Finished Chat",
                Description = "Description",
                Author = author,
                ProjectState = ProjectState.Done,
                ProjectType = ProjectType.CourseWork,
                WorkFormat = WorkFormat.Online,
                Language = "Ukrainian",
                MaxContributors = 1
            });
        await dbContext.SaveChangesAsync();

        var service = new ProjectService(dbContext, new FakeJoinRequestService());

        // Act
        var result = await service.GetChatProjectsAsync(author.Id);

        // Assert
        var chat = Assert.Single(result);
        Assert.Equal("Active Chat", chat.Title);
    }
}
