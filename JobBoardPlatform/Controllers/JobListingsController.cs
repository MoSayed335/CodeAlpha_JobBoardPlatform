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
    public class JobListingsController : ControllerBase
    {
        private readonly JobBoardDbContext _context;

        public JobListingsController(JobBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/joblistings (with filters)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobListing>>> GetJobListings(
            [FromQuery] string? search,
            [FromQuery] string? location,
            [FromQuery] string? jobType,
            [FromQuery] string? status)
        {
            var query = _context.JobListings
                .Include(j => j.Employer)
                .AsQueryable();

            // Filter by search keyword (Title, Description, Requirements, CompanyName)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(j => 
                    j.Title.ToLower().Contains(lowerSearch) || 
                    j.Description.ToLower().Contains(lowerSearch) || 
                    j.Requirements.ToLower().Contains(lowerSearch) ||
                    (j.Employer != null && j.Employer.CompanyName.ToLower().Contains(lowerSearch))
                );
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(location))
            {
                var lowerLoc = location.ToLower();
                query = query.Where(j => j.Location.ToLower().Contains(lowerLoc));
            }

            // Filter by JobType
            if (!string.IsNullOrWhiteSpace(jobType) && jobType != "All")
            {
                var lowerType = jobType.ToLower();
                query = query.Where(j => j.JobType.ToLower() == lowerType);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                var lowerStatus = status.ToLower();
                query = query.Where(j => j.Status.ToLower() == lowerStatus);
            }
            else if (string.IsNullOrWhiteSpace(status))
            {
                // Default to Open only if no status parameter is specified
                query = query.Where(j => j.Status.ToLower() == "open");
            }

            return await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
        }

        // GET: api/joblistings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobListing>> GetJobListing(int id)
        {
            var jobListing = await _context.JobListings
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobListing == null)
            {
                return NotFound(new { message = "Job listing not found" });
            }

            return jobListing;
        }

        // POST: api/joblistings
        [HttpPost]
        public async Task<ActionResult<JobListing>> PostJobListing(JobListing jobListing)
        {
            // Verify employer exists
            var employer = await _context.Employers.FindAsync(jobListing.EmployerId);
            if (employer == null)
            {
                return BadRequest(new { message = "Invalid EmployerId. Employer does not exist." });
            }

            jobListing.CreatedAt = DateTime.UtcNow;
            jobListing.UpdatedAt = DateTime.UtcNow;

            _context.JobListings.Add(jobListing);
            await _context.SaveChangesAsync();

            // Load employer reference for returned value
            jobListing.Employer = employer;

            return CreatedAtAction(nameof(GetJobListing), new { id = jobListing.Id }, jobListing);
        }

        // PUT: api/joblistings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJobListing(int id, JobListing jobListing)
        {
            if (id != jobListing.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existingListing = await _context.JobListings.FindAsync(id);
            if (existingListing == null)
            {
                return NotFound(new { message = "Job listing not found" });
            }

            existingListing.Title = jobListing.Title;
            existingListing.Description = jobListing.Description;
            existingListing.Requirements = jobListing.Requirements;
            existingListing.Location = jobListing.Location;
            existingListing.SalaryRange = jobListing.SalaryRange;
            existingListing.JobType = jobListing.JobType;
            existingListing.Status = jobListing.Status;
            existingListing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobListingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/joblistings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobListing(int id)
        {
            var jobListing = await _context.JobListings.FindAsync(id);
            if (jobListing == null)
            {
                return NotFound();
            }

            _context.JobListings.Remove(jobListing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JobListingExists(int id)
        {
            return _context.JobListings.Any(e => e.Id == id);
        }
    }
}
