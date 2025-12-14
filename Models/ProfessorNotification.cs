using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class ProfessorNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentUserId { get; set; }

        [Required]
        public int LessonId { get; set; }

        // The professor responsible for this lesson (optional)
        public string? ProfessorUserId { get; set; }

        public bool IsHandled { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
