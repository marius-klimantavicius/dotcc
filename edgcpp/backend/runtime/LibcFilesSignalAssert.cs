#nullable enable
namespace Edg.Runtime;
public sealed unsafe partial class LibcFiles
{
    // Assertion diagnostics are best effort, just as AssertFail's existing
    // write-before-abort path. The translated caller always proceeds to abort,
    // which now honors the owner's SIGABRT handler before default termination.
    public void AssertReport(sbyte* expression, sbyte* file, int line, sbyte* function)
    {
        try { WriteAssertion(expression, file, line, function); }
        catch { }
    }
}
