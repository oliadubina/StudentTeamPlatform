using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_DoneProjectAndTeamMembers_CreatesReview()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var contributor = new User { FullName = "Contributor", Email = "contributor@test.com" };
        var project = new Project
        {
            Title = "Completed Project",
            Description = "Description",
            Author = author,
            Contributors = new List<User> { contributor },
            MaxContributors = 1,
            ProjectState = ProjectState.Done,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian"
        };

        dbContext.Users.AddRange(author, contributor);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        // Act
        var result = await service.CreateReviewAsync(new CreateReviewDTO
        {
            ProjectId = project.Id,
            RevieweeId = contributor.Id,
            Rating = 5,
            Comment = "Great teammate"
        }, author.Id);

        // Assert
        Assert.True(result);

        var review = await dbContext.ProjectReviews.SingleAsync();
        Assert.Equal(project.Id, review.ProjectId);
        Assert.Equal(author.Id, review.ReviewerId);
        Assert.Equal(contributor.Id, review.RevieweeId);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Great teammate", review.Comment);
    }

    [Fact]
    public async Task CreateReviewAsync_ProjectNotDone_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var contributor = new User { FullName = "Contributor", Email = "contributor@test.com" };
        var project = new Project
        {
            Title = "Active Project",
            Description = "Description",
            Author = author,
            Contributors = new List<User> { contributor },
            MaxContributors = 1,
            ProjectState = ProjectState.InProcess,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian"
        };

        dbContext.Users.AddRange(author, contributor);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        // Act
        var result = await service.CreateReviewAsync(new CreateReviewDTO
        {
            ProjectId = project.Id,
            RevieweeId = contributor.Id,
            Rating = 5,
            Comment = "Great teammate"
        }, author.Id);

        // Assert
        Assert.False(result);
        Assert.Empty(dbContext.ProjectReviews);
    }

    [Fact]
    public async Task CreateReviewAsync_DuplicateReview_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var contributor = new User { FullName = "Contributor", Email = "contributor@test.com" };
        var project = new Project
        {
            Title = "Completed Project",
            Description = "Description",
            Author = author,
            Contributors = new List<User> { contributor },
            MaxContributors = 1,
            ProjectState = ProjectState.Done,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian"
        };

        dbContext.Users.AddRange(author, contributor);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        dbContext.ProjectReviews.Add(new ProjectReview
        {
            ProjectId = project.Id,
            ReviewerId = author.Id,
            RevieweeId = contributor.Id,
            Rating = 4,
            Comment = "Already reviewed",
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        // Act
        var result = await service.CreateReviewAsync(new CreateReviewDTO
        {
            ProjectId = project.Id,
            RevieweeId = contributor.Id,
            Rating = 5,
            Comment = "Second review"
        }, author.Id);

        // Assert
        Assert.False(result);
        Assert.Equal(1, await dbContext.ProjectReviews.CountAsync());
    }

    [Fact]
    public async Task GetPendingReviewsAsync_ReviewerAlreadyReviewedOneMember_ReturnsOnlyRemainingMembers()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var author = new User { FullName = "Author", Email = "author@test.com" };
        var contributor = new User { FullName = "Contributor", Email = "contributor@test.com" };
        var secondContributor = new User { FullName = "Second", Email = "second@test.com" };
        var project = new Project
        {
            Title = "Completed Project",
            Description = "Description",
            Author = author,
            Contributors = new List<User> { contributor, secondContributor },
            MaxContributors = 2,
            ProjectState = ProjectState.Done,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian"
        };

        dbContext.Users.AddRange(author, contributor, secondContributor);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        dbContext.ProjectReviews.Add(new ProjectReview
        {
            ProjectId = project.Id,
            ReviewerId = author.Id,
            RevieweeId = contributor.Id,
            Rating = 5,
            Comment = "Done",
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new ReviewService(dbContext);

        // Act
        var result = await service.GetPendingReviewsAsync(project.Id, author.Id);

        // Assert
        var pendingReview = Assert.Single(result);
        Assert.Equal(secondContributor.Id, pendingReview.UserId);
        Assert.Equal(secondContributor.FullName, pendingReview.FullName);
    }
}
