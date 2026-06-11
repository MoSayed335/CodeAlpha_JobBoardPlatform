using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JobBoardPlatform.Models
{
    public class JobListing
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public Employer? Employer { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string SalaryRange { get; set; } = string.Empty;
        public string JobType { get; set; } = "FullTime"; // FullTime, PartTime, Contract, Remote, Internship
        public string Status { get; set; } = "Open"; // Open, Closed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
