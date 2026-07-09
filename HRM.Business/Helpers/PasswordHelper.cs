namespace HRM.Business.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required.", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(storedPassword))
            {
                return false;
            }

            if (IsBCryptHash(storedPassword))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
                }
                catch
                {
                    return false;
                }
            }

            // Dùng cho seed data demo hiện tại: PasswordHash đang là plain text "Password@123"
            return inputPassword == storedPassword;
        }

        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < 8)
            {
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public static bool IsBCryptHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            return passwordHash.StartsWith("$2a$")
                || passwordHash.StartsWith("$2b$")
                || passwordHash.StartsWith("$2y$");
        }
    }
}
