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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            public int Id { get; set; }

            [Required, MaxLength(150)]
            public string Title { get; set; } = string.Empty;

            [Required, MaxLength(2000)]
            public string Description { get; set; } = string.Empty;

            [Required]
            public int CategoryId { get; set; }

            [MaxLength(150)]
            public string? Location { get; set; }

            [Range(1, 1000000)]
            public decimal Budget { get; set; }

            public JobType JobType { get; set; }

            public DateTime? Deadline { get; set; }

            public List<int> SelectedSkillIds { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == userId);

            if (job is null) return RedirectToPage("Index");
            if (job.Status != JobStatus.Open)
            {
                TempData["ErrorMessage"] = "Only open jobs can be edited.";
                return RedirectToPage("Index");
            }

            Input = new InputModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                CategoryId = job.CategoryId,
                Location = job.Location,
                Budget = job.Budget,
                JobType = job.JobType,
                Deadline = job.Deadline,
                SelectedSkillIds = job.JobSkills.Select(js => js.SkillId).ToList()
            };

            await LoadOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Id == Input.Id && j.EmployerId == userId);

            if (job is null) return RedirectToPage("Index");

            if (Input.SelectedSkillIds.Count == 0)
            {
                ModelState.AddModelError(nameof(Input.SelectedSkillIds), "Select at least one required skill.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            job.Title = Input.Title;
            job.Description = Input.Description;
            job.CategoryId = Input.CategoryId;
            job.Location = Input.Location;
            job.Budget = Input.Budget;
            job.JobType = Input.JobType;
            job.Deadline = Input.Deadline;

            _context.JobSkills.RemoveRange(job.JobSkills);
            foreach (var skillId in Input.SelectedSkillIds)
            {
                job.JobSkills.Add(new JobSkill { SkillId = skillId, JobId = job.Id });
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Job updated successfully.";
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
