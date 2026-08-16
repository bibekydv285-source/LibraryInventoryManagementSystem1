using System;
using System.Text.Json;
using System.Threading.Tasks;
using LibraryInventoryManagementSystem1.Dto;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Services;
using Microsoft.AspNetCore.Http;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;
        private readonly IDataProtector _rememberMeProtector;
        private readonly EmailService _emailService;

        public AuthController(
            AppDbContext context,
            IAdminNotificationService notificationService,
            IDataProtectionProvider dataProtectionProvider,
            EmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _rememberMeProtector = dataProtectionProvider.CreateProtector("RememberMeCookie");
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                ViewBag.ErrorMessage = "Kindly fill all the details";
                return View("Register");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "User with this email already exists.";
                return View("Register");
            }

            var user = new User
            {
                Email = dto.Email,
                Password = dto.Password,
                Username = dto.Email.Split('@')[0],
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Admin" : dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Notify admins that a new staff account was created
            await _notificationService.NotifyAsync(
                title: "New Staff Account Created",
                message: $"A new {user.Role} account was registered: {user.Email}",
                type: AdminNotificationType.System,
                triggeredByUser: user.Email,
                triggeredByRole: user.Role
            );

            TempData["SuccessMessage"] = "Account created successfully. Please log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(UserDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                ViewBag.ErrorMessage = "Kindly Fill All The Details";
                return View("Login");
            }

            var selectedRole = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim();
            bool wantsAdmin = selectedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (wantsAdmin)
            {
                var staff = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

                if (staff == null)
                {
                    ViewBag.ErrorMessage = "No admin account found with this email.";
                    return View("Login");
                }

                if (staff.Password != dto.Password)
                {
                    ViewBag.ErrorMessage = "Incorrect Password";
                    return View("Login");
                }

                if (staff.Role != "Admin" && staff.Role != "Librarian")
                {
                    ViewBag.ErrorMessage = "This account does not have admin access.";
                    return View("Login");
                }

                HttpContext.Session.SetString("Username", staff.Username);
                HttpContext.Session.SetInt32("UserId", staff.Id);
                HttpContext.Session.SetString("Role", staff.Role);

                if (dto.RememberMe)
                {
                    SetRememberMeCookie(staff.Username, staff.Role, userId: staff.Id, studentId: null);
                }

                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == dto.Email);

                if (student == null)
                {
                    var staffCheck = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
                    ViewBag.ErrorMessage = staffCheck != null
                        ? "This is an admin account. Please switch to the Admin tab to sign in."
                        : "No account found with this email.";
                    return View("Login");
                }

                if (student.PasswordHash != dto.Password)
                {
                    ViewBag.ErrorMessage = "Incorrect Password";
                    return View("Login");
                }

                HttpContext.Session.SetString("Username", student.Username);
                HttpContext.Session.SetInt32("StudentId", student.Id);
                HttpContext.Session.SetString("Role", "Student");

                if (dto.RememberMe)
                {
                    SetRememberMeCookie(student.Username, "Student", userId: null, studentId: student.Id);
                }

                return RedirectToAction("Dashboard", "Student");
            }
        }

        private void SetRememberMeCookie(string username, string role, int? userId, int? studentId)
        {
            var payload = new RememberMePayload(username, role, userId, studentId);
            var json = JsonSerializer.Serialize(payload);
            var encrypted = _rememberMeProtector.Protect(json);

            Response.Cookies.Append("RememberMeToken", encrypted, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMeToken");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Always show the same message, whether or not the account exists,
            // so the form can't be used to check which emails are registered.
            const string genericSuccessMessage =
                "If an account with that email exists, a reset code has been sent to it.";

            if (user == null)
            {
                TempData["SuccessMessage"] = genericSuccessMessage;
                return RedirectToAction("ForgotPassword");
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.ResetCode = code;
            user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    toEmail: user.Email,
                    subject: "Your Password Reset Code",
                    body: $"Your password reset code is: {code}\n\nThis code will expire in 10 minutes. " +
                          "If you did not request a password reset, you can safely ignore this email."
                );
            }
            catch (Exception ex)
            {
                // TEMPORARY DEBUG: shows the real error so you can diagnose the SMTP issue.
                // Replace with a generic message once this is working.
                ViewBag.ErrorMessage = $"DEBUG: {ex.Message}";
                return View();
            }

            TempData["Email"] = user.Email;
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            ViewBag.Email = TempData["Email"];
            TempData.Keep("Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string code, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.ResetCode != code || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                ViewBag.ErrorMessage = "Invalid or expired code.";
                ViewBag.Email = email;
                return View();
            }

            user.Password = newPassword;
            user.ResetCode = null;
            user.ResetCodeExpiry = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password reset successfully. Please log in.";
            return RedirectToAction("Login");
        }
    }
}