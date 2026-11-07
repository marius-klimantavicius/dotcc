namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Test assembly only. Tests perform the authoritative decode themselves.
    internal static bool R159Classify(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
        => TryClassifyNative(ip, bytes, out instruction);
    internal static bool R159Core(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
        => TryClassifyNativeCore(ip, bytes, out instruction);
    internal static bool R159Fallback(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
        => TryClassifyCanonicalEncoding(ip, bytes, out instruction);
}
