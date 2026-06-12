using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentTeamPlatform.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO?> Register(RegisterRequest registerRequest)
        {
            if (await _appDbContext.Users.AnyAsync(u => u.Email == registerRequest.Email))
            {
                return null;
            }

            var user = new User
            {
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                FullName = registerRequest.FullName,
                Role = UserRole.Student
            };

            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();

            var authResponse = CreateAuthResponse(user);
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await _appDbContext.SaveChangesAsync();

            return authResponse;
        }

        public async Task<AuthResponseDTO?> Login(LoginRequest loginRequest)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return null;
            }

            var authResponse = CreateAuthResponse(user);
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _appDbContext.SaveChangesAsync();

            return authResponse;
        }

        public async Task<AuthResponseDTO?> RefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(user =>
                user.RefreshToken == refreshToken &&
                user.RefreshTokenExpiresAt != null &&
                user.RefreshTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                return null;
            }

            var authResponse = CreateAuthResponse(user);
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _appDbContext.SaveChangesAsync();

            return authResponse;
        }

        private AuthResponseDTO CreateAuthResponse(User user)
        {
            return new AuthResponseDTO
            {
                Token = CreateAccessToken(user),
                RefreshToken = CreateRefreshToken()
            };
        }

        private string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
