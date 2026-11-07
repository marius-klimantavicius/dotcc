namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    internal Action<bool, bool>? TestCodeSnapshot201;
    partial void CodeSnapshotStep(bool compiling, bool afterBytes)
        => TestCodeSnapshot201?.Invoke(compiling, afterBytes);
}
internal sealed unsafe partial class FullCodeAuthority
{
    internal void TestSetContentSequence201(ulong value) => contentSequence = value;
}
public sealed unsafe partial class ThreadedGuestExecution
{
    internal Action? TestEligibilitySnapshot201;
    partial void FullControllerEligibilitySnapshot()
    {
        var callback = TestEligibilitySnapshot201;
        TestEligibilitySnapshot201 = null;
        callback?.Invoke();
    }
}
