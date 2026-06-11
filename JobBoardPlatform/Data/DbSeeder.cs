using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Models;

namespace JobBoardPlatform.Data
{
    public static class DbSeeder
    {
        public static void Seed(JobBoardDbContext context)
        {
            // Apply migrations and create database
            context.Database.EnsureCreated();

            // Check if database is already seeded
            if (context.Employers.Any() || context.Candidates.Any())
            {
                return;
            }

            // Seed Employers
            var employers = new[]
            {
                new Employer
                {
                    CompanyName = "TechCorp Solutions",
                    ContactEmail = "jobs@techcorp.com",
                    CompanyWebsite = "https://techcorp.example.com",
                    Description = "Leading software development and IT consultancy firm specializing in cloud and AI technologies.",
                    Location = "San Francisco, CA",
                    Industry = "Information Technology",
                    IsActive = true
                },
                new Employer
                {
                    CompanyName = "Apex Marketing Agency",
                    ContactEmail = "careers@apexmarketing.com",
                    CompanyWebsite = "https://apexmarketing.example.com",
                    Description = "A digital-first agency delivering growth hacking, SEO, and brand storytelling services globally.",
                    Location = "New York, NY",
                    Industry = "Marketing & Advertising",
                    IsActive = true
                },
                new Employer
                {
                    CompanyName = "BioMed Research Labs",
                    ContactEmail = "hr@biomedlabs.com",
                    CompanyWebsite = "https://biomedlabs.example.com",
                    Description = "Pioneering biotech firm developing breakthrough diagnostics and personalized therapies.",
                    Location = "Boston, MA",
                    Industry = "Biotechnology",
                    IsActive = true
                }
            };
            context.Employers.AddRange(employers);
            context.SaveChanges();

            // Seed Candidates
            var candidates = new[]
            {
                new Candidate
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    PhoneNumber = "+1-555-0199",
                    IsActive = true
                },
                new Candidate
                {
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@example.com",
                    PhoneNumber = "+1-555-0142",
                    IsActive = true
                },
                new Candidate
                {
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Email = "charlie.brown@example.com",
                    PhoneNumber = "+1-555-0163",
                    IsActive = false
                }
            };
            context.Candidates.AddRange(candidates);
            context.SaveChanges();

            // Seed Job Listings
            var listings = new[]
            {
                new JobListing
                {
                    EmployerId = employers[0].Id,
                    Title = "Senior Full-Stack Developer (C# / React)",
                    Description = "We are seeking a talented Senior Developer to lead the development of our enterprise cloud applications. You will collaborate with product designers, engineers, and product managers.",
                    Requirements = "• 5+ years experience with C# and ASP.NET Core\n• 3+ years experience with React or Angular\n• Strong SQL Server database design skills\n• Solid understanding of AWS or Azure",
                    Location = "San Francisco, CA (Hybrid)",
                    SalaryRange = "$130,000 - $160,000",
                    JobType = "FullTime",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new JobListing
                {
                    EmployerId = employers[0].Id,
                    Title = "Cloud Infrastructure Engineer",
                    Description = "Looking for a cloud operations expert to manage, optimize, and secure our Kubernetes and AWS infrastructure.",
                    Requirements = "• Kubernetes/Docker experience\n• Terraform infrastructure-as-code scripting\n• AWS/Azure certifications are highly preferred\n• Shell scripting expertise",
                    Location = "Remote (US)",
                    SalaryRange = "$120,000 - $145,000",
                    JobType = "Remote",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new JobListing
                {
                    EmployerId = employers[1].Id,
                    Title = "Social Media Strategist",
                    Description = "Grow our digital footprint! You will develop, implement, and track content campaigns across major platforms to drive lead generation and customer engagement.",
                    Requirements = "• 2+ years digital marketing experience\n• Proficiency with Canva, Figma, and Google Analytics\n• Excellent copywriting and communication skills\n• Experience building TikTok and Instagram campaigns",
                    Location = "New York, NY",
                    SalaryRange = "$70,000 - $85,000",
                    JobType = "PartTime",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new JobListing
                {
                    EmployerId = employers[2].Id,
                    Title = "Clinical Data Analyst",
                    Description = "Analyze clinical trial data to ensure accuracy, compliance, and scientific validity. You will work closely with lead scientists and regulatory officers.",
                    Requirements = "• BS/MS in Statistics, Bioinformatics, or Computer Science\n• Strong Python/R programming skills\n• Experience with clinical database systems\n• High attention to detail",
                    Location = "Boston, MA (Onsite)",
                    SalaryRange = "$90,000 - $110,000",
                    JobType = "Contract",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new JobListing
                {
                    EmployerId = employers[0].Id,
                    Title = "Intern Frontend Developer",
                    Description = "Exciting opportunity for a junior/student developer to gain real-world industry experience. You will work on user-facing applications and UI styling.",
                    Requirements = "• Basic knowledge of HTML, CSS, JavaScript\n• Familiarity with Git\n• Eagerness to learn and take constructive feedback",
                    Location = "San Francisco, CA",
                    SalaryRange = "$25 - $35 / hour",
                    JobType = "Internship",
                    Status = "Closed",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };
            context.JobListings.AddRange(listings);
            context.SaveChanges();

            // Seed Resume
            var resume = new Resume
            {
                CandidateId = candidates[0].Id,
                FileName = "Alice_Smith_Resume.pdf",
                FilePath = "uploads/resumes/alice_smith_resume_mock.pdf",
                UploadedAt = DateTime.UtcNow.AddDays(-2)
            };
            context.Resumes.Add(resume);
            context.SaveChanges();

            // Seed Application
            var application = new JobApplication
            {
                JobListingId = listings[0].Id,
                CandidateId = candidates[0].Id,
                ResumeId = resume.Id,
                ApplicationStatus = "Applied",
                AppliedAt = DateTime.UtcNow.AddDays(-1),
                Notes = "I am excited to apply for the Senior Full-Stack Developer position. I have extensive experience with C# and React, and I look forward to contributing to your cloud solutions."
            };
            context.JobApplications.Add(application);
            context.SaveChanges();

            // Seed Notification
            var notification = new EmployerNotification
            {
                EmployerId = employers[0].Id,
                Message = $"New application received from Alice Smith for '{listings[0].Title}'",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            context.EmployerNotifications.Add(notification);
            context.SaveChanges();
        }
    }
}
