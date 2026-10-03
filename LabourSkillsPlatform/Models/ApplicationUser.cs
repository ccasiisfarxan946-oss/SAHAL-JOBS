using Microsoft.AspNetCore.Identity;

namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// Extends ASP.NET Identity's IdentityUser to serve as the base account
    /// for all three system users: Admin, Employer, Labourer (Chapter 3, 3.2.1 - Design phase / RBAC).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// General account status flag. Used by Admin > Manage Users to enable/disable accounts.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties (one-to-one depending on role)
        public Employer? Employer { get; set; }
        public Labourer? Labourer { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
