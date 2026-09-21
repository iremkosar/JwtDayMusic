namespace JwtDayMusic.WebApi.Entities
{
    public class Artist
    {       
            public int ArtistId { get; set; }

            public string Name { get; set; }

            public string ImageUrl { get; set; }

            public string Bio { get; set; }

           public string Genres { get; set; } 

           public long MonthlyListeners { get; set; }

            public bool IsVerified { get; set; }

            public DateTime CreatedDate { get; set; }
         
            public List<Song> Songs { get; set; }        
    }
}
