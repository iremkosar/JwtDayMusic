using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtDayMusic.WebUI.Controllers
{
    public class HistoryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HistoryController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken) return RedirectToAction("SignIn", "Login");

            var response = await client.GetAsync("https://localhost:7180/api/History");

            var songs = new List<SongViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonConvert.DeserializeObject<List<ResultSongDto>>(json) ?? new List<ResultSongDto>();

                songs = dtos.Select(dto => new SongViewModel
                {
                    SongId = dto.SongId,
                    Title = dto.Title,
                    ArtistName = dto.ArtistName,
                    CoverImageUrl = dto.CoverImageUrl,
                    Duration = dto.Duration,
                    Category = dto.Genre,
                    Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage
                }).ToList();
            }

            return View(songs);
        }

        public async Task<IActionResult> Recommendations()
        {
            var client = GetAuthorizedClient(out var hasToken);
            if (!hasToken) return RedirectToAction("SignIn", "Login");

            var response = await client.GetAsync("https://localhost:7180/api/History/recommendations");

            var songs = new List<SongViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonConvert.DeserializeObject<List<RecommendedSongDto>>(json) ?? new List<RecommendedSongDto>();

                songs = dtos.Select(dto => new SongViewModel
                {
                    SongId = dto.SongId,
                    Title = dto.Title,
                    ArtistName = dto.ArtistName,
                    CoverImageUrl = dto.CoverImageUrl,
                    Duration = dto.Duration,
                    Category = dto.Genre,
                    Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage
                }).ToList();
            }

            return View(songs);
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
