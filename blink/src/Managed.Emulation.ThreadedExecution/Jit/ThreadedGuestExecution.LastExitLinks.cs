namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    private sealed partial class Worker
    {
        internal FullTranslationJit.LastExitLink FullLastExit;
    }
}
