namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Called ONLY after Core(original) failed and the original authoritative
    // long-mode decode succeeded on exactly original.Length bytes. Core must
    // remain the full ordered classifier chain, without this fallback.
    private static bool TryClassifyCanonicalEncoding(ulong ip, ReadOnlySpan<byte> original,
        out JitInstruction instruction)
    {
        instruction = default;
        Span<byte> canonical = stackalloc byte[15];
        if (!FullCanonicalEncoding.TryNormalize(original, canonical, out int n, out _) ||
            original.SequenceEqual(canonical[..n])) return false;
        // Core's relative Target calculations must see the ORIGINAL next PC.
        // Restoring the original IR location afterwards also makes emitted
        // RIP/EIP EA, call return address, fault PC, oplen and budget metadata
        // use the original instruction. All arithmetic intentionally wraps.
        ulong adjustedIp = unchecked(ip + (uint)(original.Length - n));
        if (!TryClassifyNativeCore(adjustedIp, canonical[..n], out var candidate)) return false;
        if (candidate.Ip != adjustedIp || candidate.Length != n) return false;
        instruction = candidate with { Ip = ip, Length = original.Length };
        return true;
    }
}
