using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Services.ArtistServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService;

        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }
        [HttpGet]
        public async Task<IActionResult> ArtistList()
        {
            var values=await _artistService.GetAllArtists();
            return Ok(values);
        }
        [HttpPost]
        public async Task<IActionResult> CreateArtist(CreateArtistDto createArtistDto)
        {
            await _artistService.CreateArtistAsync(createArtistDto);
            return Ok("İşlem Başarılı");
        }
    }
}
