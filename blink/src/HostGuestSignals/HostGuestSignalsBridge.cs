using global::System;

namespace Managed.Emulation;

public static partial class BlinkCore
{
    [ThreadStatic] private static Action<nint, int, int>? guestSignalHandler;

    public static void BindHostGuestSignals(Action<nint, int, int> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (guestSignalHandler != null) throw new InvalidOperationException("Guest signal handler is already bound.");
        guestSignalHandler = handler;
    }

    public static void UnbindHostGuestSignals() => guestSignalHandler = null;

    // The upstream CLI/TUI each provide this frontend hook. This bridge only
    // forwards the actual guest event; the separate C# owner decides its result.
    public static unsafe void TerminateSignal(Machine* machine, int signal, int code)
    {
        var handler = guestSignalHandler ?? throw new InvalidOperationException("Guest signal handler is not bound.");
        handler((nint)machine, signal, code);
    }
}
