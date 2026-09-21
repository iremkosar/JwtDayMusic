using JwtDayMusic.WebUI.Dtos;

namespace JwtDayMusic.WebUI.Models
{
    public class ArtistDetailsViewModel
    {
        public ResultArtistDto Artist { get; set; }
        public List<SongViewModel> Songs { get; set; } = new();
    }
}
