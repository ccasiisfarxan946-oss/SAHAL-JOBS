using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IReviewService _reviewService;

        public IndexModel(ApplicationDbContext context, IReviewService reviewService)
        {
            _context = context;
            _reviewService = reviewService;
        }

        public List<Review> Reviews { get; set; } = new();

        public async Task OnGetAsync()
        {
            Reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Job)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostFlagAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is not null)
            {
                review.IsFlagged = !review.IsFlagged;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is not null)
            {
                var revieweeId = review.RevieweeId;
                var type = review.ReviewType;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                if (type == ReviewType.EmployerToLabourer)
                {
                    await _reviewService.RecalculateLabourerRatingAsync(revieweeId);
                }

                TempData["StatusMessage"] = "Review removed.";
            }
            return RedirectToPage();
        }
    }
}
