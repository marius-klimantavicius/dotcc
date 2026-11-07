namespace Managed.Emulation.Execution;

// Compile-time only, after authoritative LONG-mode XED length/legality admission.
// R159 supplies the existing effective-prefix software profile, including REX
// cancellation and final-REX selection. Decode success is not a validity proof.
internal static class FullAddressExtensionLowering
{
    // A recognized-but-rejected family must not fall into an older, wider grammar.
    internal static bool IsEncoding(ReadOnlySpan<byte> bytes)
    {
        int p = 0;
        while (p < bytes.Length && bytes[p] is 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 or
            0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
        return p < bytes.Length && (bytes[p] is 0x8d or 0x63 || bytes[p] == 0x0f &&
            p + 1 < bytes.Length && bytes[p + 1] is 0xb6 or 0xb7 or 0xbe or 0xbf);
    }

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "AddressExtensionNotEncoding";
        if (!IsEncoding(bytes)) return false;
        Span<byte> normalized = stackalloc byte[15];
        if (!FullCanonicalEncoding.TryNormalize(bytes, normalized, out int n, out reason)) return false;
        ReadOnlySpan<byte> body = normalized[..n];
        if (!FullMemoryLowering.Prefix(body, out int p, out byte op, out byte rex,
            out bool word, out bool address32, out int segment, ref reason)) return false;
        int width = (rex & 8) != 0 ? 64 : word ? 16 : 32;
        int sourceWidth = 0;
        JitOperation operation;
        if (op == 0x8d) operation = JitOperation.Lea;
        else if (op == 0x63)
        {
            // Intel SDM 325383 MOVSX/MOVSXD: 16<-16, 32<-32, 64<-32.
            // Selected C always reads4: the16 memory form deliberately follows
            // architectural access width, and is NOT a matching GetOp oracle.
            sourceWidth = width == 16 ? 16 : 32;
            operation = width == 64 ? JitOperation.Movsx : JitOperation.Mov;
        }
        else
        {
            if (op != 0x0f || p >= n) return false;
            int ext = body[p++];
            if (ext is not (0xb6 or 0xb7 or 0xbe or 0xbf)) return false;
            sourceWidth = (ext & 1) == 0 ? 8 : 16;
            // Do not invent MOVZX/MOVSX r16,r/m16 from the selected decoder's
            // permissive field model. These cells have no ordinary ISA form.
            if (sourceWidth >= width)
            { reason = "AddressExtensionNonWideningNotArchitectural"; return false; }
            operation = ext is 0xbe or 0xbf ? JitOperation.Movsx : JitOperation.Movzx;
        }
        reason = "AddressExtensionTruncatedOrTrailing";
        if (p >= n) return false;
        byte modrm = body[p++];
        var destination = new JitOperand((modrm >> 3 & 7) | ((rex & 4) << 1), 0);
        JitOperand source = default;
        JitAddress address = default;
        bool memory = modrm < 0xc0;
        if (operation == JitOperation.Lea && !memory)
        { reason = "AddressExtensionLeaRequiresMemoryEncoding"; return false; }
        if (memory)
        {
            if (!FullMemoryLowering.Address(body, ref p, modrm, rex, address32, segment, out address)) return false;
            if (operation != JitOperation.Lea) source = new(0, 0, IsMemory: true);
            if (operation == JitOperation.Movzx) operation = JitOperation.LoadMovzx;
        }
        else
        {
            source = FullMemoryLowering.Register((modrm & 7) | ((rex & 1) << 3), sourceWidth, rex != 0);
            // Existing ordinary register MOV has no family discriminator.
            if (operation == JitOperation.Mov) sourceWidth = 0;
        }
        if (p != n) return false;
        var candidate = new JitInstruction(ip, bytes.Length, operation, width, destination, source,
            SourceWidth: sourceWidth, Address: address);
        // The non-W63 register form is an ordinary typed MOV; all other forms
        // have an unambiguous strict seam. No new enum/option/frame/ABI field.
        if (operation != JitOperation.Mov || memory)
            FullBlockEmitter.ValidateAddressExtension(candidate);
        instruction = candidate; reason = null; return true;
    }
}
