using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Models;

namespace JobBoardPlatform.Data
{
    public class JobBoardDbContext : DbContext
    {
        public JobBoardDbContext(DbContextOptions<JobBoardDbContext> options) : base(options)
        {
        }

        public DbSet<Employer> Employers => Set<Employer>();
        public DbSet<JobListing> JobListings => Set<JobListing>();
        public DbSet<Candidate> Candidates => Set<Candidate>();
        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<EmployerNotification> EmployerNotifications => Set<EmployerNotification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employer -> JobListings relationship
            modelBuilder.Entity<JobListing>()
                .HasOne(j => j.Employer)
                .WithMany(e => e.JobListings)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Candidate -> Resumes relationship
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Candidate)
                .WithMany(c => c.Resumes)
                .HasForeignKey(r => r.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure JobApplication relationships (avoid cycle cascades in SQL Server)
            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.JobListing)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.Resume)
                .WithMany()
                .HasForeignKey(a => a.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employer -> Notifications relationship
            modelBuilder.Entity<EmployerNotification>()
                .HasOne(n => n.Employer)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
