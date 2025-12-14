using System.ComponentModel.DataAnnotations;

namespace Learn_Earn.Models
{
    public class UserXp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        // Total XP accumulated
        public int TotalXp { get; set; }
    }
}
