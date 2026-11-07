using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineScalarMisc(JitInstruction i) =>
        i.Operation is JitOperation.ReadSegmentSelector or JitOperation.TranslateByte or JitOperation.NonTemporalStore;

    internal static bool ValidateScalarMisc(JitInstruction i)
    {
        if (!CanInlineScalarMisc(i)) return false;
        if (i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 || i.Extra != default ||
            i.SourceWidth != (i.Operation == JitOperation.TranslateByte ? 8 :
                i.Operation == JitOperation.NonTemporalStore ? i.Width : 16))
            throw new ArgumentException("Native scalar miscellaneous metadata");
        var memory = new JitOperand(0, 0, IsMemory: true);
        if (i.Operation == JitOperation.TranslateByte)
        {
            if (i.Width != 8 || i.Destination != new JitOperand(0, 0) || i.Source != memory || i.Options != 0 ||
                i.Address != new JitAddress(3, -1, 0, 0, AddressWidth: i.Address.AddressWidth, Segment: i.Address.Segment))
                throw new ArgumentException("Native XLAT operand shape");
        }
        else if (i.Operation == JitOperation.NonTemporalStore)
        {
            if (i.Width is not (32 or 64) || i.Destination != memory || i.Options != 0 ||
                !IntegerRegister(i.Source,i.Width) || i.Source.Immediate != 0 || i.Source.ByteOffset != 0)
                throw new ArgumentException("Native MOVNTI operand shape");
        }
        else if (i.Options >= 6 || i.Source != default ||
            (i.Destination.IsMemory ? i.Destination != memory || i.Width != 16 :
                !IntegerRegister(i.Destination, i.Width) || i.Destination.Immediate != 0 ||
                i.Destination.ByteOffset != 0 || i.Width is not (16 or 32 or 64) || i.Address != default))
            throw new ArgumentException("Native selector-read operand shape");
        if (i.Source.IsMemory || i.Destination.IsMemory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native scalar miscellaneous address");
        }
        if (!ScalarMiscLayout.Supported) throw new PlatformNotSupportedException("Native scalar miscellaneous layout");
        return true;
    }

    private static class ScalarMiscLayout
    {
        internal static readonly bool Supported = Check();
        private static unsafe bool Check()
        {
            Blink.Machine m = default; byte* b = (byte*)&m;
            fixed (ushort* es = &m.es.sel, cs = &m.cs.sel, ss = &m.ss.sel,
                ds = &m.ds.sel, fs = &m.fs.sel, gs = &m.gs.sel)
                return sizeof(Blink.DescriptorCache) == 16 && (byte*)&m.metal - b == 832 &&
                    (byte*)es - b == 456 && (byte*)cs - b == 472 &&
                    (byte*)ss - b == 488 && (byte*)ds - b == 504 &&
                    (byte*)fs - b == 520 && (byte*)gs - b == 536;
        }
    }

    private ref partial struct Writer
    {
        // Called before the ordinary EA path. RAX is the zero-extended AL
        // index added to the ORIGINAL RBX, truncated before FS/GS addition.
        private bool TryScalarMiscAddress(JitInstruction i)
        {
            if (i.Operation != JitOperation.TranslateByte) return false;
            IntegerLoad(new(0, 0), 8, 0);
            IntegerLoad(new(3, 0), 64, 1);
            IntegerRR(0x01, 0, 1);
            if (i.Address.AddressWidth == 32) IntegerMov(0, 0, 32);
            if (i.Address.Segment is 4 or 5)
            { Bytes(0x49, 0x03, 0x82); U32(i.Address.Segment == 4 ? 328u : 336u); }
            return true;
        }

        private bool TryScalarMiscOperation(in JitInstruction i)
        {
            if (!CanInlineScalarMisc(i)) return false;
            if (i.Operation == JitOperation.TranslateByte)
            {
                ScalarMiscCheckCanonical(i,8);
                NativeMemoryPrepare(i, FullMemoryAccess.Read, 8);
                NativeMemoryLoad(8); IntegerMov(8, 0); Store(i.Destination, 8);
                NativeMemoryCount();
            }
            else if (i.Operation == JitOperation.NonTemporalStore)
            {
                ScalarMiscCheckCanonical(i,i.Width);
                NativeMemoryPrepare(i,FullMemoryAccess.Write,i.Width);
                IntegerLoad(i.Source,i.Width,8); NativeMemoryStore(i.Width); NativeMemoryCount();
            }
            else
            {
                // The selected Linux user-mode producer exposes visible
                // selectors only. Never execute a host MOV from host segments.
                Bytes(0x41, 0x80, 0xbb); U32(832); Bytes(0);
                ExitCondition(0x85, FullBlockExit.Invalidated);
                if (i.Destination.IsMemory)
                { ScalarMiscCheckCanonical(i,16); NativeMemoryPrepare(i, FullMemoryAccess.Write, 16); }
                Bytes(0x45, 0x0f, 0xb7, 0x83); U32(456 + i.Options * 16); // r8d = guest selector
                if (i.Destination.IsMemory) { NativeMemoryStore(16); NativeMemoryCount(); }
                else Store(i.Destination, i.Width);
            }
            return true;
        }

        // Validate both endpoints before MMU lookup, flags or guest writes.
        // Long-mode SS has the same flat address calculation but a distinct
        // #SS vector. Explicit overrides replace the usual base-derived SS.
        private void ScalarMiscCheckCanonical(JitInstruction i,int width)
        {
            NativeMemoryAddress(i);
            IntegerMov(1,0); IntegerShift(7,1,64,47);
            IntegerConstant(0,1,1,64); IntegerConstant(7,1,1,64);
            int badStart=NearCondition(0x87);
            IntegerMov(2,0); IntegerConstant(0,2,(uint)(width/8-1),64);
            int wrap=NearCondition(0x82);
            IntegerMov(1,2); IntegerShift(7,1,64,47);
            IntegerConstant(0,1,1,64); IntegerConstant(7,1,1,64);
            int valid=NearCondition(0x86);
            Patch(badStart,position); Patch(wrap,position);
            Bytes(0x41,0xc7,0x82); U32(168); U32(FullScalarMiscLowering.UsesStackSegment(i.Address)?12u:13u);
            Bytes(0x41,0xc7,0x82); U32(172); U32(128);
            Bytes(0x49,0xc7,0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault);
            Patch(valid,position);
        }
    }
}
