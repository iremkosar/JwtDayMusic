using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Helpers;
using JwtDayMusic.WebApi.Services.SongServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSongs()
        {
            var songs = await _songService.GetAllSongsAsync();
            return Ok(songs);
        }

        // Belirli bir sanatçıya ait tüm şarkılar (Artist detay sayfası için)
        [HttpGet("by-artist/{artistId}")]
        public async Task<IActionResult> GetByArtist(int artistId)
        {
            var songs = await _songService.GetByArtistIdAsync(artistId);
            return Ok(songs);
        }

        [HttpGet("play/{songId}")]
        [Authorize]
        public async Task<IActionResult> Play(int songId)
        {
            var song = await _songService.GetByIdAsync(songId);
            if (song == null)
                return NotFound("Şarkı bulunamadı.");

            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var highestRole = PackageHierarchy.GetHighestRole(userRoles);

            if (!PackageHierarchy.HasAccess(highestRole, song.RequiredPackage))
            {
                return StatusCode(403, "Bu şarkıya erişim için üyeliğinizi yükseltmeniz gerekiyor.");
            }

            var result = new PlaySongResultDto
            {
                AudioUrl = song.AudioUrl,
                Title = song.Title
            };

            return Ok(result);
        }
    }
}
