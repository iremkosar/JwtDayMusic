using JwtDayMusic.WebUI.Dtos;

namespace JwtDayMusic.WebUI.Models
{
    public class SongViewModel
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string CoverImageUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public string Category { get; set; }

        // "Gold", "Premium", "New", "Trending", or null
        public string Badge { get; set; }
       
    }
}
