using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;

namespace JwtDayMusic.WebApi.Services.SongServices
{
    public interface ISongService
    {
        Task<List<ResultSongDto>> GetAllSongsAsync();
        Task<List<ResultSongDto>> GetByArtistIdAsync(int artistId);
        Task<List<GenreSummaryDto>> GetGenreSummaryAsync();
        Task<List<ResultSongDto>> GetByGenreAsync(string genre);
        Task<Song?> GetByIdAsync(int songId);

        // Favori sistemi
        Task<bool> ToggleFavoriteAsync(string userId, int songId);
        Task<List<int>> GetFavoriteSongIdsAsync(string userId);
        Task<List<ResultSongDto>> GetFavoritedSongsAsync(string userId);
    }
}
