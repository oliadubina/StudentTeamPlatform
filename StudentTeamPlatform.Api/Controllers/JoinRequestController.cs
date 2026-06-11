using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;

namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Захищаємо весь контролер, бо анонімам тут робити нічого
    public class JoinRequestController : ControllerBase
    {
        private readonly IJoinRequestService _joinRequestService;

        public JoinRequestController(IJoinRequestService joinRequestService)
        {
            _joinRequestService = joinRequestService;
        }

        // Хелпер для отримання Id з токена
        private string? GetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // 1. Подача заявки студентом (Тепер з roleId!)
        [HttpPost("apply/{projectId}/role/{roleId}")]
        public async Task<IActionResult> ApplyForProject(int projectId, int roleId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            // Викликаємо оновлений метод сервісу (де ми додали перевірку на roleId)
            bool isApplied = await _joinRequestService.ApplyForProjectAsync(projectId, parsedUserId, roleId);

            if (!isApplied)
            {
                return BadRequest("Не вдалося подати заявку. Можливо, ви вже подали її, місця на цю роль закінчилися, або ви є автором.");
            }

            return Ok(new { message = "Заявку успішно надіслано!" });
        }

        // 2. Перегляд заявок автором проєкту
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectRequests(int projectId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            var requests = await _joinRequestService.GetProjectRequestsAsync(projectId, parsedUserId);
            return Ok(requests);
        }

        // 3. Відповідь на заявку (Прийняти / Відхилити)
        [HttpPut("{requestId}/respond")]
        public async Task<IActionResult> RespondToRequest(int requestId, [FromQuery] RequestStatus status)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            // Перевіряємо, чи автор не намагається передати статус Pending
            if (status == RequestStatus.Pending)
            {
                return BadRequest("Неможливо змінити статус назад на Pending.");
            }

            bool isResponded = await _joinRequestService.RespondToRequestAsync(requestId, parsedUserId, status);

            if (!isResponded)
            {
                return BadRequest("Помилка обробки заявки. Перевірте, чи ви є автором, чи є вільні місця та чи заявка ще не оброблена.");
            }

            return Ok(new { message = $"Заявку успішно переведено в статус: {status}" });
        }

        // 4. Отримання студентом своїх заявок
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            // Цей метод ми обговорювали раніше (StudentSpaceRequestDTO)
            var myRequests = await _joinRequestService.GetMySubmittedRequestsAsync(parsedUserId);
            return Ok(myRequests);
        }

        // 5. Скасування студентом своєї заявки
        [HttpDelete("{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            bool isCancelled = await _joinRequestService.CancelRequestAsync(requestId, parsedUserId);

            if (!isCancelled)
            {
                return BadRequest("Неможливо скасувати заявку. Можливо, вона вже оброблена автором.");
            }

            return Ok(new { message = "Заявку успішно скасовано." });
        }
    }
}