using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers;

// Keep MVC concerns here and push API workflows into dedicated services for easier testing.
public class ProductSpecificController : Controller
{
    private readonly IOfficialControlIdApiService _apiService;
    private readonly ProductSpecificSnapshotService _snapshotService;
    private readonly ProductSpecificCommandService _commandService;
    private readonly OfficialApiBinaryFileResultFactory _binaryFileResultFactory;

    public ProductSpecificController(
        IOfficialControlIdApiService apiService,
        ProductSpecificSnapshotService snapshotService,
        ProductSpecificCommandService commandService,
        OfficialApiBinaryFileResultFactory binaryFileResultFactory)
    {
        _apiService = apiService;
        _snapshotService = snapshotService;
        _commandService = commandService;
        _binaryFileResultFactory = binaryFileResultFactory;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ProductSpecificViewModel();
        if (!EnsureConnected(model))
            return View(model);

        await _snapshotService.PopulateAllAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeIdFace(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.UpgradeIdFaceAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeEnterprise(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.UpgradeEnterpriseAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FacialSettings(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplyFacialSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QrCodeSettings(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplyQrCodeSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PowerSettings(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplyPowerSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Streaming(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplyStreamingSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SipSettings(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplySipSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshSipStatus(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.RefreshSipStatusAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeSipCall(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.MakeSipCallAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeSipCall(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.FinalizeSipCallAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckSipAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.CheckSipAudioAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadSipAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.UploadSipAudioAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadSipAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        var download = await _commandService.DownloadSipAudioAsync(model);
        if (download == null)
            return View("Index", model);

        return _binaryFileResultFactory.Create(download.Result, download.FileName, download.ContentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccessAudioSettings(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplyAccessAudioSettingsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckAccessAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.CheckAccessAudioAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAccessAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.UploadAccessAudioAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadAccessAudio(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        var download = await _commandService.DownloadAccessAudioAsync(model);
        if (download == null)
            return View("Index", model);

        return _binaryFileResultFactory.Create(download.Result, download.FileName, download.ContentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signals(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.ApplySignalsAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshLeds(ProductSpecificViewModel model)
    {
        if (!EnsureConnected(model))
            return View("Index", model);

        await _commandService.RefreshLedsAsync(model);
        return View("Index", model);
    }

    private bool EnsureConnected(ProductSpecificViewModel model)
    {
        if (_apiService.TryGetConnection(out _, out _))
            return true;

        model.ErrorMessage = "E necessario conectar-se e autenticar com um equipamento Control iD.";
        return false;
    }
}
