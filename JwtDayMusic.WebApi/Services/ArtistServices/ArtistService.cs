using AutoMapper;
using JwtDayMusic.WebApi.Context;
using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtDayMusic.WebApi.Services.ArtistServices
{
    public class ArtistService : IArtistService
    {
        private readonly JwtContext _context;
        private readonly IMapper _mapper;

        public ArtistService(JwtContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ResultArtistDto>> GetAllArtists()
        {
            var artists = await _context.Artists.ToListAsync();
            return _mapper.Map<List<ResultArtistDto>>(artists);
        }

        public async Task<ResultArtistDto?> GetArtistByIdAsync(int id)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistId == id);
            return artist == null ? null : _mapper.Map<ResultArtistDto>(artist);
        }

        public async Task CreateArtistAsync(CreateArtistDto createArtistDto)
        {
            var artist = _mapper.Map<Artist>(createArtistDto);
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ToggleFollowAsync(string userId, int artistId)
        {
            var existing = await _context.ArtistFollows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);

            if (existing != null)
            {
                _context.ArtistFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            _context.ArtistFollows.Add(new ArtistFollow
            {
                UserId = userId,
                ArtistId = artistId,
                FollowedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetFollowingArtistIdsAsync(string userId)
        {
            return await _context.ArtistFollows
                .Where(f => f.UserId == userId)
                .Select(f => f.ArtistId)
                .ToListAsync();
        }

        public async Task<List<ResultArtistDto>> GetFollowedArtistsAsync(string userId)
        {
            var artists = await _context.ArtistFollows
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.FollowedDate)
                .Join(_context.Artists, f => f.ArtistId, a => a.ArtistId, (f, a) => a)
                .ToListAsync();

            return _mapper.Map<List<ResultArtistDto>>(artists);
        }
    }
}
