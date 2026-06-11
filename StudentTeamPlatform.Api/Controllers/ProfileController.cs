using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Services;
using StudentTeamPlatform.Api.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
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
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            var profile=await _profileService.GetUserProfileAsync(parsedUserId);
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
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized("Некоректний токен");
            bool isUpdated = await _profileService.UpdateProfileAsync(updateUserProfile, parsedUserId);
            if (!isUpdated)
            {
                return NotFound("Не знайдено користувача з таким Id");
            }
            return Ok(updateUserProfile);
        }
        // Додаємо ендпоінт для перегляду публічного профілю будь-якого студента за Id
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetExternalUserProfileAsync(int userId)
        {
            var profile = await _profileService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("Користувача з таким Id не знайдено");
            }
            return Ok(profile);
        }
    }
}
