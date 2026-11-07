namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Typed extension operations are intrinsically identifiable. MOV is owned
    // only for the exact new non-W63 memory shape (ordinary memory decoder uses
    // LoadMov). Register MOV is deliberately left to its proven existing path.
    private static bool IsAddressExtension(JitInstruction i) => i.Operation is
        JitOperation.Lea or JitOperation.Movzx or JitOperation.Movsx or JitOperation.LoadMovzx ||
        i.Operation == JitOperation.Mov && i.Source.IsMemory && i.SourceWidth != 0;

    private static bool AddressExtensionRegister(JitOperand p, int width) =>
        IntegerRegister(p, width) && p.Immediate == 0;

    internal static bool ValidateAddressExtension(JitInstruction i)
    {
        if (!IsAddressExtension(i)) return false;
        if (i.Length is < 1 or > 15 || i.Width is not (16 or 32 or 64) ||
            i.Target != 0 || i.Condition != 0 || i.Options != 0 || i.Extra != default ||
            !AddressExtensionRegister(i.Destination, i.Width))
            throw new ArgumentException("Address/extension metadata or destination");
        bool lea = i.Operation == JitOperation.Lea;
        if (lea)
        {
            if (i.Source != default || i.SourceWidth != 0)
                throw new ArgumentException("LEA has no data operand");
        }
        else
        {
            bool copy = i.Operation == JitOperation.Mov;
            if (copy ? i.Width is not (16 or 32) || i.SourceWidth != i.Width || !i.Source.IsMemory :
                i.SourceWidth is not (8 or 16 or 32) || i.SourceWidth >= i.Width ||
                i.SourceWidth == 32 && (i.Operation != JitOperation.Movsx || i.Width != 64))
                throw new ArgumentException("Address/extension width pair");
            if (i.Operation == JitOperation.LoadMovzx && !i.Source.IsMemory ||
                (i.Source.IsMemory ? i.Source != new JitOperand(0, 0, IsMemory: true) :
                !AddressExtensionRegister(i.Source, i.SourceWidth)))
                throw new ArgumentException("Address/extension source");
        }
        if (lea || i.Source.IsMemory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Address/extension address");
        }
        else if (i.Address != default) throw new ArgumentException("Register extension address");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryAddressExtensionOperation(in JitInstruction i)
        {
            if (!IsAddressExtension(i)) return false;
            ValidateAddressExtension(i);
            if (i.Operation == JitOperation.Lea)
            {
                // Offset ONLY: no segment-base addition, MMU/TLB read, canonical
                // check, native-memory count, data pointer or faulting EA use.
                // Compute all inputs before Store to preserve base/index aliases.
                Address(i, truncate: false);
                if (i.Address.AddressWidth == 32 || i.Width == 32) Bytes(0x89, 0xc0);
                IntegerMov(8, 0);
                Store(i.Destination, i.Width); //16 partial;32 full zero-extended;64 full
                return true;
            }
            if (!i.Source.IsMemory) return false; // reuse existing register extension
            // Existing helpers give #GP/#SS before any data access; AS32 wraps
            // the OFFSET, then adds the full FS/GS base before byte addressing.
            // Actual reads/extension/store/count stay in the proven native path.
            ScalarMiscCheckCanonical(i, i.SourceWidth);
            if (!NativeMemoryOperation(i)) throw new ArgumentException("Address/extension native memory shape");
            return true;
        }
    }
}
