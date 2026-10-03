using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabourSkillsPlatform.Models
{
    public enum JobType
    {
        Fixed,
        Hourly
    }

    public enum JobStatus
    {
        Open,
        InProgress,
        Completed,
        Cancelled
    }

    /// <summary>
    /// A job posting created by an Employer (Employer > "Post Jobs").
    /// Required skills are captured through JobSkills for use by the matching engine
    /// (Chapter 3, 3.4 - Application Tier "Match Engine").
    /// </summary>
    public class Job
    {
        public int Id { get; set; }

        public string EmployerId { get; set; } = string.Empty;
        public Employer Employer { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [MaxLength(150)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Budget { get; set; }

        public JobType JobType { get; set; } = JobType.Fixed;

        public JobStatus Status { get; set; } = JobStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? Deadline { get; set; }

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }

    /// <summary>
    /// Join entity: required Skill tags for a Job. Basis of the multi-criteria matching score.
    /// </summary>
    public class JobSkill
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;
    }
}
