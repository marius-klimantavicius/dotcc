using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // Same current emitter with the complete descriptor set is the architectural
    // control; frozen-r16 golden probes separately establish byte equivalence.
    // Compare complete Machine and RAM at every budget, cap and guard boundary.
    static void CheckAddressPlans(byte* machine)
    {
        const ulong stackPage = 0x700000, objectPage = 0x800000, oldPage = 0x900000;
        byte* ram = (byte*)NativeMemory.AlignedAlloc(3 * 4096, 4096);
        try
        {
            var block = RetainedHotBlock();
            var plan = JitAddressPlan.Build(block);
            Check(plan.HasMemory && plan.Memory.Length == 7, "hot plan seven guarded operations");
            var full = new JitDataDescriptor[3];
            for (int i = 0; i < 3; ++i) full[i] = new() { Page = i == 0 ? stackPage : i == 1 ? objectPage : oldPage,
                Host = (nint)(ram + i * 4096), Identity = (ulong)(ram + i * 4096), Generation = 1, ValidEnd = 4096, Permissions = 3 };
            void Setup()
            {
                Reset(machine); Write64(machine, 0, block[0].Ip);
                Write64(machine, 24 + 4 * 8, stackPage + 2048);
                Write64(machine, 24 + 3 * 8, oldPage);
                Write64(machine, 24 + 7 * 8, objectPage);
                Write64(machine, 24 + 6 * 8, 1);
                new Span<byte>(ram, 3 * 4096).Clear();
                Write64(ram + 4096, 8, 0x1234);
                Write64(ram + 4096, 16, 4);
            }
            var prepared = new JitDataDescriptor[4];
            int Prepare(bool legacy)
            {
                ReadOnlySpan<ulong> regs = new(machine + 24, 16);
                Span<ulong> values = stackalloc ulong[plan.EvaluationCount];
                int evaluated = 0, count = 0;
                for (int m = 0; m < plan.Memory.Length; ++m)
                {
                    ref readonly var mem = ref plan.Memory[m];
                    ulong address;
                    if (legacy)
                    {
                        var ins = block[mem.InstructionIndex]; var a = ins.Address;
                        address = unchecked((ulong)a.Displacement);
                        if (a.Base >= 0) address = unchecked(address + regs[a.Base]);
                        if (a.Index >= 0) address = unchecked(address + (regs[a.Index] << a.Scale));
                    }
                    else if (!plan.TryEvaluateAddress(m, regs, values, ref evaluated, out address)) continue;
                    ulong page = address & ~4095UL;
                    bool duplicate = false;
                    for (int j = 0; j < count; ++j) duplicate |= prepared[j].Page == page;
                    if (duplicate) continue;
                    for (int j = 0; j < full.Length; ++j)
                        if (full[j].Page == page) { prepared[count++] = full[j]; break; }
                    if (count == 4) break;
                }
                return count;
            }
            using var code = Create(block, 32);
            var call = (delegate* unmanaged<JitFrame*, int>)(void*)code.Entry;
            Setup(); int oldCount = Prepare(true);
            long issued = 0, completed = 0; int stop = 0;
            fixed (JitDataDescriptor* d = prepared)
            {
                JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                    Budget = 100, DataGeneration = 1, DataDescriptors = d, DataDescriptorCount = (uint)oldCount };
                call(&frame);
                Check(frame.Exit == JitExit.MemoryMiss && frame.Executed == 9, "entry-register planner predictably misses hot RBX copy after nine");
            }
            Setup(); int newCount = Prepare(false);
            Check(newCount == 2 && prepared[0].Page == stackPage && prepared[1].Page == objectPage,
                "value planner selects current stack and copied RBX object pages");
            for (int cap = 1; cap <= 32; ++cap)
            {
                using var bounded = Create(block, cap);
                var run = (delegate* unmanaged<JitFrame*, int>)(void*)bounded.Entry;
                for (ulong budget = 0; budget <= 21; ++budget)
                {
                    Setup(); issued = completed = 0;
                    JitFrame expected;
                    fixed (JitDataDescriptor* d = full)
                    {
                        expected = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                            Budget = budget, DataGeneration = 1, DataDescriptors = d, DataDescriptorCount = 3 };
                        run(&expected);
                    }
                    var expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    var expectedRam = new ReadOnlySpan<byte>(ram, 3 * 4096).ToArray();
                    long expectedIssued = issued, expectedCompleted = completed;
                    Setup(); int count = Prepare(false); issued = completed = 0;
                    fixed (JitDataDescriptor* d = prepared)
                    {
                        JitFrame actual = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                            Budget = budget, DataGeneration = 1, DataDescriptors = d, DataDescriptorCount = (uint)count };
                        run(&actual);
                        Check(actual.Exit == expected.Exit && actual.NextIp == expected.NextIp && actual.Executed == expected.Executed,
                            "planned hot exact cap/budget exit");
                    }
                    Check(issued == expectedIssued && completed == expectedCompleted, "planned hot exact issue/completion");
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), "planned hot complete Machine");
                    Check(new ReadOnlySpan<byte>(ram, 3 * 4096).SequenceEqual(expectedRam), "planned hot complete RAM");
                }
            }
            // Existing descriptor guard contract, now after each hot memory
            // prefix. All underlying RAM remains valid; no guest fault occurs.
            foreach (var boundary in new[] { (0, 2048), (1, 2040), (2, 2032), (9, 17), (12, 16), (15, 2024), (19, 2008) })
            {
                Setup(); int count = Prepare(false); int descriptor = boundary.Item1 is 9 or 12 ? 1 : 0;
                prepared[descriptor].ValidStart = (uint)boundary.Item2;
                issued = completed = 0;
                fixed (JitDataDescriptor* d = prepared)
                {
                    JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                        Budget = 100, DataGeneration = 1, DataDescriptors = d, DataDescriptorCount = (uint)count };
                    call(&frame);
                    Check(frame.Exit == JitExit.MemoryMiss && frame.Executed == (ulong)boundary.Item1 &&
                        issued == boundary.Item1 && completed == boundary.Item1 && frame.NextIp == block[boundary.Item1].Ip,
                        "hot guarded prefix exact before-instruction state");
                }
            }
            Console.WriteLine("hot predictable misses: old=1/1, planned=0/1; full cap/budget corpus checked");
        }
        finally { NativeMemory.AlignedFree(ram); }
    }

    static NativeCodeAllocation Create(JitInstruction[] block, int cap)
    {
        Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(block, cap), out var allocation, out var reason), reason);
        return allocation!;
    }
    private static JitInstruction[] RetainedHotBlock()
    {
        const ulong start = 0x110001391260;
        return [
            new(start, 1, JitOperation.Push, 64, default, new(5, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 1, 2, JitOperation.Push, 64, default, new(15, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 3, 1, JitOperation.Push, 64, default, new(3, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 4, 4, JitOperation.Sub, 64, new(4, 0), new(0, 16, true)),
            new(start + 8, 5, JitOperation.Lea, 64, new(5, 0), default, Address: new(4, -1, 0, 32)),
            new(start + 13, 3, JitOperation.Mov, 64, new(3, 0), new(7, 0)),
            new(start + 16, 3, JitOperation.Mov, 64, new(15, 0), new(2, 0)),
            new(start + 19, 2, JitOperation.Test, 32, new(6, 0), new(6, 0)),
            new(start + 21, 2, JitOperation.ConditionalJump, 64, default, default, Target: start + 75, Condition: 12),
            new(start + 23, 3, JitOperation.LoadMov, 32, new(7, 0), default, SourceWidth: 32, Address: new(3, -1, 0, 16)),
            new(start + 26, 2, JitOperation.Cmp, 32, new(6, 0), new(7, 0)),
            new(start + 28, 2, JitOperation.ConditionalJump, 64, default, default, Target: start + 75, Condition: 3),
            new(start + 30, 4, JitOperation.LoadMov, 64, new(0, 0), default, SourceWidth: 64, Address: new(3, -1, 0, 8)),
            new(start + 34, 2, JitOperation.Mov, 32, new(6, 0), new(6, 0)),
            new(start + 36, 3, JitOperation.Add, 64, new(6, 0), new(0, 0)),
            new(start + 39, 4, JitOperation.StoreMov, 64, default, new(6, 0), SourceWidth: 64, Address: new(5, -1, 0, -24)),
            new(start + 43, 2, JitOperation.Mov, 32, new(6, 0), new(7, 0)),
            new(start + 45, 3, JitOperation.Add, 64, new(6, 0), new(0, 0)),
            new(start + 48, 4, JitOperation.Lea, 64, new(7, 0), default, Address: new(5, -1, 0, -24)),
            new(start + 52, 5, JitOperation.Call, 64, default, default, Target: 0x110001390f10,
                SourceWidth: 64, Address: new(4, -1, 0, -8)) ];
    }
}
