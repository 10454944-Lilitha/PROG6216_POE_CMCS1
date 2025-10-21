using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CMCS1.Models.ViewModels
{
    public class ClaimViewModel
    {
        [Key]
        public Guid ClaimId { get; set; };

        [Required(ErrorMessage = "Hours worked is required.")]
        [Range(1, 1000, ErrorMessage = "Hours must be between 1 and 1000.")]
        [Display(Name = "Hours Worked")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(0.01, 1000.00, ErrorMessage = "Rate must be between 0.01 and 1000.00.")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string Notes { get; set; }

        public string Status { get; set; } = "Pending";

        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Display(Name = "Status Update Date")]
        public DateTime? StatusUpdateDate { get; set; }

        [Display(Name = "Supporting Documents")]
        public List<IFormFile> SupportingDocuments { get; set; } = new List<IFormFile>();

        public List<string> UploadedFileNames { get; set; } = new List<string>();
    }
}
