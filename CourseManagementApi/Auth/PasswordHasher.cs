using System.Security.Cryptography;

namespace CourseManagementApi.Auth
{
    // Проста, але коректна практика хешування паролів: PBKDF2 з випадковою сіллю.
    // Ніколи не зберігаємо паролі відкритим текстом.
    public static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int KeySize = 32; // 256 біт

        public static (string Hash, string Salt) Hash(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public static bool Verify(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
            var storedHashBytes = Convert.FromBase64String(storedHash);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }
    }
}
