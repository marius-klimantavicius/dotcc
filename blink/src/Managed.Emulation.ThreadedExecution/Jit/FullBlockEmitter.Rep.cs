using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal const int RepKernelCapacity = 16 * 1024;

    // Pure emission into owner staging. Parent packs/charges/seals code and
    // authenticates its lifetime independently of the original translation.
    internal static unsafe int EmitRepKernel(FullRepInstruction specialization,
        ulong ownerCookie, ulong kernelCookie, Span<byte> destination,
        out FullRepKernelLayout layout, bool? windowsAbi = null)
        => EmitRepKernelCore(specialization, ownerCookie, kernelCookie, destination, out layout, windowsAbi,
            ownerPrivateCompletion: false);

    // Independent production kernel capability: Resume AND FaultFinalize calls
    // must obey the same private-per-invocation Completed lifetime/publication
    // contract as TryEmitOwnerPrivateCompletion. Standalone kernel tests and any
    // external/global-counter caller continue to use EmitRepKernel above.
    internal static int EmitOwnerPrivateCompletionRepKernel(FullRepInstruction specialization,
        ulong ownerCookie, ulong kernelCookie, Span<byte> destination,
        out FullRepKernelLayout layout, bool? windowsAbi = null)
        => EmitRepKernelCore(specialization, ownerCookie, kernelCookie, destination, out layout, windowsAbi,
            ownerPrivateCompletion: true);

    private static int EmitRepKernelCore(FullRepInstruction specialization,
        ulong ownerCookie, ulong kernelCookie, Span<byte> destination,
        out FullRepKernelLayout layout, bool? windowsAbi, bool ownerPrivateCompletion)
    {
        if (!FullRepLowering.IsCanonical(in specialization) || ownerCookie == 0 || kernelCookie == 0)
            throw new ArgumentException("Invalid REP kernel specialization/binding");
        if (!RepLayout.Supported)
            throw new PlatformNotSupportedException("REP requires the parent-integrated ABI6 frame/state layout");
        Span<int> labels=stackalloc int[LabelCount],heads=stackalloc int[LabelCount];
        Span<byte> overflow=stackalloc byte[11];
        var writer=new Writer(default,destination[..Math.Min(destination.Length,RepKernelCapacity)],
            labels,heads,overflow,windowsAbi??OperatingSystem.IsWindows(),
            ownerPrivateCompletion: ownerPrivateCompletion);
        int length=writer.EmitRepKernel(in specialization,ownerCookie,kernelCookie,out int fault);
        if(length==0){layout=default;throw new ArgumentException("REP kernel destination too small",nameof(destination));}
        layout=new(length,windowsAbi??OperatingSystem.IsWindows(),0,fault,ownerCookie,kernelCookie);
        return length;
    }

    private static class RepLayout { internal static readonly bool Supported=HasExpectedRepLayout(); }
    private static unsafe bool HasExpectedRepLayout()
    {
        if(FullBlockFrame.AbiVersion!=FullRepState.FrameAbi||sizeof(FullBlockFrame)!=416||
            sizeof(FullRepState)!=256||!HasExpectedLayout())return false;
        try{return Marshal.OffsetOf<FullBlockFrame>("Rep").ToInt32()==FullRepState.FrameOffset;}
        catch(ArgumentException){return false;}
    }

    // Single-site adapter also permits direct contract tests of the ACTUAL
    // common/standalone guard+Issue+latch seam. Production mixed TBs use
        // NativeRepLatch at their authenticated site, with the same kernel binding.
    internal static int EmitRepSite(FullRepInstruction instruction,ReadOnlySpan<byte> encoding,
        ulong ownerCookie,ulong kernelCookie,nint kernelEntry,Span<byte> destination,
        in FullSharedScaffold scaffold=default,bool? windowsAbi=null)
    {
        if(!RepLayout.Supported)throw new PlatformNotSupportedException("REP requires ABI6");
        if(!FullCanonicalEncoding.TryClassifyRep(instruction.Ip,encoding,out var parsed,out _)||parsed!=instruction||
            ownerCookie==0||kernelCookie==0||kernelEntry==0)
            throw new ArgumentException("REP site binding/decoded instruction mismatch");
        bool windows=windowsAbi??(scaffold.IsValid?scaffold.WindowsAbi:OperatingSystem.IsWindows());
        if(scaffold.IsValid&&windows!=scaffold.WindowsAbi)throw new ArgumentException("REP scaffold host ABI mismatch");
        Span<FullBlockInstruction> root=stackalloc FullBlockInstruction[1];
        root[0]=new(new(instruction.Ip,instruction.Length,JitOperation.Nop,64,default,default));
        Span<int> labels=stackalloc int[LabelCount],heads=stackalloc int[LabelCount];Span<byte> overflow=stackalloc byte[11];
        var writer=new Writer(root,destination[..Math.Min(destination.Length,RepKernelCapacity)],labels,heads,overflow,windows,scaffold);
        int length=writer.EmitRepSite(in instruction,encoding,ownerCookie,kernelCookie,kernelEntry);
        if(length==0)throw new ArgumentException("REP site destination too small",nameof(destination));
        return length;
    }

    private ref partial struct Writer
    {
        internal int EmitRepSite(in FullRepInstruction i,ReadOnlySpan<byte> encoding,ulong ownerCookie,ulong kernelCookie,nint entry)
        {
            EndBranch();Bytes(0x49,0x89,windowsAbi?(byte)0xca:(byte)0xfa);Bytes(0x4d,0x8b,0x5a,8);
            if(shared.IsValid)
            {
                IntegerImmediate(0,i.Ip);IntegerFrameStore(72,0);
                int dispatcher=Continuation();AbsoluteJump(shared.Entry);Patch(dispatcher,position);EndBranch();
            }
            else Prologue();
            Bytes(0x41,0x83,0xba);U32(144);Bytes(0);ExitCondition(0x85,FullBlockExit.Invalidated);
            IntegerImmediate(8,i.Ip);int operation=Continuation();
            if(shared.IsValid)AbsoluteJump(shared.InlineGuard);
            else{Bytes(0xe9);Reference(InlineGuardLabel);}
            Patch(operation,position);EndBranch();NativeRepLatch(in i,encoding,ownerCookie,kernelCookie,entry);
            if(shared.IsValid)
            {
                for(int exit=0;exit<ExitCount;++exit)
                    if(fixupHeads[MaximumInstructions+exit]>=0)
                    {Bind(MaximumInstructions+exit);AbsoluteJump(shared.Exit((FullBlockExit)exit));}
            }
            else{Scaffolding();Exits();}
            return oversized?0:position;
        }

        // NEW core seam: reached after the existing Issue, never followed by
        // the old ordinary Operation/Commit. All arguments are translation-time.
        private void NativeRepLatch(in FullRepInstruction i,ReadOnlySpan<byte> encoding,
            ulong ownerCookie,ulong kernelCookie,nint kernelEntry)
        {
            if(!FullCanonicalEncoding.TryClassifyRep(i.Ip,encoding,out var parsed,out _)||parsed!=i||
                ownerCookie==0||kernelCookie==0||kernelEntry==0)
                throw new ArgumentException("REP latch requires exact immutable decoded instruction/kernel");
            RepStatePointer();IntegerRR(0x85,2,2);ExitCondition(0x84,FullBlockExit.Invalidated);
            RepLatchCheck(0,FullRepState.CurrentVersion);RepLatchCheck(4,FullRepState.ByteSize);
            RepLatchCheck(8,(uint)FullRepPhase.Idle);
            RepStateLoad(16,0);IntegerImmediate(1,ownerCookie);IntegerRR(0x39,0,1);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            RepStateLoad(24,0);Bytes(0x49,0x3b,0x42,64);ExitCondition(0x85,FullBlockExit.Invalidated);
            // Other native sites use Temporary3. Seed the per-call expected
            // generation HERE, after Issue; never trust its pre-site contents.
            // Parent refuses generation exhaustion before native admission.
            RepStateLoad(32,0);IntegerConstant(0,0,1,64);ExitCondition(0x82,FullBlockExit.Invalidated);
            RepStateStore(32,0);IntegerFrameStore(208,0);
            IntegerFrameLoad(112,0);RepStateStore(40,0);
            RepStateImmediate(48,i.Ip);RepStateImmediate(56,unchecked(i.Ip+(uint)i.Length));
            IntegerImmediate(0,i.Ip);IntegerFrameStore(72,0);
            IntegerFrameLoad(120,0);RepStateStore(64,0);RepStateImmediate(72,kernelCookie);
            Bytes(0x41,0x8b,0x43,12);RepStateStore(80,0,32);
            RepStateDword(88,(uint)i.Operation);RepStateDword(92,(uint)i.Width);
            RepStateDword(96,(uint)i.AddressWidth);RepStateDword(100,(uint)i.Repeat);
            RepStateDword(104,unchecked((uint)i.SourceSegment));RepStateDword(108,(uint)i.Length);
            if(i.SourceSegment is 4 or 5)IntegerFrameLoad(i.SourceSegment==4?328:336,0);
            else IntegerImmediate(0,0);
            RepStateStore(112,0);RepStateImmediate(120,0);RepStateImmediate(176,1);
            Bytes(0x41,0x8b,0x82);U32(144);RepStateStore(160,0,32);
            Span<byte> bytes=stackalloc byte[16];bytes.Clear();encoding.CopyTo(bytes);
            RepStateImmediate(184,BinaryPrimitives.ReadUInt64LittleEndian(bytes));
            RepStateImmediate(192,BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..]));
            RepSnapshot();RepStateDword(12,(uint)FullRepReason.None);
            RepStateDword(8,(uint)FullRepPhase.Active); // publish only a complete identity/progress image
            // MANDATORY first handoff, before ANY iteration: parent captures
            // Plan/bytes while the old epoch protects them, then releases ALL
            // main-store leases/epochs before entering the independent kernel.
            // Direct JMP here would retain the body's managed lease over REP.
            RepReturn(FullRepState.YieldExit,FullRepReason.Latched);
        }

        internal int EmitRepKernel(in FullRepInstruction rep,ulong ownerCookie,ulong kernelCookie,out int fault)
        {
            RepEntry(in rep,ownerCookie,kernelCookie,FullRepPhase.Active);
            RepStateDword(164,0);
            // Count0 has no EA/canonical/MMU access or register/flags change.
            IntegerLoad(new(1,0),rep.AddressWidth,0);IntegerRR(0x85,0,0);
            int zero=NearCondition(0x84);
            int loop=position;
            RepPoll();
            RepProgressGuard(in rep); // rejects progress/request exhaustion BEFORE data
            var i=RepElementInstruction(in rep);
            if(rep.IsPair)
            {
                NativeStringPairPrepare(i);
                NativeStackMemoryAdopt(32);NativeMemoryLoad(rep.Width);
                if(rep.Operation==FullRepOperation.Move)
                {IntegerMov(8,0);NativeStackMemoryAdopt(80);NativeMemoryStore(rep.Width);}
                else
                {
                    IntegerFrameStore(200,0);NativeStackMemoryAdopt(80);NativeMemoryLoad(rep.Width);
                    IntegerMov(1,0);IntegerFrameLoad(200,0);NativeStringCompare(rep.Width);
                }
            }
            else
            {
                NativeMemoryAddress(i);NativeStringCanonical(rep.Width, i.Address.Segment);
                NativeMemoryPrepare(i,rep.WritesMemory?FullMemoryAccess.Write:FullMemoryAccess.Read,rep.Width);
                if(rep.Operation==FullRepOperation.Store)
                {IntegerLoad(new(0,0),rep.Width,8);NativeMemoryStore(rep.Width);}
                else
                {
                    NativeMemoryLoad(rep.Width);
                    if(rep.Operation==FullRepOperation.Load){IntegerMov(8,0);Store(new(0,0),rep.Width);}
                    else{IntegerMov(1,0);IntegerLoad(new(0,0),rep.Width,0);NativeStringCompare(rep.Width);}
                }
            }
            // No guest effects precede complete preparation above. This is
            // the only iteration commit; no Issue or Completed in the loop.
            if(rep.HasSource)NativeStringStep(6,i);
            if(rep.HasDestination)NativeStringStep(7,i);
            IntegerLoad(new(1,0),rep.AddressWidth,8);IntegerConstant(5,8,1,rep.AddressWidth);
            Store(new(1,0),rep.AddressWidth);
            RepStatePointer();Bytes(0x48,0xff,0x42,120); // ++successful iterations
            RepStateLoad(200,0,32);IntegerRR(0x85,0,0,32);
            int noDiagnosticCount=NearCondition(0x84);
            RepStatePointer();Bytes(0xff,0x82);U32(164); // only bounded explicit-quantum calls increment this uint
            Patch(noDiagnosticCount,position);
            RepSnapshot();
            IntegerLoad(new(1,0),rep.AddressWidth,0);IntegerRR(0x85,0,0);
            int exhausted=NearCondition(0x84),condition=-1;
            if(rep.Compares)
            {
                Bytes(0x41,0xf7,0x43,12);U32(64);
                condition=NearCondition(rep.Repeat==FullRepRepeat.WhileEqual?(byte)0x84:(byte)0x85);
            }
            RepStateLoad(176,0);IntegerConstant(7,0,uint.MaxValue,64);RepReject(0x84); // no wrap store
            RepStatePointer();Bytes(0x48,0xff,0x82);U32(176); // next request, never on an MMU retry
            Bytes(0x41,0xf7,0x82);U32(324);U32((uint)FullMemoryFlags.BarrierConsumed);
            int ticket=NearCondition(0x85);
            RepStateLoad(200,0,32);IntegerRR(0x85,0,0,32);
            int normal=NearCondition(0x84); // zero is genuinely unbounded
            RepStatePointer();Bytes(0x39,0x82);U32(164); // iterations >= explicit diagnostic quantum
            int quantum=NearCondition(0x83);
            Patch(normal,position);
            RepDiscardIterationMappings();
            Bytes(0xe9);int back=position;U32(0);Patch(back,loop);
            Patch(ticket,position);RepReturn(FullRepState.YieldExit,FullRepReason.TicketBoundary);
            Patch(quantum,position);RepReturn(FullRepState.YieldExit,FullRepReason.Quantum);
            Patch(zero,position);Patch(exhausted,position);if(condition>=0)Patch(condition,position);
            RepStateDword(8,(uint)FullRepPhase.Done);
            RepStateLoad(56,0);Bytes(0x49,0x89,0x03,0x41,0xc6,0x43,8,0);
            EmitCompletedIncrement(); // sole whole-instruction completion; no iteration/fault commit
            Bytes(0x49,0xff,0x42,48);NativeMemoryCount();
            RepReturn(FullBlockExit.Continue,FullRepReason.Complete);

            fault=position;
            RepEntry(in rep,ownerCookie,kernelCookie,FullRepPhase.FaultPending);
            RepFinalizeFault(rep.Compares);
            // Reused mapping primitives branch to these existing typed labels.
            // These local stubs do not use the ordinary Header/Live scaffold.
            Bind(MaximumInstructions+(int)FullBlockExit.NeedMemory);
            RepReturn(FullBlockExit.NeedMemory,FullRepReason.NeedMemory);
            Bind(MaximumInstructions+(int)FullBlockExit.Invalidated);
            RepReturn(FullRepState.YieldExit,FullRepReason.Authority);
            Bind(MaximumInstructions+(int)FullBlockExit.Stop);
            RepReturn(FullRepState.YieldExit,FullRepReason.Stop);
            Bind(MaximumInstructions+(int)FullBlockExit.Attention);
            RepReturn(FullRepState.YieldExit,FullRepReason.Attention);
            Bind(MaximumInstructions+(int)FullBlockExit.GuestFault);
            RepFinalizeFault(rep.Compares);
            Bind(EntryLabel);
            FloatRestoreHostEnvironment();RepRawReturn(FullBlockExit.Invalidated);
            Bind(AbiFailureLabel);
            // Authentication failure cannot safely read/write Rep, restore
            // flags or mark an instruction completed. Parent treats as fatal.
            RepRawReturn(FullBlockExit.Invalidated);
            return oversized?0:position;
        }

        private static JitInstruction RepElementInstruction(in FullRepInstruction r)
            =>new(0,2,r.Operation switch{FullRepOperation.Move=>JitOperation.StringMove,
                FullRepOperation.Store=>JitOperation.StringStore,FullRepOperation.Load=>JitOperation.StringLoad,
                FullRepOperation.Compare=>JitOperation.StringCompare,_=>JitOperation.StringScan},
                r.Width,default,default,SourceWidth:r.Width,
                Address:new(r.HasSource?6:7,-1,0,0,false,r.AddressWidth,r.SourceSegment));

        private void RepEntry(in FullRepInstruction r,ulong ownerCookie,ulong kernelCookie,FullRepPhase phase)
        {
            EndBranch();Bytes(0x49,0x89,windowsAbi?(byte)0xca:(byte)0xfa);
            Bytes(0x4d,0x8b,0x5a,8);
            Bytes(0x41,0x81,0x3a);U32(FullRepState.FrameAbi);RepAbiReject(0x85);
            Bytes(0x41,0x81,0x7a,4);U32(FullBlockFrame.ByteSize);RepAbiReject(0x85);
            GuardX87Frame(independentRep: true);
            RepStatePointer();IntegerRR(0x85,2,2);RepReject(0x84);
            RepCheck(0,FullRepState.CurrentVersion);RepCheck(4,256);RepCheck(8,(uint)phase);
            RepStateLoad(16,0);IntegerImmediate(1,ownerCookie);IntegerRR(0x39,0,1);RepReject(0x85);
            Bytes(0x49,0x3b,0x42,104);RepReject(0x85);
            RepMatchFrame(24,64);RepMatchFrame(32,208);RepMatchFrame(40,112);
            RepStateLoad(24,0);IntegerRR(0x85,0,0);RepReject(0x84);
            RepStateLoad(32,0);IntegerRR(0x85,0,0);RepReject(0x84);
            RepMatchFrame(48,72);Bytes(0x49,0x3b,0x03);RepReject(0x85);
            RepMatchFrame(64,120);
            RepStateLoad(72,0);IntegerImmediate(1,kernelCookie);IntegerRR(0x39,0,1);RepReject(0x85);
            RepCheck(88,(uint)r.Operation);RepCheck(92,(uint)r.Width);RepCheck(96,(uint)r.AddressWidth);
            RepCheck(100,(uint)r.Repeat);RepCheck(104,unchecked((uint)r.SourceSegment));
            RepStateLoad(108,0,32);IntegerConstant(7,0,2);RepReject(0x82);IntegerConstant(7,0,15);RepReject(0x87);
            RepStateLoad(48,1);IntegerRR(0x01,0,1);RepStatePointer();Bytes(0x48,0x3b,0x42,56);RepReject(0x85);
            RepStateLoad(160,0,32);Bytes(0x41,0x3b,0x82);U32(144);RepReject(0x85);
            Bytes(0x41,0x83,0xba);U32(152);Bytes(0);RepReject(0x85); // no ordinary reissue allowance
            RepCheckMachine(128,32);RepCheckMachine(136,72);RepCheckMachine(144,80);RepCheckMachine(152,24);
            RepStateLoad(84,0,32);Bytes(0x41,0x3b,0x43,12);RepReject(0x85);
            RepProgressGuard(in r);
            if(phase==FullRepPhase.Active)
            {
                RepStateLoad(168,0);IntegerRR(0x85,0,0);RepReject(0x84);
                Bytes(0x49,0x3b,0x82);U32(136);RepReject(0x85);
                if(r.SourceSegment is 4 or 5){RepStateLoad(112,0);IntegerFrameStore(r.SourceSegment==4?328:336,0);}
            }
        }

        private void RepPoll()
        {
            IntegerFrameLoad(32,0);IntegerRR(0x85,0,0);RepReject(0x84);
            Bytes(0x83,0x38,0);ExitCondition(0x85,FullBlockExit.Stop);
            Bytes(0x41,0x80,0x7b,11,0);ExitCondition(0x85,FullBlockExit.Attention);
            for(int n=0;n<2;++n)
            {
                int pointer=n==0?128:368;
                IntegerFrameLoad(pointer,0);IntegerRR(0x85,0,0);RepReject(0x84);
                Bytes(0x48,0x8b,0x00,0x49,0x3b,0x82);U32((uint)(pointer==128?136:376));
                ExitCondition(0x85,FullBlockExit.Invalidated);
            }
        }

        private void RepProgressGuard(in FullRepInstruction r)
        {
            RepStateLoad(120,0);IntegerConstant(0,0,1,64);RepReject(0x82); // successful-iteration counter cannot wrap
            RepStatePointer();Bytes(0x48,0x3b,0x82);U32(176);RepReject(0x85); // request == successes+1
            IntegerConstant(7,0,uint.MaxValue,64); // REX.W CMP imm32(-1) == ulong.MaxValue
            int ordinary=NearCondition(0x85);
            // RequestMax is legal only for the final possible iteration of a
            // countMax attempt. It must complete without requesting Max+1.
            IntegerLoad(new(1,0),r.AddressWidth,0);IntegerConstant(7,0,1,64);RepReject(0x85);
            Patch(ordinary,position);
        }

        private void RepFinalizeFault(bool compares)
        {
            if(compares){RepStateLoad(80,0,32);Bytes(0x41,0x89,0x43,12);RepStateStore(84,0,32);}
            RepStateDword(8,(uint)FullRepPhase.FaultFinal);
            RepReturn(FullBlockExit.GuestFault,FullRepReason.Fault);
        }

        private void RepSnapshot()
        {
            IntegerLoad(new(1,0),64,0);RepStateStore(128,0);
            IntegerLoad(new(6,0),64,0);RepStateStore(136,0);
            IntegerLoad(new(7,0),64,0);RepStateStore(144,0);
            IntegerLoad(new(0,0),64,0);RepStateStore(152,0);
            Bytes(0x41,0x8b,0x43,12);RepStateStore(84,0,32);
        }

        private void RepDiscardIterationMappings()
        {
            IntegerFrameLoad(384,0);IntegerRR(0x85,0,0);
            int missing=NearCondition(0x84);
            Bytes(0xc7,0x40,4);U32(0);Bytes(0xc7,0x40,12);U32(0);
            Patch(missing,position);
            Bytes(0x41,0xc7,0x82);U32(324);U32(0);
            Bytes(0x49,0xc7,0x82);U32(344);U32(0);
        }

        private void RepStatePointer(){Bytes(0x49,0x8b,0x92);U32(392);}
        private void RepStateLoad(int offset,int register,int width=64)
        {RepStatePointer();IntegerPrefix(width,2,register);Bytes(0x8b);Memory(register&7,2,offset);}
        private void RepStateStore(int offset,int register,int width=64)
        {RepStatePointer();IntegerPrefix(width,2,register);Bytes(0x89);Memory(register&7,2,offset);}
        private void RepStateDword(int offset,uint value)
        {RepStatePointer();Bytes(0xc7);Memory(0,2,offset);U32(value);}
        private void RepStateImmediate(int offset,ulong value)
        {IntegerImmediate(0,value);RepStateStore(offset,0);}
        private void RepReject(byte condition){Bytes(0x0f,condition);Reference(EntryLabel);}
        private void RepAbiReject(byte condition){Bytes(0x0f,condition);Reference(AbiFailureLabel);}
        private void RepCheck(int offset,uint value)
        {RepStatePointer();Bytes(0x81);Memory(7,2,offset);U32(value);RepReject(0x85);}
        private void RepLatchCheck(int offset,uint value)
        {RepStatePointer();Bytes(0x81);Memory(7,2,offset);U32(value);ExitCondition(0x85,FullBlockExit.Invalidated);}
        private void RepMatchFrame(int state,int frame)
        {RepStateLoad(state,0);Bytes(0x49,0x3b,0x82);U32((uint)frame);RepReject(0x85);}
        private void RepCheckMachine(int state,int machine)
        {RepStateLoad(state,0);Bytes(0x49,0x3b,0x83);U32((uint)machine);RepReject(0x85);}
        private void RepReturn(FullBlockExit exit,FullRepReason reason)
        {RepStateDword(12,(uint)reason);RepStateImmediate(168,0);FloatRestoreHostEnvironment();RepRawReturn(exit);}
        private void RepRawReturn(FullBlockExit exit)
        {
            Bytes(0x49,0x8b,0x03,0x49,0x89,0x42,72);
            Bytes(0xb8);U32((uint)exit);Bytes(0x41,0x89,0x42,56,0xc3);
        }
    }
}
