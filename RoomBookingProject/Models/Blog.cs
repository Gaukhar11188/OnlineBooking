using System.ComponentModel.DataAnnotations;

namespace RoomBookingProject.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]  
        public DateTime CreateDate { get; set; }

        [Required]
        public string? Img { get; set; } 
    }
}
