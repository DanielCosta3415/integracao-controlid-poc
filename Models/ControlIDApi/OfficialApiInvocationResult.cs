namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    public class OfficialApiInvocationResult
    {
        public bool Success { get; set; }
        public string RequestUrl { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ResponseContentType { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public bool ResponseBodyIsBase64 { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
