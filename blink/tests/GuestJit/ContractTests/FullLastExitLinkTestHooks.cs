namespace Managed.Emulation.Execution;

// Test-only views of the actual controller's worker-owned value. No alternate
// dispatcher, native trampoline or public product API is introduced.
public sealed unsafe partial class ThreadedGuestExecution
{
    internal FullTranslationJit.LastExitLink FullTestLastExit => current.Value!.FullLastExit;
    internal ulong FullTestLastExitInstructions => current.Value!.Instructions;
}
