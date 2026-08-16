using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Models;
using LibraryInventoryManagementSystem1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LibraryInventoryManagementSystem1.Controllers
{
    // Handles student-side book reservations (Search page "Reserve" button,
    // and the "My Reservations" list under the Student portal).
    // Every action that changes reservation state also notifies Admin
    // via IAdminNotificationService so it shows up on the Admin dashboard bell.
    public class ReservationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;

        public ReservationController(AppDbContext context, IAdminNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: /Reservation/Index
        // "My Reservations" page shown in the student portal sidebar
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.ReservedDate)
                .ToListAsync();

            return View(reservations);
        }

        // POST: /Reservation/Reserve
        // Called from the Search page when a student reserves an unavailable book
        [HttpPost]
        public async Task<IActionResult> Reserve(int bookId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            var student = await _context.Students.FindAsync(studentId);
            var book = await _context.Books.FindAsync(bookId);

            if (student == null || book == null)
            {
                TempData["ErrorMessage"] = "Unable to process this reservation. Please try again.";
                return RedirectToAction("Search", "Book");
            }

            // Prevent duplicate pending reservations for the same student + book
            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.StudentId == studentId && r.BookId == bookId && r.Status == "Pending");

            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = $"You already have a pending reservation for \"{book.Title}\".";
                return RedirectToAction("Search", "Book");
            }

            var reservation = new Reservation
            {
                StudentId = studentId.Value,
                BookId = bookId,
                ReservedDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Notify Admin dashboard
            await _notificationService.NotifyAsync(
                title: "New Book Reservation",
                message: $"{student.Name} reserved \"{book.Title}\"",
                type: AdminNotificationType.BookReserved,
                studentId: student.Id,
                bookId: book.BookId,
                reservationId: reservation.Id,
                triggeredByUser: student.Email,
                triggeredByRole: "Student"
            );

            TempData["SuccessMessage"] = $"\"{book.Title}\" has been reserved. We'll notify you when it's available.";
            return RedirectToAction("Index", "Reservation");
        }

        // POST: /Reservation/Cancel
        // Called from the "Cancel" button on the My Reservations page
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("Index");
            }

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending reservations can be cancelled.";
                return RedirectToAction("Index");
            }

            reservation.Status = "Cancelled";
            reservation.CancelledOn = DateTime.Now;
            await _context.SaveChangesAsync();

            var studentName = reservation.Student?.Name ?? "A student";
            var bookTitle = reservation.Book?.Title ?? "a book";
            var studentEmail = reservation.Student?.Email ?? string.Empty;

            // Notify Admin dashboard
            await _notificationService.NotifyAsync(
                title: "Reservation Cancelled",
                message: $"{studentName} cancelled the reservation for \"{bookTitle}\"",
                type: AdminNotificationType.System,
                studentId: reservation.StudentId,
                bookId: reservation.BookId,
                reservationId: reservation.Id,
                triggeredByUser: studentEmail,
                triggeredByRole: "Student",
                priority: "Low"
            );

            TempData["SuccessMessage"] = "Reservation cancelled.";
            return RedirectToAction("Index");
        }

        // ---------------- ADMIN-SIDE ACTIONS ----------------
        // These live here rather than AdminController to keep all
        // reservation logic (student + admin) in one place.

        // GET: /Reservation/AdminIndex
        // Admin view of all pending reservations, to fulfil them once a copy is available
        [HttpGet]
        public async Task<IActionResult> AdminIndex()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Student)
                .Include(r => r.Book)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.ReservedDate)
                .ToListAsync();

            return View(reservations);
        }

        // POST: /Reservation/Fulfill
        // Admin marks a reservation fulfilled once the book is issued to the student
        [HttpPost]
        public async Task<IActionResult> Fulfill(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("AdminIndex");
            }

            reservation.Status = "Fulfilled";
            reservation.FulfilledOn = DateTime.Now;
            await _context.SaveChangesAsync();

            var bookTitle = reservation.Book?.Title ?? "the book";

            TempData["SuccessMessage"] = $"Reservation for \"{bookTitle}\" marked as fulfilled.";
            return RedirectToAction("AdminIndex");
        }
    }
}