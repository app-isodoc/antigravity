using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Isodoc.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok(0);

            var count = await _notificationService.GetUnreadCountAsync(user.Id);
            return Ok(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok(new List<Notification>());

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
            return PartialView("_NotificationsList", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }
    }
}
