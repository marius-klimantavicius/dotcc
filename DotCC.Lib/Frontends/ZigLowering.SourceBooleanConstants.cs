#nullable enable
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly Dictionary<string, CExpr> _sourceBooleanConstants = new(System.StringComparer.Ordinal);

    // A file's immutable boolean initializer executes at comptime. In
    // particular, builtin.mode.runtimeSafety() is not a runtime side effect in
    // std.debug.runtime_safety. Use the interpreter's value, never a host guess.
    private CExpr FoldSourceModuleBoolean(string name, CExpr value)
    {
        if (UsesSourceStdlib && IsZigBoolValue(value)
            && _ir.ResolveComptimeFold(value) is LitBool folded)
        {
            _sourceBooleanConstants[name] = folded;
            return folded;
        }
        return value;
    }
}
