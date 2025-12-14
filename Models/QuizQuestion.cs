using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        // Options stored as JSON or simple '|' separated
        public string? Options { get; set; }

        // Index of correct option (0-based)
        public int CorrectIndex { get; set; }
    }
}
