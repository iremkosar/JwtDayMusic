namespace JwtDayMusic.WebUI.Dtos
{
    public class ResultPlaylistDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SongCount { get; set; }
        public string CoverImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
