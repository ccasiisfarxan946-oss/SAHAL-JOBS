using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalLabourers { get; set; }
        public int TotalEmployers { get; set; }
        public int PendingEmployerApprovals { get; set; }
        public int PendingSkillVerifications { get; set; }
        public int TotalJobs { get; set; }
        public int OpenJobs { get; set; }
        public int TotalCategories { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<Review> RecentReviews { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            TotalLabourers = await _context.Labourers.CountAsync();
            TotalEmployers = await _context.Employers.CountAsync();
            PendingEmployerApprovals = await _context.Employers.CountAsync(e => !e.IsApproved);
            PendingSkillVerifications = await _context.LabourerSkills.CountAsync(s => !s.IsVerified);
            TotalJobs = await _context.Jobs.CountAsync();
            OpenJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Open);
            TotalCategories = await _context.Categories.CountAsync();

            RecentJobs = await _context.Jobs
                .Include(j => j.Employer).ThenInclude(e => e.User)
                .Include(j => j.Category)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            RecentReviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
    }
}
