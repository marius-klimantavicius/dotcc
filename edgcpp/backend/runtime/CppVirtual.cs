using System;
using System.Diagnostics.CodeAnalysis;

namespace Edg.Runtime;

// EDG lib_src/pure_virt.c and del_virt.c call __abort_execution directly.
// These ABI failure paths do not invoke a user-installed std::terminate handler.
public static class CppVirtual
{
    [DoesNotReturn]
    public static void PureVirtual() => Environment.FailFast("EDG pure virtual call");

    [DoesNotReturn]
    public static void DeletedVirtual() => Environment.FailFast("EDG deleted virtual call");
}
