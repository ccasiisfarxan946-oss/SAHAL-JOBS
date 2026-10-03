using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LabourerModel = LabourSkillsPlatform.Models.Labourer;

namespace LabourSkillsPlatform.Areas.Employer.Pages.Search
{
    /// <summary>
    /// Employer > "Search Labourers" - lets an Employer browse the labourer pool directly,
    /// independent of a specific job posting (distinct from the automated per-job matching
    /// engine used in Jobs/Details applications list).
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool VerifiedOnly { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<LabourerModel> Results { get; set; } = new();

        public async Task OnGetAsync()
        {
            CategoryOptions = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            var query = _context.Labourers
                .Include(l => l.User)
                .Include(l => l.LabourerSkills).ThenInclude(ls => ls.Skill).ThenInclude(s => s.Category)
                .AsQueryable();

            if (CategoryId.HasValue)
            {
                query = query.Where(l => l.LabourerSkills.Any(ls => ls.Skill.CategoryId == CategoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(Location))
            {
                query = query.Where(l => l.Location != null && l.Location.Contains(Location));
            }

            if (VerifiedOnly)
            {
                query = query.Where(l => l.LabourerSkills.Any(ls => ls.IsVerified));
            }

            Results = await query.OrderByDescending(l => l.AverageRating).ToListAsync();
        }
    }
}
