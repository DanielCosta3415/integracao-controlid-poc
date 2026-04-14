namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Resposta oficial do endpoint /user_hash_password.fcgi.
    /// </summary>
    public class HashPasswordResponse
    {
        public string Password { get; set; } = string.Empty;

        public string Salt { get; set; } = string.Empty;
    }
}
