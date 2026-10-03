using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabourSkillsPlatform.Models
{
    public enum AvailabilityStatus
    {
        Available,
        Busy,
        Unavailable
    }

    /// <summary>
    /// Labourer profile (Chapter 3, 3.3 - Data Tier). Linked 1:1 with an ApplicationUser account.
    /// Holds experience, location and rating data used by the matching algorithm (Chapter 2, 2.4 -
    /// context-aware semantic matching; Chapter 3, 3.4 - Application Tier Match Engine).
    /// </summary>
    public class Labourer
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public int YearsOfExperience { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;

        /// <summary>
        /// Cached average rating computed from Reviews (kept in sync by ReviewService).
        /// </summary>
        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        public int TotalReviews { get; set; } = 0;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public ICollection<LabourerSkill> LabourerSkills { get; set; } = new List<LabourerSkill>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
