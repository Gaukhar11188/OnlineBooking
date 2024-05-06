using System.ComponentModel.DataAnnotations;

namespace RoomBookingProject.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required]
        public string? RoomType { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }
        [Required]
        public string? Img { get; set; }
        [Required]  
        public string? Description { get; set; } 
    }
}
