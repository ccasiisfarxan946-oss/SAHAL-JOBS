using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabourSkillsPlatform.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    /// <summary>
    /// A Labourer's application to a Job (Labourer > "Apply for Jobs", Employer > "Hire Labourers").
    /// MatchScore is computed by the JobMatchingService at the time of application/listing.
    /// </summary>
    public class JobApplication
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        public string LabourerId { get; set; } = string.Empty;
        public Labourer Labourer { get; set; } = null!;

        [MaxLength(1000)]
        public string? CoverMessage { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MatchScore { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    }
}
