using System.ComponentModel.DataAnnotations;

namespace ReviewAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required]
        public string BookId { get; set; } = string.Empty;      // ID från Google Books API
        
        public int UserId { get; set; }     // Foreign key till User
        public User User { get; set; } = null!;     // Navigation property, relationen till User
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}