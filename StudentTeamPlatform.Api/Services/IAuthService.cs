using StudentTeamPlatform.Api.DTO;

namespace StudentTeamPlatform.Api.Services
{
    public interface IAuthService
    {
        Task<string?> Register(RegisterRequest registerRequest);
        Task<string?> Login(LoginRequest loginRequest);
    }
}
