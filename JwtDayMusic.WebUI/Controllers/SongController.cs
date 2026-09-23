using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtDayMusic.WebUI.Controllers
{
    public class SongController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SongController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7180/api/Song");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<SongViewModel>());
            }

            var jsonData = await response.Content.ReadAsStringAsync();
            var songs = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonData) ?? new List<ResultSongDto>();

            var favoriteIds = await GetFavoriteIdsAsync();
            var viewModel = songs.Select(dto => MapToViewModel(dto, favoriteIds)).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Play(int songId)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            token = token?.Trim().Replace("\"", "");

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Şarkı oynatmak için giriş yapmalısınız." });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7180/api/Song/play/{songId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Json(new { success = false, message = "Bu şarkıya erişim için üyeliğinizi yükseltmeniz gerekiyor." });
            }

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Şarkı oynatılamadı." });
            }

            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PlaySongResultDto>(jsonData);

            return Json(new { success = true, audioUrl = result?.AudioUrl, title = result?.Title });
        }

        // Kart üzerindeki kalp butonundan AJAX ile çağrılır
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int songId)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Favorilere eklemek için giriş yapmalısınız." });
            }

            token = token.Trim().Replace("\"", "");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"https://localhost:7180/api/Song/{songId}/toggle-favorite", null);

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "İşlem gerçekleştirilemedi." });
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ToggleFavoriteResultDto>(json);

            return Json(new { success = true, favorited = result?.Favorited ?? false });
        }

        private async Task<HashSet<int>> GetFavoriteIdsAsync()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return new HashSet<int>();

            token = token.Trim().Replace("\"", "");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7180/api/Song/favorites");
            if (!response.IsSuccessStatusCode) return new HashSet<int>();

            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonConvert.DeserializeObject<List<int>>(json) ?? new List<int>();
            return ids.ToHashSet();
        }

        private static SongViewModel MapToViewModel(ResultSongDto dto, HashSet<int> favoriteIds)
        {
            return new SongViewModel
            {
                SongId = dto.SongId,
                Title = dto.Title,
                ArtistName = dto.ArtistName,
                CoverImageUrl = dto.CoverImageUrl,
                Duration = dto.Duration,
                Category = dto.Genre,
                Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage,
                IsFavorited = favoriteIds.Contains(dto.SongId)
            };
        }
    }
}
