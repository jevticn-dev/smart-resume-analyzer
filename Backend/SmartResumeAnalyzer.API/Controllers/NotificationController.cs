using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.Core.DTOs.Notification;
using SmartResumeAnalyzer.Core.Interfaces;
using System.Security.Claims;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetNotificationsAsync(userId.Value);
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAsReadAsync(id, userId.Value);
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId.Value);
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return null;
            return userId;
        }
    }
}