using System.Buffers;
using System.Text;
using Integracao.ControlID.PoC.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Callbacks
{
    public class CallbackRequestBodyReader
    {
        private readonly CallbackSecurityOptions _options;

        public CallbackRequestBodyReader(IOptions<CallbackSecurityOptions> options)
        {
            _options = options.Value;
        }

        public async Task<CallbackRequestBodyReadResult> ReadAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request.ContentLength.HasValue && request.ContentLength.Value == 0)
                return CallbackRequestBodyReadResult.Success(string.Empty);

            var maxBodyBytes = _options.MaxBodyBytes > 0 ? _options.MaxBodyBytes : 1024 * 1024;

            using var memoryStream = new MemoryStream();
            var buffer = ArrayPool<byte>.Shared.Rent(81920);

            try
            {
                while (true)
                {
                    var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                    if (bytesRead == 0)
                        break;

                    await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                    if (memoryStream.Length > maxBodyBytes)
                    {
                        return CallbackRequestBodyReadResult.Failure(
                            StatusCodes.Status413PayloadTooLarge,
                            $"Payload exceeds the configured limit of {maxBodyBytes} bytes.");
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            if (request.Body.CanSeek)
                request.Body.Position = 0;

            var bytes = memoryStream.ToArray();
            var contentType = request.ContentType ?? string.Empty;

            if (contentType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("image/", StringComparison.OrdinalIgnoreCase))
            {
                return CallbackRequestBodyReadResult.Success(Convert.ToBase64String(bytes));
            }

            return CallbackRequestBodyReadResult.Success(Encoding.UTF8.GetString(bytes));
        }
    }

    public sealed class CallbackRequestBodyReadResult
    {
        private CallbackRequestBodyReadResult(bool isSuccessful, int statusCode, string message, string body)
        {
            IsSuccessful = isSuccessful;
            StatusCode = statusCode;
            Message = message;
            Body = body;
        }

        public bool IsSuccessful { get; }
        public int StatusCode { get; }
        public string Message { get; }
        public string Body { get; }

        public static CallbackRequestBodyReadResult Success(string body)
        {
            return new CallbackRequestBodyReadResult(true, StatusCodes.Status200OK, string.Empty, body);
        }

        public static CallbackRequestBodyReadResult Failure(int statusCode, string message)
        {
            return new CallbackRequestBodyReadResult(false, statusCode, message, string.Empty);
        }
    }
}
