using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Jobs
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobMatchingService _matchingService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext context, IJobMatchingService matchingService,
            INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _matchingService = matchingService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public Job Job { get; set; } = null!;
        public decimal MyMatchScore { get; set; }
        public bool AlreadyApplied { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [MaxLength(1000)]
            [Display(Name = "Cover Message (optional)")]
            public string? CoverMessage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job is null) return RedirectToPage("Index");

            Job = job;
            MyMatchScore = await _matchingService.CalculateMatchScoreAsync(id, userId);
            AlreadyApplied = await _context.JobApplications.AnyAsync(a => a.JobId == id && a.LabourerId == userId);

            return Page();
        }

        public async Task<IActionResult> OnPostApplyAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var job = await _context.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.Id == id);
            if (job is null || job.Status != JobStatus.Open)
            {
                TempData["ErrorMessage"] = "This job is no longer accepting applications.";
                return RedirectToPage(new { id });
            }

            var alreadyApplied = await _context.JobApplications.AnyAsync(a => a.JobId == id && a.LabourerId == userId);
            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "You have already applied to this job.";
                return RedirectToPage(new { id });
            }

            var score = await _matchingService.CalculateMatchScoreAsync(id, userId);

            _context.JobApplications.Add(new JobApplication
            {
                JobId = id,
                LabourerId = userId,
                CoverMessage = Input.CoverMessage,
                MatchScore = score,
                Status = ApplicationStatus.Pending
            });

            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(job.EmployerId,
                $"A new application was received for your job '{job.Title}'.");

            TempData["StatusMessage"] = "Application submitted successfully.";
            return RedirectToPage("/Applications/Index", new { area = "Labourer" });
        }
    }
}
