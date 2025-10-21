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
        public string Notes { get; set; }

        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? StatusUpdateDate { get; set; }
        public List<IFormFile> SupportingDocuments { get; set; }
        public List<string> UploadedFileNames { get; set; } = new List<string>();
    }
}
