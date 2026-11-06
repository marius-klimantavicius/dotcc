#nullable enable

using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Both phases share the lowered function body. A compileError inside the
    // comptime-only arm must fail when interpreted, not while constructing the
    // runtime body (its message may depend on runtime parameters at this point).
    // Match the function owner so eagerly binding another function cannot inherit
    // this lexical condition.
    private readonly Stack<Symbol?> _comptimeOnlyArmOwners = new();

    private bool InComptimeOnlyArm => _comptimeOnlyArmOwners.Contains(_currentFnSym);

    private CStmt LowerInComptimePhaseArm(Item statement, bool comptimeOnly)
    {
        if (!comptimeOnly) return LowerStmt(statement);
        _comptimeOnlyArmOwners.Push(_currentFnSym);
        try { return LowerStmt(statement); }
        finally { _comptimeOnlyArmOwners.Pop(); }
    }

    private static bool InComptimeIsThen(Item condition)
    {
        while (condition.Content is Zig.Grouped grouped) condition = grouped.Arg1;
        return condition.Content is not Zig.PreNot;
    }
}
