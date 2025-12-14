using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Learn_Earn.Pages.Quizzes
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogger<CreateModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public Quiz Quiz { get; set; }

        [BindProperty]
        public IFormFile? Attachment { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? NotificationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }


        public void OnGet(int? lessonId, int? notificationId, string? returnUrl)
        {
            Quiz = new Quiz();
            if (lessonId.HasValue) Quiz.LessonId = lessonId.Value;
            NotificationId = notificationId;
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                _logger.LogWarning("ModelState invalid when creating quiz: {Errors}", JsonSerializer.Serialize(errors));
                return Page();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                Quiz.CreatedByUserId = userId;
                // handle attachment upload
                if (Attachment != null && Attachment.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var guidName = Guid.NewGuid().ToString() + Path.GetExtension(Attachment.FileName);
                    var filePath = Path.Combine(uploads, guidName);
                    using (var fs = System.IO.File.Create(filePath))
                    {
                        await Attachment.CopyToAsync(fs);
                    }
                    Quiz.AttachmentFileName = Attachment.FileName;
                    Quiz.AttachmentPath = "/uploads/" + guidName;
                    Quiz.AttachmentContentType = Attachment.ContentType;
                }

                _db.Quizzes.Add(Quiz);
                await _db.SaveChangesAsync();

                // If a return URL was provided, go back there (e.g. lesson details with notification)
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("AddQuestion", new { quizId = Quiz.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz (lessonId={LessonId}, notificationId={NotificationId})", Quiz?.LessonId, NotificationId);
                // For debugging: show full exception on the page (development only)
                ViewData["ExceptionDetails"] = ex.ToString();
                ModelState.AddModelError(string.Empty, "A intervenit o eroare la crearea quiz-ului. Detaliile apar mai jos (doar în dev).");
                return Page();
            }
        }
    }
}
