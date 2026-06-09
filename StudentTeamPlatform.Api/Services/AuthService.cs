using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace StudentTeamPlatform.Api.Services
{
    public class AuthService: IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        public AuthService(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }
        public async Task<string> Register(RegisterRequest registerRequest)
        {
            if ( await _appDbContext.Users.AnyAsync(u => u.Email==registerRequest.EmailAddress))
            {
                return null;
            }
            
            User user = new User()
            {
                Email = registerRequest.EmailAddress,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                FullName = registerRequest.FullName,           
            };

            _appDbContext.Users.Add(user);
            var result = await _appDbContext.SaveChangesAsync();
            return CreateToken(user);  
        }
        
        public async Task<string> Login(LoginRequest loginRequest)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email==loginRequest.Email);
            if(user==null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                return null;

            return CreateToken(user);
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
