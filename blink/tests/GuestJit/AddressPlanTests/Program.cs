// GuestJit.csproj recursively includes this directory. Keep its ordinary test
// executable unchanged without requiring an edit outside this task's ownership.
#if ADDRESS_PLAN_TESTS
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;

internal static partial class AddressPlanTests
{
    private static int assertions;
    private static void Check(bool condition, string message)
    {
        ++assertions;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static int Main()
    {
        CheckHotCorpus();
        CheckAliasesAndExtensions();
        CheckEntryExpressions();
        CheckAffineRecipes();
        CheckIntegerOperations();
        CheckUnknownLoads();
        CheckStackAndControlFlow();
        CheckLoops();
        CheckEffects();
        CheckBoundsAndAllocation();
        Console.WriteLine($"AddressPlanTests: {assertions} assertions passed");
        return 0;
    }

    private static ulong[] Registers()
    {
        ulong[] registers = new ulong[16];
        for (int i = 0; i < registers.Length; ++i) registers[i] = 0xfeedface12340000UL + (uint)i;
        registers[3] = 0x100000; registers[4] = 0x70004000; registers[5] = 0x200000;
        registers[6] = 7; registers[7] = 0x300000;
        return registers;
    }

    private static JitInstruction Mov(int destination, int source, int width = 64) =>
        new(0, 1, JitOperation.Mov, width, new(destination, 0), new(source, 0));
    private static JitInstruction Imm(int destination, ulong value, int width = 64) =>
        new(0, 1, JitOperation.Mov, width, new(destination, 0), new(0, value, true));
    private static JitInstruction Alu(JitOperation operation, int destination, ulong value, int width = 64) =>
        new(0, 1, operation, width, new(destination, 0), new(0, value, true));
    private static JitInstruction Load(int destination, int addressRegister, long displacement = 0, int width = 64) =>
        new(0, 1, JitOperation.LoadMov, width, new(destination, 0), default, SourceWidth: width,
            Address: new(addressRegister, -1, 0, displacement));
    private static JitInstruction Store(int source, int addressRegister, long displacement = 0) =>
        new(0, 1, JitOperation.StoreMov, 64, default, new(source, 0), SourceWidth: 64,
            Address: new(addressRegister, -1, 0, displacement));
    private static JitInstruction Push(int source) =>
        new(0, 1, JitOperation.Push, 64, default, new(source, 0), SourceWidth: 64, Address: new(4, -1, 0, -8));
    private static JitInstruction Pop(int destination) =>
        new(0, 1, JitOperation.Pop, 64, new(destination, 0), default, SourceWidth: 64, Address: new(4, -1, 0, 0));
    private static JitInstruction Lea(int destination, int @base, int index = -1, int scale = 0, long displacement = 0, int width = 64) =>
        new(0, 1, JitOperation.Lea, width, new(destination, 0), default, Address: new(@base, index, scale, displacement));

    private static JitAddressPlan CheckAddresses(string label, JitInstruction[] source, ulong[] registers, params ulong?[] expected)
    {
        var plan = JitAddressPlan.Build(source);
        Check(plan.Memory.Length == expected.Length, label + " memory count");
        Check(plan.HasMemory == (expected.Length != 0), label + " HasMemory");
        Check(plan.Charge <= JitAddressPlan.MaximumCharge, label + " bounded charge");
        Span<ulong> scratch = stackalloc ulong[JitAddressPlan.MaximumValues];
        int evaluated = 0;
        for (int i = 0; i < expected.Length; ++i)
        {
            int before = evaluated;
            bool known = plan.TryEvaluateAddress(i, registers, scratch, ref evaluated, out ulong address);
            bool genericKnown = TryEvaluateFullIr(plan, i, registers, out ulong genericAddress);
            Check(known == genericKnown && (!known || address == genericAddress), label + " agrees with full generic IR");
            Check(known == expected[i].HasValue, label + $" recipe {i} known");
            if (known) Check(address == expected[i]!.Value, label + $" recipe {i}: {address:x}");
            else Check(evaluated == before, label + " unknown recipe does no evaluation");
            Check(evaluated <= plan.EvaluationCount, label + " bounded evaluation");
        }
        return plan;
    }

    private static void CheckResult(string label, ulong expected, params JitInstruction[] operations)
    {
        var source = operations.Append(Load(15, 0)).ToArray();
        CheckAddresses(label, source, Registers(), expected);
    }

    // Exact instruction order, IPs, lengths and terminal targets transcribed
    // from astra-20261027-r2/hot-blocks.asm.txt. No guest ELF or native code runs.
    private static JitInstruction[] HotPrologue(ulong ip, ulong target, bool initialized)
    {
        List<JitInstruction> result = [];
        int offset = 0;
        void Add(JitInstruction instruction, int length)
        { result.Add(instruction with { Ip = ip + (uint)offset, Length = length }); offset += length; }
        Add(Push(5), 1); Add(Push(15), 2); Add(Push(3), 1);
        Add(Alu(JitOperation.Sub, 4, 16), 4); Add(Lea(5, 4, displacement: 32), 5);
        if (initialized)
        {
            Add(new(0, 1, JitOperation.Xor, 32, new(0, 0), new(0, 0)), 2);
            Add(Store(0, 5, -24), 4);
        }
        Add(Mov(3, 7), 3); Add(Mov(15, 2), 3);
        Add(new(0, 1, JitOperation.Test, 32, new(6, 0), new(6, 0)), 2);
        ulong sideExit = ip + (initialized ? 0x51UL : 0x4bUL);
        Add(new(0, 1, JitOperation.ConditionalJump, 32, default, default, Target: sideExit, Condition: 12), 2);
        Add(Load(7, 3, 16, 32), 3);
        Add(new(0, 1, JitOperation.Cmp, 32, new(6, 0), new(7, 0)), 2);
        Add(new(0, 1, JitOperation.ConditionalJump, 32, default, default, Target: sideExit, Condition: 3), 2);
        Add(Load(0, 3, 8), 4); Add(Mov(6, 6, 32), 2);
        Add(new(0, 1, JitOperation.Add, 64, new(6, 0), new(0, 0)), 3);
        Add(Store(6, 5, -24), 4); Add(Mov(6, 7, 32), 2);
        Add(new(0, 1, JitOperation.Add, 64, new(6, 0), new(0, 0)), 3);
        Add(Lea(7, 5, displacement: -24), 4);
        Add(new(0, 5, JitOperation.Call, 64, default, default, Target: target,
            SourceWidth: 64, Address: new(4, -1, 0, -8)), 5);
        Check(result.Count == (initialized ? 22 : 20), "retained prologue instruction count");
        Check(result[^1].Ip == ip + (initialized ? 0x3aUL : 0x34UL), "retained CALL IP");
        return result.ToArray();
    }

    private static void CheckHotCorpus()
    {
        var registers = Registers();
        ulong sp = registers[4], argument = registers[7];
        CheckFastOnly(CheckAddresses("110001391260", HotPrologue(0x110001391260, 0x110001390f10, false), registers,
            sp - 8, sp - 16, sp - 24, argument + 16, argument + 8, sp - 32, sp - 48));
        foreach (var (ip, target) in new[] { (0x1100013d8060UL, 0x1100013d7d90UL), (0x1100013be440UL, 0x1100013be1c0UL) })
            CheckFastOnly(CheckAddresses(ip.ToString("x"), HotPrologue(ip, target, true), registers,
                sp - 8, sp - 16, sp - 24, sp - 32, argument + 16, argument + 8, sp - 32, sp - 48));

        ulong root = 0x110001390f10;
        List<JitInstruction> body = [];
        void Add(int offset, int length, JitInstruction instruction) => body.Add(instruction with { Ip = root + (uint)offset, Length = length });
        Add(0, 1, Push(5)); Add(1, 3, Mov(5, 4)); Add(4, 3, Load(0, 7));
        Add(7, 3, new(0, 1, JitOperation.Cmp, 64, new(0, 0), new(6, 0)));
        Add(10, 6, new(0, 1, JitOperation.ConditionalJump, 32, default, default, Target: root + 0xde, Condition: 3));
        Add(16, 3, new(0, 1, JitOperation.LoadMovzx, 32, new(1, 0), default, SourceWidth: 8, Address: new(0, -1, 0, 0)));
        int[] testOffsets = [19, 28, 37, 42, 47];
        int[] branchLengths = [6, 6, 2, 2, 2];
        int[] branchTargets = [0xd0, 0xb0, 0x86, 0x51, 0xde];
        for (int i = 0; i < testOffsets.Length; ++i)
        {
            Add(testOffsets[i], 3, new(0, 1, JitOperation.Test, 8, new(1, 0), new(0, 1UL << i, true)));
            Add(testOffsets[i] + 3, branchLengths[i], new(0, 1, JitOperation.ConditionalJump, 32, default, default,
                Target: root + (uint)branchTargets[i], Condition: i == 4 ? 5 : 4));
        }
        Add(56, 3, new(0, 1, JitOperation.Inc, 64, new(0, 0), default));
        Add(59, 3, Store(0, 7)); Add(62, 3, Load(6, 7)); Add(65, 2, Load(0, 6, width: 32));
        Add(67, 4, Alu(JitOperation.Add, 6, 4)); Add(71, 3, Store(6, 7)); Add(74, 2, Mov(2, 0, 32));
        Add(76, 5, new(0, 1, JitOperation.Jump, 32, default, default, Target: root + 0xda));
        Check(body.Count == 24, "retained loaded-RAX corpus length");
        CheckFastOnly(CheckAddresses("110001390f10", body.ToArray(), registers, sp - 8, argument, null, argument, argument, null, argument));

        root = 0x110001391190; body.Clear();
        Add(0, 1, Push(5)); Add(1, 1, Push(3)); Add(2, 1, Push(0)); Add(3, 5, Lea(5, 4, displacement: 16));
        Add(8, 3, Load(3, 7));
        Add(11, 3, new(0, 1, JitOperation.LoadMovzx, 32, new(0, 0), default, SourceWidth: 8, Address: new(3, -1, 0, 0)));
        int[] targets = [0x53, 0x4a, 0x41, 0x38, 0x2f, 0x61];
        for (int i = 0; i < 6; ++i)
        {
            int offset = 14 + i * 4;
            Add(offset, 2, new(0, 1, JitOperation.Test, 8, new(0, 0), new(0, 1UL << i, true)));
            Add(offset + 2, 2, new(0, 1, JitOperation.ConditionalJump, 32, default, default,
                Target: root + (uint)targets[i], Condition: i == 5 ? 5 : 4));
        }
        Add(38, 4, Alu(JitOperation.Add, 3, 9)); Add(42, 3, Store(3, 7));
        Add(45, 2, new(0, 1, JitOperation.Jump, 32, default, default, Target: root + 0x5a));
        Check(body.Count == 21, "retained loaded-RBX corpus length");
        CheckFastOnly(CheckAddresses("110001391190", body.ToArray(), registers, sp - 8, sp - 16, sp - 24, argument, null, argument));

        root = 0x1100013913e0; body.Clear();
        Add(0, 1, Push(5)); Add(1, 1, Push(3)); Add(2, 1, Push(0)); Add(3, 5, Lea(5, 4, displacement: 16));
        Add(8, 3, Mov(3, 7)); Add(11, 2, new(0, 1, JitOperation.Test, 32, new(6, 0), new(6, 0)));
        Add(13, 2, new(0, 1, JitOperation.ConditionalJump, 32, default, default, Target: root + 0x34, Condition: 12));
        Add(15, 3, new(0, 1, JitOperation.MemoryCmpReverse, 32, new(6, 0), default, SourceWidth: 32, Address: new(3, -1, 0, 16)));
        Add(18, 2, new(0, 1, JitOperation.ConditionalJump, 32, default, default, Target: root + 0x34, Condition: 3));
        Add(20, 2, Mov(7, 6, 32));
        Check(body.Count == 10, "retained reverse-CMP corpus length");
        CheckFastOnly(CheckAddresses("1100013913e0", body.ToArray(), registers, sp - 8, sp - 16, sp - 24, argument + 16));
    }

    private static void CheckAliasesAndExtensions()
    {
        CheckResult("MOV r32 zero extends", 0x89abcdef, Imm(0, 0xfedcba9889abcdef), Mov(0, 0, 32));
        CheckResult("MOV r16 preserves high bits", 0x112233445566abcd, Imm(0, 0x1122334455667788), Imm(0, 0xabcd, 16));
        CheckResult("MOV r8 preserves other bytes", 0x11223344556677ab, Imm(0, 0x1122334455667788), Imm(0, 0xab, 8));
        CheckResult("MOV AH aliases RAX", 0x112233445566ab88, Imm(0, 0x1122334455667788),
            new(0, 1, JitOperation.Mov, 8, new(0, 0, ByteOffset: 1), new(0, 0xab, true)));
        CheckResult("MOV AL,AH uses before alias", 0x1122334455667777, Imm(0, 0x1122334455667788),
            new(0, 1, JitOperation.Mov, 8, new(0, 0), new(0, 0, ByteOffset: 1)));
        CheckResult("MOVZX from AH", 0x80, Imm(1, 0x7f80ff),
            new(0, 1, JitOperation.Movzx, 64, new(0, 0), new(1, 0, ByteOffset: 1), SourceWidth: 8));
        CheckResult("MOVSX AH to r64", 0xffffffffffffff80, Imm(1, 0x80ff),
            new(0, 1, JitOperation.Movsx, 64, new(0, 0), new(1, 0, ByteOffset: 1), SourceWidth: 8));
        CheckResult("MOVSX byte to r32 zeroes high half", 0xffffff80, Imm(1, 0x80),
            new(0, 1, JitOperation.Movsx, 32, new(0, 0), new(1, 0), SourceWidth: 8));
        CheckResult("MOVSX word to r64", 0xffffffffffff8000, Imm(1, 0x8000),
            new(0, 1, JitOperation.Movsx, 64, new(0, 0), new(1, 0), SourceWidth: 16));
        CheckResult("MOVSXD", 0xffffffff80000000, Imm(1, 0x80000000),
            new(0, 1, JitOperation.Movsx, 64, new(0, 0), new(1, 0), SourceWidth: 32));
        CheckResult("MOVZX into word preserves upper bits", 0x1122334455660080, Imm(0, 0x1122334455667788), Imm(1, 0x80),
            new(0, 1, JitOperation.Movzx, 16, new(0, 0), new(1, 0), SourceWidth: 8));
        CheckResult("LEA64 retains full-width sum", 0x300000030, Imm(1, 0xffffffff00000010), Imm(2, 0x100000008),
            Lea(0, 1, 2, 2));
    }

    private static void CheckIntegerOperations()
    {
        CheckResult("ADD64 wraps", 0, Imm(0, ulong.MaxValue), Alu(JitOperation.Add, 0, 1));
        CheckResult("SUB64 wraps", ulong.MaxValue, Imm(0, 0), Alu(JitOperation.Sub, 0, 1));
        CheckResult("ADD32 zero extension", 0, Imm(0, ulong.MaxValue), Alu(JitOperation.Add, 0, 1, 32));
        CheckResult("ADD16 partial wrap", 0x1122334455660000, Imm(0, 0x112233445566ffff), Alu(JitOperation.Add, 0, 1, 16));
        CheckResult("INC8 partial wrap", 0x1122334455667700, Imm(0, 0x11223344556677ff),
            new(0, 1, JitOperation.Inc, 8, new(0, 0), default));
        CheckResult("DEC32 zero extension", 0xffffffff, Imm(0, 0), new(0, 1, JitOperation.Dec, 32, new(0, 0), default));
        CheckResult("ADD AH wrap preserves AL", 0x1122334455660088, Imm(0, 0x112233445566ff88),
            new(0, 1, JitOperation.Add, 8, new(0, 0, ByteOffset: 1), new(0, 1, true)));
        CheckResult("logical widths", 0x12340040, Imm(0, ulong.MaxValue), Alu(JitOperation.And, 0, 0x123400f0, 32),
            Alu(JitOperation.Or, 0, 0xa0, 8), Alu(JitOperation.Xor, 0, 0xb0, 8));
        CheckResult("CMP and TEST retain destination", 0x123456789abcdef0, Imm(0, 0x123456789abcdef0),
            Alu(JitOperation.Cmp, 0, 0), Alu(JitOperation.Test, 0, 0));
        CheckResult("LEA64 wrap and scale", 0x10, Imm(1, ulong.MaxValue - 15), Imm(2, 4), Lea(0, 1, 2, 3));
        CheckResult("LEA32 zero extension", 0x30, Imm(1, 0xffffffff00000010), Imm(2, 0x100000008), Lea(0, 1, 2, 2, width: 32));
        var rip = Load(0, -1) with { Ip = ulong.MaxValue - 7, Length = 7, Address = new(-1, -1, 0, 33, true) };
        CheckAddresses("RIP arithmetic wraps", [rip], Registers(), 32);
        var negative = Load(0, 3, -16);
        var registers = Registers(); registers[3] = 8;
        CheckAddresses("negative displacement wraps", [negative], registers, ulong.MaxValue - 7);
        var sib = Load(0, 3) with { Address = new(3, 3, 3, -5) };
        registers[3] = 0x1000;
        CheckAddresses("same base/index aliases", [sib], registers, 0x8ffb);
    }

    private static void CheckEntryExpressions()
    {
        var registers = Registers();
        registers[0] = 0x1122334455667788; registers[1] = 0x80;
        CheckAddresses("entry-derived partial insertion", [Imm(0, 0xabcd, 16), Load(2, 0)], registers, 0x112233445566abcd);
        CheckAddresses("entry-derived AL/AH alias", [new(0, 1, JitOperation.Mov, 8, new(0, 0), new(0, 0, ByteOffset: 1)), Load(2, 0)],
            registers, 0x1122334455667777);
        CheckAddresses("entry-derived AH arithmetic", [new(0, 1, JitOperation.Sub, 8, new(0, 0, ByteOffset: 1), new(0, 0x80, true)), Load(2, 0)],
            registers, 0x112233445566f788);
        CheckAddresses("entry-derived sign extension", [new(0, 1, JitOperation.Movsx, 64, new(0, 0), new(1, 0), SourceWidth: 8), Load(2, 0)],
            registers, 0xffffffffffffff80);
        CheckAddresses("entry-derived zero extension", [Mov(0, 0, 32), Load(2, 0)], registers, 0x55667788);
        registers[3] = ulong.MaxValue - 15; registers[12] = 8;
        for (int scale = 0; scale < 4; ++scale)
        {
            ulong expected = unchecked(registers[3] + (registers[12] << scale) - 7);
            CheckAddresses("entry-derived SIB scale " + scale, [Lea(0, 3, 12, scale, -7), Load(1, 0)], registers, expected);
        }
        registers[0] = 0x12345678ffffffff;
        CheckAddresses("entry-derived full-width carry", [Alu(JitOperation.Add, 0, 1), Load(1, 0)], registers, 0x1234567900000000);
        CheckAddresses("entry-derived low-width carry", [Alu(JitOperation.Add, 0, 1, 32), Load(1, 0)], registers, 0);
    }

    private static void CheckUnknownLoads()
    {
        var registers = Registers(); ulong original = registers[3];
        CheckAddresses("load-dependent addresses skip", [Load(3, 3), Load(0, 3), Mov(1, 3), Alu(JitOperation.Add, 1, 8), Load(0, 1)],
            registers, original, null, null);
        CheckAddresses("loaded register overwritten recovers", [Load(0, 3), Imm(0, 0x1234, 32), Load(1, 0)], registers, original, 0x1234);
        CheckAddresses("zero idiom after load recovers", [Load(0, 3), new(0, 1, JitOperation.Xor, 32, new(0, 0), new(0, 0)), Load(1, 0)],
            registers, original, 0);
        CheckAddresses("partial overwrite retains unknown upper bits", [Load(0, 3), Imm(0, 0x12, 8), Load(1, 0)], registers, original, null);
        CheckAddresses("known partial slice recovers", [Load(0, 3), Imm(0, 0x12, 8),
            new(0, 1, JitOperation.Movzx, 64, new(1, 0), new(0, 0), SourceWidth: 8), Load(2, 1)], registers, original, 0x12);
        CheckAddresses("known high-byte slice recovers", [Load(0, 3),
            new(0, 1, JitOperation.Mov, 8, new(0, 0, ByteOffset: 1), new(0, 0x34, true)),
            new(0, 1, JitOperation.Movzx, 64, new(1, 0), new(0, 0, ByteOffset: 1), SourceWidth: 8), Load(2, 1)], registers, original, 0x34);
        CheckAddresses("memory compare leaves address value", [new(0, 1, JitOperation.MemoryTest, 8, default, new(0, 1, true),
            SourceWidth: 8, Address: new(3, -1, 0, 0)), Load(0, 3)], registers, original, original);
        CheckAddresses("MOVZX loaded data remains unknown", [new(0, 1, JitOperation.LoadMovzx, 32, new(0, 0), default,
            SourceWidth: 8, Address: new(3, -1, 0, 0)), Load(1, 0)], registers, original, null);
        CheckAddresses("store never forwards a loaded value", [Store(0, 3), Load(1, 3), Load(2, 1)], registers, original, original, null);
    }

    private static void CheckStackAndControlFlow()
    {
        var registers = Registers(); ulong sp = registers[4];
        CheckAddresses("PUSH RSP and stack deltas", [Push(4), Push(5), Pop(1), Store(0, 4), Alu(JitOperation.Sub, 4, 16), Load(2, 4)],
            registers, sp - 8, sp - 16, sp - 16, sp - 8, sp - 24);
        CheckAddresses("POP RSP overrides increment", [Pop(4), Load(0, 4)], registers, sp, null);
        CheckAddresses("POP unknown register does not taint RSP", [Pop(0), Load(1, 4), Load(2, 0)], registers, sp, sp + 8, null);
        var call = new JitInstruction(0x2000, 5, JitOperation.Call, 64, default, default, Target: 0x1000,
            SourceWidth: 64, Address: new(4, -1, 0, -8));
        var callPlan = CheckAddresses("CALL uses adjusted stack and terminates", [Push(5), Alu(JitOperation.Sub, 4, 16), call, Load(0, 3)],
            registers, sp - 8, sp - 32);
        Check(callPlan.Instructions.Length == 3 && !callPlan.HasInternalBackedge, "CALL remains external");
        var ret = new JitInstruction(0x3000, 1, JitOperation.Ret, 64, default, default,
            SourceWidth: 64, Address: new(4, -1, 0, 0));
        var retPlan = CheckAddresses("RET uses incremented stack and terminates", [Pop(5), ret, Load(0, 3)], registers, sp, sp + 8);
        Check(retPlan.Instructions[^1].NextIpValue != 0 && !retPlan.HasInternalBackedge, "RET target remains unknown/external");
        CheckAddresses("conditional fallthrough retains state", [Mov(3, 7),
            new(0x1001, 2, JitOperation.ConditionalJump, 32, default, default, Target: 0x5000), Load(0, 3)], registers, registers[7]);
        var stopped = CheckAddresses("unconditional exit prunes unreachable memory", [new(0x1000, 2, JitOperation.Jump, 32, default, default, Target: 0x5000), Load(0, 3)], registers);
        Check(stopped.Instructions.Length == 1 && !stopped.HasMemory, "unreachable tail absent");
        registers[4] = 0;
        CheckAddresses("PUSH wraps full-width RSP", [Push(4)], registers, ulong.MaxValue - 7);
        registers[4] = ulong.MaxValue - 7;
        CheckAddresses("POP increment wraps RSP", [Pop(0), Load(1, 4)], registers, ulong.MaxValue - 7, 0);
    }

    private static void CheckLoops()
    {
        var registers = Registers(); ulong address = registers[3];
        var plan = CheckAddresses("loop-carried address is unknown", [Load(0, 3) with { Ip = 0x1000 },
            Alu(JitOperation.Add, 3, 8) with { Ip = 0x1001 },
            new(0x1002, 2, JitOperation.ConditionalJump, 32, default, default, Target: 0x1000)], registers, (ulong?)null);
        Check(plan.HasInternalBackedge && plan.Instructions[0].MergeCount == 2, "loop merges RBX and load destination");
        CheckFastOnly(plan);
        CheckFastOnly(CheckAddresses("loop invariant address remains known", [Load(0, 3) with { Ip = 0x1000 },
            new(0x1001, 2, JitOperation.ConditionalJump, 32, default, default, Target: 0x1000)], registers, address));
        CheckFastOnly(CheckAddresses("loop overwrite from invariant recovers", [Mov(3, 7) with { Ip = 0x1000 }, Load(0, 3) with { Ip = 0x1001 },
            Alu(JitOperation.Add, 3, 8) with { Ip = 0x1002 },
            new(0x1003, 2, JitOperation.Jump, 32, default, default, Target: 0x1000)], registers, registers[7]));
        CheckAddresses("stack backedge does not use first-iteration RSP", [Push(0) with { Ip = 0x1000 },
            new(0x1001, 2, JitOperation.Jump, 32, default, default, Target: 0x1000)], registers, (ulong?)null);
        CheckFastOnly(CheckAddresses("net-restored RSP remains conservatively unknown", [Alu(JitOperation.Sub, 4, 8) with { Ip = 0x1000 },
            Store(0, 4) with { Ip = 0x1001 }, Alu(JitOperation.Add, 4, 8) with { Ip = 0x1002 },
            new(0x1003, 2, JitOperation.Jump, 32, default, default, Target: 0x1000)], registers, (ulong?)null));
        CheckAddresses("middle backedge preserves prefix recipe", [Load(0, 3) with { Ip = 0x1000 },
            Load(1, 3) with { Ip = 0x1001 }, Alu(JitOperation.Add, 3, 8) with { Ip = 0x1002 },
            new(0x1003, 2, JitOperation.Jump, 32, default, default, Target: 0x1001)], registers, address, null);
        CheckAddresses("self backedge has no memory", [new(0x1000, 2, JitOperation.Jump, 32, default, default, Target: 0x1000)], registers);
        CheckAddresses("early taken branch is external, not a merge", [Load(0, 3) with { Ip = 0x1000 },
            new(0x1001, 2, JitOperation.ConditionalJump, 32, default, default, Target: 0x1000),
            Load(1, 3) with { Ip = 0x1003 }], registers, address, address);
    }

    private static void CheckEffects()
    {
        var source = new[] { Mov(3, 7), Load(0, 3), Alu(JitOperation.Add, 0, 1), Pop(4) };
        var plan = JitAddressPlan.Build(source);
        Check(plan.Definitions.Length == 5, "per-write definitions include both POP RSP effects");
        Span<ushort> state = stackalloc ushort[16];
        for (int i = 0; i < 16; ++i) state[i] = (ushort)(i + 1);
        int applied = 0;
        foreach (var step in plan.Instructions)
        {
            Check(step.BeforeDefinitions == applied, "before definition prefix");
            foreach (var definition in plan.Definitions.Slice(step.FirstDefinition, step.DefinitionCount))
            {
                Check(state[definition.Register] == definition.Before, "definition references exact old value");
                state[definition.Register] = definition.After; ++applied;
            }
            Check(step.AfterDefinitions == applied, "after definition prefix");
            var before = step.ExitState(JitPlanBoundary.StopOrAttention);
            var miss = step.ExitState(JitPlanBoundary.MemoryMiss);
            var rejected = step.ExitState(JitPlanBoundary.RejectedIssue);
            var after = step.ExitState(JitPlanBoundary.CompletedFallThrough);
            Check(before.DefinitionPrefix == step.BeforeDefinitions && before.IssuedDelta == 0 && before.CompletedDelta == 0, "poll before state");
            Check(miss.DefinitionPrefix == before.DefinitionPrefix && miss.IssuedDelta == 0, "guard miss before issue");
            Check(rejected.DefinitionPrefix == before.DefinitionPrefix && rejected.IssuedDelta == 1 && rejected.CompletedDelta == 0, "rejected issue exact boundary");
            Check(after.DefinitionPrefix == step.AfterDefinitions && after.IssuedDelta == 1 && after.CompletedDelta == 1, "completed exact boundary");
            Check(before.PreserveOperationLength && !after.PreserveOperationLength, "oplen boundary");
            Check(after.StateIsMaterialized && after.DescriptorsUnchanged, "materialized exit with invocation descriptors");
            Check((step.Effects & (JitPlanEffects.Poll | JitPlanEffects.AtomicIssue | JitPlanEffects.AtomicComplete | JitPlanEffects.MaterializedCommit)) ==
                (JitPlanEffects.Poll | JitPlanEffects.AtomicIssue | JitPlanEffects.AtomicComplete | JitPlanEffects.MaterializedCommit), "per-instruction ordering effects retained");
        }
        Check(plan.Instructions[1].FlagsBefore == plan.Instructions[1].FlagsAfter, "load preserves flags token");
        Check(plan.Instructions[2].FlagsAfter == 3 && plan.Instructions[2].FlagsEffect == JitPlanFlagsEffect.Arithmetic, "flags recipe before later poll");
        var bump = JitAddressPlan.Build([new(0, 1, JitOperation.Inc, 8, new(0, 0), default), new(1, 1, JitOperation.Dec, 8, new(0, 0), default)]);
        Check(bump.Instructions[0].FlagsEffect == JitPlanFlagsEffect.IncPreserveCarry &&
            bump.Instructions[1].FlagsEffect == JitPlanFlagsEffect.DecPreserveCarry, "INC/DEC retain CF dependency");
        var branch = JitAddressPlan.Build([new(0x1000, 2, JitOperation.ConditionalJump, 32, default, default, Target: 0x2000)]).Instructions[0];
        Check(branch.ExitState(JitPlanBoundary.CompletedFallThrough).Ip == 0x1002 &&
            branch.ExitState(JitPlanBoundary.CompletedTaken).Ip == 0x2000, "exact taken/fallthrough exit IPs");
    }

    private static void CheckBoundsAndAllocation()
    {
        Check(Marshal.SizeOf<JitPlanValue>() == 16 && Marshal.SizeOf<JitPlanInstruction>() == 64 &&
            Marshal.SizeOf<JitPlanDefinition>() == 8 && Marshal.SizeOf<JitPlanMemory>() == 32, "charged element layout");
        Check(Marshal.OffsetOf<JitPlanMemory>("Width").ToInt32() == 6 &&
            Marshal.OffsetOf<JitPlanMemory>("InstructionIndex").ToInt32() == 8 &&
            Marshal.OffsetOf<JitPlanMemory>("Displacement").ToInt32() == 16, "recipe consumer layout");
        Check(JitAddressPlan.MaximumCharge == 20280 && JitAddressPlan.MaximumEvaluationScratchBytes == 4096 &&
            JitAddressPlan.MaximumAffineVisits == 128 && JitAddressPlan.MaximumAffineDepth == 32, "exact advertised bounds");
        Check(ReferenceEquals(JitAddressPlan.Build([]), JitAddressPlan.Empty) && !JitAddressPlan.Empty.HasMemory &&
            JitAddressPlan.Empty.EvaluationCount == 0 && JitAddressPlan.Empty.FastCount == 0, "shared empty plan");
        JitInstruction[] source = new JitInstruction[32];
        for (int i = 0; i < source.Length; ++i)
            source[i] = new((ulong)i, 1, JitOperation.MemoryCmp, 64, default, new(i % 16, 0), SourceWidth: 64,
                Address: new(i % 16, (i + 1) % 16, i % 4, i * 67 - 987));
        var plan = JitAddressPlan.Build(source);
        Check(plan.Values.Length <= 512 && plan.Definitions.Length <= 80 && plan.Memory.Length == 32 &&
            plan.EvaluationCount == 0 && plan.FastCount == 32 && plan.Charge <= 20280, "maximum fragment bounds");
        CheckFastOnly(plan);
        CheckExactCharge(plan);
        JitInstruction[] stackLoop = new JitInstruction[32];
        for (int i = 0; i < 31; ++i) stackLoop[i] = Pop(i % 16) with { Ip = 0x1000UL + (uint)i };
        stackLoop[31] = new(0x101f, 2, JitOperation.Jump, 32, default, default, Target: 0x1000);
        var stackPlan = JitAddressPlan.Build(stackLoop);
        Check(stackPlan.Definitions.Length == 78 && stackPlan.Instructions[0].MergeCount == 16 &&
            stackPlan.EvaluationCount == 0 && stackPlan.FastCount == 0 && stackPlan.Charge <= 20280, "maximum stack definition/merge pressure");
        // A generic-only control still tests incremental compaction and the
        // full per-invocation scratch bound after the all-affine case above.
        source[0] = Mov(3, 3, 32);
        for (int i = 1; i < source.Length; ++i)
            source[i] = new((ulong)i, 1, JitOperation.MemoryCmp, 64, default, new(i % 16, 0), SourceWidth: 64,
                Address: new(3, (i + 1) % 16, i % 4, i * 67 - 987));
        var genericPlan = JitAddressPlan.Build(source);
        Check(genericPlan.FastCount == 0 && genericPlan.EvaluationCount > 0 &&
            genericPlan.EvaluationCount <= 512 && genericPlan.Charge <= 20280, "generic compaction bounds retained");
        CheckExactCharge(genericPlan);
        var registers = Registers();
        Span<ulong> scratch = stackalloc ulong[JitAddressPlan.MaximumValues];
        int evaluated = 0;
        _ = genericPlan.TryEvaluateAddress(0, registers, scratch, ref evaluated, out _);
        Check(evaluated == genericPlan.Memory[0].EvaluationEnd && evaluated < genericPlan.EvaluationCount, "first recipe does not evaluate tail");
        for (int i = 1; i < genericPlan.Memory.Length; ++i) _ = genericPlan.TryEvaluateAddress(i, registers, scratch, ref evaluated, out _);
        // Warm both evaluator paths before observing managed allocations.
        evaluated = 0;
        for (int i = 0; i < plan.Memory.Length; ++i) _ = plan.TryEvaluateAddress(i, registers, scratch, ref evaluated, out _);
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int repeat = 0; repeat < 512; ++repeat)
        {
            evaluated = 0;
            for (int i = 0; i < 32; ++i) _ = plan.TryEvaluateAddress(i, registers, scratch, ref evaluated, out _);
            for (int i = 0; i < genericPlan.Memory.Length; ++i) _ = genericPlan.TryEvaluateAddress(i, registers, scratch, ref evaluated, out _);
        }
        Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "evaluation allocates zero managed bytes after warmup");
        bool bounded = false;
        try { _ = JitAddressPlan.Build(new JitInstruction[33]); }
        catch (ArgumentOutOfRangeException) { bounded = true; }
        Check(bounded, "input cannot exceed static bound");
        var registerOnly = JitAddressPlan.Build([Imm(0, 1), Alu(JitOperation.Add, 0, 2)]);
        Check(!registerOnly.HasMemory && registerOnly.EvaluationCount == 0 && registerOnly.Instructions.Length == 2,
            "register-only IR has no preparation work");
    }
}
#endif
