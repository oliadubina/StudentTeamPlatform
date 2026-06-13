using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Controllers;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Controllers;

public class JoinRequestControllerTests
{
    [Fact]
    public async Task ApplyForProject_ServiceReturnsTrue_ReturnsOk()
    {
        // Arrange
        var service = new FakeJoinRequestService { ApplyResult = true };
        var controller = new JoinRequestController(service);
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.ApplyForProject(5, 3);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(5, service.LastProjectId);
        Assert.Equal(7, service.LastStudentId);
        Assert.Equal(3, service.LastRoleId);
    }

    [Fact]
    public async Task ApplyForProject_ServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var controller = new JoinRequestController(new FakeJoinRequestService { ApplyResult = false });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.ApplyForProject(5, 3);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RespondToRequest_PendingStatus_ReturnsBadRequest()
    {
        // Arrange
        var controller = new JoinRequestController(new FakeJoinRequestService());
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.RespondToRequest(10, RequestStatus.Pending);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CancelRequest_ServiceReturnsTrue_ReturnsOk()
    {
        // Arrange
        var controller = new JoinRequestController(new FakeJoinRequestService { CancelResult = true });
        ControllerTestHelper.SetUser(controller, 7);

        // Act
        var result = await controller.CancelRequest(10);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
