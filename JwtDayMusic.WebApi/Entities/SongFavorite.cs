namespace JwtDayMusic.WebApi.Entities
{
    public class SongFavorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SongId { get; set; }
        public DateTime FavoritedDate { get; set; }

        public Song Song { get; set; }
    }
}
