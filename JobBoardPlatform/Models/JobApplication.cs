using System;

namespace JobBoardPlatform.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        public int JobListingId { get; set; }
        public JobListing? JobListing { get; set; }

        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }

        public int ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public string ApplicationStatus { get; set; } = "Applied"; // Applied, Reviewing, Interviewing, Offered, Rejected
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;
    }
}
