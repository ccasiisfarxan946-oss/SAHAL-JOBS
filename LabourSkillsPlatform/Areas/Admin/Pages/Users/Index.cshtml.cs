using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Areas.Admin.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<UserRow> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public class UserRow
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(u => u.FullName.Contains(SearchTerm) || u.Email!.Contains(SearchTerm));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserRow
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "N/A",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is not null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["StatusMessage"] = $"User account has been {(user.IsActive ? "activated" : "deactivated")}.";
            }
            return RedirectToPage();
        }
    }
}
