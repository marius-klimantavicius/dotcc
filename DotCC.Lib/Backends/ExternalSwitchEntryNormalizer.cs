using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

/// <summary>Expose externally entered switch labels to the ordinary goto-scope
/// pass. Only switches crossed by a real incoming jump are lowered. Dispatch
/// still evaluates its subject once on ordinary entry; direct label entry skips
/// dispatch, and initializer effects remain in their original statements.</summary>
internal static class ExternalSwitchEntryNormalizer
{
    public static Block Normalize(Block body)
    {
        var owners = new Dictionary<string, Switch[]>(StringComparer.Ordinal);
        var jumps = new List<(string Label, Switch[] Ancestors)>();
        var ancestors = new List<Switch>();
        void Discover(CStmt statement)
        {
            if (statement is Switch choice)
            {
                ancestors.Add(choice);
                foreach (var section in choice.Sections)
                    foreach (var child in section.Body) Discover(child);
                ancestors.RemoveAt(ancestors.Count - 1);
                return;
            }
            if (statement is Labeled label) owners[label.Name] = ancestors.ToArray();
            if (statement is Goto jump) jumps.Add((jump.Label, ancestors.ToArray()));
            foreach (var child in Children(statement)) Discover(child);
        }
        Discover(body);
        var crossed = new HashSet<Switch>(ReferenceEqualityComparer.Instance);
        foreach (var (label, from) in jumps)
        {
            if (!owners.TryGetValue(label, out var to)) continue;
            var common = 0;
            while (common < from.Length && common < to.Length && ReferenceEquals(from[common], to[common])) common++;
            for (var i = common; i < to.Length; ++i) crossed.Add(to[i]);
        }
        if (crossed.Count == 0) return body;
        var ordinal = 0;
        CStmt Rewrite(CStmt statement)
        {
            if (statement is Switch original)
            {
                var choice = original with
                {
                    Sections = original.Sections.Select(section => section with
                        { Body = section.Body.Select(Rewrite).ToList() }).ToList()
                };
                if (!crossed.Contains(original)) return choice;
                return Lower(choice, ordinal++);
            }
            return MapChildren(statement, Rewrite);
        }
        return (Block)Rewrite(body);
    }

    private static Block Lower(Switch choice, int ordinal)
    {
        var prefix = "__switch_entry_" + ordinal + "_";
        var end = prefix + "end";
        var next = 0;
        var dispatch = new List<SwitchSection>();
        var statements = new List<CStmt>();
        CStmt Rewrite(CStmt statement, bool ownsBreak)
        {
            switch (statement)
            {
                case Break when ownsBreak: return new Goto(end) { Pos = statement.Pos };
                case CaseLabelStmt label:
                {
                    var target = prefix + next++;
                    dispatch.Add(new SwitchSection(new[] {new SwitchLabel(label.CaseExpr)}, new CStmt[] {new Goto(target)}));
                    return new Labeled(target, Rewrite(label.Body, ownsBreak)) { Pos = label.Pos };
                }
                // Nested switches own their cases and breaks. A nested loop owns
                // its breaks, while cases nested in that loop still belong here.
                case Switch: return statement;
                case While loop: return loop with { Body = Rewrite(loop.Body, false) };
                case DoWhile loop: return loop with { Body = Rewrite(loop.Body, false) };
                case For loop: return loop with { Body = Rewrite(loop.Body, false) };
                default: return MapChildren(statement, child => Rewrite(child, ownsBreak));
            }
        }
        foreach (var section in choice.Sections)
        {
            var label = prefix + next++;
            if (section.Labels.Count != 0)
                dispatch.Add(new SwitchSection(section.Labels, new CStmt[] {new Goto(label)}));
            statements.Add(new Labeled(label, new Seq(Array.Empty<CStmt>())));
            statements.AddRange(section.Body.Select(child => Rewrite(child, true)));
        }
        if (!dispatch.SelectMany(section => section.Labels).Any(label => label.CaseExpr is null))
            dispatch.Add(new SwitchSection(new[] {new SwitchLabel(null)}, new CStmt[] {new Goto(end)}));
        statements.Insert(0, choice with { Sections = dispatch });
        statements.Add(new Labeled(end, new Seq(Array.Empty<CStmt>())));
        return new Block(statements) { Pos = choice.Pos };
    }

    private static IEnumerable<CStmt> Children(CStmt statement) => statement switch
    {
        Block block => block.Stmts,
        Seq sequence => sequence.Stmts,
        Labeled label => new[] {label.Body},
        CaseLabelStmt label => new[] {label.Body},
        If branch => branch.Else is { } other ? new[] {branch.Then, other} : new[] {branch.Then},
        While loop => new[] {loop.Body},
        DoWhile loop => new[] {loop.Body},
        For loop => loop.Init is { } init ? new[] {init, loop.Body} : new[] {loop.Body},
        SetjmpGuard guard => new[] {guard.TryBody, guard.CatchBody}.OfType<CStmt>(),
        SetjmpCapture capture => new[] {capture.Body},
        _ => Array.Empty<CStmt>(),
    };

    private static CStmt MapChildren(CStmt statement, Func<CStmt, CStmt> rewrite) => statement switch
    {
        Block block => block with { Stmts = block.Stmts.Select(rewrite).ToList() },
        Seq sequence => sequence with { Stmts = sequence.Stmts.Select(rewrite).ToList() },
        Labeled label => label with { Body = rewrite(label.Body) },
        CaseLabelStmt label => label with { Body = rewrite(label.Body) },
        If branch => branch with { Then = rewrite(branch.Then), Else = branch.Else is { } other ? rewrite(other) : null },
        While loop => loop with { Body = rewrite(loop.Body) },
        DoWhile loop => loop with { Body = rewrite(loop.Body) },
        For loop => loop with { Init = loop.Init is { } init ? rewrite(init) : null, Body = rewrite(loop.Body) },
        SetjmpGuard guard => guard with
        {
            TryBody = guard.TryBody is { } attempt ? rewrite(attempt) : null,
            CatchBody = guard.CatchBody is { } handler ? rewrite(handler) : null,
        },
        SetjmpCapture capture => capture with { Body = rewrite(capture.Body) },
        _ => statement,
    };
}
