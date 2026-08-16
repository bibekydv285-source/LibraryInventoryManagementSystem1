using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Dto;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class BookIssueController : Controller
    {
        private readonly AppDbContext _context;

        public BookIssueController(AppDbContext context)
        {
            _context = context;
        }

        // Issued Books list
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var issues = _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.Student)
                .Where(i => i.Status == "Issued")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                issues = issues.Where(i =>
                    i.BookId.ToString() == search ||
                    i.StudentId.ToString() == search ||
                    i.Student.Name.Contains(search));
            }

            ViewBag.IssuedCount = await _context.BookIssues.CountAsync(i => i.Status == "Issued");
            return View(await issues.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Issue()
        {
            ViewBag.Books = await _context.Books.Where(b => b.AvailableQty > 0).ToListAsync();
            ViewBag.Students = await _context.Students.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Issue(IssueBookDto dto)
        {
            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null || book.AvailableQty <= 0)
            {
                ViewBag.ErrorMessage = "Book not available";
                return View(dto);
            }

            var issue = new BookIssue
            {
                BookId = dto.BookId,
                StudentId = dto.StudentId,
                IssueDate = DateTime.Now,
                DueDate = dto.DueDate,
                Status = "Issued"
            };

            book.AvailableQty -= 1;

            _context.BookIssues.Add(issue);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Return Book
        [HttpPost]
        public async Task<IActionResult> Return(int id)
        {
            var issue = await _context.BookIssues.Include(i => i.Book).FirstOrDefaultAsync(i => i.IssueId == id);
            if (issue == null) return NotFound();

            issue.ReturnDate = DateTime.Now;
            issue.Status = "Returned";
            issue.Book.AvailableQty += 1;

            // Auto-create fine if overdue
            if (issue.ReturnDate > issue.DueDate)
            {
                int lateDays = (issue.ReturnDate.Value - issue.DueDate).Days;
                var fine = new Fine
                {
                    IssueId = issue.IssueId,
                    Amount = lateDays * 5, // e.g. Rs.5/day - adjust as needed
                    PaymentStatus = "Pending"
                };
                _context.Fines.Add(fine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Recent Book Issues & Returns
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var recent = await _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.Student)
                .OrderByDescending(i => i.IssueDate)
                .Take(20)
                .ToListAsync();

            ViewBag.RecentCount = recent.Count;
            return View(recent);
        }
    }
}