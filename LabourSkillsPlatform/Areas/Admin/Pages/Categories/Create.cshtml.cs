using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Description { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Categories.Add(new Category
            {
                Name = Input.Name,
                Description = Input.Description
            });
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Category created successfully.";
            return RedirectToPage("Index");
        }
    }
}
