using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Review> Reviews { get; set; } = new();
        public decimal AverageRating { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            Reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Job)
                .Where(r => r.RevieweeId == userId && r.ReviewType == ReviewType.EmployerToLabourer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var labourer = await _context.Labourers.FindAsync(userId);
            AverageRating = labourer?.AverageRating ?? 0;
        }
    }
}
