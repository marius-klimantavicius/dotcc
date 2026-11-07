using Managed.Emulation.Execution;
using EnvironmentFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // One entry in the existing whole R175 family gate; no separate test queue.
    static void CheckEnvironmentApprovedRequestControllers()
    {
        CheckEnvironmentApprovedAttentionControllers();
        CheckEnvironmentPreSampleInvalidationControllers();
    }

    // Grouped attention regression called by the same whole family entry.
    // Attention is injected at the actual Request RET, after native Poll/Issue
    // and before the controller releases its original capabilities or samples.
    static void CheckEnvironmentApprovedAttentionControllers()
    {
        foreach (var row in new[]
        {
            new EnvironmentCase([0x0f, 0x31], FullProcessorEnvironmentOperation.Rdtsc, 64, 0),
            new EnvironmentCase([0x0f, 0x01, 0xf9], FullProcessorEnvironmentOperation.Rdtscp, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf0], FullProcessorEnvironmentOperation.Rdrand, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf8], FullProcessorEnvironmentOperation.Rdseed, 64, 0)
        })
        {
            using var f = new EnvironmentFixture();
            var clock = new EnvironmentClock(); var entropy = new EnvironmentEntropy();
            EnvironmentBind(clock, entropy);
            try
            {
                const ulong pc = EnvironmentFixture.Base;
                f.Map(pc); f.Fill(pc);
                f.DataScope(0, () =>
                {
                    byte* code = f.Program.LookupAddress2(f.Machine(), (long)pc, 0, 0);
                    Check(code != null, "R177 attention regression has real mapped code");
                    row.Bytes.CopyTo(new Span<byte>(code, row.Bytes.Length));
                    code[row.Bytes.Length] = 0x90; // must remain unissued at this boundary
                });
                EnvironmentSeed((byte*)f.Machine(),realWorker:true);
                f.Machine()->attention = false;
                uint flags = f.Machine()->flags;
                ref var global = ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                global.state = 3; global.count = 0;
                int requests = 0, samples = 0, commits = 0;
                int clocksBefore = clock.Calls, entropyBefore = entropy.Calls;
                nint union = 0;
                f.NativeReturned = pointer =>
                {
                    var frame = (FullBlockFrame*)pointer;
                    if (frame->Exit == FullBlockExit.NeedEnvironment)
                    {
                        ++requests;
                        Check(requests == 1 && samples == 0 && f.Issued == 1 && f.Completed == 0 &&
                            frame->FirstPreissued == 0 && frame->Executed == 0 &&
                            frame->NextIp == pc && f.Machine()->ip == pc &&
                            FullProcessorEnvironmentProtocol.IsEnvironment(frame),
                            "R177 attention injected only after the exact first ENV Poll/Issue Request RET");
                        var request = (FullEnvironmentDataDescriptor*)frame->StackMemory;
                        Check(request->State == FullEnvironmentDataState.Request && request->LeaseCookie == 0 &&
                            request->Data0 == 0 && request->Data1 == 0 && request->RngState == null && request->RngCount == null,
                            "R177 attention injection precedes every service sample and RNG pointer publication");
                        union = (nint)frame->StackMemory;
                        f.Machine()->attention = true;
                    }
                    else if (frame->Exit == FullBlockExit.Continue && frame->Executed == 1)
                        ++commits; // Observe only; no Actor/signal/admission work under the RNG lease.
                };
                FullProcessorEnvironmentProtocol.TestAfterSample = _ => ++samples;
                // Budget allows a second instruction, so the boundary is proved
                // by the approved ENV transaction and pending attention, not fuel.
                f.Counts(8);
                Check(f.Step(pc), "R177 actual controller completes the approved ENV attention boundary");
                Check(requests == 1 && samples == 1 && commits == 1 && f.Issued == 1 && f.Completed == 1 &&
                    (int)f.Machine()->attention != 0 && f.Machine()->ip == pc + (uint)row.Bytes.Length &&
                    f.Machine()->oplen == 0,
                    "R177 all four ENV operations defer post-Request attention through exactly one issue/sample/commit");

                bool isClock = row.Op is FullProcessorEnvironmentOperation.Rdtsc or FullProcessorEnvironmentOperation.Rdtscp;
                Check(clock.Calls - clocksBefore == (isClock ? 1 : 0) && entropy.Calls - entropyBefore == (isClock ? 0 : 1),
                    "R177 approved attention transaction performs exactly its one clock or entropy data request");
                if (isClock)
                {
                    ulong want = EnvironmentTimestamp(12, 345678900);
                    Check(EnvironmentRead((byte*)f.Machine(), 0) == (uint)want &&
                        EnvironmentRead((byte*)f.Machine(), 2) == (uint)(want >> 32) &&
                        (row.Op != FullProcessorEnvironmentOperation.Rdtscp || EnvironmentRead((byte*)f.Machine(), 1) == 0) &&
                        f.Machine()->flags == flags,
                        "R177 pending attention retains exact emitted timestamp/register effects");
                }
                else
                {
                    ulong want = entropy.Word;
                    if (row.Op == FullProcessorEnvironmentOperation.Rdrand)
                    {
                        want = EnvironmentVigna(entropy.Word, out ulong state);
                        Check(global.count == 1 && global.state == state,
                            "R177 pending attention commits the approved actual RNG count/state once");
                    }
                    Check(EnvironmentRead((byte*)f.Machine(), 0) == want && f.Machine()->flags == (flags | 1u),
                        "R177 pending attention retains exact emitted entropy destination/CF effects");
                }
                Check(union != 0, "R177 Request observed the actual worker union");
                var dense = (FullStackMemoryDescriptor*)union;
                Check(dense->Version == FullStackMemoryDescriptor.CurrentVersion && dense->Count == 0,
                    "R177 deferred-attention commit acknowledges ENV back to the ordinary idle union");
                for (int offset = 4; offset < 128; ++offset)
                    Check(((byte*)union)[offset] == 0, "R177 acknowledged attention transaction scrubs every remaining union byte");
                var store = f.Jit.Store.GetStatistics(); var statistics = f.Statistics;
                Check(store.ActiveNativeEpochs == 0 && store.ActiveManagedLeases == 0 && statistics.ActiveDataBorrows == 0 &&
                    f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == 0 && !f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)) &&
                    !f.MappingBorrow(0) && statistics.SemanticHelperInstructions == 0 && statistics.InterpreterAttempts == 0,
                    "R177 attention boundary returns with no native/store/fetch/semantic/mapping capabilities or fallback");
                bool acquired = FullProcessorEnvironmentLease.TryAcquire(f.Program, f.Jit.Authority, f.WorkerId(0), true, out var released);
                try
                {
                    Check(acquired && released != null && !released.Sampled,
                        "R177 actual RNG/service ownership is available again after the attention boundary");
                }
                finally { released?.Dispose(); }
                Check(f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == 0 &&
                    !f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)) && f.Issued == 1 && f.Completed == 1 && samples == 1 &&
                    (int)f.Machine()->attention != 0 && f.Machine()->ip == pc + (uint)row.Bytes.Length,
                    "R177 capability check neither issues, samples, consumes attention nor executes the next instruction");
            }
            finally
            {
                f.NativeReturned = null;
                FullProcessorEnvironmentProtocol.TestAfterSample = null;
                f.RestoreEnvironment();
            }
        }
    }

    // Actual Request-RET interleavings. The callback waits ONLY for writer
    // publication; the independent writer drains native/store capabilities.
    // No service/RNG ownership exists on this unsampled request path.
    static void CheckEnvironmentPreSampleInvalidationControllers()
    {
        foreach (string schedule in new[] { "code-change", "mapping-revoke", "same-bytes-recompile" })
        foreach (var row in new[]
        {
            new EnvironmentCase([0x0f, 0x31], FullProcessorEnvironmentOperation.Rdtsc, 64, 0),
            new EnvironmentCase([0x0f, 0x01, 0xf9], FullProcessorEnvironmentOperation.Rdtscp, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf0], FullProcessorEnvironmentOperation.Rdrand, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf8], FullProcessorEnvironmentOperation.Rdseed, 64, 0)
        })
        {
            using var f = new EnvironmentFixture();
            var clock = new EnvironmentClock(); var entropy = new EnvironmentEntropy();
            EnvironmentBind(clock, entropy);
            using var published = new ManualResetEventSlim();
            using var finished = new ManualResetEventSlim();
            Thread? writer = null; Exception? writerFailure = null;
            try
            {
                const ulong pc = EnvironmentFixture.Base;
                f.Map(pc); f.Fill(pc); nint codePointer = 0;
                f.DataScope(0, () =>
                {
                    byte* code = f.Program.LookupAddress2(f.Machine(), (long)pc, 0, 0);
                    Check(code != null, "R177 invalidation schedule has actual mapped code");
                    row.Bytes.CopyTo(new Span<byte>(code, row.Bytes.Length));
                    code[row.Bytes.Length] = 0x90; codePointer = (nint)code;
                });
                EnvironmentSeed((byte*)f.Machine(),realWorker:true); f.Machine()->attention = false;
                ulong oldRax = EnvironmentRead((byte*)f.Machine(), 0); uint flags = f.Machine()->flags;
                ref var global = ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                global.state = 3; global.count = 0;
                int requests = 0, samples = 0;
                int clocksBefore = clock.Calls, entropyBefore = entropy.Calls;
                TranslationToken original = default, replacement = default;
                f.NativeReturned = pointer =>
                {
                    var frame = (FullBlockFrame*)pointer;
                    if (frame->Exit != FullBlockExit.NeedEnvironment) return;
                    ++requests;
                    if (requests != 1) return;
                    Check(samples == 0 && f.Issued == 1 && f.Completed == 0 && frame->FirstPreissued == 0,
                        "R177 concurrent invalidation starts with one approved unsampled Request");
                    original = new(checked((int)frame->Header->Slot), frame->Header->BodyGeneration);
                    ulong oldRuntime = frame->ObservedGlobalEpoch, oldCode = frame->ObservedCodeAuthorityEpoch;
                    f.Machine()->attention = true; // stop only after the same approved operation commits
                    writer = new Thread(() =>
                    {
                        bool attached=false;
                        try
                        {
                            using var binding = f.Program.__DotCcEnter();
                            f.AttachEnvironmentWriterMemory();attached=true;
                            lock (f.Jit.Authority.Gate)
                            {
                                bool mapping = schedule == "mapping-revoke";
                                if (mapping) f.Jit.Authority.BeginMappingMutation();
                                else f.Jit.Authority.BeginHostWriter();
                                try
                                {
                                    f.Jit.Store.MarkAuthorityChanged(pc, 4096);
                                    Check(f.Jit.Authority.Epoch != oldRuntime && f.Jit.Authority.CodeEpoch != oldCode,
                                        "R177 actual owner runtime/code epochs witness the Request invalidation");
                                    published.Set();
                                    // Independent thread only: the callback must
                                    // return before this native drain can finish.
                                    f.Jit.Authority.WaitNativeFrames();
                                    Check(samples == 0 && clock.Calls == clocksBefore && entropy.Calls == entropyBefore,
                                        "R177 invalidated original Request has no sample/data call before writer effects");
                                    if (schedule == "code-change")
                                        new Span<byte>((void*)codePointer, row.Bytes.Length).Fill(0x90);
                                }
                                finally
                                {
                                    if (mapping) f.Jit.Authority.EndMappingMutation();
                                    else f.Jit.Authority.EndHostWriter();
                                }
                                // Keep Gate so the original worker cannot
                                // reacquire yet, but end writer admission before
                                // asking another worker to acquire a read scope.
                                    if (schedule == "same-bytes-recompile")
                                    {
                                        Check(SpinWait.SpinUntil(() =>
                                        {
                                            var active = f.Jit.Store.GetStatistics();
                                            return active.ActiveNativeEpochs == 0 && active.ActiveManagedLeases == 0;
                                        }, TimeSpan.FromSeconds(5)), "R177 initial store capabilities drain before real cache flush");
                                        Check(f.Jit.Store.TryFlush(out _), "R177 unchanged-byte owner cache flush succeeds");
                                        Check(f.Acquire(pc, 1, out var recompiled, out _, out int error) && error == 0,
                                            "R177 other worker recompiles unchanged bytes before pending request revalidation");
                                        using (recompiled) replacement = recompiled.Token;
                                        Check(replacement != original,
                                            "R177 unchanged-byte recompilation has an actual new owner generation");
                                    }
                            }
                        }
                        catch (Exception error) { writerFailure = error; }
                        finally
                        {
                            try
                            {
                                if(attached)
                                {
                                    using var binding=f.Program.__DotCcEnter();
                                    f.DetachEnvironmentWriterMemory();
                                }
                            }
                            catch(Exception error){writerFailure??=error;}
                            finally{finished.Set();}
                        }
                    });
                    writer.Start();
                    Check(published.Wait(TimeSpan.FromSeconds(5)) && !finished.IsSet,
                        "R177 writer publishes invalidation while initial Request remains actually native-enrolled");
                };
                FullProcessorEnvironmentProtocol.TestAfterSample = _ => ++samples;
                f.Counts(8);
                Check(f.Step(pc), "R177 actual controller refetches invalidated Request with its same approved issue");
                bool joined=writer != null && writer.Join(TimeSpan.FromSeconds(5));
                if(writerFailure!=null)throw new InvalidOperationException("R177 writer "+schedule+" "+row.Op,writerFailure);
                Check(joined,"R177 independent pre-service writer finishes without callback/native drain deadlock");
                bool changed = schedule == "code-change";
                Check(f.Issued == 1 && f.Completed == 1 && samples == (changed ? 0 : 1) && requests == (changed ? 1 : 2) &&
                    f.Machine()->ip == pc + (changed ? 1u : (uint)row.Bytes.Length) && (int)f.Machine()->attention != 0 &&
                    !f.Jit.Authority.SemanticQuarantined,
                    "R177 code/mapping/new-generation invalidation is an unsampled retry, never host terminal or second issue");
                if (changed)
                    Check(clock.Calls == clocksBefore && entropy.Calls == entropyBefore && global.count == 0 && global.state == 3 &&
                        EnvironmentRead((byte*)f.Machine(), 0) == oldRax && f.Machine()->flags == flags,
                        "R177 replacement NOP consumes old allowance with zero ENV data calls or generator/destination effects");
                else
                {
                    bool isClock = row.Op is FullProcessorEnvironmentOperation.Rdtsc or FullProcessorEnvironmentOperation.Rdtscp;
                    Check(clock.Calls - clocksBefore == (isClock ? 1 : 0) && entropy.Calls - entropyBefore == (isClock ? 0 : 1) &&
                        global.count == (row.Op == FullProcessorEnvironmentOperation.Rdrand ? 1u : 0u),
                        "R177 fresh authenticated unchanged ENV request samples and commits once after normal invalidation");
                    if (schedule == "same-bytes-recompile")
                        Check(f.ResumeToken() == replacement && replacement != original,
                            "R177 retained resume names the new generation, never dereferences the retired request header");
                }
                var store = f.Jit.Store.GetStatistics(); var statistics = f.Statistics;
                Check(store.ActiveNativeEpochs == 0 && store.ActiveManagedLeases == 0 && statistics.ActiveDataBorrows == 0 &&
                    !f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)) && !f.Jit.Authority.SemanticAdmitted(f.WorkerId(1)) &&
                    f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == 0 && f.Jit.Authority.CpuSemanticDepth(f.WorkerId(1)) == 0 &&
                    statistics.SemanticHelperInstructions == 0 && statistics.InterpreterAttempts == 0,
                    "R177 invalidation retry leaves no code/data/semantic capabilities or helper/interpreter fallback");
            }
            finally
            {
                f.NativeReturned = null; FullProcessorEnvironmentProtocol.TestAfterSample = null;
                if (writer != null && !writer.Join(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("R177 pre-service invalidation writer did not finish");
                f.RestoreEnvironment();
            }
        }
    }
}
