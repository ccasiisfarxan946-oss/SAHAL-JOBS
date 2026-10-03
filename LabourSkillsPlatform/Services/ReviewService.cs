using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Services
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(int jobId, string reviewerId, string revieweeId, int rating, string? comment, ReviewType type);
        Task RecalculateLabourerRatingAsync(string labourerId);
    }

    /// <summary>
    /// Handles review creation and keeps Labourer.AverageRating in sync (used by the matching
    /// algorithm's reputation component - Chapter 3, 3.4).
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ReviewService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Review> AddReviewAsync(int jobId, string reviewerId, string revieweeId, int rating, string? comment, ReviewType type)
        {
            var review = new Review
            {
                JobId = jobId,
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                Rating = rating,
                Comment = comment,
                ReviewType = type
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            if (type == ReviewType.EmployerToLabourer)
            {
                await RecalculateLabourerRatingAsync(revieweeId);
            }

            await _notificationService.CreateAsync(revieweeId, "You received a new review.", $"/Reviews");

            return review;
        }

        public async Task RecalculateLabourerRatingAsync(string labourerId)
        {
            var labourer = await _context.Labourers.FirstOrDefaultAsync(l => l.Id == labourerId);
            if (labourer is null) return;

            var reviews = await _context.Reviews
                .Where(r => r.RevieweeId == labourerId && r.ReviewType == ReviewType.EmployerToLabourer)
                .ToListAsync();

            if (reviews.Count == 0)
            {
                labourer.AverageRating = 0;
                labourer.TotalReviews = 0;
            }
            else
            {
                labourer.AverageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 2);
                labourer.TotalReviews = reviews.Count;
            }

            await _context.SaveChangesAsync();
        }
    }
}
