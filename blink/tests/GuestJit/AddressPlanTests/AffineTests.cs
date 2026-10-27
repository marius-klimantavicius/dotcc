#if ADDRESS_PLAN_TESTS
using Managed.Emulation.Execution;

internal static partial class AddressPlanTests
{
    // Unoptimized control: evaluate the full retained value IR in ID order,
    // ignoring RecipeKind, affine fields and the compact preparation program.
    // CheckAddresses also requires the separately specified expected addresses.
    private static bool TryEvaluateFullIr(JitAddressPlan plan, int memoryIndex,
        ReadOnlySpan<ulong> registers, out ulong address)
    {
        var values = plan.Values;
        ushort root = plan.Memory[memoryIndex].Value;
        address = 0;
        if (values[root].Known == 0) return false;
        Span<ulong> results = stackalloc ulong[JitAddressPlan.MaximumValues];
        results[0] = 0;
        unchecked
        {
            for (int id = 1; id <= root; ++id)
            {
                var node = values[id];
                if (node.Known == 0) continue;
                ulong left = results[node.Left], right = results[node.Right];
                ulong mask = ulong.MaxValue >> (64 - node.Width);
                results[id] = node.Kind switch
                {
                    JitPlanValueKind.EntryRegister => registers[(int)node.Immediate],
                    JitPlanValueKind.Constant => node.Immediate,
                    JitPlanValueKind.Add => (left + right) & mask,
                    JitPlanValueKind.Sub => (left - right) & mask,
                    JitPlanValueKind.And => (left & right) & mask,
                    JitPlanValueKind.Or => (left | right) & mask,
                    JitPlanValueKind.Xor => (left ^ right) & mask,
                    JitPlanValueKind.Extract => (left >> node.Shift) & mask,
                    JitPlanValueKind.Insert => (left & ~(mask << node.Shift)) | ((right & mask) << node.Shift),
                    JitPlanValueKind.SignExtend => (left & (1UL << (node.Width - 1))) == 0 ? left & mask : left | ~mask,
                    JitPlanValueKind.ShiftLeft => left << node.Shift,
                    _ => throw new InvalidOperationException("Unknown value entered generic control")
                };
            }
        }
        address = results[root];
        return true;
    }

    private static void CheckExactCharge(JitAddressPlan plan)
    {
        static long Array(int count, int size) => count == 0 ? 0 : 24L + (long)count * size;
        long expected = 64 + Array(plan.Values.Length, 16) + Array(plan.Instructions.Length, 64) +
            Array(plan.Definitions.Length, 8) + Array(plan.Memory.Length, 32) + Array(plan.EvaluationCount, 16);
        Check(plan.Charge == expected, "charge includes full IR, 32-byte memory and generic evaluation");
    }

    // Allows unknown recipes but requires every known address to be direct.
    private static void CheckFastOnly(JitAddressPlan plan)
    {
        int knownCount = 0;
        foreach (var recipe in plan.Memory)
        {
            bool known = plan.Values[recipe.Value].Known != 0;
            Check(recipe.RecipeKind == (known ? JitPlanRecipeKind.Affine : JitPlanRecipeKind.Generic),
                "only proved known recipes become affine");
            Check(recipe.EvaluationValue == 0 && recipe.EvaluationEnd == 0, "no generic slots for affine/unknown recipe");
            if (known) ++knownCount;
        }
        Check(plan.FastCount == knownCount && plan.EvaluationCount == 0, "fast count and empty generic program");
        CheckExactCharge(plan);
    }

    private static JitAddressPlan CheckGeneric(string label, JitInstruction[] source, ulong[] registers, ulong expected)
    {
        var plan = CheckAddresses(label, source, registers, expected);
        Check(plan.FastCount == 0 && plan.Memory[0].RecipeKind == JitPlanRecipeKind.Generic &&
            plan.EvaluationCount > 0, label + " requires generic recipe");
        CheckExactCharge(plan);
        return plan;
    }

    private static void CheckAffineRecipes()
    {
        var registers = Registers();
        registers[3] = ulong.MaxValue - 15; registers[12] = 8;
        for (int scale = 0; scale < 4; ++scale)
        {
            var plan = CheckAddresses("direct two-entry scale " + scale,
                [Lea(0, 3, 12, scale, -7), Load(1, 0, -9)], registers,
                unchecked(registers[3] + (registers[12] << scale) - 16));
            CheckFastOnly(plan);
            Check(plan.Memory[0].Base == 3 && plan.Memory[0].Index == 12 && plan.Memory[0].Scale == scale &&
                plan.Memory[0].Displacement == unchecked(0UL - 16), "direct recipe fields use entry registers");
            // A scaled first term must swap with the later unscaled term.
            CheckFastOnly(CheckAddresses("scaled first term " + scale,
                [Lea(0, -1, 3, scale, long.MinValue), Lea(0, 0, 12, displacement: -17), Load(1, 0)], registers,
                unchecked((registers[3] << scale) + registers[12] + (ulong)long.MinValue - 17)));
        }

        var prologue = CheckAddresses("RSP/RBP deltas and RBX copy",
            [Push(5), Alu(JitOperation.Sub, 4, 32), Lea(5, 4, displacement: 16), Mov(3, 7),
                Store(0, 5, -24), Load(1, 3, 16)], registers,
            registers[4] - 8, registers[4] - 48, registers[7] + 16);
        CheckFastOnly(prologue);
        Check(prologue.Memory[1].Base == 4 && prologue.Memory[2].Base == 7 &&
            prologue.Definitions.Length == 5, "full reaching definitions survive direct compaction");

        foreach (long displacement in new[] { long.MinValue, -1L, 0L, 1L, long.MaxValue })
            CheckFastOnly(CheckAddresses("signed displacement " + displacement, [Load(1, 3, displacement)], registers,
                unchecked(registers[3] + (ulong)displacement)));
        CheckFastOnly(CheckAddresses("modulo subtraction of negative constant",
            [Alu(JitOperation.Sub, 3, unchecked(0UL - 17)), Load(1, 3, -2)], registers, unchecked(registers[3] + 15)));
        var constant = CheckAddresses("constant-only modulo address", [Imm(0, ulong.MaxValue), Load(1, 0, 1)], registers, 0);
        CheckFastOnly(constant);
        Check(constant.Memory[0].Base == -1 && constant.Memory[0].Index == -1, "constant affine has no entry terms");
        CheckFastOnly(CheckAddresses("RIP address wraps", [Load(1, 3) with
        { Ip = ulong.MaxValue - 3, Length = 7, Address = new(-1, -1, 0, -5, true) }], registers, ulong.MaxValue - 1));

        var doubled = CheckAddresses("same entry terms combine to power of two", [Lea(0, 3, 3), Load(1, 0)], registers,
            unchecked(registers[3] * 2));
        CheckFastOnly(doubled);
        Check(doubled.Memory[0].Base == -1 && doubled.Memory[0].Index == 3 && doubled.Memory[0].Scale == 1,
            "combined coefficient encoded once");
        CheckGeneric("non-power-of-two coefficient", [Lea(0, 3, 3, 1), Load(1, 0)], registers, unchecked(registers[3] * 3));
        CheckGeneric("two scaled terms need generic", [Lea(0, -1, 3, 1), Lea(0, 0, 12, 2), Load(1, 0)], registers,
            unchecked(registers[3] * 2 + registers[12] * 4));
        CheckGeneric("three distinct entry terms", [Lea(0, 3, 12, 2),
            new(0, 1, JitOperation.Add, 64, new(0, 0), new(7, 0)), Load(1, 0)], registers,
            unchecked(registers[3] + registers[12] * 4 + registers[7]));
        CheckGeneric("negative entry coefficient", [Imm(0, 0),
            new(0, 1, JitOperation.Sub, 64, new(0, 0), new(3, 0)), Load(1, 0)], registers, unchecked(0UL - registers[3]));

        registers[0] = 0x12345678ffffffff;
        CheckGeneric("truncation before 64-bit addition", [Mov(0, 0, 32), Alu(JitOperation.Add, 0, 1), Load(1, 0)], registers, 0x100000000);
        CheckGeneric("32-bit addition before displacement", [Alu(JitOperation.Add, 0, 1, 32), Load(1, 0, 1)], registers, 1);
        CheckGeneric("16-bit insertion retains upper entry bits", [Imm(0, 0xabcd, 16), Load(1, 0)], registers, 0x12345678ffffabcd);
        CheckGeneric("high-byte insertion", [new(0, 1, JitOperation.Mov, 8, new(0, 0, ByteOffset: 1), new(0, 0x12, true)),
            Load(1, 0)], registers, 0x12345678ffff12ff);
        CheckGeneric("sign extension remains generic", [new(0, 1, JitOperation.Movsx, 64, new(0, 0), new(0, 0), SourceWidth: 32),
            Load(1, 0)], registers, ulong.MaxValue);

        var unknown = CheckAddresses("unknown pointer never gets default affine recipe", [Load(0, 3), Load(1, 0)], registers,
            registers[3], null);
        CheckFastOnly(unknown);
        Check(unknown.HasMemory && unknown.Memory[1].RecipeKind == JitPlanRecipeKind.Generic &&
            unknown.Memory[1].EvaluationValue == 0, "unknown memory remains classified and unevaluated");

        // Stack depth and repeated-DAG work have separate, falsifiable limits.
        foreach (int changes in new[] { 30, 31 })
        {
            var source = Enumerable.Repeat(Alu(JitOperation.Sub, 4, 1), changes).Append(Store(0, 4, 1)).ToArray();
            var plan = CheckAddresses("affine depth pressure " + changes, source, registers, registers[4] - (uint)changes + 1);
            Check(plan.FastCount == (changes == 30 ? 1 : 0), "depth bound conservatively falls back");
            Check((plan.EvaluationCount == 0) == (changes == 30), "depth fallback retains generic dependencies");
        }
        foreach (int doubles in new[] { 6, 7 })
        {
            var source = new[] { Mov(0, 3) }.Concat(Enumerable.Repeat(Lea(0, 0, 0), doubles)).Append(Load(1, 0)).ToArray();
            var plan = CheckAddresses("affine repeated-DAG visits " + doubles, source, registers, unchecked(registers[3] << doubles));
            Check(plan.FastCount == (doubles == 6 ? 1 : 0), "visit bound conservatively falls back");
        }
        var shifts = Enumerable.Repeat(Lea(0, -1, 0, 3), 15).Prepend(Mov(0, 3)).Append(Load(1, 0)).ToArray();
        CheckFastOnly(CheckAddresses("combined shift 45", shifts, registers, unchecked(registers[3] << 45)));
        var tooManyShifts = Enumerable.Repeat(Lea(0, -1, 0, 3), 22).Prepend(Mov(0, 3)).Append(Load(1, 0)).ToArray();
        CheckGeneric("combined shift 66 does not mask to two", tooManyShifts, registers, 0);

        var mixed = CheckAddresses("interleaved affine and generic recipes",
            [Load(15, 4), Mov(3, 3, 32), Load(14, 3), Load(13, 4, 8), Load(12, 3, 16)], registers,
            registers[4], (uint)registers[3], registers[4] + 8, (uint)registers[3] + 16UL);
        Check(mixed.FastCount == 2 && mixed.EvaluationCount > 0, "mixed program retains only generic dependencies");
        CheckExactCharge(mixed);
        Span<ulong> scratch = stackalloc ulong[mixed.EvaluationCount];
        int evaluated = 0;
        _ = mixed.TryEvaluateAddress(0, registers, scratch, ref evaluated, out _);
        Check(evaluated == 0, "fast recipe before generic does no node evaluation");
        _ = mixed.TryEvaluateAddress(1, registers, scratch, ref evaluated, out _);
        int before = evaluated;
        _ = mixed.TryEvaluateAddress(2, registers, scratch, ref evaluated, out _);
        Check(evaluated == before, "fast recipe after generic preserves evaluation cursor");
        _ = mixed.TryEvaluateAddress(3, registers, scratch, ref evaluated, out _);
        Check(evaluated == mixed.EvaluationCount, "generic tail still completes compact program");

        Span<ulong> noScratch = [];
        evaluated = 0;
        Check(prologue.TryEvaluateAddress(0, registers, noScratch, ref evaluated, out _) && evaluated == 0,
            "all-affine plan succeeds with zero evaluation scratch");
    }
}
#endif
