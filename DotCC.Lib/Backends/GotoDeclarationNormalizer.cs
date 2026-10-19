using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

// A forward C goto can skip an initializer and later guard use of that local
// with another variable. Roslyn does not prove that correlation. Reserve only
// crossed scalar storage on block entry and retain initializer effects in place.
internal static class GotoDeclarationNormalizer
{
    internal static Block Normalize(Block body) => (Block)Rewrite(body);

    private static CStmt Rewrite(CStmt statement) => statement switch
    {
        Block block => RewriteBlock(block),
        Seq sequence => sequence with { Stmts = sequence.Stmts.Select(Rewrite).ToArray() },
        Labeled label => label with { Body = Rewrite(label.Body) },
        If branch => branch with { Then = Rewrite(branch.Then), Else = branch.Else is { } other ? Rewrite(other) : null },
        While loop => loop with { Body = Rewrite(loop.Body) },
        DoWhile loop => loop with { Body = Rewrite(loop.Body) },
        For loop => loop with { Body = Rewrite(loop.Body) },
        Switch choice => choice with { Sections = choice.Sections.Select(section => new SwitchSection(section.Labels,
            section.Body.Select(Rewrite).ToArray())).ToArray() },
        _ => statement,
    };

    private static Block RewriteBlock(Block block)
    {
        var statements = block.Stmts.Select(Rewrite).SelectMany(Flatten).ToArray();
        var labels = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < statements.Length; index++)
            for (var current = statements[index]; current is Labeled label; current = label.Body)
                labels[label.Name] = index;
        var crossed = new HashSet<int>();
        for (var index = 0; index < statements.Length; index++)
            foreach (var target in Jumps(statements[index]))
                if (labels.TryGetValue(target, out var destination) && destination > index)
                    for (var position = index + 1; position < destination; position++)
                        if (statements[position] is DeclStmt) crossed.Add(position);
        if (crossed.Count == 0) return block with { Stmts = statements };
        var storage = new List<LocalDecl>();
        foreach (var index in crossed.Order())
        {
            var declaration = (DeclStmt)statements[index];
            // Cursor references have their own scoped binding/escape contract.
            if (declaration.Decls.Any(local => VaListLifetimeValidator.IsCursorReference(local.Sym.Type))) continue;
            storage.AddRange(declaration.Decls.Select(local => local with { Init = null }));
            statements[index] = new Seq(declaration.Decls.Where(local => local.Init is not null)
                .Select(local => (CStmt)new ExprStmt(new Assign(null,
                    new VarRef(local.Sym) { Type = local.Sym.Type, IsLValue = true }, local.Init!)
                    { Type = local.Sym.Type })).ToArray());
        }
        return block with { Stmts = new CStmt[] { new DeclStmt(storage) }.Concat(statements).ToArray() };
    }

    private static IEnumerable<CStmt> Flatten(CStmt statement) => statement is Seq sequence
        ? sequence.Stmts.SelectMany(Flatten) : new[] { statement };

    private static IEnumerable<string> Jumps(CStmt statement)
    {
        if (statement is Goto jump) yield return jump.Label;
        IEnumerable<CStmt> children = statement switch
        {
            Block block => block.Stmts,
            Seq sequence => sequence.Stmts,
            Labeled label => new[] { label.Body },
            If branch => branch.Else is { } other ? new[] { branch.Then, other } : new[] { branch.Then },
            While loop => new[] { loop.Body },
            DoWhile loop => new[] { loop.Body },
            For loop => new[] { loop.Body },
            Switch choice => choice.Sections.SelectMany(section => section.Body),
            _ => Array.Empty<CStmt>(),
        };
        foreach (var child in children)
            foreach (var target in Jumps(child)) yield return target;
    }
}
