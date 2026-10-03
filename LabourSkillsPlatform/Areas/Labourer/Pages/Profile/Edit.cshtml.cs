using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Labourer.Pages.Profile
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

        public class InputModel
        {
            [Required, MaxLength(150)]
            public string FullName { get; set; } = string.Empty;

            [MaxLength(1000)]
            public string? Bio { get; set; }

            [Range(0, 60)]
            [Display(Name = "Years of Experience")]
            public int YearsOfExperience { get; set; }

            [MaxLength(150)]
            public string? Location { get; set; }

            [Display(Name = "Availability")]
            public AvailabilityStatus AvailabilityStatus { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var labourer = await _context.Labourers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == userId);
            if (labourer is null) return RedirectToPage("/Dashboard");

            Input = new InputModel
            {
                FullName = labourer.User.FullName,
                Bio = labourer.Bio,
                YearsOfExperience = labourer.YearsOfExperience,
                Location = labourer.Location,
                AvailabilityStatus = labourer.AvailabilityStatus
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = _userManager.GetUserId(User)!;
            var labourer = await _context.Labourers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == userId);
            if (labourer is null) return RedirectToPage("/Dashboard");

            labourer.User.FullName = Input.FullName;
            labourer.Bio = Input.Bio;
            labourer.YearsOfExperience = Input.YearsOfExperience;
            labourer.Location = Input.Location;
            labourer.AvailabilityStatus = Input.AvailabilityStatus;

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Profile updated successfully.";
            return RedirectToPage();
        }
    }
}
