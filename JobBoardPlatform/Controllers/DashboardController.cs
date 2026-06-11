using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoardPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly JobBoardDbContext _context;

        public DashboardController(JobBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalJobs = await _context.JobListings.CountAsync();
            var openJobs = await _context.JobListings.CountAsync(j => j.Status == "Open");
            var closedJobs = totalJobs - openJobs;

            var totalCandidates = await _context.Candidates.CountAsync();
            var totalEmployers = await _context.Employers.CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();

            // Count per status
            var statusCounts = await _context.JobApplications
                .GroupBy(a => a.ApplicationStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Count per job type
            var jobTypeCounts = await _context.JobListings
                .GroupBy(j => j.JobType)
                .Select(g => new { JobType = g.Key, Count = g.Count() })
                .ToListAsync();

            // Recent job listings
            var recentJobs = await _context.JobListings
                .Include(j => j.Employer)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    CompanyName = j.Employer != null ? j.Employer.CompanyName : "Unknown",
                    j.Location,
                    j.JobType,
                    j.Status,
                    j.CreatedAt
                })
                .ToListAsync();

            // Recent job applications
            var recentApplications = await _context.JobApplications
                .Include(a => a.JobListing)
                .Include(a => a.Candidate)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    JobTitle = a.JobListing != null ? a.JobListing.Title : "Unknown Job",
                    CandidateName = a.Candidate != null ? $"{a.Candidate.FirstName} {a.Candidate.LastName}" : "Unknown Candidate",
                    a.ApplicationStatus,
                    a.AppliedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalJobs,
                openJobs,
                closedJobs,
                totalCandidates,
                totalEmployers,
                totalApplications,
                statusCounts,
                jobTypeCounts,
                recentJobs,
                recentApplications
            });
        }
    }
}
