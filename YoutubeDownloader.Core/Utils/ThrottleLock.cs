using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils;

public class ThrottleLock(TimeSpan interval) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTimeOffset _lastRequestInstant = DateTimeOffset.MinValue;

    /// <summary>
    /// Asynchronously waits for the throttle interval before allowing the next request.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the wait operation.</param>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var timePassedSinceLastRequest = DateTimeOffset.Now - _lastRequestInstant;

            var remainingTime = interval - timePassedSinceLastRequest;
            if (remainingTime > TimeSpan.Zero)
                await Task.Delay(remainingTime, cancellationToken);

            _lastRequestInstant = DateTimeOffset.Now;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Releases the resources used by the throttle lock.
    /// </summary>
    public void Dispose() => _semaphore.Dispose();
}
