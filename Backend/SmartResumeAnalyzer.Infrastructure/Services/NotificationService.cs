using Microsoft.EntityFrameworkCore;
using SmartResumeAnalyzer.Core.DTOs.Notification;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    ProjectId = n.ProjectId ?? Guid.Empty,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                throw new NotFoundException("Notification not found.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
                notification.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task GenerateNotificationsAsync()
        {
            var users = await _context.Users
                .Include(u => u.Projects)
                .ToListAsync();

            foreach (var user in users)
            {
                foreach (var project in user.Projects)
                {
                    if (project.Status == "Draft")
                    {
                        var daysSinceCreated = (DateTime.UtcNow - project.CreatedAt).TotalDays;

                        if (daysSinceCreated >= user.ReminderIntervalDays)
                        {
                            var existing = await _context.Notifications
                                .FirstOrDefaultAsync(n => n.UserId == user.Id
                                    && n.ProjectId == project.Id
                                    && n.Type == "draft-reminder");

                            if (existing == null)
                            {
                                _context.Notifications.Add(new Notification
                                {
                                    UserId = user.Id,
                                    ProjectId = project.Id,
                                    Type = "draft-reminder",
                                    Message = $"Your application for \"{project.Title}\" is still in Draft. Consider sending it soon."
                                });
                            }
                            else if (existing.IsRead)
                            {
                                existing.IsRead = false;
                                existing.CreatedAt = DateTime.UtcNow;
                            }
                        }
                    }

                    if (project.Status == "Sent" && project.SentAt.HasValue)
                    {
                        var daysSinceSent = (DateTime.UtcNow - project.SentAt.Value).TotalDays;

                        if (daysSinceSent >= user.ReminderIntervalDays)
                        {
                            var existing = await _context.Notifications
                                .FirstOrDefaultAsync(n => n.UserId == user.Id
                                    && n.ProjectId == project.Id
                                    && n.Type == "follow-up-reminder");

                            if (existing == null)
                            {
                                _context.Notifications.Add(new Notification
                                {
                                    UserId = user.Id,
                                    ProjectId = project.Id,
                                    Type = "follow-up-reminder",
                                    Message = $"No response yet for \"{project.Title}\". Consider following up with the recruiter."
                                });
                            }
                            else if (existing.IsRead)
                            {
                                existing.IsRead = false;
                                existing.CreatedAt = DateTime.UtcNow;
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}