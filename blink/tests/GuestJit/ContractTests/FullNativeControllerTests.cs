using System.Buffers.Binary;
using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void WriteControllerCode(ControllerFixture f, ulong pc, byte[] bytes)
    {
        f.DataScope(0, () =>
        {
            byte* destination = f.Program.LookupAddress2(f.Machine(), unchecked((long)pc), 0, 0);
            Check(destination != null, "native controller code mapping");
            bytes.CopyTo(new Span<byte>(destination, bytes.Length));
        });
    }
    static void CheckFullNativeController()
    {
        CheckNativeTicketMappingOverlap();
        CheckNativeSaturatedLoads();
        CheckNativeSimdFault();
        foreach (bool tail in new[] { false, true })
        {
            using var f = new ControllerFixture();
            ulong source = ControllerFixture.Base, target = tail ? source + 64 : source;
            f.Map(source); f.Fill(source);
            WriteControllerCode(f, target, [0x90, 0xf7, 0xf3]); // NOP; DIV EBX
            if (tail)
            {
                WriteControllerCode(f, source, [0x90, 0xeb, 61]); // NOP; JMP target
                Check(f.Acquire(target, 0, out var lease, out bool multiple, out int error) && error == 0 && multiple,
                    "native divide tail target compiles with precise authority");
                using (lease) lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease, multiple);
            }
            var m = f.Machine();
            *(ulong*)((byte*)m + 24 + 0 * 8) = 17;
            *(ulong*)((byte*)m + 24 + 2 * 8) = 0;
            *(ulong*)((byte*)m + 24 + 3 * 8) = 0;
            uint flags = m->flags;
            f.Counts(100);
            var signal = f.StepSynchronousFault(source);
            Check(signal.Signal == 8 && signal.Detail?.EndsWith("signal=8 code=1") == true &&
                m->trapno == 0 && m->ip == target + 1 && m->oplen == 0 && m->faultaddr == (long)(target + 1),
                $"real controller native divide: tail={tail} signal={signal.Signal} detail={signal.Detail} trap={m->trapno} ip={m->ip:x} oplen={m->oplen} address={m->faultaddr:x} expected={target+1:x}");
            long completed = tail ? 3 : 1;
            Check(f.Completed == (ulong)completed && f.Issued == (ulong)(completed + 1) && m->flags == flags &&
                *(ulong*)((byte*)m + 24 + 0 * 8) == 17 && *(ulong*)((byte*)m + 24 + 2 * 8) == 0 &&
                f.Statistics.InterpreterAttempts == 0 && f.Statistics.SemanticHelperInstructions == 0,
                "fault preserves arithmetic state and exactly one uncompleted issue without helper/dispatch");
            Check(!tail || f.Statistics.NativeTailJumps >= 1, "divide fault site may be inside an authenticated tail target");
            var store = f.Jit.Store.GetStatistics();
            Check(store.ActiveNativeEpochs == 0 && store.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,
                "synchronous native signal delivery leaves no native capabilities or physical pins");
        }
        using (var f = new ControllerFixture())
        {
            ulong code = ControllerFixture.Base, data = code + 4096;
            f.Map(code); f.Fill(code); f.Map(data);
            byte[] bytes = [0x48,0xbb,0,0,0,0,0,0,0,0, 0x31,0xc0, 0xb9,3,0,0,0,
                0x48,0x83,0x03,1, 0xff,0xc9, 0x75,0xf8];
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(2,8), data);
            WriteControllerCode(f, code, bytes);
            f.Counts(100);
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            Check(f.Step(code), "first native controller call returns");
            for (int attempt = 0; attempt < 8 && f.Machine()->ip != code + (uint)bytes.Length; ++attempt)
                Check(f.Continue(), "native MMU continuation returns");
            Check(f.Machine()->ip == code + (uint)bytes.Length && f.Completed == 12 && f.Issued == 12,
                "native MMU miss and internal memory loop preserve issuance/completion and final PC");
            byte* physical = (byte*)f.Physical(data);
            Check(*(ulong*)physical == 3 && f.Statistics.NativeMemoryInstructions == 3 &&
                f.Statistics.NativeMemoryAssists > 0 && f.Statistics.NativeEntries is >= 2 and <= 3 &&
                f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                "lazy memory is mapped by assists; native writes avoid per-operation managed boundaries");
            Console.WriteLine($"native-controller lazy-memory: entries={f.Statistics.NativeEntries} assists={f.Statistics.NativeMemoryAssists} completed={f.Completed}");
        }
    }
    static void CheckNativeSimdFault()
    {
        foreach (bool tail in new[] { false, true })
        {
            using var f = new ControllerFixture();
            ulong source = ControllerFixture.Base, target = tail ? source + 64 : source;
            f.Map(source); f.Fill(source);
            WriteControllerCode(f, target, [0x90, 0xf3, 0x0f, 0x5e, 0xc1]); // NOP; DIVSS XMM0,XMM1
            if (tail)
            {
                WriteControllerCode(f, source, [0x90, 0xeb, 61]);
                Check(f.Acquire(target, 0, out var lease, out bool multiple, out int error) && error == 0 && multiple,
                    "native SIMD fault tail target compiles");
                using (lease) lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease, multiple);
            }
            var m = f.Machine();
            byte* xmm = (byte*)m + 160;
            new Span<byte>(xmm, 32).Clear();
            BinaryPrimitives.WriteUInt32LittleEndian(new(xmm, 4), 0x3f800000); // 1 / +0
            uint initialMxcsr = 0x1f80u & ~(1u << 9); // Unmask guest divide-by-zero only.
            m->mxcsr = initialMxcsr;
            uint flags = m->flags;
            f.Counts(100);
            var signal = f.StepSynchronousFault(source);
            Check(signal.Signal == 8 && signal.Detail?.EndsWith("signal=8 code=3") == true &&
                m->trapno == 19 && m->ip == target + 1 && m->oplen == 0 && m->faultaddr == (long)(target + 1),
                $"native SIMD guest fault: tail={tail} signal={signal.Signal} detail={signal.Detail} trap={m->trapno} ip={m->ip:x}");
            ulong completed = tail ? 3UL : 1UL;
            Check(f.Completed == completed && f.Issued == completed + 1 && m->flags == flags &&
                m->mxcsr == (initialMxcsr | 4u) && BinaryPrimitives.ReadUInt32LittleEndian(new(xmm, 4)) == 0x3f800000 &&
                f.Statistics.InterpreterAttempts == 0 && f.Statistics.SemanticHelperInstructions == 0,
                "native SIMD fault commits sticky status, withholds destination and preserves the reserved issue");
            var store = f.Jit.Store.GetStatistics();
            Check(store.ActiveNativeEpochs == 0 && store.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,
                "native SIMD guest delivery follows capability cleanup");
        }
    }
    static void CheckNativeTicketMappingOverlap()
    {
        using var f = new ControllerFixture();
        ulong code = ControllerFixture.Base, destination = code + 256;
        f.Map(code); f.Fill(code);
        byte[] bytes = [0x48,0xbb,0,0,0,0,0,0,0,0, 0xb8,0x78,0x56,0x34,0x12, 0x48,0x89,0x03, 0xeb,0xfe];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(2,8), destination);
        WriteControllerCode(f, code, bytes);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Thread? mapping = null; Exception? mappingError = null;
        int callbacks = 0;
        f.NativeReturned = pointer =>
        {
            var frame = (FullBlockFrame*)pointer;
            if (frame->MemoryFlags != FullMemoryFlags.BarrierConsumed || callbacks != 0) return;
            ++callbacks;
            mapping = new Thread(() =>
            {
                bool ownsMapping = false;
                try
                {
                    lock (f.Jit.Authority.Gate)
                    {
                        f.Jit.Authority.BeginMappingMutation(); ownsMapping = true;
                        f.Jit.Authority.Advance(); entered.Set();
                        f.Jit.Authority.WaitNativeFrames();
                    }
                    if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("mapping overlap release");
                }
                catch (Exception error) { mappingError = error; }
                finally { if (ownsMapping) f.Jit.Authority.EndMappingMutation(); }
            });
            mapping.Start();
            Check(entered.Wait(TimeSpan.FromSeconds(5)), "foreign mapping scope begins while real native ticket remains enrolled");
        };
        try
        {
            f.Counts(100);
            Check(f.Step(code), "watched native store initial cap");
            for (int attempt = 0; attempt < 8 && f.Machine()->ip != code + 18; ++attempt)
                Check(f.Continue(), "watched native store continuation");
            var s = f.Jit.Store.GetStatistics();
            Check(callbacks == 1 && f.Completed == 3 && f.Issued == 3 && f.Machine()->ip == code + 18 &&
                s.ActiveNativeEpochs == 0 && s.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,
                "ticket finalization releases its own ownership and all capabilities with foreign mapping scope open");
            bool admitted = f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch);
            if (admitted) f.Jit.Authority.ExitNative();
            Check(!admitted, "foreign scope remains active after ticket release");
        }
        finally { f.NativeReturned = null; release.Set(); if (mapping != null) Check(mapping.Join(TimeSpan.FromSeconds(5)), "mapping overlap thread joins"); }
        Check(mappingError == null, "synthetic foreign mapping admission and real native store complete without release failure");
        Check(f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch), "ordinary native admission recovers after both scopes end");
        f.Jit.Authority.ExitNative();
    }
    static void CheckNativeSaturatedLoads()
    {
        foreach (bool cells in new[] { false, true })
        {
            using var f = new ControllerFixture();
            ulong code = ControllerFixture.Base, data = code + 4096;
            f.Map(code); f.Fill(code); f.Map(data); f.Fill(data);
            byte[] bytes = [0x48,0xbb,0,0,0,0,0,0,0,0, 0x48,0x8b,0x03, 0xeb,0xfe];
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(2,8), data);
            WriteControllerCode(f, code, bytes);
            lock (f.Jit.Authority.Gate)
            {
                for (int i = 0; i < 100000 && !f.Jit.Authority.Saturated; ++i)
                {
                    nint page = (nint)(0x100000000L + (long)i * 4096);
                    if (cells) f.Jit.Authority.TryGetNativeDataPermission(page, false, out _);
                    else f.Jit.Authority.Expose(f.WorkerId(0), page);
                }
                f.Jit.Authority.EndBorrow(f.WorkerId(0));
                Check(f.Jit.Authority.Saturated, "bounded authority pool reaches a permanent checked state");
            }
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            f.Counts(100); Check(f.Step(code), "saturated native first instruction");
            for (int attempt = 0; attempt < 8 && f.Machine()->ip != code + 13; ++attempt)
                Check(f.Continue(), "saturated checked memory continuation");
            var s = f.Jit.Store.GetStatistics();
            Check(f.Machine()->ip == code + 13 && f.Completed == 2 && f.Issued == 2 &&
                f.Statistics.NativeMemoryInstructions == 1 && f.Statistics.SemanticHelperInstructions == 0 &&
                s.ActiveNativeEpochs == 0 && s.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,
                "saturated cell/node state executes native load once through a checked ticket without permanent retry");
        }
    }
}
