#nullable enable
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private string? _typeBoolQuestionMiss;
    // A types-only predicate can return a switch whose selected block walks
    // reflected fields and returns early. Reuse ordinary source lowering and
    // the existing interpreter rather than supplying a trait-specific answer.
    private bool? InterpretTypeBoolReturn(GenericFnInfo generic, Zig.StmtReturn returned,
        IReadOnlyList<(CType Type, int? Bits)> arguments)
    {
        if (!UsesSourceStdlib || generic.ErrUnion || returned.Arg1.Content is not Zig.SwitchExpr selection
            || SelectComptimeProng(selection.Arg2, selection.Arg5, out _) is not { Expr: null })
            return null;
        var depth = _comptimeBoolCallDepth;
        var previousInfos = new Dictionary<string, ZigTypeInfo>(_typeInfoBindings, System.StringComparer.Ordinal);
        using var function = new FnStateScope(this);
        using var definition = new DefinitionBindingScope(this);
        using var container = EnterContainer(generic.Owner);
        using var ownerSeeds = EnterReifiedSeeds(generic.Owner ?? "");
        using var symbols = EnterSymbolScope();
        var session = _ir.BeginComptimeSession();
        try
        {
            _typeInfoBindings.Clear();
            for (var index = 0; index < generic.Params.Count; index++)
                InstallDefinitionType(new(generic.Params[index].Name, arguments[index].Type, arguments[index].Bits));
            if (LowerType(generic.RetType).Unqualified != CType.Bool) return null;
            _currentFnName = generic.Template.Name;
            _currentFnRet = CType.Bool;
            _inGenericInstance = true;
            _comptimeBoolCallDepth = depth;
            if (SelectComptimeProng(selection.Arg2, selection.Arg5, out var payload) is not { Expr: null } selected)
                return null;
            CStmt body;
            EnterComptimeProng(selected, payload);
            try
            {
                using (EnterFreshHoist())
                {
                    // The source block returns from this predicate. Keep those
                    // real returns as statements; a terminating prong wrapped
                    // as a noreturn StatementExpression has no value to read.
                    body = LowerSelectedProng(selected);
                    if (_hoist is { Count: > 0 } preparation) body = new Block([.. preparation, body]);
                }
            }
            finally { ExitComptimeProng(); }
            var interpreted = _ir.RunComptimeSessionStmt(session, body);
            if (interpreted is { } result && result.Returned && result.Value is IrBuilder.CtBool answer)
                return answer.Value;
            _typeBoolQuestionMiss = generic.Template.Name + ": " + (interpreted is null
                ? _ir.ComptimeMiss ?? "interpreter produced no result"
                : !interpreted.Value.Returned ? "body did not return"
                : "return value " + (interpreted.Value.Value?.GetType().Name ?? "unknown"));
            return null;
        }
        finally
        {
            _ir.EndComptimeSession(session);
            _typeInfoBindings.Clear();
            foreach (var (name, info) in previousInfos) _typeInfoBindings[name] = info;
        }
    }
}
