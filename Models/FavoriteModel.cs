using System.ComponentModel.DataAnnotations;

namespace ReviewAPI.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        
        [Required]
        public string BookId { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}