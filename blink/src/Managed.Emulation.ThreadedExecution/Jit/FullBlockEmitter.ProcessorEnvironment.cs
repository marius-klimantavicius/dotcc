using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineProcessorEnvironment(JitInstruction i) => i.Operation == JitOperation.SystemState &&
        i.Options is >= (uint)FullProcessorEnvironmentOperation.Rdtsc and <= (uint)FullProcessorEnvironmentOperation.Rdseed;

    internal static unsafe bool ValidateProcessorEnvironment(JitInstruction i)
    {
        if (!CanInlineProcessorEnvironment(i)) return false;
        bool clock = i.Options is (uint)FullProcessorEnvironmentOperation.Rdtsc or (uint)FullProcessorEnvironmentOperation.Rdtscp;
        if (i.Length is < 2 or > 15 || i.Target != 0 || i.Condition != 0 || i.Extra != default ||
            i.Source != default || i.SourceWidth != 0 || i.Address != default ||
            (clock ? i.Width != 64 || i.Destination != default :
                i.Length < 3 || i.Width is not (16 or 32 or 64) ||
                !IntegerRegister(i.Destination, 64) || i.Destination.IsImmediate ||
                i.Destination.Immediate != 0 || i.Destination.ByteOffset != 0) ||
            i.Options == (uint)FullProcessorEnvironmentOperation.Rdtscp && i.Length < 3)
            throw new ArgumentException("Native processor environment metadata");
        Blink.Machine m = default;
        if (sizeof(FullEnvironmentDataDescriptor) != 128 || sizeof(FullBlockFrame) != 416 ||
            (byte*)&m.traprdtsc - (byte*)&m != 835 || (byte*)&m.segvcode - (byte*)&m != 839 ||
            (byte*)&m.flags - (byte*)&m != 12)
            throw new PlatformNotSupportedException("Native processor environment layout");
        return true;
    }

    private ref partial struct Writer
    {
        // MUST be emitted after entry establishes r10/r11 and BEFORE either
        // shared.Entry or Prologue. The top scaffold must not poll or reject
        // advancing runtime/code epochs between data sample and commit.
        private void ProcessorEnvironmentReadyEntry()
        {
            bool any = false;
            foreach (var site in instructions) any |= !site.RequiresHelper && CanInlineProcessorEnvironment(site.Instruction);
            if (!any) return;
            // No extended frame read precedes ABI/size authentication.
            Bytes(0x41,0x81,0x3a); U32(FullBlockFrame.AbiVersion);
            Bytes(0x0f,0x85); Reference(AbiFailureLabel);
            Bytes(0x41,0x81,0x7a,4); U32(FullBlockFrame.ByteSize);
            Bytes(0x0f,0x85); Reference(AbiFailureLabel);
            IntegerFrameLoad(384,9); IntegerRR(0x85,9,9); int absent = NearCondition(0x84);
            EnvCompare(0,FullEnvironmentDataDescriptor.TagValue); int other = NearCondition(0x85);
            EnvCompare(12,(uint)FullEnvironmentDataState.Ready); int notReady = NearCondition(0x85);
            EnvRequire(4,1);
            EnvFrameRequire(152,1,32); EnvFrameRequire(60,1,32);
            EnvFrameRequire(48,0); EnvFrameRequire(292,0,32); EnvFrameRequire(344,0);
            EnvFrameRequire(324,0,32); EnvFrameRequire(360,0,32);
            EnvFrameRequire(148,FullBlockFrame.NoHelperIndex,32);
            foreach (var pair in new (int D,int F)[] { (8,144),(16,96),(24,112),(32,104),
                (40,120),(48,136),(56,376),(64,304),(104,184),(112,192),(120,208) })
                EnvMatchFrame(pair.D,pair.F,pair.D==8?32:64);
            EnvLoad(120,0); IntegerRR(0x85,0,0); ExitCondition(0x84,FullBlockExit.Invalidated);
            // Immutable header identity is retained through the store epoch.
            // Live/code/current-global stamps are intentionally NOT consulted.
            EnvLoad(16,2); IntegerRR(0x85,2,2); ExitCondition(0x84,FullBlockExit.Invalidated);
            Bytes(0x48,0x8b,0x02); IntegerFrameLoad(104,1); IntegerRR(0x39,0,1);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x48,0x8b,0x42,8); IntegerFrameLoad(112,1); IntegerRR(0x39,0,1);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x48,0x8b,0x42,32); IntegerFrameLoad(120,1); IntegerRR(0x39,0,1);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            for (int index = 0; index < instructions.Length; ++index)
            {
                var site = instructions[index]; var i = site.Instruction;
                if (site.RequiresHelper || !CanInlineProcessorEnvironment(i)) continue;
                EnvCompare(8,(uint)index); int next = NearCondition(0x85);
                EnvRequire(80,i.Options); EnvRequire(84,(uint)i.Width);
                EnvLoad(72,0); IntegerImmediate(1,i.Ip); IntegerRR(0x39,0,1);
                ExitCondition(0x85,FullBlockExit.Invalidated);
                Bytes(0x49,0x3b,0x03); ExitCondition(0x85,FullBlockExit.Invalidated); // machine.ip
                bool rng = i.Options == (uint)FullProcessorEnvironmentOperation.Rdrand;
                EnvLoad(104,0); IntegerRR(0x85,0,0);
                ExitCondition(rng?(byte)0x84:(byte)0x85,FullBlockExit.Invalidated);
                EnvLoad(112,0); IntegerRR(0x85,0,0);
                ExitCondition(rng?(byte)0x84:(byte)0x85,FullBlockExit.Invalidated);
                if (i.Options != (uint)FullProcessorEnvironmentOperation.Rdtsc &&
                    i.Options != (uint)FullProcessorEnvironmentOperation.Rdtscp) EnvRequireQwordZero(96);
                // All fallible auth precedes these first numerical/state effects.
                ProcessorEnvironmentCompute(in i);
                NativeStackMemoryDword(12,(uint)FullEnvironmentDataState.Consumed);
                Bytes(0x41,0xc7,0x82); U32(152); U32(0);
                Bytes(0x41,0xc7,0x82); U32(144); U32((uint)index+1);
                Commit(unchecked(i.Ip+(uint)i.Length));
                JumpExit(FullBlockExit.Continue); // Commit's cap1 normally returns here first
                Patch(next,position);
            }
            JumpExit(FullBlockExit.Invalidated); // sampled malformed site: parent TERMINAL
            Patch(absent,position); Patch(other,position); Patch(notReady,position);
        }

        private bool TryProcessorEnvironmentOperation(in JitInstruction i)
        {
            if (!CanInlineProcessorEnvironment(i)) return false;
            if (i.Options is (uint)FullProcessorEnvironmentOperation.Rdtsc or (uint)FullProcessorEnvironmentOperation.Rdtscp)
            {
                Bytes(0x41,0x80,0xbb); U32(835); Bytes(0); int allowed = NearCondition(0x84);
                // Exact selected ThrowSegmentationFault(m,0), BEFORE sampling.
                Bytes(0x41,0xc7,0x82); U32(168); U32(14);
                Bytes(0x41,0x0f,0xbe,0x83); U32(839);
                Bytes(0x41,0x89,0x82); U32(172);
                Bytes(0x49,0xc7,0x82); U32(176); U32(0);
                JumpExit(FullBlockExit.GuestFault); Patch(allowed,position);
            }
            EnvFrameRequire(292,0,32); EnvFrameRequire(344,0); EnvFrameRequire(324,0,32);
            IntegerFrameLoad(384,9); IntegerRR(0x85,9,9); ExitCondition(0x84,FullBlockExit.Invalidated);
            ProcessorEnvironmentRequireIdleUnion(); // preserve ownership evidence on EVERY denial
            IntegerRR(0x31,0,0,32);
            for (int offset = 0; offset < 128; offset += 8) EnvStore(offset,0);
            NativeStackMemoryDword(0,FullEnvironmentDataDescriptor.TagValue);
            NativeStackMemoryDword(4,1); NativeStackMemoryDword(12,(uint)FullEnvironmentDataState.Request);
            foreach (var pair in new (int D,int F)[] { (8,144),(16,96),(24,112),(32,104),
                (40,120),(48,136),(56,376),(64,304) })
            { IntegerFrameLoad(pair.F,0); EnvStore(pair.D,0,pair.D==8?32:64); }
            IntegerImmediate(0,i.Ip); EnvStore(72,0);
            NativeStackMemoryDword(80,i.Options); NativeStackMemoryDword(84,(uint)i.Width);
            JumpExit(FullBlockExit.NeedEnvironment);
            return true;
        }

        // R170 invariant applies BEFORE changing a single union byte. ENV1
        // count/version offsets have different meanings from every MMU tag.
        // In particular, zero frame flags do not cancel descriptor.Ticket.
        private void ProcessorEnvironmentRequireIdleUnion()
        {
            EnvCompare(0,FullEnvironmentDataDescriptor.TagValue); int environment=NearCondition(0x84);
            EnvCompare(0,FullStackMemoryDescriptor.CurrentVersion); int dense=NearCondition(0x84);
            EnvCompare(0,FullCanonicalX87MemoryLowering.MemoryProbeTag); int probe=NearCondition(0x84);
            EnvCompare(0,FullEnterLowering.MemoryTag); int enter=NearCondition(0x84);
            EnvCompare(0,FullMaskedStoreDescriptor.CurrentTagVersion); int sparse=NearCondition(0x84);
            // Only an entirely zero uninitialized descriptor is otherwise idle.
            for(int offset=0;offset<128;offset+=8)EnvRequireQwordZero(offset);
            int zeroDone=IntegerJump();
            Patch(dense,position);
            EnvRequireQwordZero(24);
            EnvCompare(4,0);int emptyDense=NearCondition(0x84);
            EnvCompare(4,2);ExitCondition(0x87,FullBlockExit.Invalidated);
            EnvRequire(12,(uint)FullStackMemoryState.Consumed);int denseDone=IntegerJump();
            Patch(emptyDense,position);
            // Ordinary NativeMemoryPrepare clears Count alone. With no frame
            // or descriptor ticket, a cancelled/consumed state is not authority.
            // Still reject unknown states and a TicketReady ownership marker.
            EnvCompare(12,(uint)FullStackMemoryState.Consumed);ExitCondition(0x87,FullBlockExit.Invalidated);
            EnvCompare(12,(uint)FullStackMemoryState.TicketReady);ExitCondition(0x84,FullBlockExit.Invalidated);
            int emptyDone=IntegerJump();
            Patch(probe,position);EnvRequire(4,1);int probeShape=IntegerJump();
            Patch(enter,position);EnvRequire(4,2);int enterShape=IntegerJump();
            Patch(sparse,position);EnvRequire(4,FullMaskedStoreDescriptor.SparseKind);
            Patch(probeShape,position);Patch(enterShape,position);
            EnvRequireQwordZero(24); // ALL known memory tags share exact Ticket24
            EnvRequire(12,(uint)FullStackMemoryState.Consumed);
            int memoryDone=IntegerJump();
            Patch(environment,position);
            EnvRequire(4,1);EnvRequire(8,0);
            // Even a forged Ready->Request/Consumed transition cannot erase
            // original sampled identity merely by changing State or Cookie.
            for(int offset=16;offset<128;offset+=8)EnvRequireQwordZero(offset);
            EnvCompare(12,(uint)FullEnvironmentDataState.Empty);int emptyEnv=NearCondition(0x84);
            EnvRequire(12,(uint)FullEnvironmentDataState.Consumed);
            Patch(zeroDone,position);Patch(denseDone,position);Patch(memoryDone,position);Patch(emptyDone,position);Patch(emptyEnv,position);
        }

        private void ProcessorEnvironmentCompute(in JitInstruction i)
        {
            if (i.Options is (uint)FullProcessorEnvironmentOperation.Rdtsc or (uint)FullProcessorEnvironmentOperation.Rdtscp)
            {
                EnvLoad(88,0);
                // low64 sec*1e9+nsec, then *3, entirely emitted baseline GPR.
                Bytes(0x48,0x69,0xc0); U32(1000000000);
                EnvLoad(96,1); IntegerRR(0x01,0,1);
                Bytes(0x48,0x8d,0x04,0x40); // lea rax,[rax+rax*2]
                IntegerMov(8,0,32); Store(new(0,0),64);
                IntegerShift(5,0,64,32); IntegerMov(8,0,32); Store(new(2,0),64);
                if (i.Options == (uint)FullProcessorEnvironmentOperation.Rdtscp)
                { IntegerImmediate(8,0,32); Store(new(1,0),64); }
                return;
            }
            EnvLoad(88,8);
            if (i.Options == (uint)FullProcessorEnvironmentOperation.Rdrand)
            {
                EnvLoad(112,2); Bytes(0x8b,0x02,0xff,0x02); // eax=oldCount; uint++ wraps
                IntegerConstant(4,0,15); int noReseed = NearCondition(0x85);
                EnvLoad(104,2); Bytes(0x4c,0x89,0x02); // install sampled bytes
                Patch(noReseed,position);
                EnvLoad(104,2); Bytes(0x48,0x8b,0x02);
                IntegerImmediate(1,0x9e3779b97f4a7c15UL); IntegerRR(0x01,0,1);
                Bytes(0x48,0x89,0x02); // sole Vigna state advance in actual global
                IntegerMov(1,0); IntegerShift(5,1,64,30); IntegerRR(0x31,0,1);
                IntegerImmediate(1,0xbf58476d1ce4e5b9UL); Bytes(0x48,0x0f,0xaf,0xc1);
                IntegerMov(1,0); IntegerShift(5,1,64,27); IntegerRR(0x31,0,1);
                IntegerImmediate(1,0x94d049bb133111ebUL); Bytes(0x48,0x0f,0xaf,0xc1);
                IntegerMov(1,0); IntegerShift(5,1,64,31); IntegerRR(0x31,0,1);
                IntegerMov(8,0);
            }
            if (i.Width == 32) IntegerMov(8,8,32);
            Store(i.Destination,i.Width); //16 merges,32 zeroextends,64 replaces
            Bytes(0x41,0x83,0x4b,12,1); // selected CF=1; all other flags retained
        }

        private void EnvCompare(int offset,uint value)
        { IntegerPrefix(32,9); Bytes(0x81); Memory(7,1,offset); U32(value); }
        private void EnvRequire(int offset,uint value)
        { EnvCompare(offset,value); ExitCondition(0x85,FullBlockExit.Invalidated); }
        private void EnvLoad(int offset,int register,int width=64)
        { IntegerPrefix(width,9,register); Bytes(0x8b); Memory(register&7,1,offset); }
        private void EnvStore(int offset,int register,int width=64)
        { IntegerPrefix(width,9,register); Bytes(0x89); Memory(register&7,1,offset); }
        private void EnvMatchFrame(int descriptor,int frame,int width=64)
        {
            IntegerFrameLoad(frame,0);
            IntegerPrefix(width,9); Bytes(0x39); Memory(0,1,descriptor);
            ExitCondition(0x85,FullBlockExit.Invalidated);
        }
        private void EnvFrameRequire(int offset,ulong value,int width=64)
        { IntegerFrameLoad(offset,0); IntegerImmediate(1,value,width); IntegerRR(0x39,0,1,width); ExitCondition(0x85,FullBlockExit.Invalidated); }
        private void EnvRequireQwordZero(int offset)
        { EnvLoad(offset,0); IntegerRR(0x85,0,0); ExitCondition(0x85,FullBlockExit.Invalidated); }
    }
}
