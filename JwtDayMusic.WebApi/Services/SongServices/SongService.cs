using AutoMapper;
using JwtDayMusic.WebApi.Context;
using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtDayMusic.WebApi.Services.SongServices
{
    public class SongService : ISongService
    {
        private readonly JwtContext _context;
        private readonly IMapper _mapper;

        public SongService(JwtContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ResultSongDto>> GetAllSongsAsync()
        {
            var songs = await _context.Songs.Include(s => s.Artist).ToListAsync();
            return _mapper.Map<List<ResultSongDto>>(songs);
        }

        public async Task<List<ResultSongDto>> GetByArtistIdAsync(int artistId)
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.ArtistId == artistId)
                .ToListAsync();

            return _mapper.Map<List<ResultSongDto>>(songs);
        }

        public async Task<List<GenreSummaryDto>> GetGenreSummaryAsync()
        {
            var songs = await _context.Songs.ToListAsync();

            return songs
                .Where(s => !string.IsNullOrEmpty(s.Genre))
                .GroupBy(s => s.Genre)
                .Select(g => new GenreSummaryDto
                {
                    Genre = g.Key,
                    SongCount = g.Count(),
                    CoverImageUrl = g.First().CoverImageUrl
                })
                .OrderBy(g => g.Genre)
                .ToList();
        }

        public async Task<List<ResultSongDto>> GetByGenreAsync(string genre)
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.Genre.ToLower() == genre.ToLower())
                .ToListAsync();

            return _mapper.Map<List<ResultSongDto>>(songs);
        }

        public async Task<Song?> GetByIdAsync(int songId)
        {
            return await _context.Songs.Include(s => s.Artist)
                .FirstOrDefaultAsync(s => s.SongId == songId);
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int songId)
        {
            var existing = await _context.SongFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

            if (existing != null)
            {
                _context.SongFavorites.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            _context.SongFavorites.Add(new SongFavorite
            {
                UserId = userId,
                SongId = songId,
                FavoritedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetFavoriteSongIdsAsync(string userId)
        {
            return await _context.SongFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.SongId)
                .ToListAsync();
        }

        public async Task<List<ResultSongDto>> GetFavoritedSongsAsync(string userId)
        {
            var songs = await _context.SongFavorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.FavoritedDate)
                .Join(_context.Songs.Include(s => s.Artist), f => f.SongId, s => s.SongId, (f, s) => s)
                .ToListAsync();

            return _mapper.Map<List<ResultSongDto>>(songs);
        }
    }
}
