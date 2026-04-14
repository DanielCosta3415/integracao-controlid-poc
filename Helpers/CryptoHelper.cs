using System;
using System.Security.Cryptography;
using System.Text;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class CryptoHelper
    {
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