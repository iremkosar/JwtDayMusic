using JwtDayMusic.WebApi.Dtos;

namespace JwtDayMusic.WebApi.Services.RecommendationServices
{
    public interface IRecommendationService
    {
        Task RecordPlayAsync(string userId, int songId);
        Task<List<ResultSongDto>> GetHistoryAsync(string userId, int take = 50);
        Task<List<RecommendedSongDto>> GetRecommendationsAsync(string userId, int take = 10);
    }
}
