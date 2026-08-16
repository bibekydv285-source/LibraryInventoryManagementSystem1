using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class FineController : Controller
    {
        private readonly AppDbContext _context;
        private const decimal FinePerDay = 10m; // Rs. 10/day overdue - adjust as needed

        public FineController(AppDbContext context)
        {
            _context = context;
        }

        // Pending Books (Fine list)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fines = await _context.Fines
                .Include(f => f.BookIssue).ThenInclude(i => i!.Book)
                .Include(f => f.BookIssue).ThenInclude(i => i!.Student)
                .Where(f => f.PaymentStatus == "Pending")
                .ToListAsync();

            return View(fines);
        }

        // GET: Add Fine page (search UI)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // AJAX: live search for overdue/issued books to attach a fine to
        [HttpGet]
        public async Task<IActionResult> SearchIssues(string term)
        {
            term ??= "";

            var issueIdsWithPendingFine = _context.Fines
                .Where(f => f.PaymentStatus == "Pending")
                .Select(f => f.IssueId);

            var results = await _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.Student)
                .Where(i => i.ReturnDate == null) // still issued out
                .Where(i => !issueIdsWithPendingFine.Contains(i.IssueId))
                .Where(i =>
                    (i.Book != null && i.Book.Title.Contains(term)) ||
                    (i.Student != null && i.Student.Name.Contains(term)))
                .OrderBy(i => i.DueDate)
                .Take(15)
                .Select(i => new
                {
                    issueId = i.IssueId,
                    bookTitle = i.Book != null ? i.Book.Title : "",
                    studentName = i.Student != null ? i.Student.Name : "",
                    dueDate = i.DueDate,
                    daysOverdue = EF.Functions.DateDiffDay(i.DueDate, DateTime.Now) > 0
                        ? EF.Functions.DateDiffDay(i.DueDate, DateTime.Now)
                        : 0
                })
                .ToListAsync();

            var final = results.Select(r => new
            {
                r.issueId,
                r.bookTitle,
                r.studentName,
                dueDate = r.dueDate.ToString("dd-MMM-yyyy"),
                daysOverdue = r.daysOverdue,
                suggestedFine = r.daysOverdue * FinePerDay
            });

            return Json(final);
        }

        // POST: create the fine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int issueId, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction("Create");
            }

            bool alreadyPending = await _context.Fines
                .AnyAsync(f => f.IssueId == issueId && f.PaymentStatus == "Pending");

            if (alreadyPending)
            {
                TempData["Error"] = "A pending fine already exists for this issue.";
                return RedirectToAction("Create");
            }

            var fine = new Fine
            {
                IssueId = issueId,
                Amount = amount,
                PaymentStatus = "Pending"
            };

            _context.Fines.Add(fine);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fine added successfully.";
            return RedirectToAction("Index");
        }

        // GET: Edit fine amount
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var fine = await _context.Fines
                .Include(f => f.BookIssue).ThenInclude(i => i!.Book)
                .Include(f => f.BookIssue).ThenInclude(i => i!.Student)
                .FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null) return NotFound();
            return View(fine);
        }

        // POST: Save edited amount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, decimal amount)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null) return NotFound();

            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction("Edit", new { id });
            }

            fine.Amount = amount;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fine updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: Delete fine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null) return NotFound();

            _context.Fines.Remove(fine);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fine deleted successfully.";
            return RedirectToAction("Index");
        }

        // Record Payment
        [HttpPost]
        public async Task<IActionResult> RecordPayment(int id)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null) return NotFound();

            fine.PaymentStatus = "Paid";
            fine.PaymentDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Fine History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _context.Fines
                .Include(f => f.BookIssue).ThenInclude(i => i!.Book)
                .Include(f => f.BookIssue).ThenInclude(i => i!.Student)
                .OrderByDescending(f => f.PaymentDate)
                .ToListAsync();

            return View(history);
        }
    }
}