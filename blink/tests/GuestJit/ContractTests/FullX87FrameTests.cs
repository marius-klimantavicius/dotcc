using Managed.Emulation.Execution;
using System.Runtime.InteropServices;

unsafe partial class Program
{
    static void CheckFullX87Frame(byte* machine)
    {
        Check(FullBlockFrame.ByteSize == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(FullBlockFrame) == 416 && sizeof(TranslationBlockHeader) == 160 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87)).ToInt32() == 400 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87Reserved)).ToInt32() == 408 &&
            FullBlockEmitter.HasExpectedLayout(), "ABI7 append-only canonical80 ingress layout");
        Check(!fullOwnerScaffold.IsValid, "canonical frame fixture starts standalone");
        CheckFullX87FrameLeaves(machine);
        using var slab = CreateFullScaffold(out var scaffold);
        fullOwnerScaffold = scaffold;
        try { CheckFullX87FrameLeaves(machine); }
        finally { fullOwnerScaffold = default; }
        ResetFull(machine);
    }
    static void CheckFullX87FrameLeaves(byte* machine)
    {
        using var code = CreateFullBlock(new FullBlockInstruction[] { new(new(Ip, 1, JitOperation.Nop, 64, default, default)) });
        byte* buffer = stackalloc byte[FullX87RegisterFile.ByteSize + 15];
        var state = (FullX87RegisterFile*)(((nuint)buffer + 15) & ~(nuint)15);
        for (int witness = 0; witness < 17; ++witness)
        {
            ResetFull(machine);
            var header = FullHeader(Ip, code.Entry);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
            FullX87StateStorage.Initialize(state, (nint)machine, 17, header.OwnerCookie);
            state->ProfileActive = 1; // TEST ONLY: isolated native NOP, no guest activation path.
            var frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop);
            frame.X87 = state;
            switch (witness)
            {
                case 0: frame.X87 = null; break;
                case 2: frame.Version = 0x46420006; frame.Size = 400; frame.X87 = (FullX87RegisterFile*)1; break;
                case 3: frame.X87Reserved = 1; break;
                case 4: frame.X87 = (FullX87RegisterFile*)((byte*)state + 1); break;
                case 5: state->Version = 0; break;
                case 6: state->Size = 191; break;
                case 7: state->ProducerProtocol = 2; break;
                case 8: state->ProfileActive = 0; break;
                case 9: state->ProfileActive = 2; break;
                case 10: state->Machine++; break;
                case 11: state->OwnerGeneration = 0; break;
                case 12: state->OwnerToken = 0; break;
                case 13: state->OwnerToken++; break;
                case 14: state->Reserved[0] = 1; break;
                case 15: state->Reserved[4] = 1; break;
                case 16: state->Reserved[19] = 1; break;
            }
            var exit = RunFull(code, &frame);
            bool valid = witness <= 1;
            Check(exit == (valid ? FullBlockExit.Continue : FullBlockExit.Invalidated) &&
                issued == (valid ? 1 : 0) && completed == (valid ? 1 : 0) &&
                frame.Executed == (valid ? 1UL : 0UL) && ((ulong*)machine)[0] == Ip + (valid ? 1UL : 0UL),
                $"canonical80 frame witness {witness} authenticates before Issue");
        }
    }
}
