using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaManagement.DataRequest;
using PizzaManagement.Service;

namespace PizzaManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User, Admin")]
    public class AIController : ControllerBase
    {
        private readonly RecommendationService _service;

        public AIController(RecommendationService service)
        {
            _service = service;
        }

        [HttpGet("recommend")]
        public async Task<IActionResult> GetRecommendation()
        {
            var UserIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(UserIdClaim) || !int.TryParse(UserIdClaim, out int userId))
            {
                return Unauthorized();
            }
            var results = await _service.GetRecommendationsAsync(userId);

            return Ok(new
            {
                actions = results
            });
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackAction([FromBody] TrackRequest request)
        {
            var UserIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(UserIdClaim) || !int.TryParse(UserIdClaim, out int userId))
            {
                return Unauthorized();
            }

            await _service.TrackActionAsync(userId, request.PizzaId, request.EventType!);

            return Ok(new
            {
                message = "Tracked successfully"
            });
        }
    }
}
