namespace Integracao.ControlID.PoC.Models.ControlIDApi;

public sealed record OfficialApiStreamMetadata(
    int StatusCode,
    string ContentType,
    long? ContentLength);
