using System;
using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// This is independent owner/worker storage. It never overlays MachineFpu.
[StructLayout(LayoutKind.Explicit, Size = ByteSize)]
internal unsafe struct FullX87RegisterFile
{
    internal const uint CurrentVersion = 1, CurrentProducerProtocol = 1;
    internal const int ByteSize = 192, SlotOffset = 64, SlotStride = 16;
    [FieldOffset(0)] internal uint Version;
    [FieldOffset(4)] internal uint Size;
    [FieldOffset(8)] internal uint ProducerProtocol;
    [FieldOffset(12)] internal uint ProfileActive;
    [FieldOffset(16)] internal ulong Machine;
    [FieldOffset(24)] internal ulong OwnerGeneration;
    [FieldOffset(32)] internal ulong OwnerToken;
    [FieldOffset(40)] internal ushort CodeSelector;
    [FieldOffset(42)] internal ushort DataSelector;
    [FieldOffset(44)] internal fixed byte Reserved[20];
    [FieldOffset(64)] internal FullExtended80 R0;
    [FieldOffset(80)] internal FullExtended80 R1;
    [FieldOffset(96)] internal FullExtended80 R2;
    [FieldOffset(112)] internal FullExtended80 R3;
    [FieldOffset(128)] internal FullExtended80 R4;
    [FieldOffset(144)] internal FullExtended80 R5;
    [FieldOffset(160)] internal FullExtended80 R6;
    [FieldOffset(176)] internal FullExtended80 R7;

    internal static FullExtended80* Physical(FullX87RegisterFile* state, int physical)
    {
        if ((uint)physical >= 8) throw new ArgumentOutOfRangeException(nameof(physical));
        return (FullExtended80*)((byte*)state + SlotOffset + SlotStride * physical);
    }
    internal static int PhysicalIndex(ushort status, int logical)
    {
        if ((uint)logical >= 8) throw new ArgumentOutOfRangeException(nameof(logical));
        return ((status >> 11) + logical) & 7;
    }
}

internal static unsafe class FullX87StateStorage
{
    // Allocation/charge belong to FullWorkerNativeState. Charge BEFORE allocating;
    // failed allocation rolls back charge; publication occurs after Initialize.
    internal static ulong FixedCharge => FullX87RegisterFile.ByteSize;
    // Parent serializes these under its existing quota gate. Admission must be
    // part of the SAME total budget as worker/code/copy storage, not a new quota.
    internal static bool TryCharge(ref ulong totalBytes, ulong limit)
    {
        if (totalBytes > limit || FixedCharge > limit - totalBytes) return false;
        totalBytes += FixedCharge; return true;
    }
    internal static void ReleaseCharge(ref ulong totalBytes)
    {
        if (totalBytes < FixedCharge) throw new InvalidOperationException("x87 quota release underflow");
        totalBytes -= FixedCharge;
    }
    internal static bool TryNextGeneration(ulong current, out ulong next)
    {
        next = 0;
        if (current == ulong.MaxValue) return false;
        next = current + 1;
        return true;
    }
    internal static void Initialize(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken)
    {
        if (state == null || ((nuint)state & 15) != 0 || machine == 0 || generation == 0 || ownerToken == 0)
            throw new ArgumentException("x87 allocation identity/alignment");
        // Caller owns the entire fresh writable192 bytes. No allocation here.
        *state = default;
        state->Version = FullX87RegisterFile.CurrentVersion;
        state->Size = FullX87RegisterFile.ByteSize;
        state->ProducerProtocol = FullX87RegisterFile.CurrentProducerProtocol;
        state->Machine = (ulong)(nuint)machine;
        state->OwnerGeneration = generation; state->OwnerToken = ownerToken;
        // Deliberately inactive. Existing Machine f64 values are never imported.
    }
    internal static void Validate(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken, uint protocol)
    {
        if (state == null || ((nuint)state & 15) != 0 || machine == 0 || generation == 0 || ownerToken == 0 ||
            protocol != FullX87RegisterFile.CurrentProducerProtocol ||
            state->Version != FullX87RegisterFile.CurrentVersion || state->Size != FullX87RegisterFile.ByteSize ||
            state->ProducerProtocol != protocol || state->ProfileActive > 1 ||
            state->Machine != (ulong)(nuint)machine || state->OwnerGeneration != generation || state->OwnerToken != ownerToken)
            throw new InvalidOperationException("x87 state identity/version/lifetime mismatch");
        for (int i = 0; i < 20; ++i)
            if (state->Reserved[i] != 0) throw new InvalidOperationException("x87 reserved header");
    }
    internal static nint GetActive(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken, uint protocol)
    {
        Validate(state, machine, generation, ownerToken, protocol);
        return state->ProfileActive == 0 ? 0 : (nint)state;
    }
    // Called while both Machines are exclusively owned and no native leaf runs.
    // NewMachine already copied Machine metadata. Copy exactly values/selectors,
    // NEVER the pointer or parent's identity/header. Fresh child charge required.
    internal static void CloneValues(FullX87RegisterFile* source, nint sourceMachine,
        ulong sourceGeneration, FullX87RegisterFile* child, nint childMachine,
        ulong childGeneration, ulong ownerToken)
    {
        Validate(source, sourceMachine, sourceGeneration, ownerToken, 1);
        Validate(child, childMachine, childGeneration, ownerToken, 1);
        nuint a = (nuint)source, b = (nuint)child;
        if (sourceMachine == childMachine || sourceGeneration == childGeneration ||
            (a < b ? b - a : a - b) < FullX87RegisterFile.ByteSize)
            throw new InvalidOperationException("x87 clone aliases its parent");
        for (int i = 0; i < 8; ++i)
        {
            var destination = FullX87RegisterFile.Physical(child, i);
            *destination = *FullX87RegisterFile.Physical(source, i);
        }
        child->CodeSelector = source->CodeSelector; child->DataSelector = source->DataSelector;
        child->ProfileActive = source->ProfileActive;
    }
    internal static void Retire(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken, bool nativeReturned, bool callbacksDetached, bool workerJoined)
    {
        Validate(state, machine, generation, ownerToken, 1);
        if (!nativeReturned || !callbacksDetached || !workerJoined)
            throw new InvalidOperationException("x87 retirement requires terminal quiescence");
        // Parent clears frame400 and worker pointer BEFORE freeing, then releases
        // fixed charge exactly once. Retirement does not free/release on its own.
        *state = default;
    }
    // Only fresh startup can establish canonical authority. Clone copies its
    // already authoritative raw80 values; no late adoption of Machine f64.
    internal static void ActivateFresh(FullX87RegisterFile* state, nint machine,
        ulong generation, ulong ownerToken)
    {
        RequireActivationQualified(); // denial precedes even header mutation
        Validate(state, machine, generation, ownerToken, 1);
        if (state->ProfileActive != 0 || state->CodeSelector != 0 || state->DataSelector != 0)
            throw new InvalidOperationException("Canonical x87 activation requires fresh state.");
        for (int i = 0; i < 8; ++i)
        {
            var raw = FullX87RegisterFile.Physical(state, i);
            if (raw->Significand != 0 || raw->SignExponent != 0 || raw->Reserved10 != 0 || raw->Reserved12 != 0)
                throw new InvalidOperationException("Canonical x87 activation cannot import legacy register values.");
        }
        state->ProfileActive = 1;
    }
    internal static void RequireActivationQualified() => FullCanonicalX87Activation.RequireQualified();
}
