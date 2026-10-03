using System.ComponentModel.DataAnnotations;

namespace LabourSkillsPlatform.Models
{
    public enum ReviewType
    {
        EmployerToLabourer,
        LabourerToEmployer
    }

    /// <summary>
    /// A rating/comment left after a Job (Employer > "Rate Labourers", Labourer > "Receive Reviews").
    /// Admin can moderate reviews (Admin > "Manage Reviews").
    /// Labourer.AverageRating is recalculated whenever a new EmployerToLabourer review is added.
    /// </summary>
    public class Review
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        public string ReviewerId { get; set; } = string.Empty;
        public ApplicationUser Reviewer { get; set; } = null!;

        public string RevieweeId { get; set; } = string.Empty;
        public ApplicationUser Reviewee { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public ReviewType ReviewType { get; set; }

        public bool IsFlagged { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
