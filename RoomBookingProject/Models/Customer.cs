using System.ComponentModel.DataAnnotations;

namespace RoomBookingProject.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Phone { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
