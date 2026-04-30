using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class SessionHelper
    {
        /// <summary>
        /// Salva um objeto qualquer na sessão (serializado como JSON).
        /// </summary>
        public static void SetObjectAsJson(ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Obtém um objeto da sessão (deserializado do JSON).
        /// </summary>
        public static T? GetObjectFromJson<T>(ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Salva uma string na sessão.
        /// </summary>
        public static void SetString(ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        /// <summary>
        /// Obtém uma string da sessão.
        /// </summary>
        public static string? GetString(ISession session, string key)
        {
            return session.GetString(key);
        }

        /// <summary>
        /// Remove um item da sessão.
        /// </summary>
        public static void Remove(ISession session, string key)
        {
            session.Remove(key);
        }

        /// <summary>
        /// Limpa toda a sessão.
        /// </summary>
        public static void Clear(ISession session)
        {
            session.Clear();
        }

        /// <summary>
        /// Verifica se existe um valor na sessão para a chave informada.
        /// </summary>
        public static bool Contains(ISession session, string key)
        {
            return session.GetString(key) != null;
        }

        /// <summary>
        /// Verifica se o usuário está autenticado via sessão.
        /// </summary>
        public static bool IsAuthenticated(ISession session, string key = "IsAuthenticated")
        {
            var val = session.GetString(key);
            return !string.IsNullOrEmpty(val) && val == "true";
        }
    }
}
