using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Services.ArtistServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService;

        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        [HttpGet]
        public async Task<IActionResult> ArtistList()
        {
            var values = await _artistService.GetAllArtists();
            return Ok(values);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
                return NotFound("Sanatçı bulunamadı.");

            return Ok(artist);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArtist(CreateArtistDto createArtistDto)
        {
            await _artistService.CreateArtistAsync(createArtistDto);
            return Ok("İşlem Başarılı");
        }

        // Takip et / bırak (toggle) - login zorunlu
        [HttpPost("{id}/toggle-follow")]
        [Authorize]
        public async Task<IActionResult> ToggleFollow(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isNowFollowing = await _artistService.ToggleFollowAsync(userId, id);
            return Ok(new { following = isNowFollowing });
        }

        // Giriş yapmış kullanıcının takip ettiği sanatçı ID'leri
        [HttpGet("following")]
        [Authorize]
        public async Task<IActionResult> GetFollowing()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ids = await _artistService.GetFollowingArtistIdsAsync(userId);
            return Ok(ids);
        }

        // Giriş yapmış kullanıcının takip ettiği sanatçıların tam bilgisi (Takip Ettiklerim sayfası için)
        [HttpGet("followed")]
        [Authorize]
        public async Task<IActionResult> GetFollowedArtists()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var artists = await _artistService.GetFollowedArtistsAsync(userId);
            return Ok(artists);
        }
    }
}
