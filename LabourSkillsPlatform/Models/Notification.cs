using System.ComponentModel.DataAnnotations;

namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// In-app notification (e.g. "Your skill was verified", "New application received").
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
