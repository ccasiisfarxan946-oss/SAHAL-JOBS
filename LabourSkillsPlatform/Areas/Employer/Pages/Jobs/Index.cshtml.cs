using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Employer.Pages.Jobs
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

        public List<Job> Jobs { get; set; } = new();
        public bool IsApproved { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == userId);
            IsApproved = employer?.IsApproved ?? false;

            Jobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.EmployerId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCloseAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == userId);
            if (job is not null && job.Status is JobStatus.Open or JobStatus.InProgress)
            {
                job.Status = JobStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Job post closed.";
            }
            return RedirectToPage();
        }
    }
}
