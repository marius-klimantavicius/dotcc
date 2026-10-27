using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Independently authored scalar preparation IR. Values are unsigned 64-bit bit
// patterns; Width/Shift make truncation, extension and alias insertion explicit.
// MemoryRead and LoopInput have identity, but are NEVER evaluated by preparation.
internal enum JitPlanValueKind : byte
{
    Unknown, EntryRegister, Constant, MemoryRead, LoopInput,
    Add, Sub, And, Or, Xor, Extract, Insert, SignExtend, ShiftLeft
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
internal struct JitPlanValue
{
    [FieldOffset(0)] internal ulong Immediate;
    [FieldOffset(8)] internal ushort Left;
    [FieldOffset(10)] internal ushort Right;
    [FieldOffset(12)] internal JitPlanValueKind Kind;
    [FieldOffset(13)] internal byte Width;
    [FieldOffset(14)] internal byte Shift;
    [FieldOffset(15)] internal byte Known; // Entry-derived; not a memory lease.
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct JitPlanDefinition
{
    [FieldOffset(0)] internal byte Register;
    [FieldOffset(1)] internal byte IsLoopMerge;
    [FieldOffset(2)] internal ushort Before;
    [FieldOffset(4)] internal ushort After;
}

// Generic is deliberately zero: an unknown/default recipe never grants an
// entry-derived fast form. Affine uses only invocation-entry registers.
internal enum JitPlanRecipeKind : byte { Generic, Affine }

[StructLayout(LayoutKind.Explicit, Size = 32)]
internal struct JitPlanMemory
{
    [FieldOffset(0)] internal ushort Value; // Full IR value, even when unknown.
    [FieldOffset(2)] internal ushort EvaluationValue;
    [FieldOffset(4)] internal ushort EvaluationEnd;
    [FieldOffset(6)] internal byte Width; // Bytes, not bits.
    [FieldOffset(7)] internal JitMemoryAccess Access;
    [FieldOffset(8)] internal byte InstructionIndex;
    [FieldOffset(9)] internal JitPlanRecipeKind RecipeKind;
    [FieldOffset(10)] internal sbyte Base; // -1 means absent, for Affine only.
    [FieldOffset(11)] internal sbyte Index;
    [FieldOffset(12)] internal byte Scale; // log2 multiplier, 0..63.
    [FieldOffset(16)] internal ulong Displacement; // Modulo 2^64.
}

[Flags]
internal enum JitPlanEffects : byte
{
    Poll = 1, Guard = 2, AtomicIssue = 4, MemoryRead = 8,
    MemoryWrite = 16, MaterializedCommit = 32, AtomicComplete = 64
}

internal enum JitPlanFlagsEffect : byte { Preserve, Arithmetic, Logical, IncPreserveCarry, DecPreserveCarry }
internal enum JitPlanBoundary : byte { StopOrAttention, MemoryMiss, RejectedIssue, CompletedFallThrough, CompletedTaken }

// A definition prefix describes the state at an instruction boundary. Loop
// merges precede the instruction's BEFORE state; ordinary definitions produce
// its AFTER state. Flags IDs name entry (0), instruction-produced (index+1),
// or loop-carried (33) state. Left/Right/Result retain each flags recipe's inputs.
// These maps describe materialized state, not permission to defer any write.
[StructLayout(LayoutKind.Explicit, Size = 64)]
internal struct JitPlanInstruction
{
    [FieldOffset(0)] internal ulong Ip;
    [FieldOffset(8)] internal ulong FallThrough;
    [FieldOffset(16)] internal ulong Target;
    [FieldOffset(24)] internal ushort FirstDefinition;
    [FieldOffset(26)] internal ushort MergeCount;
    [FieldOffset(28)] internal ushort DefinitionCount;
    [FieldOffset(30)] internal ushort Left;
    [FieldOffset(32)] internal ushort Right;
    [FieldOffset(34)] internal ushort Result;
    [FieldOffset(36)] internal short MemoryIndex;
    [FieldOffset(38)] internal short InternalTarget;
    [FieldOffset(40)] internal JitOperation Operation;
    [FieldOffset(44)] internal byte Width;
    [FieldOffset(45)] internal byte SourceWidth;
    [FieldOffset(46)] internal byte Condition;
    [FieldOffset(47)] internal JitPlanEffects Effects;
    [FieldOffset(48)] internal JitPlanFlagsEffect FlagsEffect;
    [FieldOffset(49)] internal byte MergesFlags;
    [FieldOffset(50)] internal ushort FlagsBefore;
    [FieldOffset(52)] internal ushort FlagsAfter;
    [FieldOffset(54)] internal ushort MemoryValue;
    [FieldOffset(56)] internal ushort NextIpValue; // RET's unknown read value.

    internal readonly int BeforeDefinitions => FirstDefinition + MergeCount;
    internal readonly int AfterDefinitions => FirstDefinition + DefinitionCount;

    internal readonly JitPlanExitState ExitState(JitPlanBoundary boundary) => new(this, boundary);
}

// All deltas are relative to BEFORE this instruction, on every dynamic visit.
// Descriptor state stays invocation-owned and unchanged at every boundary.
// Stop/attention/miss retain pre-instruction IP/oplen/flags/registers. Rejected
// issue has the same state plus exactly one atomic issuance. Successful commit
// sets oplen=0, updates IP/state, completes once and consumes one dynamic slot.
internal readonly struct JitPlanExitState
{
    internal JitPlanExitState(JitPlanInstruction instruction, JitPlanBoundary boundary)
    {
        bool after = boundary is JitPlanBoundary.CompletedFallThrough or JitPlanBoundary.CompletedTaken;
        DefinitionPrefix = after ? instruction.AfterDefinitions : instruction.BeforeDefinitions;
        Flags = after ? instruction.FlagsAfter : instruction.FlagsBefore;
        IssuedDelta = after || boundary == JitPlanBoundary.RejectedIssue ? 1 : 0;
        CompletedDelta = after ? 1 : 0;
        PreserveOperationLength = !after;
        Ip = !after ? instruction.Ip : boundary == JitPlanBoundary.CompletedTaken ||
            instruction.Operation is JitOperation.Call or JitOperation.Jump ? instruction.Target : instruction.FallThrough;
        DynamicIpValue = after && instruction.Operation == JitOperation.Ret ? instruction.NextIpValue : (ushort)0;
    }

    internal int DefinitionPrefix { get; }
    internal ushort Flags { get; }
    internal int IssuedDelta { get; }
    internal int CompletedDelta { get; }
    internal int ExecutedDelta => CompletedDelta;
    internal bool PreserveOperationLength { get; }
    internal ulong Ip { get; }
    internal ushort DynamicIpValue { get; }
    internal bool StateIsMaterialized => true;
    internal bool DescriptorsUnchanged => true;
}
