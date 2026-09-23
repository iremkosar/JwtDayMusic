namespace JwtDayMusic.WebApi.Entities
{
    public class PlaylistSong
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public DateTime AddedDate { get; set; }

        public Playlist Playlist { get; set; }
        public Song Song { get; set; }
    }
}
