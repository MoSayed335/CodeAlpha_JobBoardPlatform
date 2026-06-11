using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoardPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly JobBoardDbContext _context;

        public UsersController(JobBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/users/candidates
        [HttpGet("candidates")]
        public async Task<IActionResult> GetCandidates()
        {
            var candidates = await _context.Candidates
                .Select(c => new
                {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.PhoneNumber,
                    c.IsActive,
                    c.CreatedAt
                })
                .ToListAsync();
            return Ok(candidates);
        }

        // GET: api/users/employers
        [HttpGet("employers")]
        public async Task<IActionResult> GetEmployers()
        {
            var employers = await _context.Employers
                .Select(e => new
                {
                    e.Id,
                    e.CompanyName,
                    e.ContactEmail,
                    e.CompanyWebsite,
                    e.Industry,
                    e.Location,
                    e.IsActive,
                    e.CreatedAt
                })
                .ToListAsync();
            return Ok(employers);
        }

        // PUT: api/users/candidates/5/status
        [HttpPut("candidates/{id}/status")]
        public async Task<IActionResult> ToggleCandidateStatus(int id, [FromBody] ToggleStatusRequest request)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound(new { message = "Candidate not found" });
            }

            candidate.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Candidate status updated", isActive = candidate.IsActive });
        }

        // PUT: api/users/employers/5/status
        [HttpPut("employers/{id}/status")]
        public async Task<IActionResult> ToggleEmployerStatus(int id, [FromBody] ToggleStatusRequest request)
        {
            var employer = await _context.Employers.FindAsync(id);
            if (employer == null)
            {
                return NotFound(new { message = "Employer not found" });
            }

            employer.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employer status updated", isActive = employer.IsActive });
        }
    }

    public class ToggleStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
