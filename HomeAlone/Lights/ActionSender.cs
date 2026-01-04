using HomeAlone.Lights;
using HomeAlone.Utils;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace HomeAlone.Lights;

internal class ActionSender(IPAddress target, ushort port, ILogger<ActionSender> logger)
{
    public IPAddress Target { get; set; } = target;
    public ushort TcpPort { get; set; } = port;
    public int MaxRetries { get; set; } = 1;
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan TimeoutPerAttempt { get; set; } = TimeSpan.FromMilliseconds(500);


    public async Task<bool> TrySendLightAction(Relais relais, LightActions action, CancellationToken cancellationToken)
    {
        int maxAttempts = MaxRetries + 1; // initial attempt + retries
        // Retry logic to handle transient network issues
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await TimeoutHelper.ExecuteWithTimeout(ct => SendLightActionCore(relais, action, ct), TimeoutPerAttempt, cancellationToken);
                return true; // Success
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                // Log the exception if needed, then retry
                logger.LogWarning(ex, "Attempt {Attempt} to send action {Action} to relais {Relais} failed.", attempt, action, relais);
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(RetryInterval, cancellationToken);
            }
        }

        return false;
    }

    private async Task SendLightActionCore(Relais relais, LightActions action, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(Target, TcpPort, cancellationToken).ConfigureAwait(false);
        using NetworkStream stream = client.GetStream();

        // 1. Send prep message and validate response
        await SendPrepMessage(stream, cancellationToken).ConfigureAwait(false);

        // 2. Build and send action message and validate response
        await SendAction(relais, action, stream, cancellationToken).ConfigureAwait(false);
    }

    private static readonly byte[] PREP_REQUEST_MESSAGE = [
      0xaf, 0x02, 0x04, 0x03, 0x00, 0x00, 0x08, 0x01,
      0x08, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xaf
    ];

    private static async Task SendPrepMessage(NetworkStream stream, CancellationToken cancellationToken)
    {
        const int ResponseSize = 32;

        ReadOnlyMemory<byte> prepRequest = PREP_REQUEST_MESSAGE;
        await stream.WriteAsync(prepRequest, cancellationToken).ConfigureAwait(false);

        byte[] responseBuffer = ArrayPool<byte>.Shared.Rent(ResponseSize);
        try
        {
            int totalRead = 0;
            while (totalRead < ResponseSize)
            {
                int read = await stream.ReadAsync(responseBuffer.AsMemory(totalRead, ResponseSize - totalRead), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    throw new InvalidOperationException("Connection closed while reading prep response.");
                totalRead += read;
            }

            // Validate that server echoed the prep message and padded with 0xFF
            if (!responseBuffer.AsSpan(0, PREP_REQUEST_MESSAGE.Length).SequenceEqual(PREP_REQUEST_MESSAGE.AsSpan()))
                throw new InvalidOperationException("Prep response did not match prep request.");

            for (int i = PREP_REQUEST_MESSAGE.Length; i < ResponseSize; i++)
            {
                if (responseBuffer[i] != 0xFF)
                    throw new InvalidOperationException("Prep response padding invalid.");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(responseBuffer);
        }
    }

    private static async Task SendAction(Relais relais, LightActions action, NetworkStream stream, CancellationToken cancellationToken)
    {
        const int ResponseSize = 32;
        const int RequestSize = 8;

        byte[] requestBuffer = ArrayPool<byte>.Shared.Rent(ResponseSize);
        byte[] responseBuffer = ArrayPool<byte>.Shared.Rent(ResponseSize);
        try
        {
            // first 8 bytes are payload written by helper; rest pad with 0xFF
            WriteActionPayload(requestBuffer, relais, action);
            for (int i = 8; i < RequestSize; i++)
                requestBuffer[i] = 0xFF;

            await stream.WriteAsync(requestBuffer, cancellationToken).ConfigureAwait(false);

            int totalRead = 0;
            while (totalRead < ResponseSize)
            {
                int read = await stream.ReadAsync(responseBuffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    throw new InvalidOperationException("Connection closed while reading action response.");
                totalRead += read;
            }

            // Validate first ActionMessageSize bytes match what we sent, rest must be 0xFF
            if (!responseBuffer.AsSpan(0, RequestSize).SequenceEqual(requestBuffer.AsSpan(0,RequestSize)))
                throw new InvalidOperationException("Action response did not match action message.");

            for (int i = RequestSize; i < ResponseSize; i++)
            {
                if (responseBuffer[i] != 0xFF)
                    throw new InvalidOperationException("Action response padding invalid.");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(requestBuffer);
            ArrayPool<byte>.Shared.Return(responseBuffer);
        }
    }

    private static void WriteActionPayload(Span<byte> buffer, Relais relais, LightActions action)
    {
        if (buffer.Length < 8)
        {
            throw new ArgumentException("Buffer too small", nameof(buffer));
        }

        // e.g. [0x3, 0x3, 0x1, 0xff, 0xff, 0x64, 0xff, 0xff]
        buffer[0] = relais.ModuleId;
        buffer[1] = (byte)(relais.ChannelId - 1);
        buffer[2] = (byte)action;
        buffer[3] = 0xff;
        buffer[4] = 0xff;
        buffer[5] = 0x64;
        buffer[6] = 0xff;
        buffer[7] = 0xff;
    }
}
