namespace JwtDayMusic.WebApi.Dtos
{
    public class RecommendedSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public long PlayCount { get; set; }
        public string RequiredPackage { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }

        // ML.NET modelinin ürettiği tahmini beğeni skoru (yüksek = daha çok önerilir)
        public float Score { get; set; }
    }
}
