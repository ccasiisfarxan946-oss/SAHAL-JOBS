using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Jobs
{
    /// <summary>
    /// Admin > "Manage Jobs" - system-wide oversight of all job postings (moderation/cancellation),
    /// distinct from an Employer's own "Manage Job Posts" (CRUD on their own postings).
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Job> Jobs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Jobs
                .Include(j => j.Employer).ThenInclude(e => e.User)
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(StatusFilter) && Enum.TryParse<JobStatus>(StatusFilter, out var status))
            {
                query = query.Where(j => j.Status == status);
            }

            Jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job is not null && job.Status != JobStatus.Completed)
            {
                job.Status = JobStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Job has been cancelled by the administrator.";
            }
            return RedirectToPage();
        }
    }
}
