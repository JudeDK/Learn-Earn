using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    }
}
