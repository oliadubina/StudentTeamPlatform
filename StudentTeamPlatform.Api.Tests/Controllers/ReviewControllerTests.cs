using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Controllers;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Controllers;

public class ReviewControllerTests
{
    [Fact]
    public async Task GetPendingReviewsAsync_AuthenticatedUser_ReturnsOkWithPendingReviews()
    {
        // Arrange
        var pendingReviews = new List<PendingReviewUserDTO>
        {
            new() { UserId = 1, FullName = "Student" }
        };
        var controller = new ReviewController(new FakeReviewService
        {
            PendingReviews = pendingReviews
        });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.GetPendingReviewsAsync(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pendingReviews, okResult.Value);
    }

    [Fact]
    public async Task CreateReviewAsync_ServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ReviewController(new FakeReviewService { CreateResult = false });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.CreateReviewAsync(new CreateReviewDTO());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReviewAsync_ServiceReturnsTrue_ReturnsOk()
    {
        // Arrange
        var service = new FakeReviewService { CreateResult = true };
        var controller = new ReviewController(service);
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.CreateReviewAsync(new CreateReviewDTO { ProjectId = 5 });

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(7, service.LastUserId);
    }

    private sealed class FakeReviewService : IReviewService
    {
        public List<PendingReviewUserDTO> PendingReviews { get; set; } = new();
        public List<ReviewResponseDTO> ProjectReviews { get; set; } = new();
        public bool CreateResult { get; set; } = true;
        public int? LastUserId { get; private set; }

        public Task<List<PendingReviewUserDTO>> GetPendingReviewsAsync(int projectId, int reviewerId)
        {
            LastUserId = reviewerId;
            return Task.FromResult(PendingReviews);
        }

        public Task<List<ReviewResponseDTO>> GetProjectReviewsAsync(int projectId, int currentUserId)
        {
            LastUserId = currentUserId;
            return Task.FromResult(ProjectReviews);
        }

        public Task<bool> CreateReviewAsync(CreateReviewDTO createReviewDTO, int reviewerId)
        {
            LastUserId = reviewerId;
            return Task.FromResult(CreateResult);
        }
    }
}
