using JwtDayMusic.WebApi.Dtos;

namespace JwtDayMusic.WebApi.Services.PlaylistServices
{
    public interface IPlaylistService
    {
        Task<List<ResultPlaylistDto>> GetMyPlaylistsAsync(string userId);
        Task<ResultPlaylistDto> CreateAsync(string userId, string name);
        Task<PlaylistDetailsDto?> GetDetailsAsync(string userId, int playlistId);
        Task<bool> AddSongAsync(string userId, int playlistId, int songId);
        Task<bool> RemoveSongAsync(string userId, int playlistId, int songId);
        Task<bool> DeleteAsync(string userId, int playlistId);
    }
}
