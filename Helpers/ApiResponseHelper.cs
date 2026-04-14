using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Gera uma resposta JSON padrão para sucesso, com resultado.
        /// </summary>
        public static IActionResult Success(object? result = null, string? message = null)
        {
            var response = new
            {
                Success = true,
                Message = message,
                Data = result
            };

            return new JsonResult(response)
            {
                StatusCode = 200,
                ContentType = "application/json"
            };
        }

        /// <summary>
        /// Gera uma resposta JSON padrão para erro, com mensagem e status HTTP.
        /// </summary>
        public static IActionResult Error(string errorMessage, int statusCode = 400, object? details = null)
        {
            var response = new
            {
                Success = false,
                Message = errorMessage,
                Details = details
            };

            return new JsonResult(response)
            {
                StatusCode = statusCode,
                ContentType = "application/json"
            };
        }

        /// <summary>
        /// Serializa um objeto para string JSON padrão, para uso fora do MVC.
        /// </summary>
        public static string ToJson(object obj, bool indented = false)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Gera resposta customizada (para uso em endpoints sem controller).
        /// </summary>
        public static string CustomResponse(bool success, string? message, object? data = null)
        {
            var response = new
            {
                Success = success,
                Message = message,
                Data = data
            };
            return ToJson(response);
        }
    }
}