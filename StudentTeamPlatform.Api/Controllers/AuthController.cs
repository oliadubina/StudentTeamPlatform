using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService=authService;
        }
        [HttpPost("register")]
        public async Task <IActionResult> Register([FromBody]RegisterRequest registerRequest)
        {
            var authResponse= await _authService.Register(registerRequest);
            if (authResponse==null)
            {
                return BadRequest("Data is wrong");
                
            }
            return Ok(authResponse);
        }
        [HttpPost("login")]
        
        public async Task<IActionResult>Login([FromBody] LoginRequest loginRequest)
        {
            var authResponse = await _authService.Login(loginRequest);
            if (authResponse==null)
            {
                return BadRequest("Email or password aren`t correct");
            }
            return Ok(authResponse);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var authResponse = await _authService.RefreshToken(refreshTokenRequest.RefreshToken);
            if (authResponse == null)
            {
                return Unauthorized("Refresh token is invalid or expired");
            }

            return Ok(authResponse);
        }
    }
}
