using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        // Optional: associate quiz with a lesson
        public int? LessonId { get; set; }

        // Duration in minutes (optional)
        public int DurationMinutes { get; set; }

        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional attachment (file-based quiz)
        public string? AttachmentFileName { get; set; }
        public string? AttachmentPath { get; set; }
        public string? AttachmentContentType { get; set; }
    }
}
