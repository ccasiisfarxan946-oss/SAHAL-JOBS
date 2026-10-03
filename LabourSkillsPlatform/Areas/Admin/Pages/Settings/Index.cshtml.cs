using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Settings
{
    /// <summary>
    /// Admin > "System Settings". Scope kept to what the thesis's core-feature list calls for:
    /// the Admin's own account security settings (password management).
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, DataType(DataType.Password), Display(Name = "Current Password")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "New Password")]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password), Compare("NewPassword"), Display(Name = "Confirm New Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToPage();

            var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Password changed successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
