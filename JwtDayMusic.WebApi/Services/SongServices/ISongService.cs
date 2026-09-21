using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;

namespace JwtDayMusic.WebApi.Services.SongServices
{
    public interface ISongService
    {
        Task<List<ResultSongDto>> GetAllSongsAsync();
        Task<List<ResultSongDto>> GetByArtistIdAsync(int artistId);
        Task<Song?> GetByIdAsync(int songId);
    }
}
