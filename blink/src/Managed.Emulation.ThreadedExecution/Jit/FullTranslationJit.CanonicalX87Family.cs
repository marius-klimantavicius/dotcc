namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Only guarded startup (or the isolated contract fixture) may admit this
    // owner. No public option or witness count can grant qualification.
    private bool canonicalX87Enabled;

    private static bool TryClassifyCanonicalX87(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction) =>
        FullCanonicalX87InvalidLowering.TryClassify(ip, bytes, out instruction) ||
        FullCanonicalX87StackLowering.TryClassify(ip, bytes, out instruction, out _) ||
        FullCanonicalX87ArithmeticLowering.TryClassify(ip, bytes, out instruction, out _) ||
        FullCanonicalX87MemoryLowering.TryClassify(ip, bytes, out instruction, out _) ||
        FullCanonicalX87ControlLowering.TryClassify(ip, bytes, out instruction, out _) ||
        FullCanonicalX87TranscendentalLowering.TryClassify(ip, bytes, out instruction, out _) ||
        FullCanonicalX87MemoryArithmeticLowering.TryClassify(ip, bytes, out instruction, out _);

    internal bool CanonicalX87Enabled { get { lock (Authority.Gate) return canonicalX87Enabled; } }

    // Run calls this after producer checks and registering the owner for cleanup,
    // before NewMachine/LoadProgram or any publication. Current blockers return
    // without allocating/emitting/sealing/charging the expensive 77-key catalog.
    internal FullCanonicalX87ActivationBlocker PrepareCanonicalX87Production(bool allowUnverifiedWindows)
    {
        lock (Authority.Gate)
        {
            ObjectDisposedException.ThrowIf(disposed || x87KernelClosing, this);
            if (canonicalX87Enabled || x87Kernels != null || Store.Publications != 0 || Store.ActiveNativeEpochs != 0)
                throw new InvalidOperationException("Canonical x87 production startup must be fresh and unpublished.");
            var blockers = FullCanonicalX87Activation.Blockers;
            if (blockers != FullCanonicalX87ActivationBlocker.None) return blockers;
            FullCanonicalX87Activation.RequireQualified();
            InitializeX87Kernels(allowUnverifiedWindows);
            // Failed/partial catalog startup remains owned and never enables.
            canonicalX87Enabled = true;
            return FullCanonicalX87ActivationBlocker.None;
        }
    }

    // Translation-time binding, never an instruction execution helper. Eager
    // catalog and exact RX owner are authenticated under the compiler's gate.
    private FullX87KernelCall BindCanonicalX87Instruction(JitInstruction instruction)
    {
        if (instruction.Operation != JitOperation.X87) return default;
        if (!canonicalX87Enabled || !Monitor.IsEntered(Authority.Gate))
            throw new InvalidOperationException("Canonical x87 compilation lacks admitted owner startup.");
        if (FullBlockEmitter.CanInlineCanonicalX87Arithmetic(instruction))
            return GetX87KernelCall(FullBlockEmitter.CanonicalX87ArithmeticKernel(instruction));
        if (FullBlockEmitter.CanInlineCanonicalX87Memory(instruction))
            return GetX87KernelCall(FullBlockEmitter.CanonicalX87MemoryKernel(instruction));
        if (FullBlockEmitter.CanInlineCanonicalX87MemoryArithmetic(instruction))
            return GetX87KernelCall(FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(instruction));
        if (FullBlockEmitter.CanInlineCanonicalX87Control(instruction) &&
            FullBlockEmitter.CanonicalX87ControlNeedsKernel(instruction))
            return GetX87KernelCall(FullBlockEmitter.CanonicalX87ControlKernel(instruction));
        if (FullBlockEmitter.CanInlineCanonicalX87Transcendental(instruction))
            return GetX87KernelCall(FullBlockEmitter.CanonicalX87TranscendentalKernel(instruction));
        return default; // raw stack/flags/state paths need no numeric entry
    }
}
