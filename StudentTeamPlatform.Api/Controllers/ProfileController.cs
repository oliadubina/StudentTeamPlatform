using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfileAsync()
        {
            var userId=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Не знайдено Id");
            }
            var profile=await _profileService.GetUserProfileAsync(int.Parse(userId));
            if (profile == null)
            {
                return NotFound("Не знайдено користувача з таким Id");
            }
            return Ok(profile);
        }
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfileAsync(UpdateUserProfileDTO updateUserProfile)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) { return Unauthorized("Не знайдено Id"); }
            bool isUpdated = await _profileService.UpdateProfileAsync(updateUserProfile, int.Parse(userId));
            if (!isUpdated)
            {
                return NotFound("Не знайдено користувача з таким Id");
            }
            return Ok(updateUserProfile);
        }

    }
}
