using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public string? NewSkillName { get; set; }

        public List<Skill> ExistingSkills { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _context.Categories.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return RedirectToPage("Index");

            Input = new InputModel { Id = category.Id, Name = category.Name, Description = category.Description };
            ExistingSkills = category.Skills.OrderBy(s => s.Name).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAddSkillAsync(int id)
        {
            if (!string.IsNullOrWhiteSpace(NewSkillName))
            {
                _context.Skills.Add(new Skill { Name = NewSkillName.Trim(), CategoryId = id });
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Skill added to category.";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteSkillAsync(int id, int skillId)
        {
            var skill = await _context.Skills.FindAsync(skillId);
            if (skill is not null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Skill removed.";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var category = await _context.Categories.FindAsync(Input.Id);
            if (category is null) return RedirectToPage("Index");

            category.Name = Input.Name;
            category.Description = Input.Description;
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Category updated successfully.";
            return RedirectToPage("Index");
        }
    }
}
