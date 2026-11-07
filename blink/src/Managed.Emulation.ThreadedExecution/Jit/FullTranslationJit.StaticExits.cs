namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Private immutable owner metadata in the existing Header160 words. Native
    // code does not interpret it. A new store starts empty; unknown tags fail
    // closed rather than reconstructing opcode selection on an execution path.
    private const ulong StaticExitTag = 0x46454c31UL << 32; // FEL1
    private readonly record struct StaticExit(int Count, int LastHelperPlusOne,
        int LastLength, bool Edge0, bool Edge1, bool TakenInternal, ulong Taken,
        ulong Fallthrough, ulong SitePc);

    private static void StampStaticExit(nint address, ReadOnlySpan<Plan> plans,
        ReadOnlySpan<FullBlockInstruction> instructions, bool terminalReturns = false)
    {
        int count = instructions.Length;
        if (count is < 1 or > FullBlockEmitter.MaximumInstructions || plans.Length != count)
            throw new InvalidOperationException("Invalid static exit publication prefix.");
        var header = (TranslationBlockHeader*)address;
        var last = instructions[^1];
        int helperPlusOne = 0;
        for (int i = 0; i < count; ++i)
            if (instructions[i].RequiresHelper) helperPlusOne = i + 1;
        ulong flags = 0, taken = 0;
        if (!terminalReturns && !last.RequiresHelper && !FullBlockEmitter.IsNativeMemoryControl(last.Instruction) &&
            last.Instruction.Operation != JitOperation.PopFlags)
        {
            var native = last.Instruction;
            if (native.Operation != JitOperation.Jump) flags |= 1;
            if (native.Operation is JitOperation.Jump or JitOperation.ConditionalJump ||
                FullBlockEmitter.IsNativeCounterBranch(native))
            {
                flags |= 2; taken = native.Target;
                foreach (ref readonly var instruction in instructions)
                    if (instruction.Instruction.Ip == taken) { flags |= 4; break; }
            }
        }
        // Count-1:9 bits; last helper index+1:10; final decoded length:4.
        // This uses only the FINAL successful halving prefix before Publish.
        header->Reserved144 = taken;
        header->Reserved152 = StaticExitTag | flags | ((ulong)(count - 1) << 3) |
            ((ulong)helperPlusOne << 12) | ((ulong)last.Instruction.Length << 22);
    }

    private static bool TryReadStaticExit(in TranslationBlockHeader header, out StaticExit exit)
    {
        exit = default;
        ulong word = header.Reserved152;
        if ((word & 0xffffffff00000000UL) != StaticExitTag || (word & 0xfc000000UL) != 0)
            return false;
        int count = (int)((word >> 3) & 511) + 1;
        int helperPlusOne = (int)((word >> 12) & 1023);
        int length = (int)((word >> 22) & 15);
        bool edge0 = (word & 1) != 0, edge1 = (word & 2) != 0, inside = (word & 4) != 0;
        if (helperPlusOne > count || length == 0 || header.Payload == 0 ||
            header.PayloadBytes != count * sizeof(Plan) || header.GuestLength < count ||
            header.GuestLength < length || header.GuestLength > count * 15 ||
            (!edge1 && (inside || header.Reserved144 != 0)) ||
            (helperPlusOne == count && (edge0 || edge1))) return false;
        ulong fallthrough = unchecked(header.GuestPc + header.GuestLength);
        exit = new(count, helperPlusOne, length, edge0, edge1, inside,
            header.Reserved144, fallthrough, unchecked(fallthrough - (uint)length));
        return true;
    }
}
