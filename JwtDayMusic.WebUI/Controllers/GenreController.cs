using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtDayMusic.WebUI.Controllers
{
    public class GenreController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GenreController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7180/api/Song/genres");

            if (!response.IsSuccessStatusCode)
                return View(new List<GenreSummaryDto>());

            var jsonData = await response.Content.ReadAsStringAsync();
            var genres = JsonConvert.DeserializeObject<List<GenreSummaryDto>>(jsonData) ?? new List<GenreSummaryDto>();

            return View(genres);
        }

        public async Task<IActionResult> GenreSongs(string genre)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7180/api/Song/by-genre/{Uri.EscapeDataString(genre)}");

            var favoriteIds = await GetFavoriteSongIdsAsync();
            var songs = new List<SongViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var songDtos = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonData) ?? new List<ResultSongDto>();

                songs = songDtos.Select(dto => new SongViewModel
                {
                    SongId = dto.SongId,
                    Title = dto.Title,
                    ArtistName = dto.ArtistName,
                    CoverImageUrl = dto.CoverImageUrl,
                    Duration = dto.Duration,
                    Category = dto.Genre,
                    Badge = dto.RequiredPackage == "Basic" ? null : dto.RequiredPackage,
                    IsFavorited = favoriteIds.Contains(dto.SongId)
                }).ToList();
            }

            ViewBag.GenreName = genre;
            return View(songs);
        }

        private async Task<HashSet<int>> GetFavoriteSongIdsAsync()
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
    }
}
