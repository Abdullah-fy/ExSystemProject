# ExSystemProject

A comprehensive educational management system with multiple roles and features, built using **ASP.NET MVC (.NET 9)**.

---

## 📘 Overview

**ExSystemProject** is a full-featured academic platform designed to streamline the management of educational institutions with multiple branches. It offers centralized control for administrators and tailored features for instructors and students.

---

## 🔐 Features

### 🔄 Role-Based Access

The system supports four distinct user roles with specific functionalities:

#### 🛠 Super Admin
- System-wide management of all branches
- Analytics dashboard with institutional insights
- Branch creation and management
- Track and course oversight
- Full user management (admins, instructors, students)

#### 🏢 Branch Manager (Admin)
- Branch-specific dashboard with KPIs
- Track and instructor management
- Supervisor assignment
- Course and exam oversight for their branch

#### 👨‍🏫 Instructor
- Course management and teaching
- Question bank creation
- Exam scheduling and grading
- Student performance tracking

#### 👨‍🎓 Student
- Course enrollment and participation
- Exam access with real-time feedback
- Grade and performance tracking
- Profile management

---

## 🛠 Technologies Used

- **Backend:** ASP.NET MVC (.NET 9)
- **Database:** SQL Server with:
  - Stored Procedures
  - Functions
  - Triggers
  - Complex Joins
- **ORM:** Entity Framework Core
- **Frontend:** Bootstrap 5, JavaScript, jQuery, Razor Pages
- **Authentication:** Custom role-based authentication with claims
- **Architecture:** N-tier with Repository Pattern and Unit of Work
- **Reporting:** SSRS integration for analytics

---

## 🧱 Architecture Highlights

- Clean separation of concerns
- Repository pattern with Unit of Work for transaction management
- Comprehensive data validation
- Optimized queries via stored procedures
- Custom middleware for authorization
- Fully responsive UI across all roles

---

## 🚀 Getting Started

### ✅ Prerequisites

- Visual Studio 2022
- SQL Server 2019 or higher
- .NET 9.0 SDK

### ⚙️ Installation Steps

1. Clone the repository:
 
   ```bash
   git clone https://github.com/Abdullah-fy/ExSystemProject
   ```

2. Update the connection string in `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "sc": "Data Source=YourServerName;Initial Catalog=ExSystemTest;Integrated Security=True;Trust Server Certificate=True"
   }
   ```

3. Run the database migrations:

   ```bash
   dotnet ef database update
   ```

4. Launch the application:

   ```bash
   dotnet run
   ```

---

## 👥 Team Members

* Reda Abd Elglel
* Tarek Ahmed
* Abdullah Fathy
* Omar Reda
* Hamdi Salah

---

## 🙏 Acknowledgements

Project supervised by:

Eng. Nadia Saleh

Special thanks to:

* Eng. Ayman Lotfy
* Eng. Rami Nagi

---

## 📄 License

This project is **proprietary** and may not be used or distributed without permission.
```

