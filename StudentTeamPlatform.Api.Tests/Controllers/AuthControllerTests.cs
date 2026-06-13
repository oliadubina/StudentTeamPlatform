using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Controllers;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Services;

namespace StudentTeamPlatform.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task RefreshToken_ValidRefreshToken_ReturnsOkWithTokenPair()
    {
        // Arrange
        var authResponse = new AuthResponseDTO
        {
            Token = "access-token",
            RefreshToken = "refresh-token"
        };
        var controller = new AuthController(new FakeAuthService(authResponse));

        // Act
        var result = await controller.RefreshToken(new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<AuthResponseDTO>(okResult.Value);
        Assert.Equal("access-token", value.Token);
        Assert.Equal("refresh-token", value.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new AuthController(new FakeAuthService(null));

        // Act
        var result = await controller.RefreshToken(new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        });

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    private sealed class FakeAuthService : IAuthService
    {
        private readonly AuthResponseDTO? _refreshTokenResult;

        public FakeAuthService(AuthResponseDTO? refreshTokenResult)
        {
            _refreshTokenResult = refreshTokenResult;
        }

        public Task<AuthResponseDTO?> Register(RegisterRequest registerRequest)
        {
            return Task.FromResult<AuthResponseDTO?>(null);
        }

        public Task<AuthResponseDTO?> Login(LoginRequest loginRequest)
        {
            return Task.FromResult<AuthResponseDTO?>(null);
        }

        public Task<AuthResponseDTO?> RefreshToken(string refreshToken)
        {
            return Task.FromResult(_refreshTokenResult);
        }
    }
}
