using LibraryInventoryManagementSystem1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Components
{
    // Renders a dismissible banner for unread student notifications.
    // Invoked from _StudentLayout.cshtml so it shows on every page of
    // the student portal, not just the dedicated Notifications screen.
    public class NotificationBannerViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public NotificationBannerViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Content(string.Empty);

            var unread = await _context.Notifications
                .Where(n => n.StudentId == studentId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToListAsync();

            return View(unread);
        }
    }
}