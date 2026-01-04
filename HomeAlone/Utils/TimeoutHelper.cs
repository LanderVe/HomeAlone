namespace HomeAlone.Utils;

internal static class TimeoutHelper
{
    /// <summary>
    /// Executes the specified asynchronous action and enforces a timeout, cancelling the operation if it does not
    /// complete within the given time period.
    /// </summary>
    /// <remarks>If the provided <paramref name="cancellationToken"/> is cancelled before the timeout elapses,
    /// the operation is cancelled and the original cancellation exception is propagated. If the timeout elapses first,
    /// a <see cref="TimeoutException"/> is thrown. The returned task may fault or be cancelled depending on the outcome
    /// of the action and cancellation tokens.</remarks>
    /// <param name="action">A function that represents the asynchronous operation to execute. The function receives a <see
    /// cref="CancellationToken"/> that is cancelled if the timeout elapses or if <paramref name="cancellationToken"/>
    /// is cancelled.</param>
    /// <param name="timeout">The maximum duration to allow the operation to run before timing out.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation before the timeout is reached. The default value
    /// is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes with the result of the action if it
    /// finishes before the timeout or cancellation.</returns>
    /// <exception cref="TimeoutException">Thrown if the operation does not complete within the specified timeout period.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via the provided <paramref name="cancellationToken"/>"
    public static async Task ExecuteWithTimeout(Func<CancellationToken, Task> action, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = new(timeout);
        CancellationToken timeoutToken = timeoutCts.Token;
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);
        CancellationToken linkedToken = linkedCts.Token;
        try
        {
            await action(linkedToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("Operation cancelled by caller.", ex, cancellationToken);
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            throw new TimeoutException("Timeout has occured", ex);
        }
    }

    /// <inheritdoc cref="ExecuteWithTimeout(Func{CancellationToken, Task}, TimeSpan, CancellationToken)"/>
    /// <typeparam name="T">The type of the result returned by the asynchronous action.</typeparam>
    public static async Task<T> ExecuteWithTimeout<T>(Func<CancellationToken, Task<T>> action, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = new(timeout);
        CancellationToken timeoutToken = timeoutCts.Token;
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);
        CancellationToken linkedToken = linkedCts.Token;
        try
        {
            return await action(linkedToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("Operation cancelled by caller.", ex, cancellationToken);
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            throw new TimeoutException("Timeout has occured", ex);
        }
    }

}
