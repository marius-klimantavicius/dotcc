#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
namespace Edg.Runtime;
public static class LibcSignals
{
    // No source/CLR handlers or destructors may intercept the default action.
    // OS delivery and raw POSIX wait status are deliberately outside this profile.
    [DoesNotReturn]
    public static void Terminate(int signal) => Environment.FailFast("Translated default signal action");
}
