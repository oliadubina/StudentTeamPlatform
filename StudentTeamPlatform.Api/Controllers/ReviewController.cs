using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Services;
using System.Security.Claims;

namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int? GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }

        [HttpGet("project/{projectId}/pending")]
        public async Task<IActionResult> GetPendingReviewsAsync(int projectId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Некоректний токен");

            var pendingReviews = await _reviewService.GetPendingReviewsAsync(projectId, userId.Value);
            return Ok(pendingReviews);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectReviewsAsync(int projectId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Некоректний токен");

            var reviews = await _reviewService.GetProjectReviewsAsync(projectId, userId.Value);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReviewAsync(CreateReviewDTO createReviewDTO)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Некоректний токен");

            var isCreated = await _reviewService.CreateReviewAsync(createReviewDTO, userId.Value);
            if (!isCreated)
            {
                return BadRequest("Не вдалося залишити відгук. Перевірте, чи проєкт завершено і чи відгук ще не створено.");
            }

            return Ok(new { message = "Відгук успішно збережено" });
        }
    }
}
