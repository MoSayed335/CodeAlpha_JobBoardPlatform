using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobBoardPlatform.Data;
using JobBoardPlatform.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JobBoardPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatesController : ControllerBase
    {
        private readonly JobBoardDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CandidatesController(JobBoardDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/candidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates()
        {
            return await _context.Candidates.ToListAsync();
        }

        // GET: api/candidates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Candidate>> GetCandidate(int id)
        {
            var candidate = await _context.Candidates
                .Include(c => c.Resumes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null)
            {
                return NotFound(new { message = "Candidate not found" });
            }

            return candidate;
        }

        // POST: api/candidates
        [HttpPost]
        public async Task<ActionResult<Candidate>> PostCandidate(Candidate candidate)
        {
            candidate.CreatedAt = DateTime.UtcNow;
            candidate.IsActive = true;
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCandidate), new { id = candidate.Id }, candidate);
        }

        // POST: api/candidates/5/resume
        [HttpPost("{id}/resume")]
        public async Task<IActionResult> UploadResume(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound(new { message = "Candidate not found" });
            }

            // Create target folder under wwwroot
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename to avoid collision
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Save resume record in DB
            var resume = new Resume
            {
                CandidateId = id,
                FileName = file.FileName,
                FilePath = $"uploads/resumes/{uniqueFileName}",
                UploadedAt = DateTime.UtcNow
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            return Ok(resume);
        }
    }
}
