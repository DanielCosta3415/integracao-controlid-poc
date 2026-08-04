using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Development;

public sealed class SimulatorViewModel
{
    public bool IsReachable { get; set; }
    public string StubUrl { get; set; } = string.Empty;
    public string Scenario { get; set; } = "normal";
    public string Profile { get; set; } = "idface";
    public int DatasetSize { get; set; } = 1;
    public IReadOnlyList<string> AvailableScenarios { get; set; } = [];
    public IReadOnlyList<string> AvailableProfiles { get; set; } = [];
    public IReadOnlyList<int> AvailableDatasetSizes { get; set; } = [];
    public IReadOnlyList<SimulatorRequestMetricViewModel> Requests { get; set; } = [];
    public string? StatusMessage { get; set; }
    public string StatusType { get; set; } = "info";

    [Required]
    public string SelectedScenario { get; set; } = "normal";

    [Range(0, 60_000)]
    public int DelayMilliseconds { get; set; } = 750;

    [StringLength(256)]
    public string Endpoint { get; set; } = string.Empty;

    [Range(1_048_577, 67_108_864)]
    public int ResponseBytes { get; set; } = 17 * 1024 * 1024;

    [Required]
    public string SelectedProfile { get; set; } = "idface";

    [Range(1, 100_000)]
    public int SelectedDatasetSize { get; set; } = 1;
}

public sealed record SimulatorRequestMetricViewModel(
    string Path,
    long Count,
    double AverageMilliseconds,
    double MaximumMilliseconds);
