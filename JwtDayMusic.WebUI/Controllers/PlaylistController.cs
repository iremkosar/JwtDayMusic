using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace JwtDayMusic.WebUI.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlaylistController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken) return RedirectToAction("SignIn", "Login");

            var response = await client.GetAsync("https://localhost:7180/api/Playlist");

            var playlists = new List<ResultPlaylistDto>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                playlists = JsonConvert.DeserializeObject<List<ResultPlaylistDto>>(json) ?? new List<ResultPlaylistDto>();
            }

            return View(playlists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken) return RedirectToAction("SignIn", "Login");

            var response = await client.GetAsync($"https://localhost:7180/api/Playlist/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var details = JsonConvert.DeserializeObject<PlaylistDetailsDto>(json);

            var viewModel = new PlaylistDetailsViewModel
            {
                Id = details?.Id ?? id,
                Name = details?.Name ?? "Playlist",
                Songs = (details?.Songs ?? new List<ResultSongDto>()).Select(dto => new SongViewModel
                {
                    SongId = dto.SongId,
                    Title = dto.Title,
                    ArtistName = dto.ArtistName,
                    CoverImageUrl = dto.CoverImageUrl,
                    Duration = dto.Duration,
                    Category = dto.Genre,
                    Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage
                }).ToList()
            };

            return View(viewModel);
        }

        // Şarkı kartındaki "playlist'e ekle" menüsü açıldığında, kullanıcının playlistlerini getirir
        [HttpGet]
        public async Task<IActionResult> MyPlaylistsJson()
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken) return Json(new List<ResultPlaylistDto>());

            var response = await client.GetAsync("https://localhost:7180/api/Playlist");
            if (!response.IsSuccessStatusCode) return Json(new List<ResultPlaylistDto>());

            var json = await response.Content.ReadAsStringAsync();
            var playlists = JsonConvert.DeserializeObject<List<ResultPlaylistDto>>(json) ?? new List<ResultPlaylistDto>();

            return Json(playlists);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken)
                return Json(new { success = false, message = "Playlist oluşturmak için giriş yapmalısınız." });

            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Playlist adı boş olamaz." });

            var body = new StringContent(JsonConvert.SerializeObject(new { name }), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7180/api/Playlist", body);

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Playlist oluşturulamadı." });

            var json = await response.Content.ReadAsStringAsync();
            var playlist = JsonConvert.DeserializeObject<ResultPlaylistDto>(json);

            return Json(new { success = true, id = playlist?.Id, name = playlist?.Name });
        }

        [HttpPost]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken)
                return Json(new { success = false, message = "Playlist'e eklemek için giriş yapmalısınız." });

            var response = await client.PostAsync($"https://localhost:7180/api/Playlist/{playlistId}/songs/{songId}", null);

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Şarkı eklenemedi." });

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken)
                return Json(new { success = false, message = "Giriş yapmalısınız." });

            var response = await client.DeleteAsync($"https://localhost:7180/api/Playlist/{playlistId}/songs/{songId}");

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Şarkı çıkarılamadı." });

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken)
                return Json(new { success = false, message = "Giriş yapmalısınız." });

            var response = await client.DeleteAsync($"https://localhost:7180/api/Playlist/{id}");

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Playlist silinemedi." });

            return Json(new { success = true });
        }

        private HttpClient GetAuthorizedClient(out bool hasToken)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            var client = _httpClientFactory.CreateClient();

            if (string.IsNullOrEmpty(token))
            {
                hasToken = false;
                return client;
            }

            token = token.Trim().Replace("\"", "");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            hasToken = true;
            return client;
        }
    }
}
