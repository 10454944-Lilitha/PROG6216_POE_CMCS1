using System.ComponentModel.DataAnnotations;

namespace CMCS1.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; } // In a real app, hash this!

        [Required]
        public string Role { get; set; } // Lecturer, Manager1, Manager2, HR

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Specific to Lecturer
        public decimal? HourlyRate { get; set; }
    }
}
