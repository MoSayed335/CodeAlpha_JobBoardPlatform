using System;

namespace JobBoardPlatform.Models
{
    public class EmployerNotification
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public Employer? Employer { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
