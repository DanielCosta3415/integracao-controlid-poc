namespace Integracao.ControlID.PoC.Logging
{
    /// <summary>
    /// Define categorias e códigos customizados de eventos para logs com Serilog.
    /// Útil para padronizar logs por área, funcionalidade ou criticidade.
    /// </summary>
    public static class SeriLogEvents
    {
        // Categoria geral
        public const string General = "General";
        public const string Startup = "Startup";
        public const string Shutdown = "Shutdown";

        // Categorias específicas
        public const string ApiRequest = "API_Request";
        public const string ApiResponse = "API_Response";
        public const string Database = "Database";
        public const string Repository = "Repository";
        public const string Controller = "Controller";
        public const string Middleware = "Middleware";
        public const string Monitor = "Monitor";
        public const string Sync = "Sync";
        public const string Authentication = "Authentication";
        public const string Session = "Session";
        public const string AccessControl = "AccessControl";
        public const string PushNotification = "PushNotification";
        public const string Hardware = "Hardware";
        public const string File = "File";
        public const string Config = "Config";
        public const string UserManagement = "UserManagement";

        // Severidade customizada (apenas para referência rápida, use o nível Serilog padrão para filtro)
        public const string Info = "INFO";
        public const string Warn = "WARN";
        public const string Error = "ERROR";
        public const string Fatal = "FATAL";
        public const string Debug = "DEBUG";
        public const string Trace = "TRACE";
    }
}