using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

// AUTHORED_UNRUN. Parent must select CheckFullX87Ownership after fresh producer
// and serial typecheck. No activation writer, f64 import, synthetic full-Machine
// reset, or mirror arithmetic implementation is used by this fixture.
namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        private bool testX87FailAllocation, testX87FailStart;
        partial void X87StorageAllocating()
        {
            if (testX87FailAllocation) throw new OutOfMemoryException("R137 injected allocation failure");
        }
        partial void X87ThreadStarting()
        {
            if (testX87FailStart) throw new System.Threading.ThreadStateException("R137 injected pre-Start failure");
        }

        internal static void TestFullX87Ownership(Action<bool, string> check)
        {
            bool quotaDenied = false;
            try { using var tooSmall = new FullControllerFixture(retentionBudget: 1L << 20); }
            catch (InvalidOperationException error) when (error.Message.Contains("configured retention quota"))
            { quotaDenied = true; }
            check(quotaDenied, "R137 fixed owner quota denies before worker allocation");
            using var fixture = new FullControllerFixture();
            var owner = fixture.Owner;
            var main = owner.workers[0]; var child = owner.workers[1];
            var m = main.Machine; var c = child.Machine;
            check(main.X87Bound && child.X87Bound && main.X87Initialized && child.X87Initialized,
                "R137 fixture binds both real Machine owners");
            check(main.X87MachineIdentity == (nint)m && child.X87MachineIdentity == (nint)c &&
                main.X87Generation != child.X87Generation && main.X87OwnerToken == fixture.Jit.Store.OwnerCookie &&
                main.X87OwnerToken != 0, "R137 exact Machine/generation/stable owner token");
            var storage = main.FullNativeState!;
            check(((nuint)storage.X87Pointer & 15) == 0 && storage.NativeStorageBytes == FullNativeMemory.WorkerNativeStorageBytes &&
                FullNativeMemory.WorkerChargeFor(FullNativeMemory.TlbCount) == FullNativeMemory.WorkerCharge &&
                FullNativeMemory.WorkerCharge - storage.NativeStorageBytes == FullNativeMemory.X87BindingAndWorkerAllowance + FullWorkerNativeState.MappingCacheCharge,
                "R137 aligned allocation and independent binding allowance");
            foreach (int count in new[] { 2, 64, 256, 4096 })
                check(FullNativeMemory.WorkerChargeFor(count) - FullNativeMemory.WorkerNativeStorageBytesFor(count) == 128 + FullWorkerNativeState.MappingCacheCharge &&
                    (FullNativeMemory.WorkerNativeStorageBytesFor(count) & 15) == 0,
                    "R137 custom TLB quota/storage alignment coherent");

            fixture.At(FullControllerFixture.Base);
            var binding = main.X87Binding!;
            check(binding.Get((nint)m, 1) == 0 && storage.X87Pointer->ProfileActive == 0,
                "R137 main bootstrap stays inactive");
            void Denied(Action body, string name)
            {
                bool denied = false;
                try { body(); } catch (InvalidOperationException) { denied = true; }
                check(denied, name);
            }
            main.Machine = null;
            try { Denied(() => owner.InitializeX87Worker(main), "R137 null/NewMachine failure refuses initialization"); }
            finally { main.Machine = m; }
            Denied(() => owner.InitializeX87Worker(main), "R137 actual NewMachine bound exactly once");
            Denied(() => binding.Get((nint)c, 1), "R137 inactive Get validates exact Machine");
            ulong generation = storage.X87Pointer->OwnerGeneration;
            storage.X87Pointer->OwnerGeneration++;
            try { Denied(() => binding.Get((nint)m, 1), "R137 stale generation denied before inactive result"); }
            finally { storage.X87Pointer->OwnerGeneration = generation; }
            ulong token = storage.X87Pointer->OwnerToken;
            storage.X87Pointer->OwnerToken++;
            try { Denied(() => binding.Get((nint)m, 1), "R137 wrong owner token denied before inactive result"); }
            finally { storage.X87Pointer->OwnerToken = token; }
            Task.Run(() => Denied(() => binding.Get(main.X87MachineIdentity, 1), "R137 wrong thread denied")).GetAwaiter().GetResult();

            // Same production clone seam, actual NewMachine metadata, fresh
            // already initialized target. Values include unsupported raw80 bits.
            var source = storage.X87Pointer; var target = child.FullNativeState!.X87Pointer;
            source->R3.Significand = 0xdeadbeef01234567UL; source->R3.SignExponent = 0x7fff;
            source->CodeSelector = 0x1234; source->DataSelector = 0xabcd;
            owner.CloneX87WorkerForTest(main, child);
            check(source != target && target->Machine == (ulong)(nuint)c && target->OwnerGeneration == child.X87Generation &&
                target->OwnerGeneration != source->OwnerGeneration && target->OwnerToken == token &&
                target->R3.Significand == source->R3.Significand && target->R3.SignExponent == source->R3.SignExponent &&
                target->CodeSelector == 0x1234 && target->DataSelector == 0xabcd && target->ProfileActive == 0,
                "R137 clone copies physical values/selectors into fresh identity");

            EnterX87Native(main);
            Denied(() => binding.Get((nint)m, 1), "R137 callback while native denied");
            main.X87TerminalQuiescent = true;
            long bytesBefore = storage.NativeStorageBytes;
            Denied(() => DisposeWorkerResources(main), "R137 uncertain native retirement refuses Free");
            check(storage.NativeStorageBytes == bytesBefore && !main.ResourcesDisposed && !main.X87Retired &&
                main.X87RetirementError != null, "R137 uncertain retirement retains pointer/reservation/error");
            // TEST ONLY: this simulated call never entered unmanaged code.
            X87NativeReturned(main);
            check(binding.Get((nint)m, 1) == 0, "R137 proven normal return reopens callback");
            main.X87TerminalQuiescent = false;

#if BLINK_FULL_X87_STATE_OWNER
            var callbacks = (IHostGuestX87State)owner;
            check(callbacks.GetX87RegisterFile((nint)m, 1) == 0, "R137 actual typed inactive callback");
            byte[] callbackImage = new byte[512]; callbackImage.AsSpan().Fill(0x96);
            check(callbacks.TransferX87State((nint)m, 1, HostX87Transfer.LinuxSave, callbackImage) == HostX87TransferResult.Inactive &&
                callbackImage[0] == 0x96, "R137 typed inactive transfer preserves owned staging");
            // Fresh native10's actual instance callback signatures and guestThreads
            // TLS path, rather than a replacement implementation/test delegate.
            check(Blink.blink_host_guest_x87_active(fixture.Program, m, 1) == 0,
                "R137 actual producer host active callback resolves exact owner");
            fixed (byte* imagePointer = callbackImage)
                check(Blink.blink_host_guest_x87_state(fixture.Program, m, 1,
                    (int)HostX87Transfer.LinuxSave, imagePointer, 512) == (int)HostX87TransferResult.Inactive &&
                    callbackImage[0] == 0x96, "R137 actual producer host transfer callback preserves inactive image");
            Blink.blink_host_guest_x87_reject_legacy(fixture.Program, m, 1);
            EnterX87Native(main);
            try
            {
                Denied(() => callbacks.GetX87RegisterFile((nint)m, 1), "R137 actual typed native-in-flight callback denied");
                Denied(() => callbacks.TransferX87State((nint)m, 1, HostX87Transfer.LinuxSave, callbackImage),
                    "R137 actual typed transfer while native denied");
                Denied(() => Blink.blink_host_guest_x87_active(fixture.Program, m, 1),
                    "R137 actual producer callback while native denied");
            }
            finally { X87NativeReturned(main); } // simulated entry only, no native call
            int returned = 0;
            fixture.NativeReturned = _ =>
            {
                check(!main.X87NativeInFlight && callbacks.GetX87RegisterFile((nint)m, 1) == 0 &&
                    Blink.blink_host_guest_x87_active(fixture.Program, m, 1) == 0,
                    "R137 actual normal native RET precedes typed callback");
                ++returned;
            };
            fixture.Map(FullControllerFixture.Base); fixture.Fill(FullControllerFixture.Base);
            fixture.Counts(1); fixture.Step(FullControllerFixture.Base);
            check(returned > 0, "R137 actual controller native-return callback reached");
            fixture.NativeReturned = null;
#endif
            // Start failures exercise the production reserve/clone/rollback
            // paths. C's Start caller retains and frees each failed child.
            fixture.At(FullControllerFixture.Base);
            int workerCount = owner.workers.Count;
            // Fixture uses both reserved slots. Remove synthetic child from
            // admission only while unpublished-failure tests run; its reservation
            // and allocation remain accounted by the fixed configured quota.
            // No allocation may succeed with a third slot: force failure before
            // allocation, then reuse the child's slot after terminal retirement.
            owner.workers.Remove(child);
            owner.testX87FailAllocation = true;
            try { TestFailedX87Start(fixture, check, workerCount - 1); }
            finally { owner.testX87FailAllocation = false; owner.workers.Add(child); }
            // Move the already cloned, never-native fixture binding to a real
            // child thread, detach there, and prove Join precedes retirement.
            fixture.At(FullControllerFixture.Base, 1);
            DetachX87Worker(child);
            Exception? childError = null;
            child.Thread = new System.Threading.Thread(() =>
            {
                try
                {
                    child.ThreadId = Environment.CurrentManagedThreadId;
                    owner.current.Value = child;
                    owner.BindX87Worker(child);
                    check(child.X87Binding!.Get(child.X87MachineIdentity, 1) == 0,
                        "R137 child bind preserves cloned identity and inactive state");
                    DetachX87Worker(child);
                }
                catch (Exception error) { childError = error; }
                finally { owner.current.Value = null; }
            });
            child.X87ThreadStarted = true;
            child.Thread.Start();
            Denied(() => owner.RetireX87Worker(child), "R137 started child cannot retire before join");
            child.Thread.Join(); child.Joined = true;
            if (childError != null) throw childError;
            child.ThreadId = Environment.CurrentManagedThreadId;
            owner.ClearExecution(child); owner.ReleaseMachine(child);
            owner.RetireX87Worker(child); owner.RetireX87Worker(child);
            check(child.X87Retired && !child.X87Bound, "R137 child detach/retire once and repeated cleanup");
            // Retired child still owns its native slab until resources disposal.
            DisposeWorkerResources(child);
            check(child.FullNativeState!.NativeStorageBytes == 0, "R137 retired child uses matching aligned free");
            owner.workers.Remove(child);
            fixture.At(FullControllerFixture.Base);
            owner.testX87FailStart = true;
            try { TestFailedX87Start(fixture, check, workerCount - 1); }
            finally { owner.testX87FailStart = false; }
            ulong sequence = owner.x87Generation; owner.x87Generation = ulong.MaxValue;
            try { TestFailedX87Start(fixture, check, workerCount - 1); }
            finally { owner.x87Generation = sequence; owner.workers.Add(child); }
            check(owner.pendingX87Rollback == null && main.FullNativeState.NativeStorageBytes == bytesBefore,
                "R137 allocation/start/exhaustion rollbacks retain fixed quota and no pending pointer");
            // Fixture cleanup now releases main; the child Machine was freed
            // before its terminal retirement and is never dereferenced again.
            fixture.Dispose(); fixture.Dispose();
            check(main.X87Retired && child.X87Retired && main.ResourcesDisposed && child.ResourcesDisposed &&
                main.FullNativeState.NativeStorageBytes == 0, "R137 main terminal and repeated fixture cleanup retire/free once");
        }

        private void CloneX87WorkerForTest(Worker parent, Worker child)
        {
            // Fixture child has never entered native or started a thread. Drop
            // its test binding, then exercise the exact production clone seam.
            DetachX87Worker(child);
            CloneX87Worker(parent, child);
            BindX87Worker(child);
        }
        private static void TestFailedX87Start(FullControllerFixture fixture, Action<bool, string> check, int workers)
        {
            var owner = fixture.Owner; var parent = owner.workers[0];
            Blink.Machine* machine = null;
            fixture.DataScope(0, () => machine = fixture.Program.NewMachine(owner.system, parent.Machine));
            check(machine != null, "R137 actual failed-start caller Machine");
            int history = owner.threadResults.Count;
            try
            {
                int result = ((IHostGuestThreads)owner).Start((nint)machine);
                check(result == 11 && owner.workers.Count == workers && owner.threadResults.Count == history &&
                    parent.PendingStarts.Count == 0 && owner.pendingX87Rollback == null,
                    "R137 deterministic failed allocation/start/exhaustion rolls back publication once");
                check(machine->blink_host_system == owner.system, "R137 Start failure leaves caller Machine owned and live");
            }
            finally { fixture.DataScope(0, () => fixture.Program.FreeMachine(machine)); }
        }
    }
}

unsafe partial class Program
{
    static void CheckFullX87Ownership() => ThreadedGuestExecution.TestFullX87Ownership(Check);
}
