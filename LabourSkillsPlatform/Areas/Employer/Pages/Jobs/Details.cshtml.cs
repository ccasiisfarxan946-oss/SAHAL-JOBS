using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Employer.Pages.Jobs
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IReviewService _reviewService;

        public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            INotificationService notificationService, IReviewService reviewService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _reviewService = reviewService;
        }

        public Job Job { get; set; } = null!;
        public List<JobApplication> Applications { get; set; } = new();
        public bool HasReview { get; set; }

        [BindProperty]
        public RatingInput RatingForm { get; set; } = new();

        public class RatingInput
        {
            [Range(1, 5)]
            public int Rating { get; set; } = 5;

            [MaxLength(1000)]
            public string? Comment { get; set; }
        }

        private async Task<bool> LoadJobAsync(int id, string employerId)
        {
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employerId);

            if (job is null) return false;
            Job = job;

            Applications = await _context.JobApplications
                .Include(a => a.Labourer).ThenInclude(l => l.User)
                .Where(a => a.JobId == id)
                .OrderByDescending(a => a.MatchScore)
                .ToListAsync();

            HasReview = await _context.Reviews.AnyAsync(r => r.JobId == id && r.ReviewType == ReviewType.EmployerToLabourer);

            return true;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var employerId = _userManager.GetUserId(User)!;
            if (!await LoadJobAsync(id, employerId)) return RedirectToPage("Index");
            return Page();
        }

        public async Task<IActionResult> OnPostHireAsync(int id, int applicationId)
        {
            var employerId = _userManager.GetUserId(User)!;
            if (!await LoadJobAsync(id, employerId)) return RedirectToPage("Index");

            var application = await _context.JobApplications
                .Include(a => a.Labourer)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobId == id);

            if (application is not null && Job.Status == JobStatus.Open)
            {
                application.Status = ApplicationStatus.Accepted;
                Job.Status = JobStatus.InProgress;

                var otherApplications = await _context.JobApplications
                    .Where(a => a.JobId == id && a.Id != applicationId)
                    .ToListAsync();
                foreach (var other in otherApplications)
                {
                    other.Status = ApplicationStatus.Rejected;
                }

                await _context.SaveChangesAsync();

                await _notificationService.CreateAsync(application.LabourerId,
                    $"Congratulations! You have been hired for the job '{Job.Title}'.");

                TempData["StatusMessage"] = $"{application.Labourer.Id} has been hired for this job.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var employerId = _userManager.GetUserId(User)!;
            if (!await LoadJobAsync(id, employerId)) return RedirectToPage("Index");

            if (Job.Status == JobStatus.InProgress)
            {
                Job.Status = JobStatus.Completed;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Job marked as completed. You can now rate the labourer.";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRateAsync(int id)
        {
            var employerId = _userManager.GetUserId(User)!;
            if (!await LoadJobAsync(id, employerId)) return RedirectToPage("Index");

            var hiredApplication = Applications.FirstOrDefault(a => a.Status == ApplicationStatus.Accepted);

            if (Job.Status == JobStatus.Completed && hiredApplication is not null && !HasReview)
            {
                await _reviewService.AddReviewAsync(id, employerId, hiredApplication.LabourerId,
                    RatingForm.Rating, RatingForm.Comment, ReviewType.EmployerToLabourer);
                TempData["StatusMessage"] = "Thank you for rating the labourer.";
            }

            return RedirectToPage(new { id });
        }
    }
}
