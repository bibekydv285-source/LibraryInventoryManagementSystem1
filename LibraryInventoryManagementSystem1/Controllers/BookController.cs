using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Dto;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        // Total Books & Details
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.Category.Contains(search) ||
                    b.BookId.ToString() == search);
            }

            ViewBag.TotalBooks = await _context.Books.CountAsync();
            return View(await books.ToListAsync());
        }

        // Available Books
        [HttpGet]
        public async Task<IActionResult> Available(string search)
        {
            var books = _context.Books.Where(b => b.AvailableQty > 0).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            return View(await books.ToListAsync());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(BookDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Author))
            {
                ViewBag.ErrorMessage = "Kindly fill all required fields";
                return View();
            }

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Category = dto.Category,
                TotalQty = dto.TotalQty,
                AvailableQty = dto.TotalQty
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, BookDto dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            int issuedCount = book.TotalQty - book.AvailableQty;

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Category = dto.Category;
            book.TotalQty = dto.TotalQty;
            book.AvailableQty = dto.TotalQty - issuedCount;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}