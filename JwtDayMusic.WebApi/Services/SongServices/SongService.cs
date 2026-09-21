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

        public async Task<Song?> GetByIdAsync(int songId)
        {
            return await _context.Songs.Include(s => s.Artist)
                .FirstOrDefaultAsync(s => s.SongId == songId);
        }
    }
}
