namespace JwtDayMusic.WebApi.Dtos
{
    public class ResultSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public long PlayCount { get; set; }
        public string RequiredPackage { get; set; }
        public string Genre { get; set; } = "Pop";
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
    }
}
