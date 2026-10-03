using System.ComponentModel.DataAnnotations;

namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// Labour category (e.g. Electrical, Plumbing, Masonry) - Admin > "Manage Labour Categories".
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
