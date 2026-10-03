using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Skills
{
    /// <summary>
    /// Admin > "Verify Labour Skills" - directly implements the thesis's skill-authentication
    /// concept (Chapter 2, 2.4 Research Gap #1) by letting an Admin approve or reject
    /// self-declared skills uploaded by Labourers.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public List<LabourerSkill> PendingSkills { get; set; } = new();
        public List<LabourerSkill> VerifiedSkills { get; set; } = new();

        public async Task OnGetAsync()
        {
            var all = await _context.LabourerSkills
                .Include(ls => ls.Labourer).ThenInclude(l => l.User)
                .Include(ls => ls.Skill).ThenInclude(s => s.Category)
                .OrderByDescending(ls => ls.UploadedAt)
                .ToListAsync();

            PendingSkills = all.Where(s => !s.IsVerified).ToList();
            VerifiedSkills = all.Where(s => s.IsVerified).Take(20).ToList();
        }

        public async Task<IActionResult> OnPostVerifyAsync(int id)
        {
            var adminId = _userManager.GetUserId(User);
            var labourerSkill = await _context.LabourerSkills
                .Include(ls => ls.Labourer)
                .Include(ls => ls.Skill)
                .FirstOrDefaultAsync(ls => ls.Id == id);

            if (labourerSkill is not null)
            {
                labourerSkill.IsVerified = true;
                labourerSkill.VerifiedByAdminId = adminId;
                labourerSkill.VerifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _notificationService.CreateAsync(labourerSkill.LabourerId,
                    $"Your skill '{labourerSkill.Skill.Name}' has been verified by an administrator.");

                TempData["StatusMessage"] = "Skill verified successfully.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var labourerSkill = await _context.LabourerSkills
                .Include(ls => ls.Skill)
                .FirstOrDefaultAsync(ls => ls.Id == id);

            if (labourerSkill is not null)
            {
                await _notificationService.CreateAsync(labourerSkill.LabourerId,
                    $"Your submitted skill '{labourerSkill.Skill.Name}' was not verified and has been removed. Please provide stronger evidence.");

                _context.LabourerSkills.Remove(labourerSkill);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Skill submission rejected and removed.";
            }
            return RedirectToPage();
        }
    }
}
