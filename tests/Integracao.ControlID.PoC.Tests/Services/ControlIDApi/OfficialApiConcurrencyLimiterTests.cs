using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public sealed class OfficialApiConcurrencyLimiterTests
{
    [Fact]
    public async Task AcquireAsync_SerializesRequestsForTheSameDevice()
    {
        using var limiter = CreateLimiter(concurrency: 1, queueLimit: 2);
        using var first = await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken);

        var secondTask = limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken).AsTask();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.False(secondTask.IsCompleted);

        first.Dispose();
        using var second = await secondTask;
    }

    [Fact]
    public async Task AcquireAsync_AllowsDifferentDevicesInParallel()
    {
        using var limiter = CreateLimiter(concurrency: 1, queueLimit: 0);
        using var first = await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken);
        using var second = await limiter.AcquireAsync("device-b:80", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AcquireAsync_RejectsWhenPerDeviceQueueIsFull()
    {
        using var limiter = CreateLimiter(concurrency: 1, queueLimit: 1);
        using var first = await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        var queued = limiter.AcquireAsync("device-a:80", cancellation.Token).AsTask();

        await Task.Delay(50, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<OfficialApiConcurrencyRejectedException>(async () =>
            await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken));

        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await queued);
    }

    [Fact]
    public async Task AcquireAsync_UsesOneBoundedOverflowGateAfterTrackedDeviceLimit()
    {
        using var limiter = CreateLimiter(concurrency: 1, queueLimit: 0, maxTrackedDevices: 1);
        using var tracked = await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken);
        using var overflow = await limiter.AcquireAsync("device-b:80", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<OfficialApiConcurrencyRejectedException>(async () =>
            await limiter.AcquireAsync("device-c:80", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Dispose_AllowsAnActiveLeaseToFinishSafely()
    {
        var limiter = CreateLimiter(concurrency: 1, queueLimit: 0);
        var lease = await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken);

        limiter.Dispose();

        lease.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await limiter.AcquireAsync("device-a:80", TestContext.Current.CancellationToken));
    }

    private static OfficialApiConcurrencyLimiter CreateLimiter(int concurrency, int queueLimit, int maxTrackedDevices = 128)
    {
        return new OfficialApiConcurrencyLimiter(Microsoft.Extensions.Options.Options.Create(new ControlIdConcurrencyOptions
        {
            MaxConcurrentRequestsPerDevice = concurrency,
            QueueLimitPerDevice = queueLimit,
            MaxTrackedDevices = maxTrackedDevices
        }));
    }
}
