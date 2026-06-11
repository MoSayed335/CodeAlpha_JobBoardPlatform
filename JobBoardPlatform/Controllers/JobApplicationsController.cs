using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Data;
using JobBoardPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoardPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobApplicationsController : ControllerBase
    {
        private readonly JobBoardDbContext _context;

        public JobApplicationsController(JobBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/jobapplications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobApplication>> GetJobApplication(int id)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobListing)
                    .ThenInclude(j => j!.Employer)
                .Include(a => a.Candidate)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(new { message = "Application not found" });
            }

            return application;
        }

        // POST: api/jobapplications
        [HttpPost]
        public async Task<ActionResult<JobApplication>> PostJobApplication(JobApplication application)
        {
            // Verify JobListing exists and is Open
            var job = await _context.JobListings.FindAsync(application.JobListingId);
            if (job == null)
            {
                return BadRequest(new { message = "Job listing does not exist." });
            }
            if (job.Status.ToLower() != "open")
            {
                return BadRequest(new { message = "Applications are closed for this job listing." });
            }

            // Verify Candidate exists
            var candidate = await _context.Candidates.FindAsync(application.CandidateId);
            if (candidate == null)
            {
                return BadRequest(new { message = "Candidate does not exist." });
            }

            // Verify Resume exists and belongs to Candidate
            var resume = await _context.Resumes.FindAsync(application.ResumeId);
            if (resume == null || resume.CandidateId != application.CandidateId)
            {
                return BadRequest(new { message = "Invalid ResumeId for this candidate." });
            }

            // Prevent duplicate applications
            var duplicate = await _context.JobApplications
                .AnyAsync(a => a.JobListingId == application.JobListingId && a.CandidateId == application.CandidateId);
            if (duplicate)
            {
                return BadRequest(new { message = "You have already applied for this job listing." });
            }

            application.ApplicationStatus = "Applied";
            application.AppliedAt = DateTime.UtcNow;

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            // Create Notification for the Employer
            var notification = new EmployerNotification
            {
                EmployerId = job.EmployerId,
                Message = $"New application received from {candidate.FirstName} {candidate.LastName} for '{job.Title}'",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.EmployerNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Populate navigation properties for returned value
            application.JobListing = job;
            application.Candidate = candidate;
            application.Resume = resume;

            return CreatedAtAction(nameof(GetJobApplication), new { id = application.Id }, application);
        }

        // GET: api/jobapplications/candidate/5
        [HttpGet("candidate/{candidateId}")]
        public async Task<ActionResult<IEnumerable<JobApplication>>> GetApplicationsByCandidate(int candidateId)
        {
            return await _context.JobApplications
                .Include(a => a.JobListing)
                    .ThenInclude(j => j!.Employer)
                .Include(a => a.Resume)
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // GET: api/jobapplications/job/5
        [HttpGet("job/{jobListingId}")]
        public async Task<ActionResult<IEnumerable<JobApplication>>> GetApplicationsByJob(int jobListingId)
        {
            return await _context.JobApplications
                .Include(a => a.Candidate)
                .Include(a => a.Resume)
                .Where(a => a.JobListingId == jobListingId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // GET: api/jobapplications/employer/5
        [HttpGet("employer/{employerId}")]
        public async Task<ActionResult<IEnumerable<JobApplication>>> GetApplicationsByEmployer(int employerId)
        {
            return await _context.JobApplications
                .Include(a => a.JobListing)
                .Include(a => a.Candidate)
                .Include(a => a.Resume)
                .Where(a => a.JobListing!.EmployerId == employerId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // PUT: api/jobapplications/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Status is required." });
            }

            var application = await _context.JobApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound(new { message = "Application not found." });
            }

            // Valid statuses: Applied, Reviewing, Interviewing, Offered, Rejected
            var validStatuses = new[] { "Applied", "Reviewing", "Interviewing", "Offered", "Rejected" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}" });
            }

            application.ApplicationStatus = request.Status;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                application.Notes = request.Notes;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Application status updated successfully", application });
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
