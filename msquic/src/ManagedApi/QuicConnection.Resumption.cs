using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed partial class QuicConnection
{
    private sealed class ResumptionValidationWork { }
    private ResumptionValidationWork? resumptionValidationPending;
    private CancellationTokenSource? resumptionValidationLifetime;
    private bool resumptionValidationClosed;

    // Called by the actual RESUMED callback. Returning a failed status rejects
    // only this PSK attempt; upstream continues with a full TLS handshake.
    private unsafe uint ValidateResumptionState(byte* state, ushort length)
    {
        if (!IsServer || length > MsQuic.QUIC_MAX_RESUMPTION_APP_DATA_LENGTH || (length != 0 && state == null))
            return Status.InvalidParameter;
        var policy = configuration.ServerResumptionValidation;
        lock (Gate) if (resumptionValidationClosed || IsClosing) return Status.Aborted;
        if (policy == null) return Status.Success;
        // No pointer into the provider's decrypted envelope crosses this callback.
        ReadOnlyMemory<byte> snapshot = new ReadOnlySpan<byte>(state, length).ToArray();
        var work = new ResumptionValidationWork();
        CancellationToken token;
        lock (Gate)
        {
            if (resumptionValidationClosed || IsClosing) return Status.Aborted;
            if (resumptionValidationPending != null)
                throw new InvalidOperationException("Concurrent server resumption validation callbacks.");
            resumptionValidationLifetime ??= new();
            token = resumptionValidationLifetime.Token;
            resumptionValidationPending = work;
        }
        try
        {
            ValueTask<bool> decision = policy(snapshot, token);
            if (decision.IsCompleted)
            {
                bool accepted = decision.GetAwaiter().GetResult();
                lock (Gate)
                {
                    if (!ReferenceEquals(resumptionValidationPending, work) || resumptionValidationClosed || IsClosing)
                        return Status.Aborted;
                    resumptionValidationPending = null;
                }
                return accepted ? Status.Success : Status.Aborted;
            }
            // The completion API queues onto this connection worker. Even if
            // this task wins scheduling, that operation runs after the current
            // native callback has returned Pending.
            _ = Task.Run(() => FinishResumptionValidationAsync(work, decision));
            return Status.Pending;
        }
        catch (Exception error)
        {
            lock (Gate)
                if (ReferenceEquals(resumptionValidationPending, work)) resumptionValidationPending = null;
            RecordCallbackFailure(error);
            return Status.Aborted;
        }
    }

    private async Task FinishResumptionValidationAsync(ResumptionValidationWork work, ValueTask<bool> decision)
    {
        bool accepted = false;
        try { accepted = await decision.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        catch (Exception error) { RecordCallbackFailure(error); }

        try
        {
            HandleOperation operation;
            lock (Gate)
            {
                if (!ReferenceEquals(resumptionValidationPending, work) || resumptionValidationClosed || IsClosing) return;
                // Claim exactly one completion, then retain the native handle for
                // that bounded call. Never retain it while awaiting application code.
                resumptionValidationPending = null;
                operation = EnterOperation();
            }
            using (operation) CompleteResumptionValidation(accepted);
        }
        catch (ObjectDisposedException) { }
        catch (Exception error) { Fault(error); }
    }

    private unsafe void CompleteResumptionValidation(bool accepted)
    {
        uint status = Runtime.Api->ConnectionResumptionTicketValidationComplete(Handle, accepted ? (byte)1 : (byte)0);
        if (status == Status.InvalidState)
        {
            lock (Gate) if (IsClosing || shutdown.Task.IsCompleted) return;
        }
        QuicError.ThrowIfFailed(status, "Complete server resumption validation");
    }

    // Main Dispose invokes this before CloseOnce. Pending application code owns
    // copied managed bytes only; ignored cancellation cannot prevent native close.
    private void CloseResumptionValidation()
    {
        CancellationTokenSource? lifetime;
        lock (Gate)
        {
            if (resumptionValidationClosed) return;
            resumptionValidationClosed = true;
            resumptionValidationPending = null;
            lifetime = resumptionValidationLifetime;
            resumptionValidationLifetime = null;
        }
        if (lifetime != null) _ = CancelResumptionValidationAsync(lifetime);
    }

    private async Task CancelResumptionValidationAsync(CancellationTokenSource lifetime)
    {
        try { await lifetime.CancelAsync().ConfigureAwait(false); }
        catch (Exception error) { RecordCallbackFailure(error); }
        finally { lifetime.Dispose(); }
    }
}
