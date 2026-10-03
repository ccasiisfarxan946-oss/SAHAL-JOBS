using LabourSkillsPlatform.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalLabourers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalJobsPosted { get; set; }
        public int TotalCategories { get; set; }

        public async Task OnGetAsync()
        {
            TotalLabourers = await _context.Labourers.CountAsync();
            TotalEmployers = await _context.Employers.CountAsync(e => e.IsApproved);
            TotalJobsPosted = await _context.Jobs.CountAsync();
            TotalCategories = await _context.Categories.CountAsync();
        }
    }
}
