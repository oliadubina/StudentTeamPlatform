using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Services;

public class JoinRequestServiceTests
{
    [Fact]
    public async Task ApplyForProjectAsync_ValidRequest_CreatesPendingJoinRequest()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var student = new User { FullName = "Student", Email = "student@test.com" };
        var project = new Project
        {
            Title = "Course Project",
            Description = "Description",
            Author = author,
            AuthorId = author.Id,
            MaxContributors = 1,
            ProjectState = ProjectState.SearchTeam,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            ProjectRoles = new List<ProjectRole>
            {
                new() { Name = "Frontend", SlotsCount = 1 }
            }
        };

        dbContext.Users.AddRange(author, student);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var roleId = project.ProjectRoles.Single().Id;
        var service = new JoinRequestService(dbContext);

        // Act
        var result = await service.ApplyForProjectAsync(project.Id, student.Id, roleId);

        // Assert
        Assert.True(result);

        var request = await dbContext.JoinRequests.SingleAsync();
        Assert.Equal(project.Id, request.ProjectId);
        Assert.Equal(student.Id, request.StudentId);
        Assert.Equal(roleId, request.ProjectRoleId);
        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    [Fact]
    public async Task ApplyForProjectAsync_DuplicateRequest_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var student = new User { FullName = "Student", Email = "student@test.com" };
        var role = new ProjectRole { Name = "Backend", SlotsCount = 1 };
        var project = new Project
        {
            Title = "Course Project",
            Description = "Description",
            Author = author,
            MaxContributors = 1,
            ProjectState = ProjectState.SearchTeam,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            ProjectRoles = new List<ProjectRole> { role }
        };

        dbContext.Users.AddRange(author, student);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        dbContext.JoinRequests.Add(new JoinRequest
        {
            ProjectId = project.Id,
            StudentId = student.Id,
            ProjectRoleId = role.Id,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new JoinRequestService(dbContext);

        // Act
        var result = await service.ApplyForProjectAsync(project.Id, student.Id, role.Id);

        // Assert
        Assert.False(result);
        Assert.Equal(1, await dbContext.JoinRequests.CountAsync());
    }

    [Fact]
    public async Task RespondToRequestAsync_AcceptsLastRoleSlot_AddsContributorAndMovesProjectToInProcess()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var student = new User { FullName = "Student", Email = "student@test.com" };
        var role = new ProjectRole { Name = "Frontend", SlotsCount = 1 };
        var project = new Project
        {
            Title = "Course Project",
            Description = "Description",
            Author = author,
            MaxContributors = 1,
            ProjectState = ProjectState.SearchTeam,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            ProjectRoles = new List<ProjectRole> { role }
        };

        dbContext.Users.AddRange(author, student);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var request = new JoinRequest
        {
            ProjectId = project.Id,
            StudentId = student.Id,
            ProjectRoleId = role.Id,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.JoinRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var service = new JoinRequestService(dbContext);

        // Act
        var result = await service.RespondToRequestAsync(request.Id, author.Id, RequestStatus.Accepted);

        // Assert
        Assert.True(result);

        var updatedProject = await dbContext.Projects
            .Include(p => p.Contributors)
            .Include(p => p.ProjectRoles)
            .SingleAsync(p => p.Id == project.Id);

        Assert.Contains(updatedProject.Contributors, contributor => contributor.Id == student.Id);
        Assert.Equal(0, updatedProject.ProjectRoles.Single().SlotsCount);
        Assert.Equal(ProjectState.InProcess, updatedProject.ProjectState);
        Assert.Equal(RequestStatus.Accepted, request.Status);
    }
}
