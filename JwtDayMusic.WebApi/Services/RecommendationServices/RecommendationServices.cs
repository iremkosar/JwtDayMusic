using AutoMapper;
using JwtDayMusic.WebApi.Context;
using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace JwtDayMusic.WebApi.Services.RecommendationServices
{
    // ML.NET'in kullandığı eğitim satırı: bir kullanıcının bir şarkıyı ne kadar dinlediği
    public class SongInteraction
    {
        public string UserId { get; set; }
        public float SongId { get; set; }
        public float Label { get; set; }
    }

    // Model tahmininin çıktısı
    public class SongScorePrediction
    {
        public float Score { get; set; }
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly JwtContext _context;
        private readonly IMapper _mapper;

        // Anlamlı bir model eğitmek için gereken minimum veri: en az bu kadar farklı
        // kullanıcı-şarkı etkileşimi, en az 2 farklı kullanıcı ve 2 farklı şarkı olmalı.
        private const int MinInteractions = 5;

        public RecommendationService(JwtContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task RecordPlayAsync(string userId, int songId)
        {
            _context.PlayHistories.Add(new PlayHistory
            {
                UserId = userId,
                SongId = songId,
                PlayedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<ResultSongDto>> GetHistoryAsync(string userId, int take = 50)
        {
            var songs = await _context.PlayHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedDate)
                .Take(take)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .Select(h => h.Song)
                .ToListAsync();

            return _mapper.Map<List<ResultSongDto>>(songs);
        }

        public async Task<List<RecommendedSongDto>> GetRecommendationsAsync(string userId, int take = 10)
        {
            // Her (kullanıcı, şarkı) çifti için kaç kez dinlendiğini say -> Label bu sayı olacak
            var allHistory = await _context.PlayHistories.ToListAsync();

            var interactions = allHistory
                .GroupBy(h => new { h.UserId, h.SongId })
                .Select(g => new SongInteraction
                {
                    UserId = g.Key.UserId,
                    SongId = g.Key.SongId,
                    Label = Math.Min(g.Count(), 10) // aşırı tekrar tek şarkıyı domine etmesin diye tavan
                })
                .ToList();

            var distinctUsers = interactions.Select(i => i.UserId).Distinct().Count();
            var distinctSongs = interactions.Select(i => i.SongId).Distinct().Count();
            var userAlreadyPlayed = interactions
                .Where(i => i.UserId == userId)
                .Select(i => (int)i.SongId)
                .ToHashSet();

            var allSongs = await _context.Songs.Include(s => s.Artist).ToListAsync();
            var candidateSongs = allSongs.Where(s => !userAlreadyPlayed.Contains(s.SongId)).ToList();

            // Yeterli veri yoksa (soğuk başlangıç / cold start) ML modeli anlamlı sonuç veremez,
            // bu durumda en çok dinlenen şarkıları öneriyoruz.
            if (interactions.Count < MinInteractions || distinctUsers < 2 || distinctSongs < 2)
            {
                return candidateSongs
                    .OrderByDescending(s => s.PlayCount)
                    .Take(take)
                    .Select(s => MapToRecommendedDto(s, score: s.PlayCount))
                    .ToList();
            }

            // ---- ML.NET: Matrix Factorization ile öneri modeli eğitimi ----
            var mlContext = new MLContext(seed: 1);
            var trainingData = mlContext.Data.LoadFromEnumerable(interactions);

            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: nameof(SongInteraction.UserId))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "SongIdEncoded", inputColumnName: nameof(SongInteraction.SongId)))
                .Append(mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "UserIdEncoded",
                    MatrixRowIndexColumnName = "SongIdEncoded",
                    LabelColumnName = nameof(SongInteraction.Label),
                    NumberOfIterations = 30,
                    ApproximationRank = 8
                }));

            var model = pipeline.Fit(trainingData);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<SongInteraction, SongScorePrediction>(model);

            var scored = candidateSongs.Select(song =>
            {
                var prediction = predictionEngine.Predict(new SongInteraction
                {
                    UserId = userId,
                    SongId = song.SongId
                });

                return MapToRecommendedDto(song, score: prediction.Score);
            });

            return scored
                .OrderByDescending(s => s.Score)
                .Take(take)
                .ToList();
        }

        private static RecommendedSongDto MapToRecommendedDto(Song song, float score)
        {
            return new RecommendedSongDto
            {
                SongId = song.SongId,
                Title = song.Title,
                CoverImageUrl = song.CoverImageUrl,
                Duration = song.Duration,
                PlayCount = song.PlayCount,
                RequiredPackage = song.RequiredPackage,
                Genre = song.Genre,
                ReleaseDate = song.ReleaseDate,
                ArtistId = song.ArtistId,
                ArtistName = song.Artist?.Name,
                Score = score
            };
        }
    }
}
