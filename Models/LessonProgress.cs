using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class LessonProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public string UserId { get; set; }

        // For simplicity store completed sections count
        public int CompletedSections { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
