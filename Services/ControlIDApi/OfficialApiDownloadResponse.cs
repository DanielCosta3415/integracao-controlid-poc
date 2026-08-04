using System.Net.Http.Headers;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public static class OfficialApiDownloadResponse
{
    public static ValueTask ApplyAsync(
        HttpResponse response,
        OfficialApiStreamMetadata metadata,
        string fileName,
        string fallbackContentType)
    {
        if (metadata.ContentLength == 0)
            throw new InvalidDataException("O equipamento retornou um arquivo vazio.");

        response.ContentType = NormalizeContentType(metadata.ContentType, fallbackContentType);
        if (metadata.ContentLength is > 0)
            response.ContentLength = metadata.ContentLength;

        var disposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = Path.GetFileName(fileName)
        };
        response.Headers.ContentDisposition = disposition.ToString();
        return ValueTask.CompletedTask;
    }

    private static string NormalizeContentType(string? contentType, string fallbackContentType)
    {
        if (MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType) &&
            !string.IsNullOrWhiteSpace(parsedContentType.MediaType))
        {
            return parsedContentType.MediaType;
        }

        return MediaTypeHeaderValue.TryParse(fallbackContentType, out var parsedFallback) &&
               !string.IsNullOrWhiteSpace(parsedFallback.MediaType)
            ? parsedFallback.MediaType
            : "application/octet-stream";
    }
}
