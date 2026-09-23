using AutoMapper;
using JwtDayMusic.WebApi.Context;
using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtDayMusic.WebApi.Services.PlaylistServices
{
    public class PlaylistService : IPlaylistService
    {
        private readonly JwtContext _context;
        private readonly IMapper _mapper;

        public PlaylistService(JwtContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ResultPlaylistDto>> GetMyPlaylistsAsync(string userId)
        {
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return playlists.Select(p => new ResultPlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                SongCount = p.PlaylistSongs.Count,
                CoverImageUrl = p.PlaylistSongs.FirstOrDefault()?.Song?.CoverImageUrl,
                CreatedDate = p.CreatedDate
            }).ToList();
        }

        public async Task<ResultPlaylistDto> CreateAsync(string userId, string name)
        {
            var playlist = new Playlist
            {
                UserId = userId,
                Name = name,
                CreatedDate = DateTime.UtcNow
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return new ResultPlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                SongCount = 0,
                CoverImageUrl = null,
                CreatedDate = playlist.CreatedDate
            };
        }

        public async Task<PlaylistDetailsDto?> GetDetailsAsync(string userId, int playlistId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null) return null;

            var songs = playlist.PlaylistSongs
                .OrderByDescending(ps => ps.AddedDate)
                .Select(ps => ps.Song)
                .ToList();

            return new PlaylistDetailsDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Songs = _mapper.Map<List<ResultSongDto>>(songs)
            };
        }

        public async Task<bool> AddSongAsync(string userId, int playlistId, int songId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null) return false;

            var alreadyExists = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (alreadyExists) return true; // zaten ekli, hata sayılmaz

            _context.PlaylistSongs.Add(new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                AddedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSongAsync(string userId, int playlistId, int songId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null) return false;

            var entry = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (entry == null) return false;

            _context.PlaylistSongs.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, int playlistId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null) return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
