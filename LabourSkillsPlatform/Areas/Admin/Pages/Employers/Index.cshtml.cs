using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LabourSkillsPlatform.Models;
using EmployerModel = LabourSkillsPlatform.Models.Employer;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Employers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public IndexModel(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public List<EmployerModel> PendingEmployers { get; set; } = new();
        public List<EmployerModel> ApprovedEmployers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var all = await _context.Employers
                .Include(e => e.User)
                .Include(e => e.Jobs)
                .OrderByDescending(e => e.RegisteredAt)
                .ToListAsync();

            PendingEmployers = all.Where(e => !e.IsApproved).ToList();
            ApprovedEmployers = all.Where(e => e.IsApproved).ToList();
        }

        public async Task<IActionResult> OnPostApproveAsync(string id)
        {
            var employer = await _context.Employers.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
            if (employer is not null)
            {
                employer.IsApproved = true;
                await _context.SaveChangesAsync();
                await _notificationService.CreateAsync(employer.Id, "Your employer account has been approved. You can now post jobs.");
                TempData["StatusMessage"] = $"{employer.CompanyName} has been approved.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRevokeAsync(string id)
        {
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == id);
            if (employer is not null)
            {
                employer.IsApproved = false;
                await _context.SaveChangesAsync();
                await _notificationService.CreateAsync(employer.Id, "Your employer approval has been revoked. Please contact support.");
                TempData["StatusMessage"] = $"{employer.CompanyName}'s approval has been revoked.";
            }
            return RedirectToPage();
        }
    }
}
