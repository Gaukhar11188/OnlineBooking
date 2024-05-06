using System.ComponentModel.DataAnnotations;

namespace RoomBookingProject.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Required field")]
        [EmailAddress]      
        public string? Email { get; set; }

        [Required(ErrorMessage = "Required field")]
        [RegularExpression(@"^.{8,}$", ErrorMessage = "Password contains minimum 8 symbols.")]
        public string? Password { get; set; }

        public bool IsAdmin { get; set; } 
    }
}
