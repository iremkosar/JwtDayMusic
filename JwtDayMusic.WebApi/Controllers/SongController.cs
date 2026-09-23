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

        [HttpGet("by-artist/{artistId}")]
        public async Task<IActionResult> GetByArtist(int artistId)
        {
            var songs = await _songService.GetByArtistIdAsync(artistId);
            return Ok(songs);
        }

        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _songService.GetGenreSummaryAsync();
            return Ok(genres);
        }

        [HttpGet("by-genre/{genre}")]
        public async Task<IActionResult> GetByGenre(string genre)
        {
            var songs = await _songService.GetByGenreAsync(genre);
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

        // Favori ekle/çıkar (toggle) - login zorunlu
        [HttpPost("{id}/toggle-favorite")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isNowFavorited = await _songService.ToggleFavoriteAsync(userId, id);
            return Ok(new { favorited = isNowFavorited });
        }

        // Giriş yapmış kullanıcının favori şarkı ID'leri
        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavoriteIds()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ids = await _songService.GetFavoriteSongIdsAsync(userId);
            return Ok(ids);
        }

        // Giriş yapmış kullanıcının favori şarkılarının tam bilgisi (Beğendiklerim sayfası için)
        [HttpGet("favorited")]
        [Authorize]
        public async Task<IActionResult> GetFavoritedSongs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var songs = await _songService.GetFavoritedSongsAsync(userId);
            return Ok(songs);
        }
    }
}
