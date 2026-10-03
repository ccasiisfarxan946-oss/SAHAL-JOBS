using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Employer.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Models.Employer? CurrentEmployer { get; set; }
        public int TotalJobsPosted { get; set; }
        public int OpenJobs { get; set; }
        public int TotalApplicationsReceived { get; set; }
        public List<Job> RecentJobs { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            CurrentEmployer = await _context.Employers
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == userId);

            var jobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.EmployerId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            TotalJobsPosted = jobs.Count;
            OpenJobs = jobs.Count(j => j.Status == JobStatus.Open);
            TotalApplicationsReceived = jobs.Sum(j => j.Applications.Count);
            RecentJobs = jobs.Take(5).ToList();
        }
    }
}
