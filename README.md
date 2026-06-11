# JobSphere - Premium Job Board Platform

JobSphere is a complete, modern Job Board Platform featuring a robust **ASP.NET Core Web API** backend connected to **SQL Server** using Entity Framework Core, alongside a premium **Glassmorphism Single-Page Application (SPA)** frontend served directly from the API static files.

---

## 🚀 Key Features

### 1. Candidate Portal
* **Advanced Job Search & Filters**: Search jobs instantly by keyword, location, job type (Full-time, Part-time, Contract, Remote, Internship), and status (Open/Closed).
* **Profile Management**: Register new candidate profiles with contact information.
* **Resume Uploads**: Upload PDF, DOCX, or TXT resume files directly. Files are stored on the server's filesystem and indexed in the database.
* **Application Tracking**: Submit applications with cover letters and track live status updates (Applied, Reviewing, Interviewing, Offered, Rejected).

### 2. Employer Portal
* **Company Profile Management**: Register company details (name, industry, website, headquarters).
* **Job Publishing**: Publish and manage job listings with titles, salaries, requirements, and job types.
* **Applicant Tracking System (ATS)**: Review job applications, download submitted resumes, and update applicants' states.
* **System Notifications**: Real-time notifications for incoming applications.

### 3. Admin Dashboard & Reporting
* **Executive Metrics**: Live counters showing total jobs, open/closed listings, candidates, employers, and applications.
* **Graphical Distributions**: CSS progress bars illustrating application status and job type breakdowns.
* **User Management System**: Deactivate or reactivate candidate and employer profiles dynamically.

---

## 🛠️ Technology Stack
* **Backend Framework**: ASP.NET Core 9.0 Web API
* **Database Mapping (ORM)**: Entity Framework Core 9.0.2
* **Database**: Microsoft SQL Server (configured with LocalDB by default)
* **Frontend UI**: Vanilla HTML5, CSS3 Custom Properties (variables), JavaScript (ES6+), and Lucide Icons

---

## 🗄️ Database Schema & Relationships

Entity Framework Core is configured to generate the database schema automatically. It includes custom fluent mappings to avoid cycles on delete cascades in SQL Server:

```
[Employer] 1 ---- * [JobListing] (Cascade Delete)
[Employer] 1 ---- * [EmployerNotification] (Cascade Delete)
[Candidate] 1 --- * [Resume] (Cascade Delete)
[Candidate] 1 --- * [JobApplication] (No Action / Restrict)
[JobListing] 1 -- * [JobApplication] (No Action / Restrict)
[Resume] 1 ------ 1 [JobApplication] (No Action / Restrict)
```

---

## 🔌 API Reference

### Job Listings
* `GET /api/joblistings`: Search/filter jobs (`search`, `location`, `jobType`, `status`).
* `GET /api/joblistings/{id}`: Fetch job details including company info.
* `POST /api/joblistings`: Create a new job posting.
* `PUT /api/joblistings/{id}`: Modify details of a listing.
* `DELETE /api/joblistings/{id}`: Remove a listing.

### Candidates & Resumes
* `GET /api/candidates`: Fetch candidate listing.
* `GET /api/candidates/{id}`: Fetch profile and list uploaded resumes.
* `POST /api/candidates`: Register a candidate.
* `POST /api/candidates/{id}/resume`: Upload a multi-part file resume.

### Job Applications & Notifications
* `POST /api/jobapplications`: Apply to an open job (prevents duplicates).
* `GET /api/jobapplications/candidate/{candidateId}`: Candidate's applications history.
* `GET /api/jobapplications/employer/{employerId}`: Applications received by employer.
* `PUT /api/jobapplications/{id}/status`: Edit status (`Applied`, `Reviewing`, `Interviewing`, `Offered`, `Rejected`).
* `GET /api/employers/{id}/notifications`: Fetch recruiter alerts.
* `PUT /api/employers/{id}/notifications/read`: Mark alerts as read.

### Dashboard Stats & User Management
* `GET /api/dashboard/stats`: Returns statistical aggregations.
* `GET /api/users/candidates`: Admin list of candidates.
* `GET /api/users/employers`: Admin list of employers.
* `PUT /api/users/candidates/{id}/status`: Toggle candidate `IsActive` state.
* `PUT /api/users/employers/{id}/status`: Toggle employer `IsActive` state.

---

## 🏃 Run Instructions

### Prerequisites
* **.NET 9.0 SDK** or newer.
* **SQL Server LocalDB** (included with standard Visual Studio/MSSQL Tools installations).

### Startup Commands
1. Navigate to the project directory:
   ```powershell
   cd C:\Users\Mohamed\.gemini\antigravity\scratch\JobBoardPlatform
   ```
2. Build the application:
   ```powershell
   dotnet build
   ```
3. Run the application:
   ```powershell
   dotnet run
   ```
4. Access the web interface in your browser:
   * **URL**: [http://localhost:5237](http://localhost:5237) or check the output port printed in the terminal.

### Automatic Migrations & Seeding
On startup, the system checks for a database connection. If the database `JobBoardPlatformDb` does not exist, Entity Framework Core will automatically:
1. Create the database and create tables.
2. Seed the database with mock employers, candidates, job listings (open/closed), resumes, applications, and logs.
