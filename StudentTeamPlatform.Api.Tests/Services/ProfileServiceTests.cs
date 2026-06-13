using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Services;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetUserProfileAsync_ExistingUserWithReviews_ReturnsProfileWithAverageRating()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var reviewer = new User { FullName = "Reviewer", Email = "reviewer@test.com" };
        var reviewee = new User
        {
            FullName = "Reviewee",
            Email = "reviewee@test.com",
            University = "Test University",
            Speciality = "Software Engineering",
            Course = 3,
            PreferredLanguage = "Ukrainian",
            WorkFormat = WorkFormat.Hybrid,
            Skills = new List<Skill>
            {
                new() { Name = "React", Category = "Frontend" }
            }
        };
        var project = new Project
        {
            Title = "Completed Project",
            Description = "Description",
            Author = reviewer,
            ProjectState = ProjectState.Done,
            ProjectType = ProjectType.CourseWork,
            WorkFormat = WorkFormat.Online,
            Language = "Ukrainian",
            MaxContributors = 1
        };

        dbContext.Users.AddRange(reviewer, reviewee);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        dbContext.ProjectReviews.AddRange(
            new ProjectReview
            {
                ProjectId = project.Id,
                ReviewerId = reviewer.Id,
                RevieweeId = reviewee.Id,
                Rating = 5,
                Comment = "Strong teammate",
                CreatedAt = DateTime.UtcNow
            },
            new ProjectReview
            {
                ProjectId = project.Id,
                ReviewerId = reviewer.Id,
                RevieweeId = reviewee.Id,
                Rating = 3,
                Comment = "Good communication",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
        await dbContext.SaveChangesAsync();

        var service = new ProfileService(dbContext);

        // Act
        var result = await service.GetUserProfileAsync(reviewee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Reviewee", result.FullName);
        Assert.Equal(4, result.AverageRating);
        Assert.Equal(2, result.ReviewsCount);
        Assert.Equal(2, result.Reviews.Count);
        Assert.Single(result.Skills!);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingUser_UpdatesProfileAndSkills()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var user = new User
        {
            FullName = "Old Name",
            Email = "student@test.com",
            University = "Old University",
            Speciality = "Old Speciality",
            Course = 1,
            PreferredLanguage = "English",
            WorkFormat = WorkFormat.Online
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new ProfileService(dbContext);
        var updateDto = new UpdateUserProfileDTO
        {
            FullName = "New Name",
            University = "New University",
            Speciality = "Computer Science",
            Course = 4,
            PreferredLanguage = "Ukrainian",
            WorkFormat = WorkFormat.Hybrid,
            Skills = new List<SkillDTO>
            {
                new() { Name = "C#", Category = "Backend" }
            }
        };

        // Act
        var result = await service.UpdateProfileAsync(updateDto, user.Id);

        // Assert
        Assert.True(result);

        var updatedUser = await dbContext.Users.Include(u => u.Skills).SingleAsync(u => u.Id == user.Id);
        Assert.Equal("New Name", updatedUser.FullName);
        Assert.Equal("New University", updatedUser.University);
        Assert.Equal("Computer Science", updatedUser.Speciality);
        Assert.Equal(4, updatedUser.Course);
        Assert.Equal(WorkFormat.Hybrid, updatedUser.WorkFormat);
        Assert.Single(updatedUser.Skills);
        Assert.Equal("C#", updatedUser.Skills.Single().Name);
    }

    [Fact]
    public async Task UpdateProfileAsync_MissingUser_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var service = new ProfileService(dbContext);

        // Act
        var result = await service.UpdateProfileAsync(new UpdateUserProfileDTO(), 404);

        // Assert
        Assert.False(result);
    }
}
