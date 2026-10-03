using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LabourSkillsPlatform.Models;

namespace LabourSkillsPlatform.Areas.Employer.Pages.Profile
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

        public bool IsApproved { get; set; }

        public class InputModel
        {
            [Required, MaxLength(150)]
            public string FullName { get; set; } = string.Empty;

            [Required, MaxLength(150)]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; } = string.Empty;

            [MaxLength(100)]
            [Display(Name = "Contact Person")]
            public string? ContactPerson { get; set; }

            [MaxLength(250)]
            [Display(Name = "Company Address")]
            public string? CompanyAddress { get; set; }

            [MaxLength(500)]
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var employer = await _context.Employers.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == userId);
            if (employer is null) return RedirectToPage("/Dashboard");

            IsApproved = employer.IsApproved;
            Input = new InputModel
            {
                FullName = employer.User.FullName,
                CompanyName = employer.CompanyName,
                ContactPerson = employer.ContactPerson,
                CompanyAddress = employer.CompanyAddress,
                Description = employer.Description
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = _userManager.GetUserId(User)!;
            var employer = await _context.Employers.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == userId);
            if (employer is null) return RedirectToPage("/Dashboard");

            employer.User.FullName = Input.FullName;
            employer.CompanyName = Input.CompanyName;
            employer.ContactPerson = Input.ContactPerson;
            employer.CompanyAddress = Input.CompanyAddress;
            employer.Description = Input.Description;

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Profile updated successfully.";
            return RedirectToPage();
        }
    }
}
