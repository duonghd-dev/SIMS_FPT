using Microsoft.AspNetCore.Identity;

namespace SIMS_FPT.Helpers
{
    
    public static class PasswordHasherHelper
    {
        private static readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();
 
        public static string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        // Verify password; also returns true if SuccessRehashNeeded (we'll treat as valid).
        public static bool Verify(string password, string hashed)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashed, password);
            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
