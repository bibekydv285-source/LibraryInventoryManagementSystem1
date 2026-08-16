using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Filters;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Controllers
{
    [AdminOnly]
    public class LibrarianController : Controller
    {
        private readonly AppDbContext _context;

        public LibrarianController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Librarian
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var librarians = await _context.Users
                .Where(u => u.Role == "Librarian")
                .ToListAsync();

            ViewBag.TotalLibrarians = librarians.Count;
            return View(librarians);
        }

        // GET: /Librarian/Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: /Librarian/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string username, string email, string password, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            bool exists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
            if (exists)
            {
                ModelState.AddModelError("", "Username or Email already in use.");
                return View();
            }

            var librarian = new User
            {
                Username = username,
                Email = email,
                Password = password, // TODO: hash this if you hash elsewhere (e.g. in Register)
                PhoneNumber = phoneNumber,
                Role = "Librarian"
            };

            _context.Users.Add(librarian);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Librarian added successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Librarian/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var librarian = await _context.Users.FindAsync(id);
            if (librarian == null || librarian.Role != "Librarian") return NotFound();

            return View(librarian);
        }

        // POST: /Librarian/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string username, string email, string phoneNumber)
        {
            var librarian = await _context.Users.FindAsync(id);
            if (librarian == null || librarian.Role != "Librarian") return NotFound();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Username and Email are required.");
                return View(librarian);
            }

            bool duplicate = await _context.Users
                .AnyAsync(u => u.Id != id && (u.Username == username || u.Email == email));
            if (duplicate)
            {
                ModelState.AddModelError("", "Username or Email already in use by another user.");
                return View(librarian);
            }

            librarian.Username = username;
            librarian.Email = email;
            librarian.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Librarian updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Librarian/Delete/5  (confirmation page)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var librarian = await _context.Users.FindAsync(id);
            if (librarian == null || librarian.Role != "Librarian") return NotFound();

            return View(librarian);
        }

        // POST: /Librarian/Delete/5 (actual delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var librarian = await _context.Users.FindAsync(id);
            if (librarian == null) return NotFound();

            _context.Users.Remove(librarian);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Librarian deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}