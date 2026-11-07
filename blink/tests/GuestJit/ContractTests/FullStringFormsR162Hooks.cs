namespace Managed.Emulation.Execution;

// Contract assembly only; no production entry point or alternate executor.
internal sealed unsafe partial class FullTranslationJit
{
    internal static bool R162ClassifyString(ulong pc, ReadOnlySpan<byte> bytes, out JitInstruction i)
        => TryClassifyNative(pc, bytes, out i);
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal static bool R162RepFault(FullRepInstruction i, in FullBlockFrame frame)
        => IsRepStringFault(i, in frame);
}
