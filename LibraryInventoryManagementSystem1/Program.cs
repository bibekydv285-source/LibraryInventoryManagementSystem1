using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConn")));
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<LibraryInventoryManagementSystem1.Services.EmailService>();

builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Ties TempData to the session instead of a separate cookie,
// so stray messages don't survive/leak across pages after session changes.
builder.Services.AddMvc().AddSessionStateTempDataProvider();

// Data Protection is used to encrypt the "Remember me" cookie payload
builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // must be after UseRouting, before UseAuthorization

// ---------------- REMEMBER ME: SILENT SESSION RESTORE ----------------
// If the session is empty (browser was closed / session expired) but a
// valid "RememberMe" cookie exists, re-populate the session from it so
// the user stays logged in without re-entering credentials.
app.Use(async (context, next) =>
{
    var alreadyLoggedIn = !string.IsNullOrEmpty(context.Session.GetString("Username"));

    if (!alreadyLoggedIn && context.Request.Cookies.TryGetValue("RememberMeToken", out var token))
    {
        try
        {
            var protector = context.RequestServices
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("RememberMeCookie");

            var json = protector.Unprotect(token);
            var payload = JsonSerializer.Deserialize<RememberMePayload>(json);

            if (payload != null)
            {
                context.Session.SetString("Username", payload.Username);
                context.Session.SetString("Role", payload.Role);

                if (payload.UserId.HasValue)
                    context.Session.SetInt32("UserId", payload.UserId.Value);

                if (payload.StudentId.HasValue)
                    context.Session.SetInt32("StudentId", payload.StudentId.Value);
            }
        }
        catch
        {
            // Cookie is invalid, tampered, or expired protection key —
            // clear it so we don't keep retrying on every request.
            context.Response.Cookies.Delete("RememberMeToken");
        }
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Splash}/{action=index}/{id?}")
    //pattern: "{controller=Admin}/{action=Dashboard}/{id?}")
    .WithStaticAssets();

app.Run();

// Small record used to serialize what goes inside the encrypted cookie
public record RememberMePayload(string Username, string Role, int? UserId, int? StudentId);
