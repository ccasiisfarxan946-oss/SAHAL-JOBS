using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages
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

        public Models.Labourer? CurrentLabourer { get; set; }
        public int TotalSkills { get; set; }
        public int VerifiedSkills { get; set; }
        public int TotalApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public List<JobApplication> RecentApplications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            CurrentLabourer = await _context.Labourers
                .Include(l => l.User)
                .Include(l => l.LabourerSkills)
                .FirstOrDefaultAsync(l => l.Id == userId);

            TotalSkills = CurrentLabourer?.LabourerSkills.Count ?? 0;
            VerifiedSkills = CurrentLabourer?.LabourerSkills.Count(s => s.IsVerified) ?? 0;

            var applications = await _context.JobApplications
                .Include(a => a.Job).ThenInclude(j => j.Employer)
                .Where(a => a.LabourerId == userId)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            TotalApplications = applications.Count;
            AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted);
            RecentApplications = applications.Take(5).ToList();
        }
    }
}
