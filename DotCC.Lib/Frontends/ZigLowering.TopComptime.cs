#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly List<Item> _topComptime = new();
    private readonly HashSet<string> _privateTopFunctions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _cExportTopFunctions = new(StringComparer.Ordinal);
    private int _topComptimeState;
    private System.Runtime.ExceptionServices.ExceptionDispatchInfo? _topComptimeFailure;

    private void ThrowTopComptimeFailure() => _topComptimeFailure?.Throw();

    private void RecordTopComptime(IReadOnlyList<Item> declarations)
    {
        if (_modulePrefix is null) { TopRoots.GetValue(_ir, _ => new TopRootModule(this)); }
        foreach (var declaration in declarations)
        {
            var inner = Unwrap(declaration);
            if (inner.Content is Zig.FnDef exported && declaration.Content is Zig.ExportFn)
                _cExportTopFunctions.Add(Tok(exported.Arg1));
            if (inner.Content is Zig.TopComptime block) { _topComptime.Add(block.Arg1); }
            if (inner.Content is Zig.FnDef function && declaration.Content is not (Zig.PubFn or Zig.ExportFn)
                && declaration.Content is not Zig.NoinlineFn { Arg0: not null }
                && !IsInlineFnName(function.Arg1))
            {
                _privateTopFunctions.Add(Tok(function.Arg1));
            }
        }
    }

    // Copy before declaration, so every lookup, call and address shares the final symbol.
    // Preserve symbol metadata while giving private source functions internal linkage.
    private Symbol ApplyZigFunctionLinkage(Symbol symbol)
    {
        if (!_moduleFnDecls.TryGetValue(symbol.Name, out var declaration) || declaration.Content is not Zig.FnDef function
            || IsTypeKeyword(function.Arg7) || CollectParamInfos(function.Arg3, out _).Any(parameter => parameter.IsComptime || parameter.Kind == ParamKind.AnyType))
            return symbol;
        var result = symbol;
        if (_privateTopFunctions.Contains(symbol.Name) && symbol.Storage == Storage.None)
        {
            result = new Symbol
            {
                Name = symbol.Name, Kind = symbol.Kind, Type = symbol.Type, Storage = Storage.Static,
                TargetName = symbol.TargetName, IsGlobal = symbol.IsGlobal, Alignment = symbol.Alignment,
                DeclaredArrayType = symbol.DeclaredArrayType, ConstValue = symbol.ConstValue,
                FromSystemHeader = symbol.FromSystemHeader, AddressTaken = symbol.AddressTaken,
                IsMacroGenerated = symbol.IsMacroGenerated, IsNoReturn = symbol.IsNoReturn,
                IsInline = symbol.IsInline, IntrinsicOverride = symbol.IntrinsicOverride,
                IsWeak = symbol.IsWeak, IsThreadLocal = symbol.IsThreadLocal, IsConstexpr = symbol.IsConstexpr,
                Nodiscard = symbol.Nodiscard, Deprecated = symbol.Deprecated,
            };
        }
        TopFunctionAbis.GetValue(result, _ => new TopFunctionAbi(this, function.Arg5, _cExportTopFunctions.Contains(symbol.Name)));
        return result;
    }

    private sealed class TopFunctionAbi(ZigLowering owner, Item? convention, bool defaultC)
    {
        internal bool CAbi => convention?.Content is Zig.CallConv call
            ? call.Arg2.Content is Zig.EnumLit literal ? Tok(literal.Arg1) == "c"
                : owner.TryEvalComptimeTag(call.Arg2, out var tag, out _) && tag == "c"
            : defaultC;
    }
    private static readonly ConditionalWeakTable<Symbol, TopFunctionAbi> TopFunctionAbis = new();

    private CExpr? TopFunctionAliasValue(string name)
    {
        if (_symbols.Resolve(name) is { IsGlobal: false } || !_declAliases.ContainsKey(name)) { return null; }
        if (ResolveExportedDecl(name) is not { Sym: { Kind: SymKind.Func } canonical, Owner: var owner }
            || owner.IsFnTemplate(canonical)) { return null; }
        return new VarRef(canonical) { Type = canonical.Type };
    }

    private void ExecuteTopComptime()
    {
        ThrowTopComptimeFailure();
        if (_topComptimeState != 0) { return; }
        _topComptimeState = 1; // import cycles may reference a module still being analyzed
        using var context = new FnStateScope(this);
        using var comptime = EnterComptime();
        try
        {
            RootExplicitExportFunctions();
            foreach (var block in _topComptime)
            {
                var session = _ir.BeginComptimeSession();
                try { ExecuteTopComptimeStmt(block, session); }
                finally { _ir.EndComptimeSession(session); }
            }
            _topComptimeState = 2;
        }
        catch (Exception failure)
        {
            _topComptimeFailure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure);
            throw;
        }
    }

    private sealed class TopComptimeJump(bool isContinue) : Exception
    {
        internal bool IsContinue { get; } = isContinue;
    }

    // Source-order execution is essential: lowering an entire runtime-shaped block
    // would analyze untaken @compileError/@export arms before interpreting the guard.
    private void ExecuteTopComptimeStmt(Item statement, IrBuilder.ComptimeSession session)
    {
        switch (statement.Content)
        {
            case Zig.Block block:
            {
                using var symbols = EnterSymbolScope();
                var restore = SaveTopComptimeBindings();
                var deferred = new Stack<Item>();
                try
                {
                    foreach (var child in Flatten(block.Arg1))
                    {
                        if (child.Content is Zig.StmtDefer defer) { deferred.Push(defer.Arg1); }
                        else if (child.Content is Zig.StmtErrdefer)
                        {
                            throw new IrUnsupportedException("zig top-level comptime: errdefer is not supported");
                        }
                        else { ExecuteTopComptimeStmt(child, session); }
                    }
                }
                finally
                {
                    try { while (deferred.TryPop(out var cleanup)) { ExecuteTopComptimeStmt(cleanup, session); } }
                    finally { restore(); }
                }
                return;
            }
            case Zig.StmtIf { Arg4: null } branch when ElseStmtParts(branch.Arg6) is { ErrName: null } other:
                ExecuteTopComptimeIf(branch.Arg2, branch.Arg5, other.Body, session); return;
            case Zig.StmtIfThen { Arg4: null } branch when ElseStmtParts(branch.Arg6) is { ErrName: null } other:
                ExecuteTopComptimeIf(branch.Arg2, branch.Arg5, other.Body, session); return;
            case Zig.StmtComptimeIf branch:
                ExecuteTopComptimeIf(branch.Arg3, branch.Arg5, null, session); return;
            case Zig.StmtComptimeIfElse branch:
                ExecuteTopComptimeIf(branch.Arg3, branch.Arg5, branch.Arg7, session); return;
            case Zig.InlineLoop:
                throw new CompileException("zig: redundant inline keyword in comptime scope");
            case Zig.ForStmt loop:
                ExecuteTopComptimeFor(loop.Arg2, loop.Arg6, loop.Arg9, loop.Arg10, session); return;
            case Zig.ForStmtThen loop:
                ExecuteTopComptimeFor(loop.Arg2, loop.Arg6, loop.Arg9, loop.Arg10, session); return;
            case Zig.WhileStmt { Arg4: null } loop:
                ExecuteTopComptimeWhile(loop.Arg2, loop.Arg5, loop.Arg6, loop.Arg7, session); return;
            case Zig.StmtBreak: throw new TopComptimeJump(false);
            case Zig.StmtContinue: throw new TopComptimeJump(true);
            case Zig.StmtExpr { Arg0.Content: Zig.BuiltinCall builtin } when Tok(builtin.Arg0) == "@export":
                ExportBuiltin(Flatten(builtin.Arg2)); return;
            case Zig.StmtExpr { Arg0: var expression } when ResolveModulePath(expression) is not null:
                return;
            case Zig.StmtAssign { Arg0.Content: Zig.Ident discard, Arg1.Content: Zig.AopAssign } assign
                when Tok(discard.Arg0) == "_" && ResolveModulePath(assign.Arg2) is not null:
                return;
            case Zig.ConstDecl declaration:
                // Evaluating an import here analyzes the module even if this local
                // binding is subsequently unused. Top-level import bindings stay lazy.
                ResolveModulePath(declaration.Arg3);
                break;
        }
        var result = ExecuteComptimeSessionStatement(session, statement);
        if (result is null)
        {
            throw new IrUnsupportedException("zig top-level comptime statement did not evaluate: "
                + statement.Content?.GetType().Name + (_ir.ComptimeMiss is { } miss ? " (" + miss + ")" : ""));
        }
        if (result.Value.Returned) { throw new CompileException("zig: return outside function in top-level comptime"); }
    }

    private void ExecuteTopComptimeIf(Item condition, Item yes, Item? no, IrBuilder.ComptimeSession session)
    {
        if (TopComptimeCondition(condition)) { ExecuteTopComptimeStmt(yes, session); }
        else if (no is not null) { ExecuteTopComptimeStmt(no, session); }
    }

    private bool TopComptimeCondition(Item condition)
    {
        if (condition.Content is Zig.Grouped grouped) { return TopComptimeCondition(grouped.Arg1); }
        if (condition.Content is Zig.PreNot not) { return !TopComptimeCondition(not.Arg1); }
        if (condition.Content is Zig.BoolAnd and) { return TopComptimeCondition(and.Arg0) && TopComptimeCondition(and.Arg2); }
        if (condition.Content is Zig.BoolOr or) { return TopComptimeCondition(or.Arg0) || TopComptimeCondition(or.Arg2); }
        if (condition.Content is Zig.CmpEq equal && TryTopModuleEquality(equal.Arg0, equal.Arg2) is { } eq) { return eq; }
        if (condition.Content is Zig.CmpNe unequal && TryTopModuleEquality(unequal.Arg0, unequal.Arg2) is { } ne) { return !ne; }
        return TryFoldRequiredComptimeCondition(condition)
            ?? throw new IrUnsupportedException("zig top-level comptime condition must be compile-time-known");
    }

    private bool? TryTopModuleEquality(Item left, Item right)
    {
        var a = TopModuleIdentity(left);
        var b = TopModuleIdentity(right);
        if (a is not null && b is not null) { return ReferenceEquals(a, b); }
        // A namespace type is distinct from a nested struct/enum type; neither
        // operand has a runtime value that could be lowered as an ordinary call.
        if (a is not null && TryTypeAliasRhs(right, out _)) { return false; }
        if (b is not null && TryTypeAliasRhs(left, out _)) { return false; }
        return null;
    }

    private ZigLowering? TopModuleIdentity(Item expression)
    {
        if (expression.Content is Zig.Grouped grouped) { return TopModuleIdentity(grouped.Arg1); }
        if (expression.Content is Zig.BuiltinCall { Arg2: null } self && IsThisBuiltin(self.Arg0))
            return _currentContainer is null || _currentContainer == _fileContainer ? this : null;
        if (expression.Content is Zig.BuiltinCall { Arg2: not null } import && Tok(import.Arg0) == "@import"
            && Flatten(import.Arg2) is [{ Content: Zig.StrLit spec }] && UnquoteStringLiteral(Tok(spec.Arg0)) == "root")
            return TopRoots.TryGetValue(_ir, out var root) ? root.Lowering : null;
        if (expression.Content is Zig.Ident identifier && _importSpecs.GetValueOrDefault(Tok(identifier.Arg0)) == "root")
            return TopRoots.TryGetValue(_ir, out var root) ? root.Lowering : null;
        return ResolveModulePath(expression)?.Lowering;
    }

    private sealed class TopRootModule(ZigLowering lowering) { internal ZigLowering Lowering { get; } = lowering; }
    private static readonly ConditionalWeakTable<IrBuilder, TopRootModule> TopRoots = new();

    private void ExecuteTopComptimeWhile(Item condition, Item? continuation, Item body, Item? other,
        IrBuilder.ComptimeSession session)
    {
        var broke = false;
        var count = 0;
        while (TopComptimeCondition(condition))
        {
            if (++count > InlineUnrollCap) { throw new IrUnsupportedException("zig top-level comptime loop exceeded iteration limit"); }
            try { ExecuteTopComptimeStmt(body, session); }
            catch (TopComptimeJump jump) { if (!jump.IsContinue) { broke = true; break; } }
            if (continuation is not null)
            {
                CStmt step = continuation.Content switch
                {
                    Zig.WhileContAssign assign => LowerAssignOpStmt(assign.Arg2, assign.Arg3, assign.Arg4),
                    Zig.WhileContExpr expression => new ExprStmt(LowerExpr(expression.Arg2)),
                    _ => throw new IrUnsupportedException("zig top-level comptime: unsupported while continuation"),
                };
                if (_ir.RunComptimeSessionStmt(session, TopComptimeStores(step)) is null)
                    throw new IrUnsupportedException("zig top-level comptime while continuation did not evaluate");
            }
        }
        if (!broke && ForElseBody(other) is { } elseBody) { ExecuteTopComptimeStmt(elseBody, session); }
    }

    // Published session locals lower as live comptime folds. Reads need that
    // wrapper; a store must retain the underlying storage identity.
    private static CStmt TopComptimeStores(CStmt statement) => statement switch
    {
        ExprStmt expression => expression with { Expr = TopComptimeStoreExpression(expression.Expr) },
        Seq sequence => sequence with { Stmts = sequence.Stmts.Select(TopComptimeStores).ToList() },
        Block block => block with { Stmts = block.Stmts.Select(TopComptimeStores).ToList() },
        If branch => branch with { Then = TopComptimeStores(branch.Then), Else = branch.Else is { } other ? TopComptimeStores(other) : null },
        While loop => loop with { Body = TopComptimeStores(loop.Body) },
        DoWhile loop => loop with { Body = TopComptimeStores(loop.Body) },
        For loop => loop with
        {
            Init = loop.Init is { } init ? TopComptimeStores(init) : null,
            Post = loop.Post is { } step ? TopComptimeStoreExpression(step) : null,
            Body = TopComptimeStores(loop.Body),
        },
        Labeled label => label with { Body = TopComptimeStores(label.Body) },
        Switch selection => selection with { Sections = selection.Sections.Select(section => section with { Body = section.Body.Select(TopComptimeStores).ToList() }).ToList() },
        CaseLabelStmt label => label with { Body = TopComptimeStores(label.Body) },
        DeferGuard guard => guard with { Body = TopComptimeStores(guard.Body), Cleanup = TopComptimeStores(guard.Cleanup) },
        _ => statement,
    };

    private static CExpr TopComptimeStoreExpression(CExpr expression) => expression switch
    {
        Assign assignment => assignment with { Target = TopComptimeStorage(assignment.Target) },
        StatementExpression block => block with { Body = (Block)TopComptimeStores(block.Body) },
        _ => expression,
    };

    private static CExpr TopComptimeStorage(CExpr expression) => expression is ComptimeFold { Live: true } fold
        ? TopComptimeStorage(fold.Inner) : expression;

    private (bool Returned, IrBuilder.ComptimeValue? Value)? ExecuteComptimeSessionStatement(
        IrBuilder.ComptimeSession session, Item statement)
        => _ir.RunComptimeSessionStmt(session, TopComptimeStores(LowerStmt(statement)));

    private void ExecuteTopComptimeFor(Item objectsItem, Item capturesItem, Item body, Item? other,
        IrBuilder.ComptimeSession session)
    {
        var (objects, captures) = DecomposeForMulti(objectsItem, capturesItem);
        var lists = new ZigComptimeList?[objects.Count];
        int? count = null;
        for (var i = 0; i < objects.Count; i++)
        {
            if (captures[i].ByRef) { throw new IrUnsupportedException("zig top-level comptime: by-reference for capture is not supported"); }
            if (objects[i].IsRange)
            {
                if (objects[i].End is not { } end) { continue; }
                var low = ComptimeIntValue(LowerExpr(objects[i].Expr));
                var high = ComptimeIntValue(LowerExpr(end));
                if (low is null || high is null || high < low || high - low > InlineUnrollCap)
                    throw new IrUnsupportedException("zig top-level comptime: invalid or excessive for range");
                lists[i] = new ZigComptimeList { Label = "range", Ints = Enumerable.Range(0, (int)(high - low)).Select(k => low.Value + k).ToList() };
            }
            else if (TryComptimeIterable(objects[i].Expr, out var list)) { lists[i] = list; }
            else
            {
                var value = _ir.EvalComptimeValue(LowerExpr(objects[i].Expr));
                var elements = value switch
                {
                    IrBuilder.CtArray array => array.Elems,
                    IrBuilder.CtSlice slice => slice.Backing.Elems.Skip((int)slice.Offset).Take((int)slice.Length).ToArray(),
                    _ => throw new IrUnsupportedException("zig top-level comptime: for operand is not a supported comptime list"),
                };
                lists[i] = new ZigComptimeList { Label = "values", Ints = elements.Select(element => element is IrBuilder.CtInt integer && integer.Value >= long.MinValue && integer.Value <= long.MaxValue
                    ? (long)integer.Value : throw new IrUnsupportedException("zig top-level comptime: noninteger array capture is not supported")).ToList() };
            }
            var length = lists[i]!.Count;
            if (count is { } previous && previous != length) { throw new CompileException("zig: for loop operands have unequal lengths"); }
            count = length;
        }
        if (count is null || count > InlineUnrollCap) { throw new IrUnsupportedException("zig top-level comptime for requires a bounded iterable"); }
        for (var i = 0; i < lists.Length; i++)
        {
            if (lists[i] is not null) { continue; }
            var low = ComptimeIntValue(LowerExpr(objects[i].Expr))
                ?? throw new IrUnsupportedException("zig top-level comptime: range start must be known");
            lists[i] = new ZigComptimeList { Label = "range", Ints = Enumerable.Range(0, count.Value).Select(k => low + k).ToList() };
        }
        var broke = false;
        for (var k = 0; k < count.Value; k++)
        {
            var shadows = new List<ComptimeCaptureShadow>();
            using var scope = EnterSymbolScope();
            try
            {
                for (var i = 0; i < captures.Count; i++) { shadows.Add(SeedComptimeCapture(captures[i].Name, lists[i]!, k)); }
                ExecuteTopComptimeStmt(body, session);
            }
            catch (TopComptimeJump jump) { if (!jump.IsContinue) { broke = true; break; } }
            finally { for (var i = shadows.Count - 1; i >= 0; i--) { RestoreComptimeCapture(shadows[i]); } }
        }
        if (!broke && ForElseBody(other) is { } elseBody) { ExecuteTopComptimeStmt(elseBody, session); }
    }

    private Action SaveTopComptimeBindings()
    {
        Action Save<K, V>(Dictionary<K, V> map) where K : notnull
        {
            var saved = map.ToArray();
            return () => { map.Clear(); foreach (var (key, value) in saved) { map.Add(key, value); } };
        }
        var restores = new[] { Save(_imports), Save(_importSpecs), Save(_importModules), Save(_moduleAliasPaths), Save(_declAliases),
            Save(_typeAliases), Save(_comptimeValues), Save(_comptimeStrings), Save(_fnAliases) };
        return () => { foreach (var restore in restores) { restore(); } };
    }

    private sealed class TopExportTable
    {
        internal readonly Dictionary<string, Symbol> Names = new(StringComparer.Ordinal);
    }
    private static readonly ConditionalWeakTable<IrBuilder, TopExportTable> TopExports = new();

    private IEnumerable<Symbol> TopExportRuntimeRoots()
        => TopExports.TryGetValue(_ir, out var table) ? table.Names.Values.Distinct() : Enumerable.Empty<Symbol>();

    private CExpr ExportBuiltin(IReadOnlyList<Item> arguments)
    {
        if (_comptimeDepth == 0) { throw new CompileException("zig @export must run at comptime"); }
        if (arguments.Count != 2 || arguments[1].Content is not Zig.AnonStructInit options)
            throw new IrUnsupportedException("zig @export expects (declaration pointer, .{ .name = comptime_string })");
        string? name = null;
        foreach (var option in Flatten(options.Arg2))
        {
            if (option.Content is not Zig.FieldInit field || Tok(field.Arg1) != "name" || name is not null)
                throw new IrUnsupportedException("zig @export: only the .name option is supported");
            name = ComptimeName(field.Arg3)
                ?? throw new CompileException("zig @export .name must be a comptime string");
        }
        if (string.IsNullOrEmpty(name) || name.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')) || char.IsAsciiDigit(name[0]))
            throw new IrUnsupportedException("zig @export: export name must be a nonempty managed identifier");
        var operand = Unparen(LowerExpr(arguments[0]));
        if (operand is Unary { Op: UnOp.AddrOf, Operand: var addressed }) { operand = Unparen(addressed); }
        if (operand is not VarRef { Sym: { Kind: SymKind.Func } canonical } || canonical.Type.Unqualified is not CType.Func signature
            || signature.Variadic || IsFnTemplate(canonical))
            throw new IrUnsupportedException("zig @export: only concrete function declarations are supported (data exports require target support)");
        if (!TopFunctionAbis.TryGetValue(canonical, out var abi) || !abi.CAbi)
            throw new IrUnsupportedException("zig @export: function must use callconv(.c) (other calling conventions are not supported)");
        if (!TopExportAbiType(signature.Return, true) || signature.Params.Any(parameter => !TopExportAbiType(parameter, false)))
            throw new IrUnsupportedException("zig @export: function has an unsupported ABI parameter or return type");
        RegisterTopExportName(name, canonical);
        // Root the actual body through the ordinary on-demand path; never duplicate its algorithm.
        RootTopExportBody(canonical);
        if (canonical.TargetName == _names.Escape(name))
            canonical.TargetName = _names.Escape("__zig_export_body_" + name);
        if (_ir.Functions.Any(function => function.Sym.TargetName == _names.Escape(name))
            || _exportedFns.Values.Any(function => !ReferenceEquals(function, canonical) && function.TargetName == _names.Escape(name))
            || _moduleGraph?.RootFunctionTargetNames().Contains(_names.Escape(name)) == true)
            throw new CompileException("zig: exported name collides with a function: " + name);
        var alias = new Symbol { Name = name, TargetName = _names.Escape(name), Kind = SymKind.Func, Type = signature, IsGlobal = true };
        var parameters = signature.Params.Select((type, index) => new Symbol
            { Name = "arg" + index, TargetName = "arg" + index, Type = type, Kind = SymKind.Param }).ToList();
        var call = new Call(canonical.Name, parameters.Select(parameter => (CExpr)new VarRef(parameter) { Type = parameter.Type }).ToList(), signature.Params, canonical)
            { Type = signature.Return };
        var statements = signature.Return.Unqualified is CType.VoidType
            ? new List<CStmt> { new ExprStmt(call), new Return(null) } : new List<CStmt> { new Return(call) };
        _ir.Functions.Add(new FuncDef(alias, parameters, new Block(statements), false));
        RecordRuntimeCall(canonical);
        return new DefaultLit { Type = CType.Void };
    }

    private bool TopExportAbiType(CType type, bool returns) => TopExportAbiType(type, returns, false, new HashSet<string>(StringComparer.Ordinal));

    private bool TopExportAbiType(CType type, bool returns, bool field, HashSet<string> visiting) => type.Unqualified switch
    {
        CType.VoidType => returns,
        CType.Prim { IsComptimeInt: false } => true,
        CType.Enum or CType.Pointer or CType.Func => true,
        CType.Array { Count: >= 0 } array => field && TopExportAbiType(array.Element, false, true, visiting),
        CType.Named named => TopExportAggregate(named.Name, visiting),
        _ => false,
    };

    private bool TopExportAggregate(string name, HashSet<string> visiting)
    {
        var definition = _ir.Types.FirstOrDefault(type => type.Name == name);
        if (definition is not { Layout: AggregateLayout.Sequential, IsIncomplete: false } || !visiting.Add(name)) { return false; }
        try
        {
            return definition.Fields.All(field => !field.IsFlexibleArray && field.BitWidth is null
                && TopExportAbiType(field.Type, false, true, visiting));
        }
        finally { visiting.Remove(name); }
    }
}
