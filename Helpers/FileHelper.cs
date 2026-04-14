using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Salva bytes recebidos em um arquivo no caminho especificado.
        /// </summary>
        public static async Task SaveFileAsync(string filePath, byte[] bytes)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(filePath, bytes);
        }

        /// <summary>
        /// Lê um arquivo e retorna seu conteúdo em bytes.
        /// </summary>
        public static async Task<byte[]> ReadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo não encontrado.", filePath);

            return await File.ReadAllBytesAsync(filePath);
        }

        /// <summary>
        /// Remove um arquivo do disco se ele existir.
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Valida extensão de arquivo.
        /// </summary>
        public static bool IsExtensionAllowed(string fileName, params string[] allowedExtensions)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && allowedExtensions.Contains(ext);
        }

        /// <summary>
        /// Valida tamanho máximo do arquivo (em bytes).
        /// </summary>
        public static bool IsFileSizeAllowed(long fileSize, long maxBytes)
        {
            return fileSize <= maxBytes;
        }

        /// <summary>
        /// Gera nome de arquivo único baseado em GUID, mantendo a extensão original.
        /// </summary>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            var ext = Path.GetExtension(originalFileName);
            var guid = Guid.NewGuid().ToString("N");
            return $"{guid}{ext}";
        }

        /// <summary>
        /// Tenta obter nome de arquivo sem caminho.
        /// </summary>
        public static string GetSafeFileName(string fileName)
        {
            return Path.GetFileName(fileName);
        }

        /// <summary>
        /// Cria um diretório, caso não exista.
        /// </summary>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
    }
}
