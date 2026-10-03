using System;
using System.Diagnostics.CodeAnalysis;
namespace Edg.Runtime;
// Callback ordering is generated, sharing the C++ normal-finalizer stack.
// This final primitive never joins workers or releases still-live owner storage.
public static class LibcProcessExit
{
    [DoesNotReturn]
    public static void Exit(int status) => Environment.Exit(status);
}
