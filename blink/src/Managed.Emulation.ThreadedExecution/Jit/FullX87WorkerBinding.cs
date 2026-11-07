using System;
using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

// Non-owning identity guard. Parent FullWorkerNativeState owns/charges/frees the
// aligned192-byte allocation and constructs this BEFORE publishing its pointer.
internal sealed unsafe class FullX87WorkerBinding
{
    private FullX87RegisterFile* state;
    private readonly nint machine;
    private readonly ulong generation, ownerToken;
    private int threadId, nativeDepth, callbackDepth;
    internal FullX87WorkerBinding(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken)
    {
        FullX87StateStorage.Validate(state, machine, generation, ownerToken, 1);
        this.state = state; this.machine = machine;
        this.generation = generation; this.ownerToken = ownerToken;
    }
    internal void Bind()
    {
        if (state == null || threadId != 0) throw new InvalidOperationException("x87 worker already bound/retired");
        threadId = Environment.CurrentManagedThreadId;
    }
    private void RequireCurrent(nint pointer, uint protocol, bool requireReturned)
    {
        if (state == null || pointer != machine || threadId != Environment.CurrentManagedThreadId ||
            (requireReturned && nativeDepth != 0))
            throw new InvalidOperationException("x87 callback is not the current returned CPU owner");
        FullX87StateStorage.Validate(state, pointer, generation, ownerToken, protocol);
    }
    internal nint Get(nint pointer, uint protocol)
    {
        RequireCurrent(pointer, protocol, true);
        return state->ProfileActive == 0 ? 0 : (nint)state;
    }
    internal HostX87TransferResult Transfer(nint pointer, uint protocol, HostX87Transfer operation, Span<byte> image)
    {
        RequireCurrent(pointer, protocol, true);
        if (callbackDepth == int.MaxValue) throw new InvalidOperationException("x87 callback depth exhausted");
        ++callbackDepth;
        try { return FullX87StateBridge.Transfer(state, pointer, generation, ownerToken, protocol, operation, image); }
        finally { --callbackDepth; }
    }
    internal void RejectLegacy(nint pointer, uint protocol)
    {
        if (Get(pointer, protocol) != 0)
            throw new InvalidOperationException("Active x87 cannot use legacy double or XMM-backed MMX state");
    }
    internal void EnterNative()
    {
        RequireCurrent(machine, 1, true);
        if (callbackDepth != 0) throw new InvalidOperationException("native entry inside x87 state copy");
        nativeDepth = 1;
    }
    internal void NativeReturned()
    {
        RequireCurrent(machine, 1, false);
        if (nativeDepth != 1) throw new InvalidOperationException("unbalanced x87 native return");
        nativeDepth = 0;
    }
    internal void Detach()
    {
        RequireCurrent(machine, 1, true);
        if (callbackDepth != 0) throw new InvalidOperationException("x87 callback still in flight");
        threadId = 0;
    }
    // Called by terminal parent cleanup after worker join; no access to freed
    // Machine bytes. Invalidates header before parent frees its allocation.
    internal void RetireAfterJoin(bool allFramesDetached, bool workerJoined)
    {
        if (state == null || threadId != 0 || nativeDepth != 0 || callbackDepth != 0)
            throw new InvalidOperationException("x87 binding not detached");
        FullX87StateStorage.Retire(state, machine, generation, ownerToken, allFramesDetached, true, workerJoined);
        state = null;
    }
}
