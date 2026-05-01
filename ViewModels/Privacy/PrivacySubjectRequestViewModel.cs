using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Privacy;

public sealed class PrivacySubjectRequestViewModel
{
    [Display(Name = "Identificador do titular")]
    [StringLength(160, ErrorMessage = "Informe ate 160 caracteres.")]
    public string Identifier { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public PrivacySubjectReportViewModel? Report { get; set; }
}

public sealed class PrivacySubjectReportViewModel
{
    public DateTime GeneratedAtUtc { get; set; }

    public string IdentifierRef { get; set; } = string.Empty;

    public List<string> MatchedUserRefs { get; set; } = [];

    public List<PrivacyDataCategorySummaryViewModel> DataCategories { get; set; } = [];

    public List<string> RightsCoverage { get; set; } = [];

    public List<string> RequiredHumanDecisions { get; set; } = [];

    public bool HasMatches => DataCategories.Any(category => category.RecordCount > 0);
}

public sealed class PrivacyDataCategorySummaryViewModel
{
    public string Area { get; set; } = string.Empty;

    public string Classification { get; set; } = string.Empty;

    public int RecordCount { get; set; }

    public string Retention { get; set; } = string.Empty;

    public string FulfillmentGuidance { get; set; } = string.Empty;
}
