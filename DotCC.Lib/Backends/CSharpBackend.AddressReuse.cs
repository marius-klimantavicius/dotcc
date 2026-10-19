using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private Dictionary<Symbol, string> _globalAddressBases = new(ReferenceEqualityComparer.Instance);
    private HashSet<string> _addressLocalNames = new(StringComparer.Ordinal);
    private bool _addressReuseAllowed;
    private Dictionary<Symbol, string> _stableAddressLocals = new(ReferenceEqualityComparer.Instance);
    private string _stableAddressDeclarations = "";
    private bool _usesGlobalAddressCache;

    private string AllocateAddressLocal()
    {
        string name;
        do { name = "__dotcc_address" + _clCounter++; }
        while (!_addressLocalNames.Add(name) || !_addressLocalNames.Add(name + "_Get"));
        return name;
    }

    private void PrepareStableAddressScope(Action<Action<CExpr>> walk)
    {
        var counts = new Dictionary<Symbol, int>(ReferenceEqualityComparer.Instance);
        walk(expression => { if (AddressUse(expression) is { } root) counts[root] = counts.GetValueOrDefault(root) + 1; });
        _stableAddressLocals = new(ReferenceEqualityComparer.Instance);
        foreach (var root in counts.Where(pair => pair.Value >= 2).Select(pair => pair.Key).OrderBy(root => root.TargetName, StringComparer.Ordinal))
            _stableAddressLocals.Add(root, AllocateAddressLocal());
        _usesGlobalAddressCache |= _stableAddressLocals.Count != 0;
    }

    private string StableAddressDeclarations() => string.Concat(_stableAddressLocals
        .Select(pair => GlobalStorageReferences.AddressDeclaration(pair.Key.TargetName, AddressStorageType(pair.Key), pair.Value) + "\n"));

    // Pointer objects live in nint slots. Cache the slot's address, never the
    // pointer value it holds; T* itself cannot be a generic type argument.
    private string AddressStorageType(Symbol symbol) => NintStorage(symbol) ? "nint" : Cs(symbol.Type);

    private static Symbol? AddressRoot(CExpr expression) => expression switch
    {
        Paren p => AddressRoot(p.Inner),
        Member { Arrow: false } member => AddressRoot(member.Base),
        VarRef { Sym: { IsGlobal: true, Kind: SymKind.Var, IsThreadLocal: false, FromSystemHeader: false } symbol }
            when symbol.Type.Unqualified is CType.Named or CType.Prim or CType.Enum or CType.Pointer or CType.Func => symbol,
        // A pointer value, call, index, TLS accessor or runtime-owned property is
        // not the address of an invariant C storage object. Never cache it here.
        _ => null,
    };

    private static Symbol? AddressUse(CExpr expression) => expression switch
    {
        Unary { Op: UnOp.AddrOf } address => AddressRoot(address.Operand),
        Member { Arrow: false, Type.Unqualified: CType.Array } member => AddressRoot(member.Base),
        _ => null,
    };

    private static IEnumerable<CStmt> AddressStatementChildren(CStmt statement) => statement switch
    {
        Block block => block.Stmts,
        Seq sequence => sequence.Stmts,
        Labeled label => new[] { label.Body },
        If branch => branch.Else is { } other ? new[] { branch.Then, other } : new[] { branch.Then },
        While loop => new[] { loop.Body },
        DoWhile loop => new[] { loop.Body },
        For loop => loop.Init is { } init ? new[] { init, loop.Body } : new[] { loop.Body },
        Switch choice => choice.Sections.SelectMany(section => section.Body),
        SetjmpGuard guard => new[] { guard.TryBody, guard.CatchBody }.OfType<CStmt>(),
        SetjmpCapture capture => new[] { capture.Body },
        DeferGuard guard => new[] { guard.Body, guard.Cleanup },
        _ => Array.Empty<CStmt>(),
    };

    private static IEnumerable<CExpr> AddressStatementExpressions(CStmt statement) => statement switch
    {
        DeclStmt declaration => declaration.Decls.Select(local => local.Init).OfType<CExpr>(),
        ExprStmt expression => new[] { expression.Expr },
        Return { Value: { } value } => new[] { value },
        If branch => new[] { branch.Cond },
        While loop => new[] { loop.Cond },
        DoWhile loop => new[] { loop.Cond },
        For loop => new[] { loop.Cond, loop.Post }.OfType<CExpr>(),
        Switch choice => new[] { choice.Subject },
        SetjmpGuard guard => new[] { guard.Env },
        SetjmpCapture capture => new[] { capture.Env, capture.Target },
        ArrayDecl array => (array.Inits ?? Array.Empty<CExpr>()).Concat(new[] { array.CountExpr }.OfType<CExpr>()),
        _ => Array.Empty<CExpr>(),
    };

    private static void WalkAddressStatements(CStmt statement, Action<CStmt> visitStatement, Action<CExpr> visitExpression)
    {
        visitStatement(statement);
        foreach (var expression in AddressStatementExpressions(statement)) VaListLifetimeValidator.Walk(expression, visitExpression);
        foreach (var child in AddressStatementChildren(statement)) WalkAddressStatements(child, visitStatement, visitExpression);
    }

    private static void WalkStableAddressStatements(CStmt statement, Action<CExpr> visit, bool repeated = false)
    {
        bool loop = statement is While or DoWhile or For;
        foreach (var expression in AddressStatementExpressions(statement))
            VaListLifetimeValidator.Walk(expression, node =>
            {
                visit(node);
                // A single textual use in a loop is still a repeated dynamic
                // address calculation. Initialization remains lazy for count0.
                if (repeated || loop) visit(node);
            });
        foreach (var child in AddressStatementChildren(statement))
            WalkStableAddressStatements(child, visit, repeated || (loop && !(statement is For f && ReferenceEquals(child, f.Init))));
    }

    // Which global addresses are reached in this statement before any conditional
    // arm, call or statement-expression? A later access may reuse an existing ref,
    // but cannot introduce it ahead of an unexecuted branch or changed owner.
    private static void EntryAddresses(CExpr expression, HashSet<Symbol> addresses, ref bool barrier)
    {
        if (barrier) return;
        if (AddressUse(expression) is { } root) addresses.Add(root);
        switch (expression)
        {
            case Paren p: EntryAddresses(p.Inner, addresses, ref barrier); break;
            case Cast cast: EntryAddresses(cast.Operand, addresses, ref barrier); break;
            case Unary unary: EntryAddresses(unary.Operand, addresses, ref barrier); break;
            case Member member: EntryAddresses(member.Base, addresses, ref barrier); break;
            case Binary binary:
                EntryAddresses(binary.Left, addresses, ref barrier);
                if (binary.Op is not (BinOp.LogAnd or BinOp.LogOr)) EntryAddresses(binary.Right, addresses, ref barrier);
                else barrier = true;
                break;
            case Assign assignment:
                EntryAddresses(assignment.Target, addresses, ref barrier);
                EntryAddresses(assignment.Value, addresses, ref barrier);
                break;
            case CondExpr conditional: EntryAddresses(conditional.Cond, addresses, ref barrier); barrier = true; break;
            case Call call:
                foreach (var argument in call.Args) EntryAddresses(argument, addresses, ref barrier);
                barrier = true;
                break;
            case IndirectCall call:
                EntryAddresses(call.Callee, addresses, ref barrier);
                foreach (var argument in call.Args) EntryAddresses(argument, addresses, ref barrier);
                barrier = true;
                break;
            case CommaOp comma:
                foreach (var item in comma.Items) EntryAddresses(item, addresses, ref barrier);
                break;
            case DotCC.Ir.Index index:
                EntryAddresses(index.Base, addresses, ref barrier);
                EntryAddresses(index.Idx, addresses, ref barrier);
                break;
            case StatementExpression: barrier = true; break;
        }
    }

    private void EmitAddressReuseBlock(StringBuilder output, Block block, int indentation)
    {
        var counts = new Dictionary<Symbol, int>(ReferenceEqualityComparer.Instance);
        bool hasEntryLabel = false;
        if (_addressReuseAllowed)
            WalkAddressStatements(block, statement => hasEntryLabel |= statement is Labeled or CaseLabelStmt or SetjmpCapture,
                expression => { if (AddressUse(expression) is { } root) counts[root] = counts.GetValueOrDefault(root) + 1; });
        var previous = _globalAddressBases;
        _globalAddressBases = new(previous, ReferenceEqualityComparer.Instance);
        output.Append(Pad(indentation)).Append("{\n");
        output.Append(_stableAddressDeclarations);
        _stableAddressDeclarations = "";
        int level = indentation + 1;
        bool invalidated = false;
        foreach (var statement in block.Stmts)
        {
            // Objects can later be linked into contextual storage. Calls may
            // change that owner, including in a nested branch; do not retain a
            // cached owner/address over such a boundary. No field value is cached.
            bool callBarrier = false;
            WalkAddressStatements(statement,
                child => callBarrier |= child is SetjmpGuard or SetjmpCapture or DeferGuard,
                expression => callBarrier |= expression is Call or IndirectCall or StatementExpression);
            if (callBarrier) { _globalAddressBases.Clear(); invalidated = true; }
            if (!callBarrier && !hasEntryLabel && statement is ExprStmt or DeclStmt or Return or If or While or Switch)
            {
                var entries = new HashSet<Symbol>(ReferenceEqualityComparer.Instance);
                bool barrier = false;
                foreach (var expression in AddressStatementExpressions(statement)) EntryAddresses(expression, entries, ref barrier);
                foreach (var root in entries.OrderBy(symbol => symbol.TargetName, StringComparer.Ordinal))
                {
                    if (_globalAddressBases.ContainsKey(root) || counts.GetValueOrDefault(root) < 2) continue;
                    string name = AllocateAddressLocal();
                    // The cached address is a managed byref. Convert it to an
                    // unmanaged pointer only at the original access, so a GC
                    // safepoint cannot leave a stale unpinned pointer in a local.
                    var storageType = AddressStorageType(root);
                    var declaration = "ref " + storageType + " " + name + " = ref " + GlobalName(root) + ";";
                    output.Append(Pad(level)).Append(_stableAddressLocals.ContainsKey(root)
                        ? GlobalStorageReferences.ContextAddress(root.TargetName, declaration) : declaration).Append('\n');
                    _globalAddressBases.Add(root, "((" + storageType + "*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref " + name + "))");
                }
            }
            Stmt(output, statement, level);
            if (callBarrier) _globalAddressBases.Clear();
        }
        output.Append(Pad(indentation)).Append("}\n");
        if (invalidated) previous.Clear();
        _globalAddressBases = previous;
    }
}
