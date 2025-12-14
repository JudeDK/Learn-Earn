using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Learn_Earn.Models;

namespace Learn_Earn.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Learn_Earn.Models.Quiz> Quizzes { get; set; }
        public DbSet<Learn_Earn.Models.QuizQuestion> QuizQuestions { get; set; }
        public DbSet<Learn_Earn.Models.QuizAttempt> QuizAttempts { get; set; }
        public DbSet<Learn_Earn.Models.ProfessorNotification> ProfessorNotifications { get; set; }
        public DbSet<Learn_Earn.Models.LessonProgress> LessonProgresses { get; set; }
        public DbSet<Learn_Earn.Models.UserXp> UserXps { get; set; }
        public DbSet<Learn_Earn.Models.Badge> Badges { get; set; }
        public DbSet<Learn_Earn.Models.UserDailyCompletion> UserDailyCompletions { get; set; }
        public DbSet<Learn_Earn.Models.UserQuizDailyCompletion> UserQuizDailyCompletions { get; set; }
    }
}
