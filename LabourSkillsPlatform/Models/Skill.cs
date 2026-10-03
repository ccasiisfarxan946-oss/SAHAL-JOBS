using System.ComponentModel.DataAnnotations;

namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// A specific skill tag (e.g. "Residential Wiring") belonging to a Category.
    /// Used both as a Job requirement tag and a Labourer skill tag for matching (Chapter 2, 2.2.2).
    /// </summary>
    public class Skill
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<LabourerSkill> LabourerSkills { get; set; } = new List<LabourerSkill>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}
