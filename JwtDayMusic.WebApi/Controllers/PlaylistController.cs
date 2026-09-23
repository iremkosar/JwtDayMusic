using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Services.PlaylistServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Playlistler tamamen kişisel, tüm action'lar login zorunlu
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetMyPlaylists()
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();
            var playlists = await _playlistService.GetMyPlaylistsAsync(UserId);
            return Ok(playlists);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePlaylistDto dto)
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();
            if (string.IsNullOrWhiteSpace(dto?.Name))
                return BadRequest("Playlist adı boş olamaz.");

            var playlist = await _playlistService.CreateAsync(UserId, dto.Name.Trim());
            return Ok(playlist);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var details = await _playlistService.GetDetailsAsync(UserId, id);
            if (details == null) return NotFound("Playlist bulunamadı.");

            return Ok(details);
        }

        [HttpPost("{id}/songs/{songId}")]
        public async Task<IActionResult> AddSong(int id, int songId)
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var success = await _playlistService.AddSongAsync(UserId, id, songId);
            if (!success) return BadRequest("Şarkı eklenemedi.");

            return Ok();
        }

        [HttpDelete("{id}/songs/{songId}")]
        public async Task<IActionResult> RemoveSong(int id, int songId)
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var success = await _playlistService.RemoveSongAsync(UserId, id, songId);
            if (!success) return BadRequest("Şarkı çıkarılamadı.");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (string.IsNullOrEmpty(UserId)) return Unauthorized();

            var success = await _playlistService.DeleteAsync(UserId, id);
            if (!success) return NotFound("Playlist bulunamadı.");

            return Ok();
        }
    }
}
