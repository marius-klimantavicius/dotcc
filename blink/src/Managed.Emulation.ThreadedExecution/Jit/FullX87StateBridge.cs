using System;
using System.Buffers.Binary;
using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

// Integer/byte-only transport. No host FP, guest arithmetic or allocations.
// Parent verifies ownership and a fully returned native leaf BEFORE calling.
internal static unsafe class FullX87StateBridge
{
    internal const int StatusOffset = 648, TagOffset = 652, OpcodeOffset = 656,
        IpOffset = 664, ControlOffset = 672, DpOffset = 680, MxcsrOffset = 688,
        XmmOffset = 160;
    internal const uint MxcsrMask = 0x0000ffff;
    private static ushort U16(ReadOnlySpan<byte> b, int at) => BinaryPrimitives.ReadUInt16LittleEndian(b.Slice(at, 2));
    private static uint U32(ReadOnlySpan<byte> b, int at) => BinaryPrimitives.ReadUInt32LittleEndian(b.Slice(at, 4));
    private static ulong U64(ReadOnlySpan<byte> b, int at) => BinaryPrimitives.ReadUInt64LittleEndian(b.Slice(at, 8));
    private static void W16(Span<byte> b, int at, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(b.Slice(at, 2), v);
    private static void W32(Span<byte> b, int at, uint v) => BinaryPrimitives.WriteUInt32LittleEndian(b.Slice(at, 4), v);
    private static void W64(Span<byte> b, int at, ulong v) => BinaryPrimitives.WriteUInt64LittleEndian(b.Slice(at, 8), v);
    private static Span<byte> Machine(nint pointer) => new((void*)pointer, MxcsrOffset + 4);

    internal static int ImageSize(HostX87Transfer op) => HostX87StateProtocol.ImageSize(op);

    internal static HostX87TransferResult Transfer(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken, uint protocol, HostX87Transfer op, Span<byte> image)
    {
        FullX87StateStorage.Validate(state, machine, generation, ownerToken, protocol);
        int expected = ImageSize(op); // Unknown protocol/operation never falls back.
        if (image.Length != expected) throw new InvalidOperationException("x87 host image size mismatch");
        if (state->ProfileActive == 0) return HostX87TransferResult.Inactive;
        var m = Machine(machine);
        switch (op)
        {
            case HostX87Transfer.LinuxSave: SaveFx(state, m, image, true, 16, true); break;
            case HostX87Transfer.LinuxValidate:
                return ValidFx(image, true) ? HostX87TransferResult.Complete : HostX87TransferResult.BadImage;
            case HostX87Transfer.LinuxRestore:
                if (!ValidFx(image, true)) return HostX87TransferResult.BadImage;
                RestoreFx(state, m, image, true, 16); break;
            case HostX87Transfer.Fx64Save: SaveFx(state, m, image, true, 16, false); break;
            case HostX87Transfer.FxLong32Save: SaveFx(state, m, image, false, 16, false); break;
            case HostX87Transfer.FxLegacySave: SaveFx(state, m, image, false, 8, false); break;
            case HostX87Transfer.Fx64Restore:
            case HostX87Transfer.FxLong32Restore:
            case HostX87Transfer.FxLegacyRestore:
                if (!ValidFx(image, false)) return HostX87TransferResult.BadImage;
                RestoreFx(state, m, image, op == HostX87Transfer.Fx64Restore,
                    op == HostX87Transfer.FxLegacyRestore ? 8 : 16); break;
            case HostX87Transfer.Env14Save: SaveEnvironment(state, m, image, true, false); break;
            case HostX87Transfer.Env28Save: SaveEnvironment(state, m, image, false, false); break;
            case HostX87Transfer.Save94: SaveEnvironment(state, m, image, true, true); break;
            case HostX87Transfer.Save108: SaveEnvironment(state, m, image, false, true); break;
            case HostX87Transfer.Env14Restore: RestoreEnvironment(state, m, image, true, false); break;
            case HostX87Transfer.Restore94: RestoreEnvironment(state, m, image, true, true); break;
            case HostX87Transfer.Env28Restore: RestoreEnvironment(state, m, image, false, false); break;
            case HostX87Transfer.Restore108: RestoreEnvironment(state, m, image, false, true); break;
            case HostX87Transfer.EnvironmentSaved:
                // Separate post-copy commit. A faulted FNSTENV must not mask CW.
                W32(m, ControlOffset, (uint)(U16(m, ControlOffset) | 0x3f)); break;
            case HostX87Transfer.Finit: Reset(state, m, false, false); break;
            case HostX87Transfer.CpuReset:
            case HostX87Transfer.LinuxHandlerEnter:
            case HostX87Transfer.LinuxReset: Reset(state, m, true, true); break;
            default: throw new InvalidOperationException("x87 operation inventory differs");
        }
        return HostX87TransferResult.Complete;
    }

    private static bool ValidFx(ReadOnlySpan<byte> b, bool linux)
    {
        // The mask comes from the software CPU profile, NEVER the untrusted
        // image's MXCSR_MASK. Reserved FTW/FOP/padding bits are ignored.
        if ((U32(b, 24) & ~MxcsrMask) != 0) return false;
        // This profile exposes FX-only Linux frames. Extended XSAVE contexts
        // require a different checked size/feature protocol; never truncate one.
        if (linux && U32(b, 464) == 0x46505853) return false;
        return true;
    }
    internal static ushort ClassifyTag(FullExtended80 v)
    {
        uint exponent = (uint)v.SignExponent & 0x7fff;
        if (exponent == 0 && v.Significand == 0) return 1;
        if (exponent == 0 || exponent == 0x7fff || (v.Significand & 0x8000000000000000) == 0) return 2;
        return 0;
    }
    internal static byte AbridgedTag(ushort tags)
    {
        byte ftw = 0;
        for (int p = 0; p < 8; ++p) if (((tags >> (p * 2)) & 3) != 3) ftw |= (byte)(1 << p);
        return ftw;
    }
    // Intel093 Volume1 8.1.3.3: B reflects ES (8087 compatibility).
    // A forged image cannot create independently stored B/ES state.
    private static ushort LoadedStatus(ushort value)
        => (ushort)((value & 0x7fff) | ((value & 0x0080) << 8));
    private static ushort ExpandTags(FullX87RegisterFile* state, byte abridged)
    {
        ushort tags = 0;
        for (int p = 0; p < 8; ++p)
            tags |= (ushort)(((abridged & (1 << p)) == 0 ? 3 : ClassifyTag(*FullX87RegisterFile.Physical(state, p))) << (2 * p));
        return tags;
    }
    private static void PutRaw(Span<byte> image, int offset, FullExtended80 value)
    { W64(image, offset, value.Significand); W16(image, offset + 8, value.SignExponent); }
    private static FullExtended80 GetRaw(ReadOnlySpan<byte> image, int offset)
        => new(U64(image, offset), U16(image, offset + 8));

    private static void SaveFx(FullX87RegisterFile* s, Span<byte> m, Span<byte> b,
        bool pointers64, int xmmCount, bool linux)
    {
        // SignalFrame is wholly owned and initialized; hardware FXSAVE callers
        // copy only its meaningful prefix, preserving software-owned tail bytes.
        b.Clear();
        ushort sw = U16(m, StatusOffset);
        W16(b, 0, U16(m, ControlOffset)); W16(b, 2, sw);
        b[4] = AbridgedTag(U16(m, TagOffset)); W16(b, 6, (ushort)(U16(m, OpcodeOffset) & 0x7ff));
        if (pointers64)
        { W64(b, 8, U64(m, IpOffset)); W64(b, 16, U64(m, DpOffset)); }
        else
        {
            W32(b, 8, unchecked((uint)U64(m, IpOffset))); W16(b, 12, s->CodeSelector);
            W32(b, 16, unchecked((uint)U64(m, DpOffset))); W16(b, 20, s->DataSelector);
        }
        W32(b, 24, U32(m, MxcsrOffset)); W32(b, 28, MxcsrMask);
        // Raw registers are logical ST0..7. FTW above remains physical R0..7.
        for (int logical = 0; logical < 8; ++logical)
            PutRaw(b, 32 + logical * 16, *FullX87RegisterFile.Physical(s, FullX87RegisterFile.PhysicalIndex(sw, logical)));
        m.Slice(XmmOffset, xmmCount * 16).CopyTo(b.Slice(160, xmmCount * 16));
        // Linux sw_reserved stays zero: no advertised XSAVE extensions.
        _ = linux;
    }
    private static void RestoreFx(FullX87RegisterFile* s, Span<byte> m, ReadOnlySpan<byte> b,
        bool pointers64, int xmmCount)
    {
        ushort sw = LoadedStatus(U16(b, 2));
        // All validation preceded the first mutation. Load raw data BEFORE
        // reconstructing tags, using the restored TOP rather than old TOP.
        for (int logical = 0; logical < 8; ++logical)
        {
            var destination = FullX87RegisterFile.Physical(s, FullX87RegisterFile.PhysicalIndex(sw, logical));
            *destination = GetRaw(b, 32 + logical * 16);
        }
        W32(m, ControlOffset, U16(b, 0)); W32(m, StatusOffset, sw);
        W32(m, TagOffset, ExpandTags(s, b[4])); W32(m, OpcodeOffset, (uint)(U16(b, 6) & 0x7ff));
        W64(m, IpOffset, pointers64 ? U64(b, 8) : U32(b, 8));
        W64(m, DpOffset, pointers64 ? U64(b, 16) : U32(b, 16));
        s->CodeSelector = pointers64 ? (ushort)0 : U16(b, 12);
        s->DataSelector = pointers64 ? (ushort)0 : U16(b, 20);
        W32(m, MxcsrOffset, U32(b, 24));
        b.Slice(160, xmmCount * 16).CopyTo(m.Slice(XmmOffset, xmmCount * 16));
    }

    // Protected/long-mode images ONLY. Real/VM86/SMM are explicitly MISSING.
    // These operations copy environment; save's architectural post-copy FINIT
    // and FNSTENV masking are separate operations after checked guest copyout.
    private static void SaveEnvironment(FullX87RegisterFile* s, Span<byte> m, Span<byte> b, bool small, bool registers)
    {
        b.Clear();
        ushort sw = U16(m, StatusOffset);
        W16(b, 0, U16(m, ControlOffset));
        if (small)
        {
            W16(b, 2, sw); W16(b, 4, U16(m, TagOffset));
            W16(b, 6, unchecked((ushort)U64(m, IpOffset))); W16(b, 8, s->CodeSelector);
            W16(b, 10, unchecked((ushort)U64(m, DpOffset))); W16(b, 12, s->DataSelector);
        }
        else
        {
            // Reserved high halves are saved as ones by this software profile.
            W16(b, 2, 0xffff); W16(b, 6, 0xffff); W16(b, 10, 0xffff); W16(b, 26, 0xffff);
            W16(b, 4, sw); W16(b, 8, U16(m, TagOffset));
            W32(b, 12, unchecked((uint)U64(m, IpOffset))); W16(b, 16, s->CodeSelector);
            W16(b, 18, (ushort)(U16(m, OpcodeOffset) & 0x7ff));
            W32(b, 20, unchecked((uint)U64(m, DpOffset))); W16(b, 24, s->DataSelector);
        }
        if (registers)
            for (int logical = 0; logical < 8; ++logical)
                PutRaw(b, (small ? 14 : 28) + logical * 10,
                    *FullX87RegisterFile.Physical(s, FullX87RegisterFile.PhysicalIndex(sw, logical)));
    }
    private static void RestoreEnvironment(FullX87RegisterFile* s, Span<byte> m, ReadOnlySpan<byte> b, bool small, bool registers)
    {
        ushort sw = LoadedStatus(U16(b, small ? 2 : 4)), tags = U16(b, small ? 4 : 8);
        if (registers)
            for (int logical = 0; logical < 8; ++logical)
            {
                var destination = FullX87RegisterFile.Physical(s, FullX87RegisterFile.PhysicalIndex(sw, logical));
                *destination = GetRaw(b, (small ? 14 : 28) + logical * 10);
            }
        // FLDENV/FRSTOR only trust empty/nonempty and classify actual values.
        W32(m, ControlOffset, U16(b, 0)); W32(m, StatusOffset, sw);
        W32(m, TagOffset, ExpandTags(s, AbridgedTag(tags)));
        W64(m, IpOffset, small ? U16(b, 6) : U32(b, 12));
        W64(m, DpOffset, small ? U16(b, 10) : U32(b, 20));
        s->CodeSelector = U16(b, small ? 8 : 16); s->DataSelector = U16(b, small ? 12 : 24);
        // Authorized Linux x86-64 virtual-CPU profile: small protected images
        // clear omitted FOP. AMD witness-backed policy, not universal ISA or
        // an undefined-value license. Large images still load exactly11 bits.
        W32(m, OpcodeOffset, small ? 0u : (uint)(U16(b, 18) & 0x7ff));
    }
    private static void Reset(FullX87RegisterFile* s, Span<byte> m, bool clearValues, bool sse)
    {
        W32(m, ControlOffset, 0x037f); W32(m, StatusOffset, 0); W32(m, TagOffset, 0xffff);
        W32(m, OpcodeOffset, 0); W64(m, IpOffset, 0); W64(m, DpOffset, 0);
        s->CodeSelector = 0; s->DataSelector = 0;
        if (clearValues)
            for (int p = 0; p < 8; ++p)
            {
                var destination = FullX87RegisterFile.Physical(s, p);
                *destination = default;
            }
        if (sse) { m.Slice(XmmOffset, 256).Clear(); W32(m, MxcsrOffset, 0x1f80); }
    }

    // Reference commit seams for the parent's native emitter. Never call from
    // generated native code. Waiting exceptions and memory completion occur first.
    internal static ulong MmxRead(FullX87RegisterFile* s, int physical)
        => FullX87RegisterFile.Physical(s, physical)->Significand;
    internal static void MmxWrite(FullX87RegisterFile* s, nint machine, int physical, ulong value)
    {
        var slot = FullX87RegisterFile.Physical(s, physical);
        *slot = new FullExtended80(value, 0xffff);
        MmxTransition(machine);
    }
    internal static void MmxTransition(nint machine)
    {
        var m = Machine(machine);
        W32(m, StatusOffset, (uint)(U16(m, StatusOffset) & ~0x3800)); W32(m, TagOffset, 0);
    }
    internal static void Emms(nint machine) => W32(Machine(machine), TagOffset, 0xffff);
}
