namespace LFE.Application.Services.Security
{
    public static class PasswordHasher
    {
        public static string ComputeHash(string source)
        {
            string result = BCrypt.HashPassword(source + PasswordConstants.HASH_SALT, PasswordConstants.HASH_WORK_FACTOR);
            return result;
        }

        public static bool VerifyHash(string text, string hash)
        {
            return BCrypt.Verify(text + PasswordConstants.HASH_SALT, hash);
        }
    }
}
