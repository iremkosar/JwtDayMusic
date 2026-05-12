namespace JwtDayMusic.WebApi.Entities
{
    public class Song
    {
        public int SongId { get; set; }

        public string Title { get; set; }

        public string CoverImageUrl { get; set; }

        public string AudioUrl { get; set; }

        public TimeSpan Duration { get; set; }

        public long PlayCount { get; set; }

        public bool IsPremium { get; set; }

        public DateTime ReleaseDate { get; set; }
      
        public int ArtistId { get; set; }
    
        public Artist Artist { get; set; }
    }
}
