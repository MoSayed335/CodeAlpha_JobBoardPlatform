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
    public class EmployersController : ControllerBase
    {
        private readonly JobBoardDbContext _context;

        public EmployersController(JobBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/employers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employer>>> GetEmployers()
        {
            return await _context.Employers.ToListAsync();
        }

        // GET: api/employers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employer>> GetEmployer(int id)
        {
            var employer = await _context.Employers.FindAsync(id);

            if (employer == null)
            {
                return NotFound(new { message = "Employer not found" });
            }

            return employer;
        }

        // POST: api/employers
        [HttpPost]
        public async Task<ActionResult<Employer>> PostEmployer(Employer employer)
        {
            employer.CreatedAt = DateTime.UtcNow;
            employer.IsActive = true;
            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployer), new { id = employer.Id }, employer);
        }

        // PUT: api/employers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployer(int id, Employer employer)
        {
            if (id != employer.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existingEmployer = await _context.Employers.FindAsync(id);
            if (existingEmployer == null)
            {
                return NotFound(new { message = "Employer not found" });
            }

            existingEmployer.CompanyName = employer.CompanyName;
            existingEmployer.ContactEmail = employer.ContactEmail;
            existingEmployer.CompanyWebsite = employer.CompanyWebsite;
            existingEmployer.Description = employer.Description;
            existingEmployer.Location = employer.Location;
            existingEmployer.Industry = employer.Industry;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployerExists(id))
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

        // GET: api/employers/5/notifications
        [HttpGet("{id}/notifications")]
        public async Task<ActionResult<IEnumerable<EmployerNotification>>> GetNotifications(int id)
        {
            return await _context.EmployerNotifications
                .Where(n => n.EmployerId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // PUT: api/employers/5/notifications/read
        [HttpPut("{id}/notifications/read")]
        public async Task<IActionResult> MarkNotificationsAsRead(int id)
        {
            var notifications = await _context.EmployerNotifications
                .Where(n => n.EmployerId == id && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Notifications marked as read" });
        }

        private bool EmployerExists(int id)
        {
            return _context.Employers.Any(e => e.Id == id);
        }
    }
}
