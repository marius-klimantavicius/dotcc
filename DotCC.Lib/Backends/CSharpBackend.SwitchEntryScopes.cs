using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly HashSet<Switch> _functionScopeSwitches = new(ReferenceEqualityComparer.Instance);

    /// <summary>Only scopes that contain entry labels need flattened storage.
    /// Share this analysis between ordinary dispatch and external-entry hoisting.</summary>
    private sealed class SwitchEntryAnalysis
    {
        public readonly List<CaseLabelStmt> NestedCases = new();
        public readonly List<LocalDecl> Locals = new();
        public readonly List<(ArrayDecl Declaration, int Count)> Arrays = new();
        private readonly HashSet<Symbol> _seenLocals = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<CStmt, bool> _entryLabels = new(ReferenceEqualityComparer.Instance);

        public SwitchEntryAnalysis(Switch statement)
        {
            foreach (var section in statement.Sections)
                foreach (var child in section.Body) Discover(child);
        }

        private bool HasEntryLabel(CStmt current)
        {
            if (_entryLabels.TryGetValue(current, out var result)) return result;
            result = current is CaseLabelStmt or Labeled || SwitchChildren(current).Any(HasEntryLabel);
            _entryLabels.Add(current, result);
            return result;
        }

        // Flatten only ancestors of entry labels. Keeping other control flow structured
        // avoids Roslyn definite-assignment blowups; Seq adds no scope to preserve.
        public bool KeepStructured(CStmt current) =>
            current is Block or If or While or DoWhile or For && !HasEntryLabel(current);

        private void Discover(CStmt current)
        {
            if (KeepStructured(current)) return;
            if (current is CaseLabelStmt label) NestedCases.Add(label);
            if (current is DeclStmt declaration)
                foreach (var local in declaration.Decls)
                    if (_seenLocals.Add(local.Sym)) Locals.Add(local with { Init = null });
            if (current is ArrayDecl array && _seenLocals.Add(array.Sym))
            {
                var count = array.Inits?.Count;
                if (count is null && array.CountExpr is LitInt { Value: { } fixedCount }
                    && fixedCount >= 0 && fixedCount <= int.MaxValue)
                    count = (int)fixedCount;
                if (count is null)
                    throw new IrUnsupportedException("variable-length array across a switch entry");
                Arrays.Add((array, count.Value));
            }
            foreach (var child in SwitchChildren(current)) Discover(child);
        }
    }

    private Block PrepareFunctionSwitchEntries(Block body)
    {
        _functionScopeSwitches.Clear();
        var direct = new List<Switch>();
        void Direct(CStmt current)
        {
            if (current is Switch statement) direct.Add(statement);
            else if (current is Labeled label) Direct(label.Body);
            else if (current is Seq sequence)
                foreach (var child in sequence.Stmts) Direct(child);
        }
        foreach (var child in body.Stmts) Direct(child);

        var owners = new Dictionary<string, Switch>(StringComparer.Ordinal);
        var jumps = new List<(string Label, Switch[] Ancestors)>();
        var ancestors = new List<Switch>();
        void Walk(CStmt current)
        {
            switch (current)
            {
                case Switch statement:
                    ancestors.Add(statement);
                    foreach (var section in statement.Sections)
                        foreach (var child in section.Body) Walk(child);
                    ancestors.RemoveAt(ancestors.Count - 1);
                    return;
                case Goto jump:
                    jumps.Add((jump.Label, ancestors.ToArray()));
                    return;
                case Labeled label when ancestors.Count != 0:
                    owners[label.Name] = ancestors[^1];
                    break;
                case SetjmpGuard guard:
                    if (guard.TryBody is { } attempt) Walk(attempt);
                    if (guard.CatchBody is { } handler) Walk(handler);
                    return;
                case SetjmpCapture capture:
                    Walk(capture.Body);
                    return;
            }
            foreach (var child in SwitchChildren(current)) Walk(child);
        }
        Walk(body);
        foreach (var (label, chain) in jumps)
        {
            if (!owners.TryGetValue(label, out var owner)
                || chain.Any(scope => ReferenceEquals(scope, owner))) continue;
            if (!direct.Any(scope => ReferenceEquals(scope, owner)))
                throw new IrUnsupportedException("goto into a switch nested inside another statement is not supported (label '" + label + "')");
            _functionScopeSwitches.Add(owner);
        }
        if (_functionScopeSwitches.Count == 0) return body;

        // C labels have function scope. Removing the dispatcher's synthetic
        // block exposes its labels, but a jump can also skip declarations there.
        // Materialize only those scopes' storage at function entry; initializer
        // effects stay at their original positions in the switch renderer.
        var storage = new List<CStmt>();
        foreach (var statement in direct.Where(_functionScopeSwitches.Contains))
        {
            var analysis = new SwitchEntryAnalysis(statement);
            if (analysis.Locals.Count != 0) storage.Add(new DeclStmt(analysis.Locals));
            foreach (var (array, count) in analysis.Arrays)
                storage.Add(array with
                {
                    CountExpr = new LitInt(count.ToString(System.Globalization.CultureInfo.InvariantCulture), count) { Type = CType.Int },
                    Inits = null,
                });
        }
        return body with { Stmts = storage.Concat(body.Stmts).ToList() };
    }
}
