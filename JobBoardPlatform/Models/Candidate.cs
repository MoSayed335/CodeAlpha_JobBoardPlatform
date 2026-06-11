using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JobBoardPlatform.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();

        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
