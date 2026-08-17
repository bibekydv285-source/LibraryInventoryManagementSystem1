# 📚 Library Inventory Management System

A full-featured **Library Inventory Management System** built with **ASP.NET Core MVC (.NET)**, featuring role-based portals for **Admins, Librarians, and Students** covering book inventory, borrowing/issuing, fines, reservations, real-time notifications, and secure email-based password recovery.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/SQL%20Server-EF%20Core-CC2927?style=for-the-badge&logo=microsoftsqlserver" />
  <img src="https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge" />
</p>

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Data Model](#-data-model-core-entities)
- [Authentication & Roles](#-authentication--roles)
- [Password Reset Flow](#-password-reset-flow)
- [Getting Started](#-getting-started)
- [Roadmap](#-roadmap--future-improvements)
- [License](#-license)
- [Author](#-author)

---

## 🧾 Overview

This system digitizes day-to-day library operations across three role-based portals:

| Role | Purpose |
|---|---|
| **Admin** | Oversees the system  dashboard analytics, notifications, and reservation management |
| **Librarian** | Manages book inventory and issues/records student borrowing activity |
| **Student** | Browses books, borrows/returns, tracks fines, and manages their profile |

---

## ✨ Features

### 👨‍💼 Admin Portal
- 📊 Dashboard with system-wide overview (strongly-typed ViewModels)
- 🔔 Real-time bell-icon notification system with unread count
- 📥 Manage notifications mark as read, archive
- 📤 Send notifications to users
- 📅 Manage reservations

### 👩‍🏫 Librarian Portal
- 📚 Full CRUD on books (Add, Edit, Delete, View)
- 🔁 Book issuing and recent activity tracking
- 🎓 Manage student records

### 🎓 Student Portal
- 🏠 Personal dashboard
- 🔍 Browse & search available books
- 📖 Borrow books and view borrowed history
- ↩️ Return history tracking
- 📌 Reservations
- 💰 Fines overview
- 🔔 Notifications
- 👤 Profile management
- 🔑 Change password
- 📧 Secure forgot/reset password via email (Gmail SMTP + MailKit)

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC |
| Language | C# |
| Database | SQL Server (LocalDB) |
| ORM | Entity Framework Core |
| Email | MailKit (Gmail SMTP) |
| Auth | Session based authentication |
| Frontend | Razor Views, Bootstrap, CSS |
| IDE | Visual Studio 2022 |

---

## 🏗️ Project Structure

```
LibraryInventoryManagementSystem1/
│
├── Components/
│   └── NotificationBannerViewComponent.cs
│
├── Constants/
│   └── LayoutPaths.cs
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
├── Data/
│   └── AppDbContext.cs
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
├── Filters/
│   └── AdminOnlyAttribute.cs
│
├── Migrations/
│   └── (EF Core migration history)
│
├── Models/
│   ├── AdminNotification.cs
│   ├── Book.cs
│   ├── BookIssue.cs
│   ├── ErrorViewModel.cs
│   ├── Fine.cs
│   ├── Notification.cs
│   ├── Reservation.cs
│   ├── Student.cs
│   └── User.cs
│
├── Services/
│   ├── AdminNotificationService.cs
│   ├── EmailService.cs
│   ├── IAdminNotificationService.cs
│   ├── INotificationService.cs
│   └── NotificationService.cs
│
├── Views/
│   ├── Admin/
│   │   ├── _NotificationListPartial.cshtml
│   │   ├── Dashboard.cshtml
│   │   ├── Notifications.cshtml
│   │   ├── Reservations.cshtml
│   │   └── SendNotification.cshtml
│   │
│   ├── Auth/
│   │   ├── ForgotPassword.cshtml
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── ResetPassword.cshtml
│   │
│   ├── Book/
│   │   ├── Add.cshtml
│   │   ├── Available.cshtml
│   │   ├── Delete.cshtml
│   │   ├── Edit.cshtml
│   │   └── Index.cshtml
│   │
│   ├── BookIssue/
│   │   ├── Index.cshtml
│   │   ├── Issue.cshtml
│   │   └── Recent.cshtml
│   │
│   ├── Fine/
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── History.cshtml
│   │   └── Index.cshtml
│   │
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   │
│   ├── Librarian/
│   │   ├── Add.cshtml
│   │   ├── Delete.cshtml
│   │   ├── Edit.cshtml
│   │   └── Index.cshtml
│   │
│   ├── Shared/
│   │   ├── Components/
│   │   │   └── NotificationBanner/
│   │   │       └── Default.cshtml
│   │   ├── _AdminLayout.cshtml
│   │   ├── _Layout.cshtml
│   │   ├── _SidebarNav.cshtml
│   │   ├── _StudentLayout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   │
│   ├── Splash/
│   │   └── index.cshtml
│   │
│   ├── Student/
│   │   ├── Add.cshtml
│   │   ├── Delete.cshtml
│   │   ├── Edit.cshtml
│   │   └── Index.cshtml
│   │
│   ├── StudentDashboard/
│   │   ├── AvailableBooks.cshtml
│   │   ├── BorrowedBooks.cshtml
│   │   ├── ChangePassword.cshtml
│   │   ├── Dashboard.cshtml
│   │   ├── Fines.cshtml
│   │   ├── Notifications.cshtml
│   │   ├── Profile.cshtml
│   │   ├── Reservations.cshtml
│   │   ├── ReturnHistory.cshtml
│   │   └── Search.cshtml
│   │
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── wwwroot/
│   ├── css/
│   │   ├── site.css
│   │   └── StudentDashboard.css
│   ├── js/
│   ├── lib/
│   └── favicon.ico
│
├── appsettings.json
└── Program.cs
```

---

## 🗄️ Data Model (Core Entities)

| Entity | Key Fields |
|---|---|
| **Book** | `BookId`, `Title`, `Author`, `Category`, `TotalQty`, `AvailableQty` |
| **Student** | Linked to borrowing activity and portal access |
| **User** | `Username`, `PasswordHash`, `Role` |
| **BookIssue** | `IssueDate`, `DueDate`, `ReturnDate`, `Status` |
| **Reservation** | `ReservedDate`, fulfillment/cancellation tracking |
| **Fine** | `FineId`, `IssueId` (FK), `Amount`, `PaymentStatus`, `PaymentDate` |
| **Notification / AdminNotification** | In-app notification system with title & read state |

---

## 🔐 Authentication & Roles

- **Session-based authentication** using `Username`, `Role`, and `StudentId`
- Custom `[AdminOnlyAttribute]` action filter restricts admin/librarian-only actions
- Roles: **Admin**, **Librarian**, **Student**

> ⚠️ **Security note:** Passwords are currently stored in plain text in the `PasswordHash` field. Migrating to a proper hashing algorithm (e.g., BCrypt or ASP.NET Core Identity) is a recommended next step before production use.

### 🧪 Default Admin Login (Local Development Only)

> 🚨 **Do not commit real credentials to a public README.** The values below are placeholders see [`SEED_ADMIN.md`](./SEED_ADMIN.md) (git-ignored) for your actual local login. If this repo is or will be public on GitHub, rotate any credentials that were ever pushed, since scrapers index public commits within minutes.

| Field | Value |
|---|---|
| Username | `admin@example.com` |
| Password | `changeme` |
| Role | `Admin` |

**Before deploying or pushing publicly:**
- [ ] Change the seeded admin password
- [ ] Confirm `SEED_ADMIN.md` and `appsettings.Development.json` are in `.gitignore`
- [ ] Rotate the Gmail app password if it was ever hardcoded anywhere

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
git clone https://github.com/bibekydv285-source/LibraryInventoryManagementSystem1.git
cd LibraryInventoryManagementSystem1

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

### Configuration

`appsettings.json` (safe to commit — no secrets):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConn": "Server=(localdb)\\MSSQLLocalDB;Database=LibraryInventoryManagementSystem1;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": ""
  },
  "AllowedHosts": "*"
}
```

> 🚨 **Never put `SenderPassword` (Gmail app password) directly in `appsettings.json` if this repo is or will be public.** Use one of the options below — both are already excluded via `.gitignore`.

**Option A — `appsettings.Development.json` (git-ignored, simplest for local dev):**

```json
{
  "EmailSettings": {
    "SenderPassword": "your-16-char-gmail-app-password"
  }
}
```

**Option B — .NET User Secrets (recommended, never touches disk in the repo folder):**

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SenderPassword" "your-16-char-gmail-app-password"
```

> If a Gmail app password has ever been pasted into a README, commit, chat log, or issue that reached GitHub, revoke it from your [Google App Passwords page](https://myaccount.google.com/apppasswords) and generate a new one — treat it as compromised the moment it's public.

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

Built by **Bibek Yadav** — BSc (Hons) Cyber Security student, ISMT College (University of Sunderland) — as part of an ongoing full-stack ASP.NET Core MVC learning project.

🔗 [bibek-yadav.com.np](https://bibek-yadav.com.np) · GitHub: [@bibekydv285-source](https://github.com/bibekydv285-source)
