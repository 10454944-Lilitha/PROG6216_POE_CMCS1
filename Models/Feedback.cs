using System.ComponentModel.DataAnnotations;

namespace CMCS1.Models
{
    public class Feedback
    {
        [Key]
        public Guid FeedbackId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters.")]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string ReviewedBy { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
}
