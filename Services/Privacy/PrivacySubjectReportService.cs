using System.Data;
using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Models.Security;
using Integracao.ControlID.PoC.ViewModels.Privacy;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Services.Privacy;

public sealed class PrivacySubjectReportService
{
    private readonly IntegracaoControlIDContext _dbContext;

    public PrivacySubjectReportService(IntegracaoControlIDContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PrivacySubjectReportViewModel> BuildReportAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = (identifier ?? string.Empty).Trim();
        var identifierRef = PrivacyLogHelper.PseudonymizeIdentifier(normalizedIdentifier, "ref:not-informed");
        var matchedUsers = await FindMatchingUsersAsync(normalizedIdentifier, cancellationToken);
        var userIds = BuildCandidateUserIds(normalizedIdentifier, matchedUsers.Select(user => user.Id));
        var userKeys = BuildCandidateUserKeys(normalizedIdentifier, userIds, matchedUsers.Select(user => user.Username), matchedUsers.Select(user => user.Registration));
        var counts = await CountRelatedRecordsAsync(userIds, userKeys, cancellationToken);

        var report = new PrivacySubjectReportViewModel
        {
            GeneratedAtUtc = DateTime.UtcNow,
            IdentifierRef = identifierRef,
            MatchedUserRefs = userIds
                .Select(id => PrivacyLogHelper.PseudonymizeIdentifier(id))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList()
        };

        report.DataCategories.Add(Category(
            "Usuarios locais",
            "Pessoal comum e credencial derivada",
            matchedUsers.Count,
            "Enquanto a conta local for necessaria; exclusao exige avaliacao de acesso administrativo.",
            "Confirmar titularidade e usar os fluxos administrativos de usuario; nunca expor hash, salt ou senha."));

        report.DataCategories.Add(Category(
            "Sessoes locais",
            "Credencial/confidencial e tecnico identificavel",
            counts.Sessions,
            "Curto prazo; sessoes podem ser encerradas pelo administrador.",
            "Encerrar sessoes ativas antes de exportar ou eliminar dados relacionados."));

        report.DataCategories.Add(Category(
            "Fotos faciais locais",
            "Sensivel quando identifica pessoa",
            counts.Photos,
            "Minimo necessario para homologacao; evitar dados reais na PoC.",
            "Confirmar base legal/RIPD antes de compartilhar ou eliminar; nao exportar Base64 por este relatorio."));

        report.DataCategories.Add(Category(
            "Templates biometricos locais",
            "Sensivel",
            counts.BiometricTemplates,
            "Minimo necessario; alto risco.",
            "Exigir decisao DPO/juridico e nao exportar template bruto por canais inseguros."));

        report.DataCategories.Add(Category(
            "Cartoes RFID/tags",
            "Pessoal e credencial de acesso fisico",
            counts.Cards,
            "Minimo necessario para controle de acesso.",
            "Tratar valor do cartao como segredo operacional; revogar antes de eliminar quando aplicavel."));

        report.DataCategories.Add(Category(
            "QR Codes",
            "Pessoal e credencial de acesso fisico",
            counts.QrCodes,
            "Minimo necessario para controle de acesso.",
            "Tratar valor do QR Code como segredo operacional; revogar antes de eliminar quando aplicavel."));

        report.DataCategories.Add(Category(
            "Logs de acesso locais",
            "Pessoal/operacional",
            counts.AccessLogs,
            "Minimo necessario para auditoria e QA.",
            "Avaliar obrigacao de preservacao antes de anonimizar, bloquear ou eliminar."));

        report.DataCategories.Add(Category(
            "Callbacks e monitoramento",
            "Pessoal, tecnico e possivelmente sensivel",
            counts.MonitorEvents,
            "Curto prazo; expurgo guiado por retencao.",
            "Payload bruto nao aparece neste relatorio; usar expurgo por retencao quando autorizado."));

        report.DataCategories.Add(Category(
            "Push e resultados",
            "Pessoal, tecnico e possivelmente sensivel",
            counts.PushCommands,
            "Curto prazo; expurgo guiado por retencao.",
            "Payload bruto nao aparece neste relatorio; usar expurgo por retencao quando autorizado."));

        report.RightsCoverage =
        [
            "Confirmacao/acesso: este relatorio informa categorias e contagens sem payload sensivel bruto.",
            "Correcao: usar telas administrativas especificas apos confirmar titularidade.",
            "Eliminacao/bloqueio/anonimizacao: exige decisao humana, verificacao de obrigacao de retencao e confirmacao por fluxo de alto impacto.",
            "Portabilidade: exportacao bruta por titular ainda requer formato aprovado e revisao DPO/juridico.",
            "Informacao sobre compartilhamento: consultar docs/seguranca-privacidade/privacy-and-data-retention.md e docs/seguranca-privacidade/privacy-governance-runbook.md."
        ];

        report.RequiredHumanDecisions =
        [
            "Validar base legal do tratamento antes de cumprir a solicitacao.",
            "Confirmar identidade, poderes do solicitante e escopo da solicitacao.",
            "Verificar se existe obrigacao de preservacao de logs, auditoria, seguranca ou defesa de direitos.",
            "Registrar decisao do controlador/DPO antes de qualquer exclusao, exportacao bruta ou compartilhamento.",
            "Acionar runbook de incidente se a solicitacao revelar tratamento indevido ou vazamento."
        ];

        return report;
    }

    private async Task<List<MatchedUser>> FindMatchingUsersAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return [];

        var numericId = long.TryParse(identifier, out var parsedId) ? parsedId : (long?)null;
        var normalized = LocalIdentityPolicy.NormalizeIdentifier(identifier);

        return await _dbContext.Users
            .Where(user =>
                (numericId.HasValue && user.Id == numericId.Value) ||
                user.Registration == identifier ||
                user.NormalizedUsername == normalized ||
                user.NormalizedEmail == normalized ||
                user.Phone == identifier)
            .Select(user => new MatchedUser(user.Id, user.Username, user.Registration))
            .Take(25)
            .ToListAsync(cancellationToken);
    }

    private static List<long> BuildCandidateUserIds(string identifier, IEnumerable<long> matchedUserIds)
    {
        var ids = matchedUserIds.ToHashSet();
        if (long.TryParse(identifier, out var parsedId))
            ids.Add(parsedId);

        return ids.Order().ToList();
    }

    private static List<string> BuildCandidateUserKeys(string identifier, IEnumerable<long> userIds, IEnumerable<string> usernames, IEnumerable<string> registrations)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddIfPresent(keys, identifier);

        foreach (var userId in userIds)
            keys.Add(userId.ToString(System.Globalization.CultureInfo.InvariantCulture));

        foreach (var username in usernames)
            AddIfPresent(keys, username);

        foreach (var registration in registrations)
            AddIfPresent(keys, registration);

        return keys.Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static void AddIfPresent(ISet<string> values, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            values.Add(value.Trim());
    }

    private static PrivacyDataCategorySummaryViewModel Category(
        string area,
        string classification,
        int recordCount,
        string retention,
        string fulfillmentGuidance)
    {
        return new PrivacyDataCategorySummaryViewModel
        {
            Area = area,
            Classification = classification,
            RecordCount = recordCount,
            Retention = retention,
            FulfillmentGuidance = fulfillmentGuidance
        };
    }

    private async Task<RelatedRecordCounts> CountRelatedRecordsAsync(
        IReadOnlyCollection<long> userIds,
        IReadOnlyCollection<string> userKeys,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0 && userKeys.Count == 0)
            return RelatedRecordCounts.Empty;

        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            var userIdParameters = AddParameters(command, "userId", userIds.Cast<object>());
            var userKeyParameters = AddParameters(command, "userKey", userKeys.Cast<object>());
            command.CommandText = $"""
                SELECT
                    (SELECT COUNT(*) FROM "Sessions" WHERE "Username" IN ({userKeyParameters})),
                    (SELECT COUNT(*) FROM "Photos" WHERE "UserId" IN ({userIdParameters})),
                    (SELECT COUNT(*) FROM "BiometricTemplates" WHERE "UserId" IN ({userIdParameters})),
                    (SELECT COUNT(*) FROM "Cards" WHERE "UserId" IN ({userIdParameters})),
                    (SELECT COUNT(*) FROM "QRCodes" WHERE "UserId" IN ({userIdParameters})),
                    (SELECT COUNT(*) FROM "AccessLogs" WHERE "UserId" IN ({userIdParameters})),
                    (SELECT COUNT(*) FROM "MonitorEvents" WHERE "UserId" IN ({userKeyParameters})),
                    (SELECT COUNT(*) FROM "PushCommands" WHERE "UserId" IN ({userKeyParameters}));
                """;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return RelatedRecordCounts.Empty;

            return new RelatedRecordCounts(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetInt32(7));
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private static string AddParameters(
        System.Data.Common.DbCommand command,
        string prefix,
        IEnumerable<object> values)
    {
        var names = new List<string>();
        foreach (var value in values)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{prefix}{names.Count}";
            parameter.Value = value;
            command.Parameters.Add(parameter);
            names.Add(parameter.ParameterName);
        }

        return names.Count == 0 ? "NULL" : string.Join(", ", names);
    }

    private sealed record MatchedUser(long Id, string Username, string Registration);

    private sealed record RelatedRecordCounts(
        int Sessions,
        int Photos,
        int BiometricTemplates,
        int Cards,
        int QrCodes,
        int AccessLogs,
        int MonitorEvents,
        int PushCommands)
    {
        public static RelatedRecordCounts Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);
    }
}
