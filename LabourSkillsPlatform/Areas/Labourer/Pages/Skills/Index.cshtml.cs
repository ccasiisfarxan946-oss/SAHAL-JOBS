using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Skills
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

        public List<LabourerSkill> MySkills { get; set; } = new();
        public List<SelectListItem> SkillOptions { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, Display(Name = "Skill")]
            public int SkillId { get; set; }

            [Display(Name = "Proficiency Level")]
            public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;

            [MaxLength(500)]
            [Display(Name = "Evidence / Notes (e.g. prior projects)")]
            public string? Evidence { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            var alreadyExists = await _context.LabourerSkills
                .AnyAsync(ls => ls.LabourerId == userId && ls.SkillId == Input.SkillId);

            if (alreadyExists)
            {
                ModelState.AddModelError(string.Empty, "You have already added this skill.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            _context.LabourerSkills.Add(new LabourerSkill
            {
                LabourerId = userId,
                SkillId = Input.SkillId,
                ProficiencyLevel = Input.ProficiencyLevel,
                Evidence = Input.Evidence,
                IsVerified = false
            });

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Skill submitted. It will appear as verified once an administrator reviews it.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var skill = await _context.LabourerSkills.FirstOrDefaultAsync(ls => ls.Id == id && ls.LabourerId == userId);
            if (skill is not null)
            {
                _context.LabourerSkills.Remove(skill);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Skill removed.";
            }
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            MySkills = await _context.LabourerSkills
                .Include(ls => ls.Skill).ThenInclude(s => s.Category)
                .Where(ls => ls.LabourerId == userId)
                .OrderByDescending(ls => ls.UploadedAt)
                .ToListAsync();

            var existingSkillIds = MySkills.Select(s => s.SkillId).ToHashSet();

            SkillOptions = await _context.Skills
                .Include(s => s.Category)
                .Where(s => !existingSkillIds.Contains(s.Id))
                .OrderBy(s => s.Category.Name).ThenBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Category.Name + " — " + s.Name })
                .ToListAsync();
        }
    }
}
