using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Jobs
{
    /// <summary>
    /// Labourer > "View Jobs" - displays open jobs ranked by the matching engine's score
    /// against the current Labourer's skills, rating and location (Chapter 3, 3.4).
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobMatchingService _matchingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, IJobMatchingService matchingService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _matchingService = matchingService;
            _userManager = userManager;
        }

        public List<(Job Job, decimal Score)> RecommendedJobs { get; set; } = new();
        public HashSet<int> AppliedJobIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;

            RecommendedJobs = await _matchingService.GetRecommendedJobsAsync(userId, take: 50);

            AppliedJobIds = (await _context.JobApplications
                .Where(a => a.LabourerId == userId)
                .Select(a => a.JobId)
                .ToListAsync()).ToHashSet();
        }
    }
}
