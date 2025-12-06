using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; }

        [Required]
        [StringLength(50)]
        public string Difficulty { get; set; }

        [Range(1, 10000)]
        public int DurationMinutes { get; set; }

        public string? Content { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? CreatedByEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
