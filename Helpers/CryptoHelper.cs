using System;
using System.Security.Cryptography;
using System.Text;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class CryptoHelper
    {
        private const int Pbkdf2SaltBytes = 16;
        private const int Pbkdf2HashBytes = 32;
        private const int Pbkdf2Iterations = 210_000;
        private const string Pbkdf2Prefix = "PBKDF2";

        /// <summary>
        /// Gera hash SHA256 para uma string, com salt opcional.
        /// </summary>
        public static string ComputeSha256Hash(string rawData, string? salt = null)
        {
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(salt) ? rawData : rawData + salt);
            var hashBytes = sha256.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Gera um salt criptográfico aleatório.
        /// </summary>
        public static string GenerateSalt(int size = 16)
        {
            var saltBytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Compara um hash SHA256 com um valor informado e salt.
        /// </summary>
        public static bool VerifySha256Hash(string rawData, string hash, string? salt = null)
        {
            var computedHash = ComputeSha256Hash(rawData, salt);
            return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
        }

        public static string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            var salt = RandomNumberGenerator.GetBytes(Pbkdf2SaltBytes);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                Pbkdf2HashBytes);

            return string.Join(
                "$",
                Pbkdf2Prefix,
                Pbkdf2Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedHash, string? legacySalt = null)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            if (!IsPbkdf2Hash(storedHash))
                return VerifySha256Hash(password, storedHash, legacySalt);

            var parts = storedHash.Split('$');
            if (parts.Length != 4 ||
                !int.TryParse(parts[1], out var iterations) ||
                iterations <= 0)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsPbkdf2Hash(string? storedHash)
        {
            return storedHash?.StartsWith(Pbkdf2Prefix + "$", StringComparison.Ordinal) == true;
        }

        /// <summary>
        /// Gera um token seguro e único, por padrão 32 bytes (256 bits), em Base64.
        /// </summary>
        public static string GenerateSecureToken(int size = 32)
        {
            var tokenBytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        /// <summary>
        /// Gera hash MD5 (NÃO recomendado para segurança).
        /// </summary>
        public static string ComputeMd5Hash(string rawData)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(rawData);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
