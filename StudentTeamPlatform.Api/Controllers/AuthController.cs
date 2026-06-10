using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.DTO;
using Microsoft.AspNetCore.Authorization;
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
            var token= await _authService.Register(registerRequest);
            if (token==null)
            {
                return BadRequest("Data is wrong");
                
            }
            return Ok("Registration has been succeeded");
        }
        [HttpPost("login")]
        
        public async Task<IActionResult>Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _authService.Login(loginRequest);
            if (token==null)
            {
                return BadRequest("Email or password aren`t correct");
            }
            return Ok( token );
        }
        
    }
}
