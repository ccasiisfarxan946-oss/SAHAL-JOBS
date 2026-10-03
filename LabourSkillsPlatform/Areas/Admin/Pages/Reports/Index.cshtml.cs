using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<(string CategoryName, int JobCount)> JobsByCategory { get; set; } = new();
        public List<(string Status, int Count)> JobsByStatus { get; set; } = new();
        public int TotalApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public decimal AcceptanceRate { get; set; }
        public double AverageMatchScore { get; set; }
        public List<(string LabourerName, decimal Rating, int Reviews)> TopRatedLabourers { get; set; } = new();

        public async Task OnGetAsync()
        {
            JobsByCategory = await _context.Jobs
                .Include(j => j.Category)
                .GroupBy(j => j.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Select(g => new ValueTuple<string, int>(g.Category, g.Count))
                .ToListAsync();

            var statusGroups = await _context.Jobs
                .GroupBy(j => j.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            JobsByStatus = statusGroups.Select(g => (g.Key.ToString(), g.Count)).ToList();

            TotalApplications = await _context.JobApplications.CountAsync();
            AcceptedApplications = await _context.JobApplications.CountAsync(a => a.Status == ApplicationStatus.Accepted);
            AcceptanceRate = TotalApplications == 0 ? 0 : Math.Round((decimal)AcceptedApplications / TotalApplications * 100, 1);

            AverageMatchScore = await _context.JobApplications.AnyAsync()
                ? (double)await _context.JobApplications.AverageAsync(a => a.MatchScore)
                : 0;

            TopRatedLabourers = await _context.Labourers
                .Include(l => l.User)
                .Where(l => l.TotalReviews > 0)
                .OrderByDescending(l => l.AverageRating)
                .Take(5)
                .Select(l => new ValueTuple<string, decimal, int>(l.User.FullName, l.AverageRating, l.TotalReviews))
                .ToListAsync();
        }
    }
}
