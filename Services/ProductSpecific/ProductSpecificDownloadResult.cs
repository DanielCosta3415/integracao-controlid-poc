using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Services.ProductSpecific;

public sealed record ProductSpecificDownloadResult(
    OfficialApiInvocationResult Result,
    string FileName,
    string ContentType);
