using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int Score { get; set; }

        public int Total { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        // Professor validation fields
        public bool IsValidated { get; set; }
        public string? ValidatedByUserId { get; set; }
        public bool? Passed { get; set; }
        public string? ValidationNote { get; set; }
        // Student file submission (for file-based quizzes)
        public string? StudentSubmissionFileName { get; set; }
        public string? StudentSubmissionPath { get; set; }
        public string? StudentSubmissionContentType { get; set; }
    }
}
