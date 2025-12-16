using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class UserQuizDailyCompletion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        // store only date part (UTC)
        public DateTime Date { get; set; }
    }
}
