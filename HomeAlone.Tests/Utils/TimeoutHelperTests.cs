using System;
using System.Threading;
using System.Threading.Tasks;
using HomeAlone.Utils;
using Xunit;

namespace HomeAlone.Tests.Utils
{
    public class TimeoutHelperTests
    {
        [Fact]
        public async Task ExecuteWithTimeout_CompletesBeforeTimeout()
        {
            await TimeoutHelper.ExecuteWithTimeout(async ct =>
            {
                await Task.Delay(50, ct);
            }, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task ExecuteWithTimeout_ThrowsTimeoutException_WhenTimeoutElapsed()
        {
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TimeoutHelper.ExecuteWithTimeout(async ct =>
                {
                    await Task.Delay(1000, ct);
                }, TimeSpan.FromMilliseconds(100));
            });
        }

        [Fact]
        public async Task ExecuteWithTimeout_PropagatesCancellationFromCaller()
        {
            using CancellationTokenSource cts = new();
            cts.CancelAfter(50);

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TimeoutHelper.ExecuteWithTimeout(async ct =>
                {
                    // Respect the linked token
                    await Task.Delay(1000, ct);
                }, TimeSpan.FromSeconds(5), cts.Token);
            });
        }

        [Fact]
        public async Task ExecuteWithTimeout_Generic_CompletesBeforeTimeout_ReturnsResult()
        {
            int result = await TimeoutHelper.ExecuteWithTimeout<int>(async ct =>
            {
                await Task.Delay(50, ct);
                return 42;
            }, TimeSpan.FromSeconds(1));

            Assert.Equal(42, result);
        }

        [Fact]
        public async Task ExecuteWithTimeout_Generic_ThrowsTimeoutException_WhenTimeoutElapsed()
        {
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TimeoutHelper.ExecuteWithTimeout<int>(async ct =>
                {
                    await Task.Delay(1000, ct);
                    return 1;
                }, TimeSpan.FromMilliseconds(100));
            });
        }
    }
}
