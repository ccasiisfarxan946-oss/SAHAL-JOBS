using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Skills)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id);
            if (category is not null)
            {
                if (category.Skills.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete a category that still has skills assigned to it.";
                }
                else
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Category deleted successfully.";
                }
            }
            return RedirectToPage();
        }
    }
}
