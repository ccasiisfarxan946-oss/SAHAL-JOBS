using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Applications
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

        public List<JobApplication> Applications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            Applications = await _context.JobApplications
                .Include(a => a.Job).ThenInclude(j => j.Employer)
                .Include(a => a.Job).ThenInclude(j => j.Category)
                .Where(a => a.LabourerId == userId)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();
        }
    }
}
