using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JobBoardPlatform.Models
{
    public class Employer
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();

        [JsonIgnore]
        public ICollection<EmployerNotification> Notifications { get; set; } = new List<EmployerNotification>();
    }
}
