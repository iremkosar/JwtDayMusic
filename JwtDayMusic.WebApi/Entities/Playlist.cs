namespace JwtDayMusic.WebApi.Entities
{
    public class Playlist
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<PlaylistSong> PlaylistSongs { get; set; }
    }
}
