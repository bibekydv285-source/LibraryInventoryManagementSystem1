# 📚 Library Inventory Management System

A full-featured **Library Inventory Management System** built with **ASP.NET Core MVC (.NET)**, featuring role-based portals for **Admins, Librarians, and Students** — covering book inventory, borrowing/issuing, fines, reservations, notifications, and secure email-based password recovery.

---

## ✨ Features

### 👨‍💼 Admin Portal
- Dashboard with system overview
- Manage notifications (mark as read, archive)
- Send notifications to users
- Manage reservations

### 👩‍🏫 Librarian Portal
- Full CRUD on books (Add, Edit, Delete, View)
- Book issuing and recent activity tracking
- Manage student records

### 🎓 Student Portal
- Personal dashboard
- Browse & search available books
- Borrow books and view borrowed history
- Return history tracking
- Reservations
- Fines overview
- Notifications
- Profile management
- Change password
- Secure forgot/reset password via email (Gmail SMTP + MailKit)

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC |
| Language | C# |
| Database | SQL Server (LocalDB) |
| ORM | Entity Framework Core |
| Email | MailKit (Gmail SMTP) |
| Auth | Session-based authentication |
| Frontend | Razor Views, CSS |

---

## 🏗️ Project Structure

```
LibraryInventoryManagementSystem1/
│
├── Controllers/
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── BookController.cs
│   ├── BookIssueController.cs
│   ├── FineController.cs
│   ├── LibrarianController.cs
│   ├── ReservationController.cs
│   ├── SplashController.cs
│   └── StudentController.cs
│
├── Services/
│   ├── AdminNotificationService.cs
│   ├── IAdminNotificationService.cs
│   ├── NotificationService.cs
│   ├── INotificationService.cs
│   └── EmailService.cs
│
├── Models/
│   ├── AdminNotification.cs
│   ├── Book.cs
│   ├── BookIssue.cs
│   ├── Fine.cs
│   ├── Notification.cs
│   ├── Reservation.cs
│   ├── Student.cs
│   ├── User.cs
│   └── ErrorViewModel.cs
│
├── Dto/
│   ├── BookDto.cs
│   ├── FineDto.cs
│   ├── IssueBookDto.cs
│   ├── SendNotificationDto.cs
│   ├── StudentDto.cs
│   ├── StudentPortalDtos.cs
│   └── UserDto.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Filters/
│   └── AdminOnlyAttribute.cs
│
├── Constants/
│   └── LayoutPaths.cs
│
├── Views/
│   ├── Admin/            (Dashboard, Notifications, Reservations, SendNotification)
│   ├── Auth/             (Login, Register, ForgotPassword, ResetPassword)
│   ├── Book/             (Index, Add, Edit, Delete, Available)
│   ├── BookIssue/        (Issue, Recent)
│   ├── Fine/             (Index, Create, Edit, History)
│   ├── Home/             (Index, Privacy)
│   ├── Librarian/        (Index, Add, Edit, Delete)
│   ├── Student/          (Index, Add, Edit, Delete)
│   ├── StudentDashboard/ (Dashboard, AvailableBooks, BorrowedBooks,
│   │                      Reservations, Fines, Notifications, Profile,
│   │                      ChangePassword, ReturnHistory, Search)
│   ├── Shared/           (_Layout, _AdminLayout, _StudentLayout,
│   │                      _SidebarNav, Error, Components/NotificationBanner)
│   └── Splash/
│
├── wwwroot/
│   ├── css/ (site.css, StudentDashboard.css)
│   ├── js/
│   └── lib/
│
├── appsettings.json
└── Program.cs
```

---

## 🗄️ Data Model (Core Entities)

- **Book** — `BookId, Title, Author, Category, TotalQty, AvailableQty`
- **Student** — student records linked to borrowing activity
- **User** — login accounts (`Username`, `Role`)
- **BookIssue** — tracks issued/returned books
- **Reservation** — book reservation requests
- **Fine** — overdue/fine records
- **Notification / AdminNotification** — in-app notification system

---

## 🔐 Authentication & Roles

- **Session-based authentication** using `Username`, `Role`, and `StudentId`
- Custom `[AdminOnlyAttribute]` filter restricts admin-only actions/pages
- Roles: **Admin**, **Librarian**, **Student**

> ⚠️ **Security note:** Passwords are currently stored in plain text in the `PasswordHash` field. Migrating to a proper hashing algorithm (e.g., BCrypt or ASP.NET Core Identity) is a recommended next step before production use.

---

## 📧 Password Reset Flow

Secure forgot-password/reset-password flow implemented using **Gmail SMTP via MailKit**:

1. User requests reset from `ForgotPassword.cshtml`
2. `EmailService.cs` sends a reset link via Gmail SMTP (App Password)
3. User resets password from `ResetPassword.cshtml`

> Gmail App Password should be stored via **User Secrets** or environment variables — never committed to source control.

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (matching project target framework)
- SQL Server / LocalDB
- Visual Studio 2022 (or VS Code)

### Setup

```bash
# Clone the repository
git clone https://github.com/<your-username>/LibraryInventoryManagementSystem1.git
cd LibraryInventoryManagementSystem1

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

### Configuration

Update `appsettings.json` with your local connection string and email settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryInventoryDb;Trusted_Connection=True;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "" // Use User Secrets, do not commit
  }
}
```

Set secrets locally instead of committing them:

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SenderPassword" "your-gmail-app-password"
```

---

## 🗺️ Roadmap / Future Improvements

- [ ] Hash passwords (replace plain-text `PasswordHash`)
- [ ] Add unit/integration tests
- [ ] Add pagination & search filters to book listings
- [ ] Add API endpoints for mobile/front-end integration
- [ ] Deploy to Azure App Service

---

## 📄 License

This project is for educational purposes. Add a license (e.g., MIT) if you plan to open-source it.

---

## 🙋 Author

Built by **Abhishek** as part of an ongoing full-stack ASP.NET Core MVC learning project.
