namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    internal Action? TestRetainedDeferred205;
    partial void RetainedValidationDeferred() => TestRetainedDeferred205?.Invoke();
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal sealed partial class FullControllerFixture
    {
        internal TranslationBlockHeader R205Header(TranslationToken token)
        {
            Require(Jit.Store.TryAcquireToken(token, out var lease), "R205 resident body remains addressable");
            using (lease) return Jit.Store.GetHeaderCopy(in lease);
        }
    }
}
