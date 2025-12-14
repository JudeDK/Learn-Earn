using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class UserDailyCompletion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        // Store only date part
        public DateTime Date { get; set; }
    }
}
