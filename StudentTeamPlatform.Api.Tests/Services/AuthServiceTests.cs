using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.Tests.TestInfrastructure;

namespace StudentTeamPlatform.Api.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var user = new User
        {
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = UserRole.Student
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new AuthService(dbContext, TestConfigurationFactory.Create());

        // Act
        var result = await service.Login(new LoginRequest
        {
            Email = "student@test.com",
            Password = "Password123"
        });

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal(result.RefreshToken, user.RefreshToken);
        Assert.True(user.RefreshTokenExpiresAt > DateTime.UtcNow);
        Assert.True(user.RefreshTokenExpiresAt <= DateTime.UtcNow.AddDays(7).AddSeconds(5));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsNull()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Users.Add(new User
        {
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = UserRole.Student
        });
        await dbContext.SaveChangesAsync();

        var service = new AuthService(dbContext, TestConfigurationFactory.Create());

        // Act
        var result = await service.Login(new LoginRequest
        {
            Email = "student@test.com",
            Password = "WrongPassword123"
        });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshToken_ValidRefreshToken_RotatesRefreshTokenAndReturnsNewAccessToken()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var user = new User
        {
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = UserRole.Student,
            RefreshToken = "existing-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var service = new AuthService(dbContext, TestConfigurationFactory.Create());

        // Act
        var result = await service.RefreshToken("existing-refresh-token");

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.NotEqual("existing-refresh-token", result.RefreshToken);
        Assert.Equal(result.RefreshToken, user.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ExpiredRefreshToken_ReturnsNull()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Users.Add(new User
        {
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = UserRole.Student,
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await dbContext.SaveChangesAsync();

        var service = new AuthService(dbContext, TestConfigurationFactory.Create());

        // Act
        var result = await service.RefreshToken("expired-refresh-token");

        // Assert
        Assert.Null(result);
    }
}
