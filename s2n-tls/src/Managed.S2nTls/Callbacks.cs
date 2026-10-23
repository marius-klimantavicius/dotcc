using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

internal sealed class CallbackState
{
    internal Func<string, bool>? VerifyHost;
    internal Action<ReadOnlyMemory<byte>>? KeyLog;
    internal Func<CancellationToken, ValueTask<bool>>? ValidateCertificate;
    internal Func<S2nRenegotiation>? Renegotiate;
}

// This scope exists only during synchronous entry to translated C. It keeps
// exceptions on the calling operation/thread, including for shared configs.
internal sealed class CallbackScope : IDisposable
{
    [ThreadStatic] internal static CallbackScope? Current;
    internal static readonly AsyncLocal<bool> InApplicationCallback = new();
    internal readonly S2nConnection Connection;
    internal readonly CancellationToken Cancellation;
    internal ExceptionDispatchInfo? Exception;
    internal CallbackScope(S2nConnection connection, CancellationToken cancellation)
    {
        if (Current != null) throw new InvalidOperationException("Reentrant translated TLS entry from a callback is forbidden.");
        Connection = connection; Cancellation = cancellation; Current = this;
    }
    internal static void Capture(Exception error)
    { if (Current is { } current) current.Exception ??= ExceptionDispatchInfo.Capture(error); }
    public void Dispose() => Current = null;
}

internal static unsafe class Callbacks
{
    private static CallbackState State(void* context) => (CallbackState)GCHandle.FromIntPtr((nint)context).Target!;
    internal static byte VerifyHost(byte* name, ulong length, void* context)
    {
        try { return State(context).VerifyHost!(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(name, checked((int)length)))) ? (byte)1 : (byte)0; }
        catch (Exception error) { CallbackScope.Capture(error); return 0; }
    }
    internal static int KeyLog(void* context, Raw.s2n_connection* connection, byte* line, ulong length)
    {
        try { State(context).KeyLog!(new ReadOnlySpan<byte>(line, checked((int)length)).ToArray()); return 0; }
        catch (Exception error) { CallbackScope.Capture(error); return -1; }
    }
    internal static int ValidateCertificate(Raw.s2n_connection* connection, Raw.s2n_cert_validation_info* info, void* context)
    {
        try
        {
            var scope = CallbackScope.Current ?? throw new InvalidOperationException("Certificate callback outside an owning entry.");
            ValueTask<bool> decision;
            bool previous = CallbackScope.InApplicationCallback.Value;
            CallbackScope.InApplicationCallback.Value = true;
            try { decision = State(context).ValidateCertificate!(scope.Cancellation); }
            finally { CallbackScope.InApplicationCallback.Value = previous; }
            if (decision.IsCompletedSuccessfully)
                return decision.Result ? Raw.s2n_cert_validation_accept(info) : Raw.s2n_cert_validation_reject(info);
            scope.Connection.SetPendingValidation((nint)info, decision.AsTask());
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return -1; }
    }
    internal static int Renegotiate(Raw.s2n_connection* connection, void* context, Raw.s2n_renegotiate_response* response)
    {
        try
        {
            S2nRenegotiation decision = State(context).Renegotiate!();
            if (!Enum.IsDefined(decision)) throw new InvalidOperationException("Invalid renegotiation response.");
            *response = (Raw.s2n_renegotiate_response)(int)decision;
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return -1; }
    }
}
