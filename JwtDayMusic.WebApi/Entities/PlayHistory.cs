namespace JwtDayMusic.WebApi.Entities
{
    public class PlayHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SongId { get; set; }
        public DateTime PlayedDate { get; set; }

        public Song Song { get; set; }
    }
}
