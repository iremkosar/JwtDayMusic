using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtDayMusic.WebUI.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavoriteController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Login");
            }

            token = token.Trim().Replace("\"", "");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7180/api/Song/favorited");

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
                    IsFavorited = true // bu sayfadaki her şarkı zaten favori
                }).ToList();
            }

            return View(songs);
        }
    }
}
