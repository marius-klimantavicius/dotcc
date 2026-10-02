#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Edg.Runtime;

public static class LibcProcess
{
    /// <summary>Abnormal process termination. This deliberately never throws a
    /// catchable managed exception and never unwinds translated destructors.
    /// Translated SIGABRT handlers require the separate signal provider.</summary>
    [DoesNotReturn]
    public static void Abort() => Environment.FailFast("Translated libc abort");
}
