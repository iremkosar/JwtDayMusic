using JwtDayMusic.WebUI.Dtos;
using JwtDayMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtDayMusic.WebUI.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArtistController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> ArtistList()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7180/api/Artist");

            var artists = new List<ResultArtistDto>();
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                artists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonData) ?? new List<ResultArtistDto>();
            }

            // Giriş yapmış kullanıcının takip ettiği sanatçıları da alıp View'a taşıyoruz
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            var followingIds = new HashSet<int>();

            if (!string.IsNullOrEmpty(token))
            {
                token = token.Trim().Replace("\"", "");
                var authClient = _httpClientFactory.CreateClient();
                authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var followingResponse = await authClient.GetAsync("https://localhost:7180/api/Artist/following");
                if (followingResponse.IsSuccessStatusCode)
                {
                    var followingJson = await followingResponse.Content.ReadAsStringAsync();
                    var ids = JsonConvert.DeserializeObject<List<int>>(followingJson) ?? new List<int>();
                    followingIds = ids.ToHashSet();
                }
            }

            ViewBag.FollowingIds = followingIds;
            return View(artists);
        }

        public async Task<IActionResult> ArtistDetails(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var artistTask = client.GetAsync($"https://localhost:7180/api/Artist/{id}");
            var songsTask = client.GetAsync($"https://localhost:7180/api/Song/by-artist/{id}");

            await Task.WhenAll(artistTask, songsTask);

            var artistResponse = artistTask.Result;
            var songsResponse = songsTask.Result;

            if (!artistResponse.IsSuccessStatusCode)
                return NotFound();

            var artistJson = await artistResponse.Content.ReadAsStringAsync();
            var artist = JsonConvert.DeserializeObject<ResultArtistDto>(artistJson);

            var songs = new List<SongViewModel>();
            if (songsResponse.IsSuccessStatusCode)
            {
                var songsJson = await songsResponse.Content.ReadAsStringAsync();
                var songDtos = JsonConvert.DeserializeObject<List<ResultSongDto>>(songsJson) ?? new List<ResultSongDto>();

                songs = songDtos.Select(dto => new SongViewModel
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

            var viewModel = new ArtistDetailsViewModel
            {
                Artist = artist,
                Songs = songs
            };

            return View(viewModel);
        }

        // Kullanıcının takip ettiği sanatçılar sayfası
        public async Task<IActionResult> Following()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Login");
            }

            token = token.Trim().Replace("\"", "");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7180/api/Artist/followed");

            var artists = new List<ResultArtistDto>();
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                artists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonData) ?? new List<ResultArtistDto>();
            }

            return View(artists);
        }

        // Kart üzerindeki "Takip Et" butonundan AJAX ile çağrılır
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(int artistId)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Takip etmek için giriş yapmalısınız." });
            }

            token = token.Trim().Replace("\"", "");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"https://localhost:7180/api/Artist/{artistId}/toggle-follow", null);

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "İşlem gerçekleştirilemedi." });
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ToggleFollowResultDto>(json);

            return Json(new { success = true, following = result?.Following ?? false });
        }
    }
}
