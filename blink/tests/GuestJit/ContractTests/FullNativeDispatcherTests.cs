using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckFullNativeDispatcher(byte* machine)
    {
        ulong[] ips = [Ip, Ip + 1, Ip + 16, Ip + 17];
        JitInstruction[][] blocks = [
            [new(Ip, 1, JitOperation.Inc, 64, new(0, 0), default), new(Ip + 1, 5, JitOperation.Jump, 64, default, default, Target: Ip + 16)],
            [new(Ip + 1, 5, JitOperation.Jump, 64, default, default, Target: Ip + 16)],
            [new(Ip + 16, 1, JitOperation.Dec, 64, new(0, 0), default), new(Ip + 17, 5, JitOperation.Jump, 64, default, default, Target: Ip)],
            [new(Ip + 17, 5, JitOperation.Jump, 64, default, default, Target: Ip)] ];
        foreach (int cap in new[] { 1, 2, 3, 7, 31, 32 })
        {
            NativeCodeAllocation[] chained = new NativeCodeAllocation[4], ordinary = new NativeCodeAllocation[4];
            try
            {
                for (int i = 0; i < 4; ++i)
                {
                    Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit((ReadOnlySpan<JitInstruction>)blocks[i], cap, true), out chained[i], out string reason), reason);
                    ordinary[i] = Create(blocks[i], cap);
                }
                using var dispatcher = new FullNativeDispatcher(4); // All four ways of one bucket.
                foreach (int omitted in new[] { -1, 0, 1, 2, 3 })
                for (ulong budget = 0; budget <= 34; ++budget)
                foreach (string poll in new[] { "none", "stop", "attention" })
                {
                    Reset(machine); if (poll == "attention") machine[11] = 1;
                    long expectedIssued = 0, expectedCompleted = 0; int stopValue = poll == "stop" ? 1 : 0;
                    ulong expectedTotal = 0; uint expectedBlocks = 0;
                    JitExit expectedExit = JitExit.Continue;
                    while (expectedTotal < 32)
                    {
                        int index = Array.IndexOf(ips, *(ulong*)machine);
                        if (index < 0 || index == omitted) break;
                        JitFrame frame = new() { Machine = machine, Issued = &expectedIssued, Completed = &expectedCompleted,
                            Stop = &stopValue, Budget = budget, NativeLimit = (uint)(32 - expectedTotal) };
                        ((delegate* unmanaged<JitFrame*, int>)ordinary[index].Entry)(&frame);
                        ++expectedBlocks; expectedTotal += frame.Executed; expectedExit = frame.Exit;
                        if (frame.Exit != JitExit.Continue) break;
                    }
                    byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    Reset(machine); if (poll == "attention") machine[11] = 1;
                    dispatcher.Begin(42);
                    for (int i = 0; i < 4; ++i) if (i != omitted) dispatcher.Set(i, ips[i], chained[i].Entry);
                    long issued = 0, completed = 0;
                    JitFrame actual = new() { Machine = machine, Issued = &issued, Completed = &completed,
                        Stop = &stopValue, Budget = budget };
                    dispatcher.Run(&actual);
                    Check(issued == expectedIssued && completed == expectedCompleted && actual.Executed == expectedTotal,
                        "native chain exact aggregate issue/completion");
                    Check(actual.Exit == expectedExit && actual.NextIp == *(ulong*)machine && dispatcher.Blocks == expectedBlocks,
                        "native chain typed exits/four-way lookup/miss/polls");
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected),
                        "native chain complete Machine at every budget boundary");
                    if (omitted == -1 && budget >= 32 && poll == "none")
                        Check(dispatcher.Blocks > 1 && actual.Executed == 32, "positive distinct-block native tail jumps");
                }
                // A table publication from a prior invocation grants no
                // authority after Begin clears it, even for the same owner/IP.
                Reset(machine); dispatcher.Begin(42);
                long staleIssued = 0, staleCompleted = 0; int staleStop = 0;
                JitFrame stale = new() { Machine = machine, Issued = &staleIssued, Completed = &staleCompleted,
                    Stop = &staleStop, Budget = 100 };
                dispatcher.Run(&stale);
                Check(dispatcher.Blocks == 0 && staleIssued == 0 && staleCompleted == 0, "retired native table has no residual target");
            }
            finally { foreach (var allocation in chained) allocation?.Dispose(); foreach (var allocation in ordinary) allocation?.Dispose(); }
        }
        var memory = new JitInstruction(Ip, 4, JitOperation.LoadMov, 64, new(0, 0), default,
            SourceWidth: 64, Address: new(3, -1, 0, 0));
        bool rejected = false;
        try { X64BlockEmitter.Emit((ReadOnlySpan<JitInstruction>)[memory], 32, true); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "native table excludes unleased memory emitter targets");
    }
}
