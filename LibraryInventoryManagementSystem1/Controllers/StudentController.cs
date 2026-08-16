using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Dto;
using LibraryInventoryManagementSystem1.Models;
using LibraryInventoryManagementSystem1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;

        public StudentController(AppDbContext context, IAdminNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        private int? GetCurrentStudentId()
        {
            return HttpContext.Session.GetInt32("StudentId");
        }

        private async Task<string> GenerateUniqueStudentCodeAsync()
        {
            int year = DateTime.Now.Year;
            string code;
            bool exists;
            int count = await _context.Students
                .CountAsync(s => s.StudentCode.StartsWith($"STU-{year}-"));

            do
            {
                count++;
                code = $"STU-{year}-{count:D4}";
                exists = await _context.Students.AnyAsync(s => s.StudentCode == code);
            }
            while (exists);

            return code;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsAdmin())
                return RedirectToAction("Add");

            var students = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students.Where(s =>
                    s.Name.Contains(search) ||
                    s.Email.Contains(search) ||
                    (s.Course != null && s.Course.Contains(search)));
            }

            ViewBag.SearchTerm = search;

            return View(await students.ToListAsync());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new StudentDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(StudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");

                ViewBag.ErrorMessage = "Please fix: " + string.Join(" | ", errors);
                return View(dto);
            }

            bool usernameTaken = await _context.Students.AnyAsync(s => s.Username == dto.Username);
            if (usernameTaken)
            {
                ViewBag.ErrorMessage = "That username is already taken.";
                return View(dto);
            }

            bool emailTaken = await _context.Students.AnyAsync(s => s.Email == dto.Email);
            if (emailTaken)
            {
                ViewBag.ErrorMessage = "That email is already registered.";
                return View(dto);
            }

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Username = dto.Username,
                Address = dto.Address,
                Course = dto.Course,
                Age = dto.Age,
                PasswordHash = dto.Password,
                StudentCode = await GenerateUniqueStudentCodeAsync()
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Registration successful! Your Student ID is {student.StudentCode}. Please log in with your email and password.";

            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet("Student-Dashboard/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var issuedBooks = await _context.BookIssues
                .Include(i => i.Book)
                .Where(i => i.StudentId == student.Id && i.Status == "Issued")
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            var pendingFines = await _context.Fines
                .Include(f => f.BookIssue)
                .Where(f => f.BookIssue!.StudentId == student.Id && f.PaymentStatus == "Pending")
                .ToListAsync();

            ViewBag.StudentName = student.Name;
            ViewBag.IssuedCount = issuedBooks.Count;
            ViewBag.PendingFineTotal = pendingFines.Sum(f => f.Amount);
            ViewBag.IssuedBooks = issuedBooks;

            return View("~/Views/Student Dashboard/Dashboard.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsAdmin())
                return RedirectToAction("Add");

            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            ViewBag.StudentId = id;

            var dto = new StudentDto
            {
                Name = student.Name,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Username = student.Username,
                Address = student.Address,
                Course = student.Course,
                Age = student.Age
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentDto dto)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsAdmin())
                return RedirectToAction("Add");

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill in all required fields correctly.";
                ViewBag.StudentId = id;
                return View(dto);
            }

            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            student.Name = dto.Name;
            student.Email = dto.Email;
            student.PhoneNumber = dto.PhoneNumber;
            student.DateOfBirth = dto.DateOfBirth;
            student.Gender = dto.Gender;
            student.Username = dto.Username;
            student.Address = dto.Address;
            student.Course = dto.Course;
            student.Age = dto.Age;

            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsAdmin())
                return RedirectToAction("Add");

            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsAdmin())
                return RedirectToAction("Add");

            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Student-Dashboard/Profile")]
        public async Task<IActionResult> Profile()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var dto = new ProfileDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                Name = student.Name,
                Username = student.Username,
                Course = student.Course,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address
            };

            return View("~/Views/Student Dashboard/Profile.cshtml", dto);
        }

        [HttpPost("Student-Dashboard/Profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View("~/Views/Student Dashboard/Profile.cshtml", dto);

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return NotFound();

            student.Email = dto.Email;
            student.PhoneNumber = dto.PhoneNumber;
            student.Address = dto.Address;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("Student-Dashboard/Search")]
        public async Task<IActionResult> Search(string? query, string? category)
        {
            if (GetCurrentStudentId() == null) return RedirectToAction("Login", "Auth");

            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(query) ||
                    b.Author.Contains(query));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                booksQuery = booksQuery.Where(b => b.Category == category);
            }

            var dto = new BookSearchDto
            {
                Query = query,
                Category = category,
                Results = await booksQuery.OrderBy(b => b.Title).ToListAsync()
            };

            ViewBag.Categories = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View("~/Views/Student Dashboard/Search.cshtml", dto);
        }

        [HttpGet("Student-Dashboard/AvailableBooks")]
        public async Task<IActionResult> AvailableBooks()
        {
            if (GetCurrentStudentId() == null) return RedirectToAction("Login", "Auth");

            var books = await _context.Books
                .Where(b => b.AvailableQty > 0)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View("~/Views/Student Dashboard/AvailableBooks.cshtml", books);
        }

        [HttpGet("Student-Dashboard/BorrowedBooks")]
        public async Task<IActionResult> BorrowedBooks()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var issues = await _context.BookIssues
                .Include(i => i.Book)
                .Where(i => i.StudentId == studentId && i.Status == "Issued")
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            return View("~/Views/Student Dashboard/BorrowedBooks.cshtml", issues);
        }

        [HttpGet("Student-Dashboard/ReturnHistory")]
        public async Task<IActionResult> ReturnHistory()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var returned = await _context.BookIssues
                .Include(i => i.Book)
                .Where(i => i.StudentId == studentId && i.Status == "Returned")
                .OrderByDescending(i => i.DueDate)
                .ToListAsync();

            return View("~/Views/Student Dashboard/ReturnHistory.cshtml", returned);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int issueId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            var issue = await _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.Student)
                .FirstOrDefaultAsync(i => i.IssueId == issueId && i.StudentId == studentId.Value);

            if (issue == null)
            {
                TempData["ErrorMessage"] = "Book record not found.";
                return RedirectToAction(nameof(BorrowedBooks));
            }

            if (issue.Status == "Returned")
            {
                TempData["ErrorMessage"] = "This book has already been returned.";
                return RedirectToAction(nameof(BorrowedBooks));
            }

            issue.Status = "Returned";
            issue.ReturnDate = DateTime.Now;

            if (issue.Book != null)
                issue.Book.AvailableQty += 1;

            await _context.SaveChangesAsync();

            await _notificationService.NotifyAsync(
                title: "Book Returned",
                message: $"{issue.Student?.Name} returned \"{issue.Book?.Title}\".",
                type: AdminNotificationType.System,
                studentId: studentId,
                bookId: issue.BookId,
                triggeredByUser: issue.Student?.Username,
                triggeredByRole: "Student"
            );

            TempData["SuccessMessage"] = $"\"{issue.Book?.Title}\" marked as returned.";
            return RedirectToAction(nameof(BorrowedBooks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            const int maxBooksAllowed = 5;

            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            string redirectTarget = Request.Headers["Referer"].ToString();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            int currentlyIssuedCount = await _context.BookIssues
                .CountAsync(i => i.StudentId == studentId && i.Status == "Issued");

            if (currentlyIssuedCount >= maxBooksAllowed)
            {
                TempData["ErrorMessage"] =
                    $"You already have {currentlyIssuedCount} books issued. Return one before borrowing another (limit: {maxBooksAllowed}).";
                return Redirect(string.IsNullOrEmpty(redirectTarget) ? Url.Action(nameof(AvailableBooks))! : redirectTarget);
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return Redirect(string.IsNullOrEmpty(redirectTarget) ? Url.Action(nameof(AvailableBooks))! : redirectTarget);
            }

            if (book.AvailableQty <= 0)
            {
                TempData["ErrorMessage"] = $"\"{book.Title}\" is not currently available.";
                return Redirect(string.IsNullOrEmpty(redirectTarget) ? Url.Action(nameof(AvailableBooks))! : redirectTarget);
            }

            var newIssue = new BookIssue
            {
                StudentId = studentId.Value,
                BookId = book.BookId,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Issued"
            };

            book.AvailableQty -= 1;

            _context.BookIssues.Add(newIssue);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyAsync(
                title: "Book Borrowed",
                message: $"{student.Name} borrowed \"{book.Title}\".",
                type: AdminNotificationType.System,
                studentId: studentId,
                bookId: book.BookId,
                triggeredByUser: student.Username,
                triggeredByRole: "Student"
            );

            TempData["SuccessMessage"] = $"\"{book.Title}\" borrowed successfully. Due back {newIssue.DueDate:dd-MMM-yyyy}.";
            return Redirect(string.IsNullOrEmpty(redirectTarget) ? Url.Action(nameof(AvailableBooks))! : redirectTarget);
        }

        [HttpGet("Student-Dashboard/Reservations")]
        public async Task<IActionResult> Reservations()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.ReservedDate)
                .ToListAsync();

            ViewBag.AllBooks = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View("~/Views/Student Dashboard/Reservations.cshtml", reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int bookId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            bool alreadyReserved = await _context.Reservations.AnyAsync(r =>
                r.StudentId == studentId && r.BookId == bookId && r.Status == "Pending");

            if (!alreadyReserved)
            {
                var reservation = new Reservation
                {
                    StudentId = studentId.Value,
                    BookId = bookId,
                    ReservedDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                var student = await _context.Students.FindAsync(studentId);
                var book = await _context.Books.FindAsync(bookId);

                await _notificationService.NotifyAsync(
                    title: "New Book Reservation",
                    message: $"{student?.Name} reserved \"{book?.Title}\"",
                    type: AdminNotificationType.BookReserved,
                    studentId: studentId,
                    bookId: bookId,
                    reservationId: reservation.Id,
                    triggeredByUser: student?.Email,
                    triggeredByRole: "Student"
                );

                TempData["SuccessMessage"] = "Book reserved. You'll be notified when it's approved.";
            }
            else
            {
                TempData["SuccessMessage"] = "You already have a pending reservation for this book.";
            }

            return RedirectToAction(nameof(Reservations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation != null && reservation.StudentId == studentId)
            {
                reservation.Status = "Cancelled";
                await _context.SaveChangesAsync();

                await _notificationService.NotifyAsync(
                    title: "Reservation Cancelled",
                    message: $"{reservation.Student?.Name} cancelled the reservation for \"{reservation.Book?.Title}\"",
                    type: AdminNotificationType.System,
                    studentId: reservation.StudentId,
                    bookId: reservation.BookId,
                    reservationId: reservation.Id,
                    triggeredByUser: reservation.Student?.Email,
                    triggeredByRole: "Student",
                    priority: "Low"
                );

                TempData["SuccessMessage"] = "Reservation cancelled.";
            }

            return RedirectToAction(nameof(Reservations));
        }

        [HttpGet("Student-Dashboard/Fines")]
        public async Task<IActionResult> Fines()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var fines = await _context.Fines
                .Include(f => f.BookIssue)!.ThenInclude(i => i!.Book)
                .Where(f => f.BookIssue!.StudentId == studentId)
                .OrderByDescending(f => f.FineId)
                .ToListAsync();

            return View("~/Views/Student Dashboard/Fines.cshtml", fines);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFine(int fineId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var fine = await _context.Fines
                .Include(f => f.BookIssue)!.ThenInclude(i => i!.Book)
                .Include(f => f.BookIssue)!.ThenInclude(i => i!.Student)
                .FirstOrDefaultAsync(f => f.FineId == fineId && f.BookIssue!.StudentId == studentId);

            if (fine == null)
            {
                TempData["ErrorMessage"] = "Fine record not found.";
                return RedirectToAction(nameof(Fines));
            }

            if (fine.PaymentStatus == "Paid")
            {
                TempData["ErrorMessage"] = "This fine has already been paid.";
                return RedirectToAction(nameof(Fines));
            }

            fine.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();

            await _notificationService.NotifyAsync(
                title: "Fine Paid",
                message: $"{fine.BookIssue?.Student?.Name} paid a fine of {fine.Amount:C} for \"{fine.BookIssue?.Book?.Title}\".",
                type: AdminNotificationType.System,
                studentId: studentId,
                bookId: fine.BookIssue?.BookId,
                triggeredByUser: fine.BookIssue?.Student?.Username,
                triggeredByRole: "Student"
            );

            TempData["SuccessMessage"] = $"Payment of {fine.Amount:C} received. Thank you!";
            return RedirectToAction(nameof(Fines));
        }

        // ---------- NOTIFICATIONS ----------
        // Does NOT bulk-mark everything as read on page load. Marking as
        // read happens per-item via MarkNotificationRead, or in bulk via
        // MarkAllNotificationsRead, both triggered from the bell dropdown
        // or this full-history page.
        [HttpGet("Student-Dashboard/Notifications")]
        public async Task<IActionResult> Notifications()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var notifications = await _context.Notifications
                .Where(n => n.StudentId == studentId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View("~/Views/Student Dashboard/Notifications.cshtml", notifications);
        }

        public class MarkNotificationReadRequest
        {
            public int Id { get; set; }
        }

        // Marks a single notification as read. Called from both the bell
        // dropdown (NotificationBannerViewComponent) and the full
        // Notifications.cshtml page's detail modal.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Consumes("application/json")]
        public async Task<IActionResult> MarkNotificationRead([FromBody] MarkNotificationReadRequest request)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.Id && n.StudentId == studentId);

            if (notification == null) return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // Marks every unread notification for the current student as read
        // in one call. Called from the bell dropdown's "Mark all as read"
        // button.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => n.StudentId == studentId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                unread.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("Student-Dashboard/ChangePassword")]
        public IActionResult ChangePassword()
        {
            if (GetCurrentStudentId() == null) return RedirectToAction("Login", "Auth");

            return View("~/Views/Student Dashboard/ChangePassword.cshtml", new ChangePasswordDto());
        }

        [HttpPost("Student-Dashboard/ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View("~/Views/Student Dashboard/ChangePassword.cshtml", dto);

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return NotFound();

            if (student.PasswordHash != dto.CurrentPassword)
            {
                ModelState.AddModelError(nameof(dto.CurrentPassword), "Current password is incorrect.");
                return View("~/Views/Student Dashboard/ChangePassword.cshtml", dto);
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(dto.ConfirmPassword), "Passwords do not match.");
                return View("~/Views/Student Dashboard/ChangePassword.cshtml", dto);
            }

            student.PasswordHash = dto.NewPassword;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(ChangePassword));
        }
    }
}