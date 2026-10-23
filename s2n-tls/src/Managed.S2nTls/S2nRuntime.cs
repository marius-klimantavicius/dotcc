using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

public sealed record S2nRuntimeOptions(bool CallerOwnsCryptoInitialization = false);
public sealed record S2nRuntimeSnapshot(int References, long Initializations, long FinalCleanups, string? Failure);

/// <summary>Shares the translated library's single global initialization domain.
/// Configurations and credentials retain it after this public owner is disposed.</summary>
public sealed class S2nRuntime : IDisposable
{
    private static readonly object Gate = new();
    private static int _references;
    private static long _initializations, _finalCleanups;
    private static bool? _callerOwnsCrypto;
    private static Exception? _failure;
    private Lease? _lease;

    public S2nRuntime(S2nRuntimeOptions? options = null)
    {
        options ??= new();
        lock (Gate)
        {
            if (_failure != null) throw new InvalidOperationException("The s2n global domain is quarantined after a failed initialization or cleanup.", _failure);
            if (_callerOwnsCrypto is bool prior && prior != options.CallerOwnsCryptoInitialization)
                throw new InvalidOperationException("Crypto initialization ownership cannot change in this process.");
            if (_references == 0)
            {
                try
                {
                    Native.Check(Raw.s2n_disable_atexit(), "s2n_disable_atexit");
                    if (options.CallerOwnsCryptoInitialization && _callerOwnsCrypto == null)
                        Native.Check(Raw.s2n_crypto_disable_init(), "s2n_crypto_disable_init");
                    _callerOwnsCrypto = options.CallerOwnsCryptoInitialization;
                    Native.Check(Raw.s2n_init(), "s2n_init");
                    _initializations++;
                }
                catch (Exception error)
                {
                    // Upstream cleanup_final rejects partially initialized state. Do not
                    // pretend this domain can safely restart after a partial init failure.
                    _failure = error;
                    throw;
                }
            }
            _references++;
            _lease = new Lease();
        }
    }

    public static S2nRuntimeSnapshot Inspect()
    { lock (Gate) return new(_references, _initializations, _finalCleanups, _failure?.Message); }

    internal Lease Retain()
    {
        lock (Gate)
        {
            ObjectDisposedException.ThrowIf(_lease == null, this);
            if (_failure != null) throw new InvalidOperationException("The s2n global domain is quarantined.", _failure);
            _references++;
            return new Lease();
        }
    }

    internal static void Quarantine(Exception error) { lock (Gate) _failure ??= error; }
    public void Dispose()
    {
        Lease? lease;
        lock (Gate) { lease = _lease; _lease = null; }
        lease?.Dispose();
        GC.SuppressFinalize(this);
    }
    ~S2nRuntime() { try { Dispose(); } catch (Exception error) { Quarantine(error); } }

    internal sealed class Lease : IDisposable
    {
        private int _released;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) != 0) return;
            lock (Gate)
            {
                if (--_references != 0 || _failure != null) return;
                try
                {
                    Native.Check(Raw.s2n_cleanup_final(), "s2n_cleanup_final");
                    _finalCleanups++;
                }
                catch (Exception error) { _failure = error; throw; }
            }
        }
    }
}
