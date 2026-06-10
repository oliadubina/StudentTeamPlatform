using Microsoft.AspNetCore.Mvc;
using StudentTeamPlatform.Api.Services;

namespace StudentTeamPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JoinRequestController: ControllerBase
    {
        private readonly IJoinRequestService _joinRequestService;
        public JoinRequestController(IJoinRequestService joinRequestService)
        {
            _joinRequestService = joinRequestService;
        }
        [HttpGet("")]
    }
}
