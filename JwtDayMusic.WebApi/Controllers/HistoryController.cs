using JwtDayMusic.WebApi.Services.RecommendationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Geçmiş ve öneriler tamamen kişisel, login zorunlu
    public class HistoryController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public HistoryController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var history = await _recommendationService.GetHistoryAsync(UserId);
            return Ok(history);
        }

        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var recommendations = await _recommendationService.GetRecommendationsAsync(UserId);
            return Ok(recommendations);
        }
    }
}
