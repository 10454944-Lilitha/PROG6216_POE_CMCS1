using System.ComponentModel.DataAnnotations;

namespace CMCS1.Models
{
    public class Claim
    {
        [Key]
        public Guid ClaimId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Hours worked is required.")]
        [Range(1, 1000, ErrorMessage = "Hours must be between 1 and 1000.")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(0.01, 1000.00, ErrorMessage = "Rate must be between 0.01 and 1000.00.")]
        public decimal HourlyRate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? StatusUpdateDate { get; set; }
        public List<string> UploadedFileNames { get; set; } = new List<string>();
        
        // Approval tracking
        public bool Manager1Approved { get; set; } = false;
        public bool Manager2Approved { get; set; } = false;
        public DateTime? Manager1ApprovalDate { get; set; }
        public DateTime? Manager2ApprovalDate { get; set; }

        // Navigation property for feedback
        public List<Feedback> FeedbackList { get; set; } = new List<Feedback>();

        // Calculated property for total claim amount
        public decimal TotalAmount => HoursWorked * HourlyRate;
        
        // Helper property to check if both managers approved
        public bool IsFinallyApproved => Manager1Approved && Manager2Approved;

        // Foreign Key to User (Lecturer)
        public int LecturerId { get; set; }
        public User? Lecturer { get; set; }
    }
}
