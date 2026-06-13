using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Controllers;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Controllers;

public class ProfileControllerTests
{
    [Fact]
    public async Task GetUserProfileAsync_ExistingAuthenticatedUser_ReturnsOkWithProfile()
    {
        // Arrange
        var profile = new UserProfileResponseDTO
        {
            FullName = "Student",
            EmailAddress = "student@test.com"
        };
        var controller = new ProfileController(new FakeProfileService(profile, true));
        ControllerTestHelper.SetUser(controller, 10);

        // Act
        var result = await controller.GetUserProfileAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(profile, okResult.Value);
    }

    [Fact]
    public async Task GetUserProfileAsync_MissingUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new ProfileController(new FakeProfileService(null, true));
        ControllerTestHelper.SetAnonymousUser(controller);

        // Act
        var result = await controller.GetUserProfileAsync();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetExternalUserProfileAsync_ProfileNotFound_ReturnsNotFound()
    {
        // Arrange
        var controller = new ProfileController(new FakeProfileService(null, true));

        // Act
        var result = await controller.GetExternalUserProfileAsync(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var controller = new ProfileController(new FakeProfileService(null, false));
        ControllerTestHelper.SetUser(controller, 10);

        // Act
        var result = await controller.UpdateUserProfileAsync(new UpdateUserProfileDTO());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private sealed class FakeProfileService : IProfileService
    {
        private readonly UserProfileResponseDTO? _profile;
        private readonly bool _updateResult;

        public FakeProfileService(UserProfileResponseDTO? profile, bool updateResult)
        {
            _profile = profile;
            _updateResult = updateResult;
        }

        public Task<UserProfileResponseDTO> GetUserProfileAsync(int userId)
        {
            return Task.FromResult(_profile!);
        }

        public Task<bool> UpdateProfileAsync(UpdateUserProfileDTO updateUserProfile, int id)
        {
            return Task.FromResult(_updateResult);
        }
    }
}
