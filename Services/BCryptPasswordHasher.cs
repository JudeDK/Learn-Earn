using Microsoft.AspNetCore.Identity;

namespace Learn_Earn.Services
{
    // This class kept for compatibility but delegates to the default ASP.NET Identity hasher.
    public class BCryptPasswordHasher : IPasswordHasher<IdentityUser>
    {
        private readonly PasswordHasher<IdentityUser> _inner = new PasswordHasher<IdentityUser>();

        public string HashPassword(IdentityUser user, string password)
        {
            return _inner.HashPassword(user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(IdentityUser user, string hashedPassword, string providedPassword)
        {
            return _inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }
}
