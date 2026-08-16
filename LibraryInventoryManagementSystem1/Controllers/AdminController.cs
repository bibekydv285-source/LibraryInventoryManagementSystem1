using LibraryInventoryManagementSystem1.Services;
using LibraryInventoryManagementSystem1.Models;
using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;
        private readonly INotificationService _studentNotificationService;

        public AdminController(
            AppDbContext context,
            IAdminNotificationService notificationService,
            INotificationService studentNotificationService)
        {
            _context = context;
            _notificationService = notificationService;
            _studentNotificationService = studentNotificationService;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        private bool IsAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Librarian";
        }

        // ---------------- DASHBOARD ----------------

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.AvailableBooks = await _context.Books.SumAsync(b => b.AvailableQty);
            ViewBag.IssuedBooks = await _context.BookIssues.CountAsync(i => i.Status == "Issued");
            ViewBag.PendingBooks = await _context.Fines.CountAsync(f => f.PaymentStatus == "Pending");
            ViewBag.TotalLibrarians = await _context.Users.CountAsync(u => u.Role == "Librarian");

            ViewBag.RecentActivities = await _context.BookIssues
                .Include(bi => bi.Book)
                .Include(bi => bi.Student)
                .OrderByDescending(bi => bi.ReturnDate ?? bi.IssueDate)
                .Take(8)
                .ToListAsync();

            return View();
        }

        // ---------------- RESERVATIONS ----------------

        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            var reservations = await _context.Reservations
                .Include(r => r.Student)
                .Include(r => r.Book)
                .OrderByDescending(r => r.ReservedDate)
                .ToListAsync();

            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> FulfillReservation(int id)
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction(nameof(Reservations));
            }

            if (reservation.Status == "Fulfilled")
            {
                TempData["ErrorMessage"] = "This reservation has already been fulfilled.";
                return RedirectToAction(nameof(Reservations));
            }

            if (reservation.Book == null || reservation.Book.AvailableQty <= 0)
            {
                TempData["ErrorMessage"] = "No available copies to issue for this book.";
                return RedirectToAction(nameof(Reservations));
            }

            // Actually issue the book
            var issue = new BookIssue
            {
                BookId = reservation.BookId,
                StudentId = reservation.StudentId,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14), // adjust loan period as needed
                Status = "Issued"
            };
            _context.BookIssues.Add(issue);

            // Decrement available stock
            reservation.Book.AvailableQty -= 1;

            // Mark reservation fulfilled
            reservation.Status = "Fulfilled";
            reservation.FulfilledOn = DateTime.Now;

            await _context.SaveChangesAsync();

            // Notify the admin panel (existing system notification)
            await _notificationService.NotifyAsync(
                title: "Reservation Fulfilled",
                message: $"Your reserved book \"{reservation.Book.Title}\" has been issued to you.",
                type: AdminNotificationType.System,
                studentId: reservation.StudentId,
                bookId: reservation.BookId,
                reservationId: reservation.Id,
                triggeredByUser: "Admin",
                triggeredByRole: "Admin"
            );

            // Also notify the student directly so it shows on their dashboard
            await _studentNotificationService.SendToStudentAsync(
                reservation.StudentId,
                title: "Reservation Fulfilled",
                message: $"Your reserved book \"{reservation.Book.Title}\" has been issued to you. Please check your borrowed books for the due date.",
                type: "Reservation"
            );

            TempData["SuccessMessage"] = $"Reservation for \"{reservation.Book.Title}\" fulfilled and book issued.";
            return RedirectToAction(nameof(Reservations));
        }

        // ---------------- ADMIN NOTIFICATIONS (system alerts to admin) ----------------

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            var notifications = await _notificationService.GetAllAsync();
            return View(notifications);
        }

        [HttpGet]
        public async Task<JsonResult> UnreadNotificationCount()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return Json(new { count = 0 });

            var count = await _notificationService.GetUnreadCountAsync();
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> RecentNotifications()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return PartialView("_NotificationListPartial", new List<AdminNotification>());

            var notifications = await _notificationService.GetRecentAsync(10);
            return PartialView("_NotificationListPartial", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return Unauthorized();

            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveNotification(int id)
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return Unauthorized();

            await _notificationService.ArchiveAsync(id);
            return Ok();
        }

        // ---------------- SEND NOTIFICATION TO STUDENT(S) ----------------

        // GET: /Admin/SendNotification
        [HttpGet]
        public async Task<IActionResult> SendNotification()
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            ViewBag.Students = await _context.Students
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(new SendNotificationDto());
        }

        // POST: /Admin/SendNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(SendNotificationDto dto)
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return RedirectToAction("Login", "Auth");

            if (!dto.SendToAll && dto.StudentId == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a student, or choose 'Send to all students'.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Students = await _context.Students.OrderBy(s => s.Name).ToListAsync();
                return View(dto);
            }

            if (dto.SendToAll)
            {
                await _studentNotificationService.SendToAllStudentsAsync(dto.Title, dto.Message, dto.Type);
                TempData["SuccessMessage"] = "Notification sent to all students.";
            }
            else
            {
                await _studentNotificationService.SendToStudentAsync(dto.StudentId!.Value, dto.Title, dto.Message, dto.Type);
                TempData["SuccessMessage"] = "Notification sent to the selected student.";
            }

            return RedirectToAction(nameof(SendNotification));
        }

        // ---------------- GLOBAL SEARCH ----------------

        [HttpGet]
        public async Task<JsonResult> GlobalSearch(string q)
        {
            if (!IsLoggedIn() || !IsAdminRole())
                return Json(new { books = new object[0], students = new object[0] });

            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(new { books = new object[0], students = new object[0] });

            var term = q.Trim();

            var books = await _context.Books
                .Where(b => EF.Functions.Like(b.Title, $"%{term}%")
                         || EF.Functions.Like(b.Author, $"%{term}%")
                         || EF.Functions.Like(b.ISBN, $"%{term}%"))
                .OrderBy(b => b.Title)
                .Take(6)
                .Select(b => new
                {
                    id = b.BookId,
                    title = b.Title,
                    subtitle = b.Author,
                    url = Url.Action("Edit", "Book", new { id = b.BookId })
                })
                .ToListAsync();

            var students = await _context.Students
                .Where(s => EF.Functions.Like(s.Name, $"%{term}%")
                         || EF.Functions.Like(s.StudentCode, $"%{term}%")
                         || EF.Functions.Like(s.Email, $"%{term}%"))
                .OrderBy(s => s.Name)
                .Take(6)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Name,
                    subtitle = s.StudentCode,
                    url = Url.Action("Edit", "Student", new { id = s.Id })
                })
                .ToListAsync();

            return Json(new { books, students });
        }
    }
}