using Learn_Earn.Models;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure DB is created (migrations should already be applied)
            await db.Database.EnsureCreatedAsync();

            if (await db.Lessons.AnyAsync()) return; // already seeded

            var lessons = new List<Lesson>();
            var langs = new[] { "English", "Spanish", "French", "German", "Italian", "Portuguese", "Russian", "Japanese", "Korean", "Chinese", "Arabic", "Turkish", "Polish", "Romanian", "Dutch" };
            for (int i = 0; i < 15; i++)
            {
                lessons.Add(new Lesson
                {
                    Title = $"Lecția demo {i + 1}",
                    Language = langs[i % langs.Length],
                    Difficulty = (i % 3 == 0) ? "Easy" : (i % 3 == 1) ? "Medium" : "Hard",
                    DurationMinutes = 5 + (i * 3),
                    Content = $"Conținut demo pentru lecția {i + 1} în {langs[i % langs.Length]}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.Lessons.AddRange(lessons);
            await db.SaveChangesAsync();
        }
    }
}
