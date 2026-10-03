using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Employer.Pages.Jobs
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<Skill> AllSkills { get; set; } = new();

        public class InputModel
        {
            [Required, MaxLength(150)]
            public string Title { get; set; } = string.Empty;

            [Required, MaxLength(2000)]
            public string Description { get; set; } = string.Empty;

            [Required, Display(Name = "Category")]
            public int CategoryId { get; set; }

            [MaxLength(150)]
            public string? Location { get; set; }

            [Range(1, 1000000)]
            public decimal Budget { get; set; }

            public JobType JobType { get; set; } = JobType.Fixed;

            public DateTime? Deadline { get; set; }

            [Display(Name = "Required Skills")]
            public List<int> SelectedSkillIds { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == userId);
            if (employer is null || !employer.IsApproved)
            {
                TempData["ErrorMessage"] = "Your employer account must be approved before you can post jobs.";
                return RedirectToPage("Index");
            }

            await LoadOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == userId);
            if (employer is null || !employer.IsApproved)
            {
                TempData["ErrorMessage"] = "Your employer account must be approved before you can post jobs.";
                return RedirectToPage("Index");
            }

            if (Input.SelectedSkillIds.Count == 0)
            {
                ModelState.AddModelError(nameof(Input.SelectedSkillIds), "Select at least one required skill.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            var job = new Job
            {
                EmployerId = userId,
                Title = Input.Title,
                Description = Input.Description,
                CategoryId = Input.CategoryId,
                Location = Input.Location,
                Budget = Input.Budget,
                JobType = Input.JobType,
                Deadline = Input.Deadline,
                Status = JobStatus.Open
            };

            foreach (var skillId in Input.SelectedSkillIds)
            {
                job.JobSkills.Add(new JobSkill { SkillId = skillId });
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Job posted successfully.";
            return RedirectToPage("Index");
        }

        private async Task LoadOptionsAsync()
        {
            CategoryOptions = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            AllSkills = await _context.Skills.Include(s => s.Category).OrderBy(s => s.Category.Name).ThenBy(s => s.Name).ToListAsync();
        }
    }
}
