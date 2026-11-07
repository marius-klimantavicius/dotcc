using System;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Run owns this sequence. Neither code-arena resets nor worker-cache serials
    // participate in canonical identity. gate serializes admission and burning.
    partial void X87StorageAllocating();
    partial void X87ThreadStarting();
    private ulong x87Generation;
    private Worker? pendingX87Rollback;

    private sealed partial class Worker
    {
        internal FullX87WorkerBinding? X87Binding;
        internal nint X87MachineIdentity;
        internal ulong X87Generation, X87OwnerToken;
        internal bool X87Initialized, X87Bound, X87Detached, X87Retired;
        internal bool X87NativeInFlight, X87ThreadStarted, X87TerminalQuiescent;
        internal Exception? X87RetirementError;
        internal Worker? X87PendingRollbackNext;
    }

    private void EnsureX87Storage(Worker worker)
    {
        if (fullJit == null || worker.FullNativeState != null) return;
        if (fullJit.Store.OwnerCookie == 0)
            throw new InvalidOperationException("Canonical x87 requires a nonzero owner token.");
        // FullTranslationJit's fixed quota reserves maximumWorkers of these
        // slots before any allocation; no second192-byte charge is made here.
        // Failed allocation consumes no dynamic charge. The fixed reservation
        // remains until owner disposal, including any pending retirement.
        X87StorageAllocating(); // erased in production; deterministic test failure
        worker.FullNativeState = new();
    }

    private void InitializeX87Worker(Worker worker)
    {
        if (fullJit == null) return;
        if (worker.Machine == null || worker.X87Initialized || worker.X87Retired)
            throw new InvalidOperationException("Canonical x87 bootstrap requires one fresh live Machine.");
        EnsureX87Storage(worker);
        lock (gate)
        {
            if (!FullX87StateStorage.TryNextGeneration(x87Generation, out ulong next))
                throw new InvalidOperationException("Canonical x87 worker generation exhausted.");
            x87Generation = next; // burn even if later binding allocation fails
            worker.X87Generation = next;
        }
        worker.X87MachineIdentity = (nint)worker.Machine;
        worker.X87OwnerToken = fullJit.Store.OwnerCookie;
        FullX87StateStorage.Initialize(worker.FullNativeState!.X87Pointer,
            worker.X87MachineIdentity, worker.X87Generation, worker.X87OwnerToken);
        if (fullJit.CanonicalX87Enabled)
            FullX87StateStorage.ActivateFresh(worker.FullNativeState.X87Pointer,
                worker.X87MachineIdentity, worker.X87Generation, worker.X87OwnerToken);
        worker.X87Initialized = true;
        worker.X87Binding = new(worker.FullNativeState.X87Pointer,
            worker.X87MachineIdentity, worker.X87Generation, worker.X87OwnerToken);
        if (worker.Bound) BindX87Worker(worker); // main: Bind preceded NewMachine
    }

    private void BindX87Worker(Worker worker)
    {
        if (fullJit == null || worker.Machine == null) return; // main bootstrap
        if (!worker.X87Initialized || worker.X87Binding == null || worker.X87Bound || worker.X87Retired ||
            worker.X87MachineIdentity != (nint)worker.Machine || worker.ThreadId != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Canonical x87 bind identity/lifecycle mismatch.");
        worker.X87Binding.Bind();
        worker.X87Bound = true; worker.X87Detached = false;
    }

    private void CloneX87Worker(Worker parent, Worker child)
    {
        if (fullJit == null) return;
        var binding = parent.X87Binding ?? throw new InvalidOperationException("Canonical x87 clone parent missing.");
        _ = binding.Get((nint)parent.Machine, 1); // inactive still validates returned owner
        if (!child.X87Initialized || child.X87Bound || child.X87Retired || child.X87ThreadStarted)
            throw new InvalidOperationException("Canonical x87 child must be fresh and unpublished.");
        FullX87StateStorage.CloneValues(parent.FullNativeState!.X87Pointer,
            parent.X87MachineIdentity, parent.X87Generation, child.FullNativeState!.X87Pointer,
            child.X87MachineIdentity, child.X87Generation, parent.X87OwnerToken);
        // Machine metadata was copied by NewMachine. Never import f64 shadow.
    }

    private static FullX87RegisterFile* GetX87NativePointer(Worker worker)
    {
#if BLINK_FULL_X87_STATE_OWNER
        if (!worker.X87Bound || worker.X87Detached || worker.X87Retired || worker.X87NativeInFlight ||
            worker.X87MachineIdentity != (nint)worker.Machine || worker.X87Binding == null)
            throw new InvalidOperationException("Canonical x87 frame publication has no exact live owner.");
        return (FullX87RegisterFile*)worker.X87Binding.Get((nint)worker.Machine, 1);
#else
        return null;
#endif
    }

    private static void EnterX87Native(Worker worker)
    {
        if (!worker.X87Bound || worker.X87Detached || worker.X87Retired || worker.X87NativeInFlight ||
            worker.X87MachineIdentity != (nint)worker.Machine)
            throw new InvalidOperationException("Native entry has no exact canonical x87 owner.");
        var binding = worker.X87Binding ?? throw new InvalidOperationException("Native x87 binding missing.");
        binding.EnterNative();
        worker.X87NativeInFlight = true;
    }

    private static void X87NativeReturned(Worker worker)
    {
        // This is called ONLY on the straight-line path after the actual native
        // call. No catch/finally or fault-stack walker can establish normal RET.
        if (!worker.X87NativeInFlight) throw new InvalidOperationException("Canonical x87 native return is unbalanced.");
        worker.X87Binding!.NativeReturned();
        worker.X87NativeInFlight = false;
    }

    private static void DetachX87Worker(Worker worker)
    {
        if (!worker.X87Initialized || worker.X87Detached || worker.X87Retired) return;
        if (worker.X87NativeInFlight)
            throw new InvalidOperationException("Canonical x87 native return is uncertain; Machine and storage retained.");
        if (worker.X87Bound)
        {
            worker.X87Binding!.Detach(); // checks returned native and callback depth
            worker.X87Bound = false;
        }
        worker.X87Detached = true;
        // ABI6 frames have no canonical pointer. Parent owns future frame400
        // reference tracking/clearing; this integration grants no ABI7 readiness.
    }

    private void RetireX87Worker(Worker worker)
    {
        if (worker.X87Retired) return; // do not call the non-idempotent binding twice
        try
        {
            bool terminal = worker.Main ? worker.X87TerminalQuiescent :
                worker.X87ThreadStarted ? worker.Joined : worker.X87TerminalQuiescent;
            if (worker.X87NativeInFlight || !terminal || worker.X87Bound ||
                (worker.X87Initialized && !worker.X87Detached && worker.X87ThreadStarted))
                throw new InvalidOperationException("Canonical x87 retirement is pending detachment and terminal quiescence.");
            if (worker.X87Initialized)
            {
                // Machine may already be freed. Binding/storage validate the
                // retained INTEGER identity and header; never dereference it.
                if (worker.X87Binding != null)
                    worker.X87Binding.RetireAfterJoin(allFramesDetached: true, workerJoined: terminal);
                else
                    FullX87StateStorage.Retire(worker.FullNativeState!.X87Pointer,
                        worker.X87MachineIdentity, worker.X87Generation, worker.X87OwnerToken, true, true, terminal);
            }
            worker.FullNativeState?.ClearX87Work(); // quiescent; no live native continuation
            worker.X87Retired = true;
            worker.X87RetirementError = null;
        }
        catch (Exception error)
        {
            worker.X87RetirementError = error;
            throw; // pointer, native allocation, and fixed owner charge stay owned
        }
    }

    private void RetirePendingX87Rollbacks()
    {
        // Unpublished failed starts are not thread-result/history workers. Keep
        // a nonallocating chain so even cleanup failure cannot lose ownership.
        while (pendingX87Rollback is { } worker)
        {
            DisposeWorkerResources(worker);
            pendingX87Rollback = worker.X87PendingRollbackNext;
            worker.X87PendingRollbackNext = null;
        }
    }
}
