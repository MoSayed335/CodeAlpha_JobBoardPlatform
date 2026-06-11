using System;
using System.Text.Json.Serialization;

namespace JobBoardPlatform.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public Candidate? Candidate { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
