using System.Text.Json;
using Integracao.ControlID.PoC.Services.Privacy;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Privacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers;

[Authorize(Roles = AppSecurityRoles.Administrator)]
public sealed class PrivacyController : Controller
{
    private readonly PrivacySubjectReportService _subjectReportService;

    public PrivacyController(PrivacySubjectReportService subjectReportService)
    {
        _subjectReportService = subjectReportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? identifier, CancellationToken cancellationToken)
    {
        var model = new PrivacySubjectRequestViewModel
        {
            Identifier = identifier ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(identifier))
            model.Report = await _subjectReportService.BuildReportAsync(identifier, cancellationToken);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Export(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return BadRequest("Informe um identificador do titular.");

        var report = await _subjectReportService.BuildReportAsync(identifier, cancellationToken);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return File(bytes, "application/json", $"privacy-subject-report-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }
}
