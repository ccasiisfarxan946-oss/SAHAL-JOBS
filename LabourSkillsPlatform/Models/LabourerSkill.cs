using System.ComponentModel.DataAnnotations;

namespace LabourSkillsPlatform.Models
{
    public enum ProficiencyLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    /// <summary>
    /// Join entity between Labourer and Skill. Directly implements the thesis's
    /// "multi-tiered skill verification" concept (Chapter 2, 2.4 Research Gap #1):
    /// a Labourer uploads a skill, and an Admin verifies it before it is treated as trusted
    /// in the matching algorithm (Admin > "Verify Labour Skills").
    /// </summary>
    public class LabourerSkill
    {
        public int Id { get; set; }

        public string LabourerId { get; set; } = string.Empty;
        public Labourer Labourer { get; set; } = null!;

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Beginner;

        [MaxLength(500)]
        public string? Evidence { get; set; } // e.g. description of prior work / portfolio note

        public bool IsVerified { get; set; } = false;

        public string? VerifiedByAdminId { get; set; }
        public DateTime? VerifiedDate { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
