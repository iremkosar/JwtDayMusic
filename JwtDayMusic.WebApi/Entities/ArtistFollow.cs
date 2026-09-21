namespace JwtDayMusic.WebApi.Entities
{
    public class ArtistFollow
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ArtistId { get; set; }
        public DateTime FollowedDate { get; set; }

        public Artist Artist { get; set; }
    }
}
