using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private static IEnumerable<CStmt> SwitchChildren(CStmt statement) => statement switch
    {
        Block block => block.Stmts,
        Seq sequence => sequence.Stmts,
        Labeled label => new[] { label.Body },
        CaseLabelStmt label => new[] { label.Body },
        If conditional => conditional.Else is { } other ? new[] { conditional.Then, other } : new[] { conditional.Then },
        While loop => new[] { loop.Body },
        DoWhile loop => new[] { loop.Body },
        For loop => loop.Init is { } init ? new[] { init, loop.Body } : new[] { loop.Body },
        _ => Array.Empty<CStmt>(), // A nested switch owns its own case labels.
    };

    private static bool ContainsSwitchEntryLabel(CStmt statement) =>
        statement is CaseLabelStmt or Labeled || SwitchChildren(statement).Any(ContainsSwitchEntryLabel);

    /// <summary>C permits jumping directly into a block, conditional or loop at
    /// a nested case label. Dispatch once to ordinary same-scope labels, then
    /// express the original control flow with explicit edges. Local storage is
    /// declared before dispatch, but initializer effects stay at their C sites.</summary>
    private void RenderSwitchWithNestedLabels(StringBuilder output, Switch statement, int indent)
    {
        var prefix = "__sw_flat_" + _switchPastSeq++ + "_";
        var nextLabel = 0;
        string Fresh() => prefix + nextLabel++;
        var end = Fresh();
        var sections = statement.Sections.Select(_ => Fresh()).ToArray();
        var nested = new Dictionary<CaseLabelStmt, string>(ReferenceEqualityComparer.Instance);
        var locals = new List<LocalDecl>();
        var arrays = new List<(ArrayDecl Declaration, int Count)>();
        var seenLocals = new HashSet<Symbol>(ReferenceEqualityComparer.Instance);
        var entryLabels = new Dictionary<CStmt, bool>(ReferenceEqualityComparer.Instance);
        bool HasEntryLabel(CStmt current)
        {
            if (entryLabels.TryGetValue(current, out var result)) return result;
            result = current is CaseLabelStmt or Labeled || SwitchChildren(current).Any(HasEntryLabel);
            entryLabels.Add(current, result);
            return result;
        }
        // Only ancestors of entry labels must become a shared label scope.
        // Ordinary blocks/loops keep both their structured control flow and
        // their local storage. Flattening all branches in a large C switch
        // makes Roslyn repeatedly clone/join thousands of assignment states.
        // Seq is deliberately excluded: it does not introduce a C scope.
        bool KeepStructured(CStmt current) =>
            current is Block or If or While or DoWhile or For && !HasEntryLabel(current);
        void Discover(CStmt current)
        {
            if (KeepStructured(current)) return;
            if (current is CaseLabelStmt label) nested.Add(label, Fresh());
            if (current is DeclStmt declaration)
                foreach (var local in declaration.Decls)
                    if (seenLocals.Add(local.Sym)) locals.Add(local with { Init = null });
            if (current is ArrayDecl array && seenLocals.Add(array.Sym))
            {
                // A fixed array's storage exists when its enclosing block is
                // entered, including entry directly at a later case label.
                // Only initializer effects belong at the declaration site.
                var count = array.Inits?.Count;
                if (count is null && array.CountExpr is LitInt { Value: { } fixedCount }
                    && fixedCount >= 0 && fixedCount <= int.MaxValue)
                    count = (int)fixedCount;
                if (count is null)
                    throw new IrUnsupportedException("variable-length array across a switch entry");
                arrays.Add((array, count.Value));
            }
            foreach (var child in SwitchChildren(current)) Discover(child);
        }
        foreach (var section in statement.Sections)
            foreach (var child in section.Body) Discover(child);

        var pad = Pad(indent);
        var bodyIndent = indent + 1;
        var bodyPad = Pad(bodyIndent);
        output.Append(pad).Append("{\n");
        EmitDeclStmt(output, new DeclStmt(locals), bodyPad);
        foreach (var (array, count) in arrays)
        {
            var element = Cs(array.Element);
            output.Append(bodyPad).Append(element).Append("* ").Append(array.Sym.TargetName)
                .Append(" = stackalloc ").Append(element).Append('[')
                .Append(count.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("];\n");
        }
        var subject = DecayEnum(statement.Subject);
        var value = Hoist(output, bodyPad, () => Coerced(subject, CType.IntegerPromote(subject.Type)));
        output.Append(bodyPad).Append("switch (").Append(value).Append(")\n").Append(bodyPad).Append("{\n");
        var hasDefault = false;
        void Dispatch(SwitchLabel label, string destination)
        {
            hasDefault |= label.CaseExpr is null;
            var text = label.CaseExpr is not { } low ? "default:"
                : label.HiExpr is { } high ? $"case >= {CaseValue(low, statement.Subject.Type)} and <= {CaseValue(high, statement.Subject.Type)}:"
                : $"case {CaseValue(low, statement.Subject.Type)}:";
            output.Append(bodyPad).Append("    ").Append(text).Append(" goto ").Append(destination).Append(";\n");
        }
        for (var index = 0; index < statement.Sections.Count; ++index)
            foreach (var label in statement.Sections[index].Labels) Dispatch(label, sections[index]);
        foreach (var pair in nested) Dispatch(new SwitchLabel(pair.Key.CaseExpr), pair.Value);
        if (!hasDefault) output.Append(bodyPad).Append("    default: goto ").Append(end).Append(";\n");
        output.Append(bodyPad).Append("}\n");

        var savedCases = _gotoCaseMap;
        var savedBreak = _breakAsGoto;
        var savedContinue = _continueAsGoto;
        _gotoCaseMap = null;
        void Label(string label) => output.Append(bodyPad).Append(label).Append(": ;\n");
        void Jump(string label) => output.Append(bodyPad).Append("goto ").Append(label).Append(";\n");
        void BranchFalse(CExpr condition, string destination)
        {
            var expression = Hoist(output, bodyPad, () => LoopCondition(condition));
            output.Append(bodyPad).Append("if (!(").Append(expression).Append(")) goto ").Append(destination).Append(";\n");
        }
        void Render(CStmt current, string breakTarget, string? continueTarget)
        {
            _breakAsGoto = breakTarget;
            _continueAsGoto = continueTarget;
            if (KeepStructured(current))
            {
                Stmt(output, current, bodyIndent);
                return;
            }
            switch (current)
            {
                case Block or Seq:
                    foreach (var child in SwitchChildren(current)) Render(child, breakTarget, continueTarget);
                    break;
                case CaseLabelStmt label:
                    Label(nested[label]); Render(label.Body, breakTarget, continueTarget); break;
                case Labeled label:
                    Label(DotCC.EmitHelpers.Id(label.Name)); Render(label.Body, breakTarget, continueTarget); break;
                case DeclStmt declaration:
                    foreach (var local in declaration.Decls)
                        if (local.Init is { } initial)
                        {
                            var expression = Hoist(output, bodyPad, () => Coerced(initial, local.Sym.Type));
                            output.Append(bodyPad).Append(local.Sym.TargetName).Append(" = ").Append(expression).Append(";\n");
                        }
                    break;
                case ArrayDecl array:
                    if (array.Inits is { } values)
                        for (var index = 0; index < values.Count; ++index)
                        {
                            var elementValue = Hoist(output, bodyPad, () => Coerced(values[index], array.Element));
                            output.Append(bodyPad).Append(array.Sym.TargetName).Append('[').Append(index)
                                .Append("] = ").Append(elementValue).Append(";\n");
                        }
                    break;
                case If conditional:
                {
                    var other = Fresh(); var past = Fresh();
                    BranchFalse(conditional.Cond, other);
                    Render(conditional.Then, breakTarget, continueTarget); Jump(past); Label(other);
                    if (conditional.Else is { } branch) Render(branch, breakTarget, continueTarget);
                    Label(past);
                    break;
                }
                case While loop:
                {
                    var condition = Fresh(); var past = Fresh();
                    Label(condition); BranchFalse(loop.Cond, past);
                    Render(loop.Body, past, condition); Jump(condition); Label(past);
                    break;
                }
                case DoWhile loop:
                {
                    var body = Fresh(); var condition = Fresh(); var past = Fresh();
                    Label(body); Render(loop.Body, past, condition); Label(condition);
                    BranchFalse(loop.Cond, past); Jump(body); Label(past);
                    break;
                }
                case For loop:
                {
                    var condition = Fresh(); var increment = Fresh(); var past = Fresh();
                    if (loop.Init is { } init) Render(init, breakTarget, continueTarget);
                    Label(condition);
                    if (loop.Cond is { } test) BranchFalse(test, past);
                    Render(loop.Body, past, increment); Label(increment);
                    if (loop.Post is { } post) Stmt(output, new ExprStmt(post), bodyIndent);
                    Jump(condition); Label(past);
                    break;
                }
                default:
                    Stmt(output, current, bodyIndent);
                    break;
            }
        }
        try
        {
            for (var index = 0; index < statement.Sections.Count; ++index)
            {
                Label(sections[index]);
                foreach (var child in statement.Sections[index].Body) Render(child, end, savedContinue);
            }
            Label(end);
        }
        finally
        {
            _gotoCaseMap = savedCases;
            _breakAsGoto = savedBreak;
            _continueAsGoto = savedContinue;
        }
        output.Append(pad).Append("}\n");
    }
}
