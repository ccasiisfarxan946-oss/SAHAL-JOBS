using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// Employer profile (Chapter 3, 3.3 - Data Tier). Linked 1:1 with an ApplicationUser account.
    /// Employers must be approved by Admin before they can post jobs (Chapter 2, 2.4 - trust architecture;
    /// Admin "Approve Employers" core feature).
    /// </summary>
    public class Employer
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? CompanyAddress { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Approval gate enforced before an Employer may post jobs (RBAC / trust control).
        /// </summary>
        public bool IsApproved { get; set; } = false;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
