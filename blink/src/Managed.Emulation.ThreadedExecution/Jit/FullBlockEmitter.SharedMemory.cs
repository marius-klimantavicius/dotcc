namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        private void NativeTlbLookup(FullMemoryAccess access, bool atomicFast = false)
        {
            if (!shared.IsValid)
            {
                // Keep standalone instruction bytes and guard ordering exact.
                NativeTlbLookupInline(access, atomicFast);
                return;
            }
            // Selection happens at translation time. No runtime opcode, width,
            // rights or atomic-mode dispatch is added to the native guard.
            nint target = atomicFast ? shared.MemoryAtomicFast : access switch
            {
                FullMemoryAccess.Read => shared.MemoryRead,
                FullMemoryAccess.Write => shared.MemoryWrite,
                (FullMemoryAccess.Read | FullMemoryAccess.Write) => shared.MemoryReadWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(access))
            };
            // RAX may be the SECOND page EA, not Frame.MemoryAddress. Preserve
            // that input across the LEA; neither R9 nor the host stack is scratch.
            Bytes(0x48, 0x89, 0xc1); // rcx = input EA
            Bytes(0x48, 0x8d, 0x05); int landing = position; U32(0);
            Bytes(0x49, 0x89, 0x42, 72); // private native continuation
            Bytes(0x48, 0x89, 0xc8); // rax = input EA
            Bytes(0x48, 0xba); U64(unchecked((ulong)target)); Bytes(0xff, 0xe2);
            Patch(landing, position); EndBranch();
            // RAX/R8 retain the inline lookup's result scratch, RDX the checked
            // host pointer. Restore root before ANY later boundary/link guard.
            Bytes(0x48, 0xb9); U64(instructions[0].Instruction.Ip);
            Bytes(0x49, 0x89, 0x4a, 72);
        }

        private int SharedMemoryGuard(FullMemoryAccess access, bool atomicFast = false)
        {
            int entry = position;
            EndBranch();
            // Same R181 fold/rights/lifetime and R183 atomic predicates, in the
            // same order, with the same NeedMemory exits as standalone bodies.
            NativeTlbLookupInline(access, atomicFast);
            Bytes(0x41, 0xff, 0x62, 72); // JMP qword ptr [r10+NextIp]
            return entry;
        }
    }
}
