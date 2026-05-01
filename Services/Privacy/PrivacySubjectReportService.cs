using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
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
            await CountSessionsAsync(userKeys, cancellationToken),
            "Curto prazo; sessoes podem ser encerradas pelo administrador.",
            "Encerrar sessoes ativas antes de exportar ou eliminar dados relacionados."));

        report.DataCategories.Add(Category(
            "Fotos faciais locais",
            "Sensivel quando identifica pessoa",
            await CountByUserIdsAsync(_dbContext.Photos, userIds, cancellationToken),
            "Minimo necessario para homologacao; evitar dados reais na PoC.",
            "Confirmar base legal/RIPD antes de compartilhar ou eliminar; nao exportar Base64 por este relatorio."));

        report.DataCategories.Add(Category(
            "Templates biometricos locais",
            "Sensivel",
            await CountByUserIdsAsync(_dbContext.BiometricTemplates, userIds, cancellationToken),
            "Minimo necessario; alto risco.",
            "Exigir decisao DPO/juridico e nao exportar template bruto por canais inseguros."));

        report.DataCategories.Add(Category(
            "Cartoes RFID/tags",
            "Pessoal e credencial de acesso fisico",
            await CountByUserIdsAsync(_dbContext.Cards, userIds, cancellationToken),
            "Minimo necessario para controle de acesso.",
            "Tratar valor do cartao como segredo operacional; revogar antes de eliminar quando aplicavel."));

        report.DataCategories.Add(Category(
            "QR Codes",
            "Pessoal e credencial de acesso fisico",
            await CountByUserIdsAsync(_dbContext.QRCodes, userIds, cancellationToken),
            "Minimo necessario para controle de acesso.",
            "Tratar valor do QR Code como segredo operacional; revogar antes de eliminar quando aplicavel."));

        report.DataCategories.Add(Category(
            "Logs de acesso locais",
            "Pessoal/operacional",
            await CountNullableUserIdAsync(userIds, cancellationToken),
            "Minimo necessario para auditoria e QA.",
            "Avaliar obrigacao de preservacao antes de anonimizar, bloquear ou eliminar."));

        report.DataCategories.Add(Category(
            "Callbacks e monitoramento",
            "Pessoal, tecnico e possivelmente sensivel",
            await CountMonitorEventsAsync(userKeys, cancellationToken),
            "Curto prazo; expurgo guiado por retencao.",
            "Payload bruto nao aparece neste relatorio; usar expurgo por retencao quando autorizado."));

        report.DataCategories.Add(Category(
            "Push e resultados",
            "Pessoal, tecnico e possivelmente sensivel",
            await CountPushCommandsAsync(userKeys, cancellationToken),
            "Curto prazo; expurgo guiado por retencao.",
            "Payload bruto nao aparece neste relatorio; usar expurgo por retencao quando autorizado."));

        report.RightsCoverage =
        [
            "Confirmacao/acesso: este relatorio informa categorias e contagens sem payload sensivel bruto.",
            "Correcao: usar telas administrativas especificas apos confirmar titularidade.",
            "Eliminacao/bloqueio/anonimizacao: exige decisao humana, verificacao de obrigacao de retencao e confirmacao por fluxo de alto impacto.",
            "Portabilidade: exportacao bruta por titular ainda requer formato aprovado e revisao DPO/juridico.",
            "Informacao sobre compartilhamento: consultar docs/privacy-and-data-retention.md e docs/privacy-governance-runbook.md."
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

        return await _dbContext.Users
            .Where(user =>
                (numericId.HasValue && user.Id == numericId.Value) ||
                user.Registration == identifier ||
                user.Username == identifier ||
                user.Email == identifier ||
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

    private static async Task<int> CountByUserIdsAsync<TEntity>(
        IQueryable<TEntity> query,
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (userIds.Count == 0)
            return 0;

        return typeof(TEntity).Name switch
        {
            nameof(PhotoLocal) => await query.Cast<PhotoLocal>().CountAsync(item => userIds.Contains(item.UserId), cancellationToken),
            nameof(BiometricTemplateLocal) => await query.Cast<BiometricTemplateLocal>().CountAsync(item => userIds.Contains(item.UserId), cancellationToken),
            nameof(CardLocal) => await query.Cast<CardLocal>().CountAsync(item => userIds.Contains(item.UserId), cancellationToken),
            nameof(QRCodeLocal) => await query.Cast<QRCodeLocal>().CountAsync(item => userIds.Contains(item.UserId), cancellationToken),
            _ => 0
        };
    }

    private async Task<int> CountNullableUserIdAsync(IReadOnlyCollection<long> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
            return 0;

        return await _dbContext.AccessLogs
            .CountAsync(item => item.UserId.HasValue && userIds.Contains(item.UserId.Value), cancellationToken);
    }

    private async Task<int> CountSessionsAsync(IReadOnlyCollection<string> userKeys, CancellationToken cancellationToken)
    {
        if (userKeys.Count == 0)
            return 0;

        return await _dbContext.Sessions
            .CountAsync(item => userKeys.Contains(item.Username), cancellationToken);
    }

    private async Task<int> CountMonitorEventsAsync(IReadOnlyCollection<string> userKeys, CancellationToken cancellationToken)
    {
        if (userKeys.Count == 0)
            return 0;

        return await _dbContext.MonitorEvents
            .CountAsync(item => userKeys.Contains(item.UserId), cancellationToken);
    }

    private async Task<int> CountPushCommandsAsync(IReadOnlyCollection<string> userKeys, CancellationToken cancellationToken)
    {
        if (userKeys.Count == 0)
            return 0;

        return await _dbContext.PushCommands
            .CountAsync(item => userKeys.Contains(item.UserId), cancellationToken);
    }

    private sealed record MatchedUser(long Id, string Username, string Registration);
}
