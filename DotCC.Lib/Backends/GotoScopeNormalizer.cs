#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Backends;

using DotCC.Ir;

/// <summary>
/// C#-backend IR pass: make every <c>goto</c> target legal under C# label
/// scoping. C lets a <c>goto</c> jump INTO a nested block (chibi:
/// <c>goto adjust;</c> from outside the <c>if</c> whose body declares
/// <c>adjust:</c>); C# scopes a label to its enclosing block, so that render
/// is CS0159.
/// </summary>
/// <remarks>
/// The normalization hoists the labeled TAIL of the offending block out to the
/// parent, one level at a time, until the label is visible from every goto:
/// <code>
///   if (P) { head; L: tail; }   rest;
/// </code>
/// becomes
/// <code>
///   if (P) { head; goto L; }  goto __skip;  L: tail;  __skip: ;  rest;
/// </code>
/// Control-flow equivalent: head still falls into the tail (via the explicit
/// goto), every other exit of the <c>if</c> skips the tail, and the tail's own
/// fall-through reaches <c>rest</c> exactly as block-exit did. Only if-arms and
/// plain nested blocks are hoisted through. An externally entered loop is lowered
/// to explicit condition/body/post labels, preserving later iterations and the
/// targets of break/continue. Crossed local storage is reserved at function entry;
/// initializer effects remain in place. Switch entry has its own backend pass.
/// Functions with no scope violation pass through untouched (the common case —
/// the pass costs one read-only scan).
/// </remarks>
internal static class GotoScopeNormalizer
{
    public static Block Normalize(Block body)
    {
        // Each hoist lifts the label one block level; a C function body has
        // bounded nesting, so this converges fast — the guard is a backstop.
        for (var guard = 0; guard < 64; guard++)
        {
            var label = FindViolation(body);
            if (label == null) { return body; }
            var h = new Hoister(label, guard);
            body = h.Rewrite(body);
            if (h.Done && h.Storage.Count != 0)
                body = body with { Stmts = h.Storage.Concat(body.Stmts).ToList() };
            if (!h.Done)
            {
                throw new IrUnsupportedException(
                    $"goto into a block the normalizer cannot hoist through (label '{label}' — a loop/switch body, or an unhandled nesting shape)");
            }
        }
        throw new IrUnsupportedException("goto/label normalization did not converge");
    }

    /// <summary>Find a label declared in a block that does NOT enclose one of
    /// its gotos (C# visibility rule: the label's scope chain must be a prefix
    /// of the goto's). Labels inside a <see cref="Switch"/> are skipped —
    /// RenderSwitch owns their internal entry paths. PrepareFunctionSwitchEntries
    /// handles external entry into function-body switches and diagnoses unsupported
    /// enclosing scopes before emission.</summary>
    private static string? FindViolation(Block body)
    {
        var labelChain = new Dictionary<string, List<object>>(StringComparer.Ordinal);
        var inSwitch = new HashSet<string>(StringComparer.Ordinal);
        var gotos = new List<(string Label, List<object> Chain)>();
        var chain = new List<object>();
        var switchDepth = 0;

        // The C# renderer braces embedded statements even when C omitted braces.
        // Track those scopes too, so a directly labeled loop/if body cannot make
        // an external entry look visible merely because the C AST lacks a Block.
        void WalkScoped(CStmt? statement)
        {
            if (statement is null) return;
            if (statement is Block) { Walk(statement); return; }
            chain.Add(statement);
            Walk(statement);
            chain.RemoveAt(chain.Count - 1);
        }

        void Walk(CStmt? s)
        {
            switch (s)
            {
                case null: return;
                case Block b:
                    chain.Add(b);
                    foreach (var st in b.Stmts) { Walk(st); }
                    chain.RemoveAt(chain.Count - 1);
                    return;
                case Seq q: // braceless — no scope of its own
                    foreach (var st in q.Stmts) { Walk(st); }
                    return;
                case Labeled l:
                    labelChain[l.Name] = new List<object>(chain);
                    if (switchDepth > 0) { inSwitch.Add(l.Name); }
                    Walk(l.Body);
                    return;
                case Goto g: gotos.Add((g.Label, new List<object>(chain))); return;
                case If f: WalkScoped(f.Then); WalkScoped(f.Else); return;
                case While w: WalkScoped(w.Body); return;
                case DoWhile dw: WalkScoped(dw.Body); return;
                case For fo: Walk(fo.Init); WalkScoped(fo.Body); return;
                case SetjmpGuard sj: Walk(sj.TryBody); Walk(sj.CatchBody); return;
                // The capture's own goto-restart label/goto are backend-synthetic (never IR
                // nodes), so only its body carries user gotos/labels to normalize.
                case SetjmpCapture sc: Walk(sc.Body); return;
                case CaseLabelStmt cl: Walk(cl.Body); return;
                case Switch sw:
                    switchDepth++;
                    foreach (var sec in sw.Sections)
                    {
                        foreach (var st in sec.Body) { Walk(st); }
                    }
                    switchDepth--;
                    return;
                default: return;
            }
        }
        Walk(body);

        foreach (var (label, gchain) in gotos)
        {
            if (!labelChain.TryGetValue(label, out var lchain) || inSwitch.Contains(label)) { continue; }
            var visible = lchain.Count <= gchain.Count
                && !lchain.Where((blk, i) => !ReferenceEquals(blk, gchain[i])).Any();
            if (!visible) { return label; }
        }
        return null;
    }

    /// <summary>One hoist of one label: finds the statement whose if-arm /
    /// nested block declares the label at top level, splits the arm there, and
    /// splices the tail (plus the skip jump) into the parent statement list.</summary>
    private sealed class Hoister
    {
        private readonly string _label;
        public bool Done { get; private set; }

        private readonly int _ordinal;
        private readonly List<CStmt> _storage = new();
        public IReadOnlyList<CStmt> Storage => _storage;

        public Hoister(string label, int ordinal) { _label = label; _ordinal = ordinal; }

        public Block Rewrite(Block b)
        {
            if (Done) { return b; }
            var stmts = new List<CStmt>(b.Stmts);
            for (var i = 0; i < stmts.Count; i++)
            {
                if (TryExtract(stmts[i], out var replaced, out var tail))
                {
                    var skip = $"__skip_{_label}_{_ordinal}";
                    var insert = new List<CStmt> { replaced, new Goto(skip) };
                    insert.AddRange(tail);
                    insert.Add(new Labeled(skip, new Block(Array.Empty<CStmt>())));
                    stmts.RemoveAt(i);
                    stmts.InsertRange(i, insert);
                    Done = true;
                    return new Block(stmts) { Pos = b.Pos };
                }
            }
            for (var i = 0; i < stmts.Count && !Done; i++) { stmts[i] = RewriteStmt(stmts[i]); }
            return new Block(stmts) { Pos = b.Pos };
        }

        /// <summary>When <paramref name="s"/> is an <c>if</c> (either arm) or a
        /// plain nested block whose TOP-LEVEL statements declare the label:
        /// produce the statement with the labeled tail replaced by
        /// <c>goto label;</c>, and the extracted tail.</summary>
        private bool TryExtract(CStmt s, out CStmt replaced, out List<CStmt> tail)
        {
            switch (s)
            {
                case If f when SplitArm(f.Then, out var thenHead, out tail!):
                    replaced = f with { Then = thenHead };
                    return true;
                case If f when f.Else is { } el && SplitArm(el, out var elseHead, out tail!):
                    replaced = f with { Else = elseHead };
                    return true;
                // Braceless nested if/loop statements have no intervening Block
                // list. Extract through either arm and rebuild that branch spine.
                case If f when TryExtract(f.Then, out var newThen, out tail!):
                    replaced = f with { Then = newThen };
                    return true;
                case If f when f.Else is { } nested && TryExtract(nested, out var newElse, out tail!):
                    replaced = f with { Else = newElse };
                    return true;
                case Block nb when SplitArm(nb, out var head, out tail!):
                    replaced = head;
                    return true;
                case Labeled label when TryExtract(label.Body, out var labeledHead, out tail!):
                    replaced = label with { Body = labeledHead };
                    return true;
                case While loop when SplitArm(loop.Body, out var loopHead, out tail!):
                    LowerLoop(loopHead, tail, loop.Cond, null, false, out replaced, out tail);
                    return true;
                case DoWhile loop when SplitArm(loop.Body, out var loopHead, out tail!):
                    LowerLoop(loopHead, tail, loop.Cond, null, true, out replaced, out tail);
                    return true;
                case For loop when SplitArm(loop.Body, out var loopHead, out tail!):
                    LowerLoop(loopHead, tail, loop.Cond, loop.Post, false, out replaced, out tail);
                    if (loop.Init is { } init)
                        replaced = new Seq(new[] { LiftStorage(init), replaced });
                    return true;
            }
            replaced = s;
            tail = new List<CStmt>();
            return false;
        }

        /// <summary>Split a block at <c>label:</c> (top level only): head keeps
        /// everything before it plus the re-entry <c>goto label;</c>.</summary>
        private bool SplitArm(CStmt arm, out CStmt head, out List<CStmt> tail)
        {
            var ab = AsBlock(arm);
            {
                for (var k = 0; k < ab.Stmts.Count; k++)
                {
                    if (ab.Stmts[k] is Labeled l && l.Name == _label)
                    {
                        // Once a label crosses this scope, its locals need function-entry
                        // storage even on entry that skips their initializer effects.
                        var lifted = ab.Stmts.Select(LiftStorage).ToList();
                        tail = lifted.Skip(k).ToList();
                        var headStmts = lifted.Take(k).Append(new Goto(_label)).ToList();
                        head = new Block(headStmts) { Pos = ab.Pos };
                        return true;
                    }
                }
            }
            head = arm;
            tail = new List<CStmt>();
            return false;
        }

        private CStmt LiftStorage(CStmt statement)
        {
            switch (statement)
            {
                case DeclStmt declaration:
                    _storage.Add(declaration with
                    {
                        Decls = declaration.Decls.Select(local => local with { Init = null }).ToList()
                    });
                    return new Seq(declaration.Decls.Where(local => local.Init is not null)
                        .Select(local => (CStmt)new ExprStmt(new Assign(null,
                            new VarRef(local.Sym) { Type = local.Sym.Type }, local.Init!)
                            { Type = local.Sym.Type })).ToList());
                case ArrayDecl array:
                    var count = array.Inits?.Count;
                    if (count is null && array.CountExpr is LitInt { Value: { } fixedCount }
                        && fixedCount >= 0 && fixedCount <= int.MaxValue) count = (int)fixedCount;
                    if (count is null)
                        throw new IrUnsupportedException("variable-length array across a goto entry");
                    _storage.Add(array with
                    {
                        CountExpr = new LitInt(count.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), count.Value)
                            { Type = CType.Int },
                        Inits = null
                    });
                    return new Seq((array.Inits ?? Array.Empty<CExpr>()).Select((value, index) =>
                        (CStmt)new ExprStmt(new Assign(null,
                            new Index(new VarRef(array.Sym) { Type = array.Sym.Type },
                                new LitInt(index.ToString(System.Globalization.CultureInfo.InvariantCulture), index) { Type = CType.Int })
                                { Type = array.Element }, value) { Type = array.Element })).ToList());
                case Seq sequence:
                    return sequence with { Stmts = sequence.Stmts.Select(LiftStorage).ToList() };
                case Labeled label:
                    return label with { Body = LiftStorage(label.Body) };
                default:
                    return statement;
            }
        }

        // Lower only a loop crossed by an actual incoming goto. The first jump
        // enters its original test (or body for do/while); external label entry
        // skips that path. Every later iteration preserves test/post ordering.
        private void LowerLoop(CStmt head, List<CStmt> originalTail, CExpr? condition,
            CExpr? post, bool doFirst, out CStmt replaced, out List<CStmt> tail)
        {
            var prefix = $"__goto_loop_{_label}_{_ordinal}";
            var body = prefix + "_body";
            var test = prefix + "_test";
            var next = prefix + "_next";
            var end = prefix + "_end";
            replaced = new Goto(doFirst ? body : test);
            tail = new List<CStmt> { new Labeled(body, RewriteLoopTransfers(head, end, next, false)) };
            tail.AddRange(originalTail.Select(statement => RewriteLoopTransfers(statement, end, next, false)));
            tail.Add(new Labeled(next, new Block(Array.Empty<CStmt>())));
            if (post is not null) tail.Add(new ExprStmt(post));
            tail.Add(new Labeled(test, condition is null
                ? new Goto(body)
                : new If(condition, new Goto(body), null)));
            tail.Add(new Labeled(end, new Block(Array.Empty<CStmt>())));
        }

        private static CStmt RewriteLoopTransfers(CStmt statement, string end, string next, bool inSwitch) => statement switch
        {
            Break when !inSwitch => new Goto(end),
            Continue => new Goto(next),
            Block block => block with { Stmts = block.Stmts.Select(child => RewriteLoopTransfers(child, end, next, inSwitch)).ToList() },
            Seq sequence => sequence with { Stmts = sequence.Stmts.Select(child => RewriteLoopTransfers(child, end, next, inSwitch)).ToList() },
            Labeled label => label with { Body = RewriteLoopTransfers(label.Body, end, next, inSwitch) },
            If branch => branch with
            {
                Then = RewriteLoopTransfers(branch.Then, end, next, inSwitch),
                Else = branch.Else is { } other ? RewriteLoopTransfers(other, end, next, inSwitch) : null
            },
            Switch choice => choice with
            {
                Sections = choice.Sections.Select(section => new SwitchSection(section.Labels,
                    section.Body.Select(child => RewriteLoopTransfers(child, end, next, true)).ToList())).ToList()
            },
            CaseLabelStmt label => label with { Body = RewriteLoopTransfers(label.Body, end, next, inSwitch) },
            // Nested loops own both break and continue; exception entry remains
            // unsupported rather than moving control across protected regions.
            _ => statement
        };

        private static Block AsBlock(CStmt statement) => statement as Block ?? new Block(new[] { statement });

        private CStmt RewriteStmt(CStmt s) => s switch
        {
            Block b => Rewrite(b),
            Seq q => new Seq(q.Stmts.Select(RewriteStmt).ToList()) { Pos = q.Pos },
            Labeled l => l with { Body = RewriteStmt(l.Body) },
            If f => f with { Then = Rewrite(AsBlock(f.Then)), Else = f.Else is { } e ? Rewrite(AsBlock(e)) : null },
            While w => w with { Body = Rewrite(AsBlock(w.Body)) },
            DoWhile dw => dw with { Body = Rewrite(AsBlock(dw.Body)) },
            For fo => fo with { Body = Rewrite(AsBlock(fo.Body)) },
            SetjmpGuard sj => sj with
            {
                TryBody = sj.TryBody is { } tb ? RewriteStmt(tb) : null,
                CatchBody = sj.CatchBody is { } cb ? RewriteStmt(cb) : null,
            },
            SetjmpCapture sc => sc with { Body = RewriteStmt(sc.Body) },
            CaseLabelStmt cl => cl with { Body = RewriteStmt(cl.Body) },
            Switch sw => sw with
            {
                Sections = sw.Sections
                    .Select(sec => new SwitchSection(sec.Labels, sec.Body.Select(RewriteStmt).ToList()))
                    .ToList(),
            },
            _ => s,
        };
    }
}
