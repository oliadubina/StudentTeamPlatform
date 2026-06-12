using StudentTeamPlatform.Api.DTO;

namespace StudentTeamPlatform.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> Register(RegisterRequest registerRequest);
        Task<AuthResponseDTO?> Login(LoginRequest loginRequest);
        Task<AuthResponseDTO?> RefreshToken(string refreshToken);
    }
}
