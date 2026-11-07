using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private static bool IsEnumCatchAllItem(Item item)
        => item.Content is Zig.Ident id && Tok(id.Arg0) == "_";

    // `_` covers unnamed backing values, rather than missing named tags. It can
    // use the target's default label only after every named tag is accounted for.
    private void ValidateEnumCatchAll(CType subject, Item prongsItem)
    {
        var prongs = Flatten(prongsItem).Select(DecomposeProng).ToList();
        var catchAlls = prongs.Count(p => p.CaseVals.Content is not Zig.CaseElse
            && WalkCaseValItems(p.CaseVals).Any(v => v.Hi is null && IsEnumCatchAllItem(v.Lo)));
        if (catchAlls == 0) return;
        if (subject.Unqualified is not CType.Enum en || !_nonExhaustiveEnums.Contains(en.Name)
            || !_enumMembers.TryGetValue(en.Name, out var members))
            throw new IrUnsupportedException($"zig `_` switch prong requires a source-known nonexhaustive enum (got {subject.Describe()}, {_module?.Path ?? _fileStem}, function '{_currentFnName}')");
        var remaining = Flatten(prongsItem).Where(p => DecomposeProng(p).CaseVals.Content is Zig.CaseElse).ToList();
        if (catchAlls != 1 || remaining.Count > 1
            || remaining.Count == 1 && remaining[0].Content is not Zig.InlineProng)
            throw new IrUnsupportedException("zig nonexhaustive enum catch-all requires at most one distinct inline else prong");
        // `_` handles unnamed backing values. Inline else expands the remaining
        // declared tags into explicit cases, so it is not a second default.
        if (remaining.Count == 1) return;
        var labels = prongs.SelectMany(p => LowerCaseVals(p.CaseVals, subject)).Where(l => l.CaseExpr is not null).ToList();
        foreach (var member in members.Values)
        {
            if (!labels.Any(l => _ir.ConstEval(l.CaseExpr!) is { } lo && member.ConstValue >= lo
                && member.ConstValue <= (l.HiExpr is { } hi ? _ir.ConstEval(hi) ?? lo : lo)))
                throw new IrUnsupportedException($"zig nonexhaustive enum catch-all does not cover named tag '{member.Name}'");
        }
    }
}
