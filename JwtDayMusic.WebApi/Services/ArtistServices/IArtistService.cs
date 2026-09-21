using JwtDayMusic.WebApi.Dtos;

namespace JwtDayMusic.WebApi.Services.ArtistServices
{
    public interface IArtistService
    {
        Task<List<ResultArtistDto>> GetAllArtists();
        Task<ResultArtistDto?> GetArtistByIdAsync(int id);
        Task CreateArtistAsync(CreateArtistDto createArtistDto);

        // Takip sistemi
        Task<bool> ToggleFollowAsync(string userId, int artistId);
        Task<List<int>> GetFollowingArtistIdsAsync(string userId);
        Task<List<ResultArtistDto>> GetFollowedArtistsAsync(string userId);
    }
}
