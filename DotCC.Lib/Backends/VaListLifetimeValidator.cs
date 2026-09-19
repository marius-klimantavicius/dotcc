#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

/// <summary>C# ref-struct constraints belong to this backend, not to C's type system.
/// Function pointer signatures may contain cursors; storing the cursor value itself
/// in unmanaged or heap storage is different. Unsafe C# only warns for some escapes,
/// so reject those in the typed IR before producing an unsafe method.</summary>
internal static class VaListLifetimeValidator
{
    internal static bool IsVaList(CType type) => type.Unqualified is CType.Named { Name: "VaList" };

    internal static bool HasByRefLikeSignature(CType.Func signature)
        => IsVaList(signature.Return) || signature.Params.Any(IsVaList);

    private static bool ContainsCursor(CType type) => type.Unqualified switch
    {
        CType.Array a => ContainsCursor(a.Element),
        _ => IsVaList(type),
    };

    private static void Fail(string reason, SrcPos pos = default)
        => throw new CompileException($"C# va_list lifetime: {reason} ({pos}).");

    private static void CheckType(CType type, SrcPos pos = default)
    {
        switch (type.Unqualified)
        {
            case CType.Pointer p:
                if (ContainsCursor(p.Pointee)) Fail("pointer/address to va_list is unsupported", pos);
                CheckType(p.Pointee, pos);
                break;
            case CType.Array a:
                if (ContainsCursor(a.Element)) Fail("array storage of va_list is unsupported", pos);
                CheckType(a.Element, pos);
                break;
            case CType.Func f:
                CheckType(f.Return, pos);
                foreach (var p in f.Params) CheckType(p, pos);
                break;
        }
    }

    internal static void Validate(IrBuilder unit)
    {
        foreach (var use in unit.RuntimeLayoutUses)
            if (ContainsCursor(use.Type)) Fail(use.Operation + " has no unmanaged va_list layout", use.Pos);
        foreach (var global in unit.Globals)
        {
            if (ContainsCursor(global.Sym.Type)) Fail("global storage of va_list is unsupported");
            CheckType(global.Sym.Type);
            if (global.Init is { } init) Walk(init, CheckExpression);
        }
        foreach (var aggregate in unit.Types)
            foreach (var field in aggregate.Fields)
            {
                if (ContainsCursor(field.Type)) Fail($"field {aggregate.Name}.{field.Name} cannot store va_list");
                CheckType(field.Type);
            }
        foreach (var function in unit.Functions)
        {
            CheckType(function.Sym.Type);
            var expressions = new List<CExpr>();
            var declarations = new List<LocalDecl>();
            var returns = new List<Return>();
            Visit(function.Body, expression => { CheckExpression(expression); expressions.Add(expression); }, declarations, returns);
            var local = FindLocalCursors(expressions, declarations);
            foreach (var statement in returns)
                if (statement.Value is { } value && IsVaList(value.Type) && BorrowsLocal(value, local))
                    Fail("return would escape a locally started va_list", value.Pos);
        }
    }

    /// <summary>All aliases of a cursor started in this invocation need scoped local
    /// declarations. Borrowed incoming parameters and copies remain returnable.</summary>
    internal static HashSet<Symbol> ScopedLocals(FuncDef function)
    {
        var expressions = new List<CExpr>();
        var declarations = new List<LocalDecl>();
        Visit(function.Body, expressions.Add, declarations, new List<Return>());
        return FindLocalCursors(expressions, declarations);
    }

    private static HashSet<Symbol> FindLocalCursors(List<CExpr> expressions, List<LocalDecl> declarations)
    {
        var local = new HashSet<Symbol>(ReferenceEqualityComparer.Instance);
        foreach (var expression in expressions)
            if (expression is Call { Callee: "va_start", Args.Count: > 0 } start && CursorSymbol(start.Args[0]) is { } symbol)
                local.Add(symbol);
        bool changed;
        do
        {
            changed = false;
            foreach (var declaration in declarations)
                if (IsVaList(declaration.Sym.Type) && declaration.Init is { } init && BorrowsLocal(init, local))
                    changed |= local.Add(declaration.Sym);
            foreach (var expression in expressions)
            {
                if (expression is Assign a && IsVaList(a.Target.Type) && CursorSymbol(a.Target) is { } target && BorrowsLocal(a.Value, local))
                    changed |= local.Add(target);
                if (expression is Call { Callee: "va_copy", Args.Count: >= 2 } copy && CursorSymbol(copy.Args[0]) is { } destination && BorrowsLocal(copy.Args[1], local))
                    changed |= local.Add(destination);
            }
        } while (changed);
        return local;
    }

    private static Symbol? CursorSymbol(CExpr expression) => expression switch
    {
        VarRef v => v.Sym,
        Paren p => CursorSymbol(p.Inner),
        _ => null,
    };

    private static bool BorrowsLocal(CExpr expression, HashSet<Symbol> local)
    {
        if (!IsVaList(expression.Type)) return false;
        return expression switch
        {
            VarRef v => local.Contains(v.Sym),
            Paren p => BorrowsLocal(p.Inner, local),
            Cast c => BorrowsLocal(c.Operand, local),
            Assign a => BorrowsLocal(a.Value, local),
            CondExpr c => BorrowsLocal(c.Then, local) || BorrowsLocal(c.Else, local),
            CommaOp c => BorrowsLocal(c.Items[^1], local),
            CommaSeq c => BorrowsLocal(c.Items[^1], local),
            // An expanded variadic pack cannot outlive its call expression. Fixed
            // cursor arguments propagate their borrow through by-value forwarding.
            Call c => c.CalleeSym?.Type.Unqualified is CType.Func { Variadic: true } || c.Args.Any(a => BorrowsLocal(a, local)),
            IndirectCall c => FunctionType(c.Callee.Type) is { Variadic: true } || c.Args.Any(a => BorrowsLocal(a, local)),
            _ => false,
        };
    }

    private static CType.Func? FunctionType(CType type) => type.Unqualified switch
    {
        CType.Func f => f,
        CType.Pointer p => FunctionType(p.Pointee),
        _ => null,
    };

    private static void CheckExpression(CExpr expression)
    {
        CheckType(expression.Type, expression.Pos);
        switch (expression)
        {
            case SizeOfExpr s when ContainsCursor(s.Of): Fail("sizeof has no unmanaged va_list layout", s.Pos); break;
            case OffsetOf o when ContainsCursor(o.StructType) || o.MemberType is { } member && ContainsCursor(member):
                Fail("offsetof has no unmanaged va_list layout", o.Pos); break;
            case VaArgGet a when IsVaList(a.Target): Fail("variadic argument cannot contain va_list", a.Pos); break;
            case Call c when c.Callee is not ("va_start" or "va_copy" or "va_end"):
                CheckArguments(c.Args, c.ParamTypes?.Count ?? 0); break;
            case IndirectCall c:
                CheckArguments(c.Args, FunctionType(c.Callee.Type)?.Params.Count ?? 0); break;
        }
    }

    private static void CheckArguments(IReadOnlyList<CExpr> arguments, int fixedCount)
    {
        for (int i = fixedCount; i < arguments.Count; i++)
            if (IsVaList(arguments[i].Type)) Fail("variadic argument cannot contain va_list", arguments[i].Pos);
    }

    internal static void ValidateCapture(IReadOnlyList<CExpr> items)
    {
        if (IsCursorValue(items[^1]))
            Fail("comma delegate result cannot contain va_list", items[^1].Pos);
        foreach (var item in items)
            Walk(item, e => { if (e is VarRef && IsVaList(e.Type)) Fail("comma delegate cannot capture va_list", e.Pos); });
    }

    internal static void ValidateTuple(IReadOnlyList<CExpr> items)
    {
        foreach (var item in items)
            if (IsCursorValue(item))
                Fail("comma tuple cannot store va_list", item.Pos);
    }

    private static bool IsCursorValue(CExpr expression) => IsVaList(expression.Type) || expression switch
    {
        Paren p => IsCursorValue(p.Inner),
        Call { Callee: "va_start" or "va_copy" } => true,
        _ => false,
    };

    // Walk C expressions explicitly. This pass deliberately does not inspect emitted
    // text, CLR reflection, or unrelated Zig-only nodes.
    internal static void Walk(CExpr e, Action<CExpr> action)
    {
        action(e);
        void One(CExpr? child) { if (child is not null) Walk(child, action); }
        void Many(IEnumerable<CExpr>? children) { if (children is not null) foreach (var child in children) One(child); }
        switch (e)
        {
            case Unary u: One(u.Operand); break;
            case Binary b: One(b.Left); One(b.Right); break;
            case Assign a: One(a.Target); One(a.Value); break;
            case Call c: Many(c.Args); break;
            case IndirectCall c: One(c.Callee); Many(c.Args); break;
            case Cast c: One(c.Operand); break;
            case BitCast c: One(c.Operand); break;
            case CondExpr c: One(c.Cond); One(c.Then); One(c.Else); break;
            case DotCC.Ir.Index i: One(i.Base); One(i.Idx); break;
            case Member m: One(m.Base); break;
            case CommaOp c: Many(c.Items); break;
            case StatementExpression statement:
                Visit(statement.Body, action, new List<LocalDecl>(), new List<Return>());
                One(statement.Value);
                break;
            case CommaSeq c: Many(c.Items); break;
            case StructInit s: foreach (var member in s.Members) One(member.Value); break;
            case InlineArrayInit a: Many(a.Elems); break;
            case FlexibleAggregateInit f: One(f.Header); Many(f.Elems); break;
            case StackArray a: Many(a.Elems); break;
            case PinnedArray a: One(a.Count); Many(a.Elems); break;
            case VaArgGet a: One(a.Ap); break;
            case Paren p: One(p.Inner); break;
            case ComptimeFold c: One(c.Resolved ?? c.Inner); break;
        }
    }

    private static void Visit(CStmt statement, Action<CExpr> action, List<LocalDecl> declarations, List<Return> returns)
    {
        void Expr(CExpr? e) { if (e is not null) Walk(e, action); }
        void Stmt(CStmt? s) { if (s is not null) Visit(s, action, declarations, returns); }
        void Stmts(IEnumerable<CStmt> ss) { foreach (var s in ss) Stmt(s); }
        switch (statement)
        {
            case Block b: Stmts(b.Stmts); break;
            case Seq s: Stmts(s.Stmts); break;
            case DeclStmt d:
                foreach (var declaration in d.Decls) { CheckType(declaration.Sym.Type, d.Pos); declarations.Add(declaration); Expr(declaration.Init); }
                break;
            case ArrayDecl a:
                if (ContainsCursor(a.Element)) Fail("array storage of va_list is unsupported", a.Pos);
                CheckType(a.Element, a.Pos); Expr(a.CountExpr); if (a.Inits is { } initializers) foreach (var init in initializers) Expr(init);
                break;
            case ExprStmt e: Expr(e.Expr); break;
            case If i: Expr(i.Cond); Stmt(i.Then); Stmt(i.Else); break;
            case While w: Expr(w.Cond); Stmt(w.Body); break;
            case DoWhile w: Stmt(w.Body); Expr(w.Cond); break;
            case For f: Stmt(f.Init); Expr(f.Cond); Expr(f.Post); Stmt(f.Body); break;
            case Return r: returns.Add(r); Expr(r.Value); break;
            case Labeled l: Stmt(l.Body); break;
            case SetjmpGuard s: Expr(s.Env); Stmt(s.TryBody); Stmt(s.CatchBody); break;
            case SetjmpCapture s: Expr(s.Env); Expr(s.Target); Stmt(s.Body); break;
            case DeferGuard d: Stmt(d.Body); Stmt(d.Cleanup); break;
            case Switch s:
                Expr(s.Subject);
                foreach (var section in s.Sections) { foreach (var label in section.Labels) { Expr(label.CaseExpr); Expr(label.HiExpr); } Stmts(section.Body); }
                break;
            case CaseLabelStmt c: Expr(c.CaseExpr); Stmt(c.Body); break;
        }
    }
}
