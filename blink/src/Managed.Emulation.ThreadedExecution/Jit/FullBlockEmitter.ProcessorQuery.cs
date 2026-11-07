using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineProcessorQuery(JitInstruction i) => i.Operation == JitOperation.SystemState &&
        i.Options is >= (uint)FullProcessorQueryOperation.Cpuid and <= (uint)FullProcessorQueryOperation.Rdpid;

    internal static bool ValidateProcessorQuery(JitInstruction i)
    {
        if (!CanInlineProcessorQuery(i)) return false;
        if (i.Length is < 2 or > 15 || i.Target != 0 || i.Condition != 0 || i.Extra != default)
            throw new ArgumentException("Native processor query metadata");
        var op = (FullProcessorQueryOperation)i.Options;
        bool Register(JitOperand p) => IntegerRegister(p, 64) && !p.IsImmediate && p.Immediate == 0 && p.ByteOffset == 0;
        if (op == FullProcessorQueryOperation.Cpuid)
        {
            if (i.Width != 32 || i.Destination != default || i.Source != default || i.SourceWidth != 0 || i.Address != default)
                throw new ArgumentException("Native CPUID shape");
        }
        else if (op == FullProcessorQueryOperation.Rdpid)
        {
            if (i.Length < 4 || i.Width != 64 || !Register(i.Destination) || i.Source != default || i.SourceWidth != 0 || i.Address != default)
                throw new ArgumentException("Native RDPID shape");
        }
        else
        {
            if (i.Length < 3 || i.Width is not (16 or 32 or 64) || !Register(i.Destination) || i.SourceWidth != 16 ||
                (i.Source.IsMemory ? i.Source != new JitOperand(0, 0, IsMemory: true) : !Register(i.Source)))
                throw new ArgumentException("Native LSL shape");
            var a = i.Address;
            if (i.Source.IsMemory ? a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1) : a != default)
                throw new ArgumentException("Native LSL address");
        }
        if (!ProcessorQueryLayoutSupported) throw new PlatformNotSupportedException("Native processor query layout");
        return true;
    }

    internal static bool ProcessorQueryLayoutSupported => ProcessorQueryLayout.Supported;
    private static class ProcessorQueryLayout
    {
        internal static readonly bool Supported = Check();
        private static unsafe bool Check()
        {
            Blink.Machine m = default; Blink.System s = default;
            byte* b = (byte*)&m; byte* t = (byte*)&s;
            return sizeof(nint) == 8 && (byte*)&m.flags - b == 12 && (byte*)&m.trapcpuid - b == 836 &&
                (byte*)&m.metal - b == 832 && (byte*)&m.blink_host_system - b == 808 &&
                (byte*)&m.segvcode - b == 839 &&
                (byte*)&m.readaddr - b == 424 && (byte*)&m.readsize - b == 440 &&
                (byte*)&s.gdt_limit - t == 10 && (byte*)&s.real - t == 32 && (byte*)&s.gdt_base - t == 40 &&
                Blink.kRealSize == FullProcessorQueryAdmission.RealSize;
        }
    }

    // Compile-time profile data only. Generated code contains immediate constants
    // and local conditional branches, never a runtime table/pointer dispatch.
    private readonly record struct ProcessorCpuidRow(uint Leaf, uint Subleaf, bool MatchSubleaf,
        uint Ax, uint Bx, uint Cx, uint Dx);
    private static readonly ProcessorCpuidRow[] ProcessorCpuidRows =
    [
        new(0,0,false,7,0x756e6547,0x6c65746e,0x49656e69),
        new(1,0,false,0,0,0x80000000,0x07888151),
        new(2,0,false,0x76035a01,0x00f0b0ff,0,0x00ca0000),
        new(4,0,true,0x1c004121,0x01c0003f,0x3f,0),
        new(4,1,true,0x1c004122,0x01c0003f,0x3f,0),
        new(4,2,true,0x1c004143,0x01c0003f,0x1ff,0),
        new(4,3,true,0x1c03c163,0x03c0003f,0x1fff,6),
        new(6,0,false,0,0,0,0),
        new(7,0,true,0,0,0,0),
        new(0x80000000,0,false,0x80000001,0x756e6547,0x6c65746e,0x49656e69),
        new(0x80000001,0,false,0,0,0,0x21908901),
        new(0x80000007,0,false,0,0,0,0),
        new(0x40000000,0,false,0,0x756e6547,0x42656e69,0x6b6e696c),
        // Explicit requested guest-profile correction: selected generated source
        // has Unknown for BOTH strings, not the host OS/architecture.
        new(0x40031337,0,false,0,0x756e694c,0x78,0),
        new(0x40031338,0,false,0,0x5f363878,0x3436,0)
    ];

    private ref partial struct Writer
    {
        private bool TryProcessorQueryOperation(in JitInstruction i)
        {
            if (!CanInlineProcessorQuery(i)) return false;
            // Ordinary selected Linux long mode; no host segment dependency.
            Bytes(0x41,0x80,0xbb); U32(832); Bytes(0);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            switch ((FullProcessorQueryOperation)i.Options)
            {
                case FullProcessorQueryOperation.Cpuid: ProcessorCpuid(); break;
                case FullProcessorQueryOperation.Rdpid:
                    IntegerImmediate(8,0,32); Store(i.Destination,64); break;
                case FullProcessorQueryOperation.Lsl: ProcessorLsl(i); break;
            }
            return true;
        }

        private void ProcessorCpuid()
        {
            Bytes(0x41,0x80,0xbb); U32(836); Bytes(0);
            int allowed = NearCondition(0x84);
            // Preserve the SELECTED ThrowSegmentationFault(m,0) path: Blink
            // trap14, existing signed segvcode (including0 and BUS_ADRERR -2),
            // and address0. Do not silently substitute architectural #GP13.
            Bytes(0x41,0xc7,0x82); U32(168); U32(14);
            Bytes(0x41,0x0f,0xbe,0x83); U32(839);
            Bytes(0x41,0x89,0x82); U32(172);
            Bytes(0x49,0xc7,0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault); Patch(allowed,position);
            // Baseline AMD64 serialization, not feature discovery. R9 is volatile
            // on BOTH ABIs. Exact RBX restore, no CALL, RSP or frame expansion.
            Bytes(0x49,0x89,0xd9,0x31,0xc0,0x0f,0xa2,0x4c,0x89,0xcb);
            IntegerLoad(new(0,0),32,0); IntegerLoad(new(1,0),32,1);
            Span<int> done = stackalloc int[ProcessorCpuidRows.Length]; int count = 0;
            foreach (var row in ProcessorCpuidRows)
            {
                IntegerConstant(7,0,row.Leaf); int nextLeaf = NearCondition(0x85), nextSub = -1;
                if (row.MatchSubleaf) { IntegerConstant(7,1,row.Subleaf); nextSub = NearCondition(0x85); }
                ProcessorCpuidResult(row.Ax,row.Bx,row.Cx,row.Dx); done[count++] = IntegerJump();
                Patch(nextLeaf,position); if (nextSub >= 0) Patch(nextSub,position);
            }
            ProcessorCpuidResult(0,0,0,0);
            foreach (int patch in done) Patch(patch,position);
        }

        private void ProcessorCpuidResult(uint ax,uint bx,uint cx,uint dx)
        {
            IntegerImmediate(8,ax,32); Store(new(0,0),32);
            IntegerImmediate(8,bx,32); Store(new(3,0),32);
            IntegerImmediate(8,cx,32); Store(new(1,0),32);
            IntegerImmediate(8,dx,32); Store(new(2,0),32);
        }

        private void ProcessorLsl(JitInstruction i)
        {
            if (i.Source.IsMemory)
            {
                // Both source endpoints/SS-vs-GP, then both MMU pages, BEFORE
                // descriptor lookup/null/TI tests or flags/destination effects.
                ScalarMiscCheckCanonical(i,16); NativeMemoryPrepare(i,FullMemoryAccess.Read,16);
                NativeMemoryLoad(16); NativeMemoryCount();
            }
            else IntegerLoad(i.Source,16,0);
            Span<int> failed = stackalloc int[12]; int failures = 0;
            IntegerMov(1,0,32); IntegerConstant(4,1,4); failed[failures++] = NearCondition(0x85); // no LDTR
            IntegerConstant(4,0,0xfff8); IntegerConstant(7,0,8); failed[failures++] = NearCondition(0x82);
            Bytes(0x4d,0x8b,0x8b); U32(808); // r9 = live system
            IntegerRR(0x85,9,9); ExitCondition(0x84,FullBlockExit.Invalidated);
            Bytes(0x41,0x0f,0xb7,0x91); U32(10); // edx = inclusive GDT limit
            IntegerMov(1,0,32); IntegerConstant(0,1,7); IntegerRR(0x39,1,2,32);
            failed[failures++] = NearCondition(0x87);
            // An actual retained mapping lease is necessary, even for a nonnull
            // system.real. This tag is invocation-owned and not embedded in IR.
            Bytes(0x41,0x81,0xba); U32(156); U32(FullProcessorQueryAdmission.DescriptorMappingTag);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            IntegerFrameStore(192,2); IntegerFrameStore(200,0);
            Bytes(0x49,0x8b,0x49,40); // rcx = GDT base
            IntegerRR(0x01,1,0); failed[failures++] = NearCondition(0x82);
            IntegerConstant(7,1,FullProcessorQueryAdmission.RealSize-8,64); failed[failures++] = NearCondition(0x87);
            IntegerFrameStore(208,1); // real-relative address for bounds/tracking
            Bytes(0x49,0x8b,0x51,32); IntegerRR(0x85,2,2);
            ExitCondition(0x84,FullBlockExit.Invalidated);
            IntegerRR(0x01,2,1); ExitCondition(0x82,FullBlockExit.Invalidated);
            Bytes(0x4c,0x8b,0x02); // r8 = first descriptor qword, bounded/retained
            IntegerFrameStore(184,2);
            Bytes(0x49,0x89,0x8b); U32(424); // selected descriptor read tracking
            Bytes(0x49,0xc7,0x83); U32(440); U32(8);
            IntegerMov(1,8); IntegerShift(5,1,64,40); IntegerConstant(4,1,0x1f);
            IntegerMov(9,1,32); IntegerConstant(4,9,0x10); int codeData = NearCondition(0x85);
            // Only LDT/available64TSS/busy64TSS are valid IA-32e system types.
            IntegerConstant(7,1,2); int typeLdt = NearCondition(0x84);
            IntegerConstant(7,1,9); int typeTss = NearCondition(0x84);
            IntegerConstant(7,1,11); failed[failures++] = NearCondition(0x85);
            Patch(typeLdt,position); Patch(typeTss,position);
            IntegerFrameLoad(200,0); IntegerConstant(0,0,15);
            IntegerFrameLoad(192,2); IntegerRR(0x39,0,2,32); failed[failures++] = NearCondition(0x87);
            IntegerFrameLoad(208,0); IntegerConstant(7,0,FullProcessorQueryAdmission.RealSize-16,64);
            failed[failures++] = NearCondition(0x87);
            IntegerFrameLoad(184,2); Bytes(0x48,0x8b,0x42,8);
            IntegerShift(5,0,64,40); IntegerConstant(4,0,0x1f); failed[failures++] = NearCondition(0x85);
            int systemPrivilege = IntegerJump();
            Patch(codeData,position);
            // Conforming CODE alone bypasses DPL. Data type bit2 is expand-down.
            IntegerConstant(4,1,0x1c); IntegerConstant(7,1,0x1c); int conforming = NearCondition(0x84);
            Patch(systemPrivilege,position);
            IntegerMov(1,8); IntegerShift(5,1,64,45); IntegerConstant(4,1,3);
            IntegerConstant(7,1,3); failed[failures++] = NearCondition(0x85); // CPL3 dominates all RPLs
            Patch(conforming,position);
            // LSL intentionally does NOT test descriptor.P. No accessed-bit write.
            IntegerMov(0,8,32); IntegerConstant(4,0,0xffff);
            IntegerMov(1,8); IntegerShift(5,1,64,32); IntegerConstant(4,1,0xf0000); IntegerRR(0x09,0,1,32);
            IntegerMov(1,8); IntegerShift(5,1,64,55); IntegerConstant(4,1,1); int byteGranular = NearCondition(0x84);
            IntegerShift(4,0,32,12); IntegerConstant(1,0,0xfff);
            Patch(byteGranular,position); IntegerMov(8,0,32); Store(i.Destination,i.Width);
            Bytes(0x41,0x83,0x4b,12,0x40); int done = IntegerJump();
            for (int n = 0; n < failures; ++n) Patch(failed[n],position);
            Bytes(0x41,0x83,0x63,12,0xbf); // ZF0 only, destination unchanged
            Patch(done,position);
        }
    }
}
