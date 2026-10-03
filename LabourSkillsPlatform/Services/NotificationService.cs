using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string message, string? link = null);
        Task<List<Notification>> GetForUserAsync(string userId, int take = 10);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
    }

    /// <summary>
    /// Centralized notification creation used across Employer/Labourer/Admin actions
    /// (e.g. application received, skill verified, job hired).
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(string userId, string message, string? link = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Link = link
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetForUserAsync(string userId, int take = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification is not null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
