namespace JwtDayMusic.WebUI.Models
{
    public class PlaylistDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SongViewModel> Songs { get; set; } = new();
    }
}
