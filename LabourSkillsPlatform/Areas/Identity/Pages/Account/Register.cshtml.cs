using System.ComponentModel.DataAnnotations;
using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployerModel = LabourSkillsPlatform.Models.Employer;
using LabourerModel = LabourSkillsPlatform.Models.Labourer;

namespace LabourSkillsPlatform.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Public self-registration for the two client-facing roles (Employer, Labourer).
    /// Admin accounts are never created here (Chapter 3, 3.2.1 - RBAC design).
    /// Employers registered here start with IsApproved = false and must be approved
    /// by an Admin before posting jobs (Chapter 2, 2.4 trust architecture).
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required, MaxLength(150)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [Display(Name = "I am registering as")]
            public string AccountType { get; set; } = Roles.Labourer; // "Employer" or "Labourer"

            // Employer-only optional field
            [MaxLength(150)]
            [Display(Name = "Company Name (Employers only)")]
            public string? CompanyName { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (Input.AccountType == Roles.Employer && string.IsNullOrWhiteSpace(Input.CompanyName))
            {
                ModelState.AddModelError("Input.CompanyName", "Company name is required for Employer accounts.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                if (Input.AccountType == Roles.Employer)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Employer);
                    _context.Employers.Add(new EmployerModel
                    {
                        Id = user.Id,
                        CompanyName = Input.CompanyName ?? string.Empty,
                        IsApproved = false
                    });
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, Roles.Labourer);
                    _context.Labourers.Add(new LabourerModel
                    {
                        Id = user.Id
                    });
                }

                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);

                return Input.AccountType == Roles.Employer
                    ? RedirectToPage("/Dashboard", new { area = "Employer" })
                    : RedirectToPage("/Dashboard", new { area = "Labourer" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
