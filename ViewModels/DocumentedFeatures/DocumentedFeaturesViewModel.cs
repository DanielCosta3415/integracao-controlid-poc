using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.DocumentedFeatures
{
    public class DocumentedFeaturesViewModel
    {
        public string ActiveSection { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;

        [Display(Name = "Habilitar modo ponto")]
        public bool AttendanceModeEnabled { get; set; }

        [Display(Name = "Permitir tipos personalizados de batida")]
        public bool AttendanceCustomLogTypesEnabled { get; set; }

        [Required]
        [Display(Name = "Limpeza de usuarios expirados")]
        public string AttendanceClearExpiredUsers { get; set; } = "visitors";

        [Display(Name = "Reutilizar ID de device existente")]
        public bool OnlineUseExistingDevice { get; set; }

        [Display(Name = "ID do device existente")]
        public long? OnlineExistingDeviceId { get; set; }

        [Display(Name = "Server ID atual")]
        public long? OnlineCurrentServerId { get; set; }

        [Required]
        [Display(Name = "Nome do servidor")]
        public string OnlineServerName { get; set; } = "Control iD PoC";

        [Required]
        [Display(Name = "URL do servidor")]
        public string OnlineServerUrl { get; set; } = "http://localhost:5000";

        [Display(Name = "Chave publica")]
        public string OnlinePublicKey { get; set; } = string.Empty;

        [Display(Name = "Habilitar modo online")]
        public bool OnlineEnabled { get; set; }

        [Display(Name = "Manter identificacao local (Pro)")]
        public bool OnlineLocalIdentification { get; set; } = true;

        [Display(Name = "Extrair template")]
        public bool OnlineExtractTemplate { get; set; }

        [Range(1, 30)]
        [Display(Name = "Maximo de tentativas")]
        public int OnlineMaxRequestAttempts { get; set; } = 3;

        [Display(Name = "SSH habilitado")]
        public bool SecuritySshEnabled { get; set; }

        [Display(Name = "USB habilitado")]
        public bool SecurityUsbPortEnabled { get; set; } = true;

        [Display(Name = "Interface Web habilitada")]
        public bool SecurityWebServerEnabled { get; set; } = true;

        [Display(Name = "SNMP habilitado")]
        public bool SecuritySnmpEnabled { get; set; }

        [Required]
        [Display(Name = "Limpeza de usuarios/visitantes expirados")]
        public string VisitorsClearExpiredUsers { get; set; } = "visitors";

        [Display(Name = "Exigir deposito do cartao do visitante")]
        public bool VisitorsCollectCardOnExit { get; set; }

        [Required]
        [Display(Name = "Endereco Push")]
        public string IdCloudPushRemoteAddress { get; set; } = "https://push.idsecure.com.br/api";

        [Range(1, 120000)]
        [Display(Name = "Timeout da requisicao (ms)")]
        public int IdCloudPushRequestTimeout { get; set; } = 30000;

        [Range(1, 3600)]
        [Display(Name = "Periodo de polling (s)")]
        public int IdCloudPushRequestPeriod { get; set; } = 5;

        [Display(Name = "Codigo de verificacao atual")]
        public string IdCloudVerificationCode { get; set; } = string.Empty;

        [Display(Name = "Alarme ativo")]
        public bool AlarmActive { get; set; }

        [Display(Name = "Causa atual")]
        public int AlarmCause { get; set; }

        [Display(Name = "Violacao do dispositivo")]
        public bool AlarmDeviceViolationEnabled { get; set; }

        [Range(0, 60000)]
        [Display(Name = "Timeout do sensor apos fechamento (ms)")]
        public int AlarmDoorSensorAlarmTimeoutAfterClosure { get; set; }

        [Range(0, 60000)]
        [Display(Name = "Atraso do sensor de porta (ms)")]
        public int AlarmDoorSensorDelay { get; set; } = 5;

        [Display(Name = "Sensor de porta habilitado")]
        public bool AlarmDoorSensorEnabled { get; set; } = true;

        [Range(0, 60000)]
        [Display(Name = "Debounce de acesso forcado (ms)")]
        public int AlarmForcedAccessDebounce { get; set; } = 2;

        [Display(Name = "Acesso forcado habilitado")]
        public bool AlarmForcedAccessEnabled { get; set; }

        [Display(Name = "Cartao de panico habilitado")]
        public bool AlarmPanicCardEnabled { get; set; } = true;

        [Range(0, 60000)]
        [Display(Name = "Delay do dedo de panico (s)")]
        public int AlarmPanicFingerDelay { get; set; } = 1;

        [Display(Name = "Dedo de panico habilitado")]
        public bool AlarmPanicFingerEnabled { get; set; } = true;

        [Display(Name = "Senha de panico habilitada")]
        public bool AlarmPanicPasswordEnabled { get; set; } = true;

        [Display(Name = "PIN de panico habilitado")]
        public bool AlarmPanicPinEnabled { get; set; } = true;

        [Required]
        [Display(Name = "Payload JSON do relatorio")]
        public string ReportPayload { get; set; } = "{\n  \"object\": \"users\",\n  \"order\": [\"ascending\", \"name\"],\n  \"where\": {\n    \"users\": {},\n    \"groups\": {},\n    \"time_zones\": {}\n  },\n  \"delimiter\": \";\",\n  \"line_break\": \"\\r\\n\",\n  \"header\": \"Name (User);Id (User)\",\n  \"file_name\": \"\",\n  \"join\": \"LEFT\",\n  \"columns\": [\n    {\n      \"type\": \"object_field\",\n      \"object\": \"users\",\n      \"field\": \"name\"\n    },\n    {\n      \"type\": \"object_field\",\n      \"object\": \"users\",\n      \"field\": \"id\"\n    }\n  ]\n}";

        [Required]
        [Display(Name = "Modo AFD")]
        public string AfdMode { get; set; } = "595";

        [Display(Name = "NSR inicial")]
        public int? AfdInitialNsr { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data inicial")]
        public DateTime? AfdInitialDate { get; set; }

        [Display(Name = "Categoria config")]
        public bool AuditConfig { get; set; } = true;

        [Display(Name = "Categoria API")]
        public bool AuditApi { get; set; } = true;

        [Display(Name = "Categoria USB")]
        public bool AuditUsb { get; set; } = true;

        [Display(Name = "Categoria rede")]
        public bool AuditNetwork { get; set; } = true;

        [Display(Name = "Categoria horario")]
        public bool AuditTime { get; set; } = true;

        [Display(Name = "Categoria online")]
        public bool AuditOnline { get; set; } = true;

        [Display(Name = "Categoria menu")]
        public bool AuditMenu { get; set; } = true;
    }
}
