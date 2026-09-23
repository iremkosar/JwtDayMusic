namespace JwtDayMusic.WebApi.Dtos
{
    public class PlaylistDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ResultSongDto> Songs { get; set; } = new();
    }
}
