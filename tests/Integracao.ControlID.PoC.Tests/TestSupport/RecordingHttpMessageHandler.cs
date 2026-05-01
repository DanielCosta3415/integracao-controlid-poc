using System.Net;
using System.Text;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = [];

    public List<RecordedHttpRequest> Requests { get; } = [];

    public void EnqueueJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses.Enqueue(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new RecordedHttpRequest(
            request.Method.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            body,
            request.Content?.Headers.ContentType?.MediaType ?? string.Empty,
            request.Headers.ToDictionary(
                header => header.Key,
                header => string.Join(",", header.Value),
                StringComparer.OrdinalIgnoreCase)));

        return _responses.Count == 0
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }
            : _responses.Dequeue();
    }
}

public sealed record RecordedHttpRequest(
    string Method,
    string Url,
    string Body,
    string ContentType,
    IReadOnlyDictionary<string, string> Headers);
