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

            // Şarkı listesi herkese açık, ama kullanıcı login olmuşsa
            // ileride "favorilerim" gibi kişisel veriler için token'ı yine de ekliyoruz
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                token = token.Trim().Replace("\"", "");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("https://localhost:7180/api/Song");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<SongViewModel>());
            }

            var jsonData = await response.Content.ReadAsStringAsync();
            var songs = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonData) ?? new List<ResultSongDto>();

            var viewModel = songs.Select(MapToViewModel).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Play(int songId)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            token = token?.Trim().Replace("\"", "");

            System.Diagnostics.Debug.WriteLine("=== PLAY ACTION ÇALIŞTI ===");
            System.Diagnostics.Debug.WriteLine("SONG ID: " + songId);
            System.Diagnostics.Debug.WriteLine("TOKEN VAR MI: " + !string.IsNullOrEmpty(token));
            System.Diagnostics.Debug.WriteLine("TOKEN: " + token);

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Şarkı oynatmak için giriş yapmalısınız." });
            }
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"https://localhost:7180/api/Song/play/{songId}");

            System.Diagnostics.Debug.WriteLine("API'DEN DÖNEN STATUS CODE: " + response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Json(new { success = false, message = "Bu şarkıya erişim için üyeliğinizi yükseltmeniz gerekiyor." });
            }
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("HATA BODY: " + errorBody);
                return Json(new { success = false, message = "Şarkı oynatılamadı." });
            }
            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PlaySongResultDto>(jsonData);
            return Json(new { success = true, audioUrl = result?.AudioUrl, title = result?.Title });
        }

        private static SongViewModel MapToViewModel(ResultSongDto dto)
        {
            return new SongViewModel
            {
                SongId = dto.SongId,
                Title = dto.Title,
                ArtistName = dto.ArtistName,
                CoverImageUrl = dto.CoverImageUrl,
                Duration = dto.Duration,
                Category = dto.Genre,
                Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage
            }; 
        }
        [HttpGet]
        public async Task<IActionResult> WhoAmI()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            var rawToken = token;
            token = token?.Trim().Replace("\"", "");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7180/api/Song/whoami");
            var json = await response.Content.ReadAsStringAsync();

            return Json(new
            {
                HasTokenInSession = !string.IsNullOrEmpty(rawToken),
                TokenPreview = rawToken?.Substring(0, Math.Min(30, rawToken?.Length ?? 0)),
                ApiStatusCode = (int)response.StatusCode,
                ApiBody = json
            });
        }

    }
}