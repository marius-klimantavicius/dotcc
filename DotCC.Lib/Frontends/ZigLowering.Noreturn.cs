#nullable enable

using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private static bool IsNoreturn(CExpr value) => value.Type.Unqualified is CType.NoReturnType;

    /// <summary>Bottom meets a value sink by executing its terminating statement. There is no
    /// result expression, default value or synthetic return. StatementExpression keeps this lazy
    /// inside a conditional arm and supports pointer sinks without a generic pointer argument.</summary>
    private static CExpr CoerceNoreturn(CExpr value, CType? sink)
    {
        if (sink?.Unqualified is CType.NoReturnType)
        {
            if (!IsNoreturn(value)) { throw new IrUnsupportedException("cannot materialize a value of type 'noreturn'"); }
            return value;
        }
        if (!IsNoreturn(value) || sink is null) { return value; }
        return new StatementExpression(new Block(new[] { NoreturnStatement(value) }), null) { Type = sink };
    }

    private static CType NoreturnPeerType(CType? sink, params CExpr[] arms)
    {
        var values = arms.Where(a => !IsNoreturn(a)).ToArray();
        if (values.Any(a => a.Type.Unqualified is CType.VoidType)
            && (values.Any(a => a.Type.Unqualified is not CType.VoidType)
                || sink is not null && sink.Unqualified is not CType.VoidType))
        {
            throw new IrUnsupportedException("incompatible peer types: void does not produce a value (unlike noreturn)");
        }
        return sink ?? values.FirstOrDefault()?.Type ?? CType.NoReturn;
    }

    private static CExpr NoreturnConditional(CExpr cond, CExpr then, CExpr otherwise, CType? sink)
    {
        var type = NoreturnPeerType(sink, then, otherwise);
        return new CondExpr(CoerceNoreturn(cond, CType.Bool), CoerceNoreturn(then, type), CoerceNoreturn(otherwise, type)) { Type = type };
    }

    /// <summary>A bottom expression used as a statement retains its real calls. A bottom if/switch
    /// is control flow, so it never asks C# to construct a void conditional or switch value.</summary>
    private static CStmt NoreturnStatement(CExpr value) => value switch
    {
        Paren p => NoreturnStatement(p.Inner),
        CondExpr c when IsNoreturn(c) => new If(CoerceNoreturn(c.Cond, CType.Bool), NoreturnStatement(c.Then), NoreturnStatement(c.Else)),
        SwitchExpr s when IsNoreturn(s) => new Switch(s.Subject, s.Arms.Select(a => new SwitchSection(
            a.Labels ?? new[] { new SwitchLabel(null) }, new[] { NoreturnStatement(a.Value) })).ToList()),
        _ => new ExprStmt(value),
    };

    private static void RejectNoreturnStorage(CType type)
    {
        if (type.Unqualified is CType.NoReturnType)
        {
            throw new IrUnsupportedException("cannot materialize a value of type 'noreturn'");
        }
    }

    private static void ValidateNoreturnParameters(IEnumerable<CType> parameters)
    {
        if (parameters.Any(t => t.Unqualified is CType.NoReturnType))
        {
            throw new IrUnsupportedException("parameter of type 'noreturn' not allowed");
        }
    }

    /// <summary>Project bottom statements before backend emission, and check a noreturn function
    /// actually cannot complete. This is a structural check; no function names imply termination.</summary>
    private Block PrepareNoreturnBody(Symbol fn, Block body)
    {
        if (fn.Type.Unqualified is not CType.Func signature) { return body; }
        ValidateNoreturnParameters(signature.Params);
        fn.IsNoReturn = signature.Return.Unqualified is CType.NoReturnType;
        var normalized = (Block)NormalizeNoreturnStatement(body);
        if (fn.IsNoReturn && NoreturnBodyCanComplete(normalized))
        {
            throw new IrUnsupportedException($"function '{fn.Name}' declared 'noreturn' can return or fall through");
        }
        return normalized;
    }

    private static CStmt NormalizeNoreturnStatement(CStmt stmt) => stmt switch
    {
        Block b => b with { Stmts = b.Stmts.Select(NormalizeNoreturnStatement).ToList() },
        Seq s => s with { Stmts = s.Stmts.Select(NormalizeNoreturnStatement).ToList() },
        ExprStmt e when IsNoreturn(e.Expr) => NoreturnStatement(e.Expr),
        Return { Value: { } value } when IsNoreturn(value) => NoreturnStatement(value),
        DeclStmt d when d.Decls.Any(v => v.Sym.Type.Unqualified is CType.NoReturnType) => new Seq(d.Decls.Select(v =>
            v.Sym.Type.Unqualified is CType.NoReturnType
                ? v.Init is { } init && IsNoreturn(init) ? NoreturnStatement(init)
                    : throw new IrUnsupportedException("cannot materialize a value of type 'noreturn'")
                : (CStmt)new DeclStmt(new[] { v })).ToList()),
        If f => f with { Cond = CoerceNoreturn(f.Cond, CType.Bool), Then = NormalizeNoreturnStatement(f.Then), Else = f.Else is { } e ? NormalizeNoreturnStatement(e) : null },
        While w => w with { Cond = CoerceNoreturn(w.Cond, CType.Bool), Body = NormalizeNoreturnStatement(w.Body) },
        DoWhile d => d with { Cond = CoerceNoreturn(d.Cond, CType.Bool), Body = NormalizeNoreturnStatement(d.Body) },
        For f => f with { Init = f.Init is { } i ? NormalizeNoreturnStatement(i) : null,
            Cond = f.Cond is { } c ? CoerceNoreturn(c, CType.Bool) : null, Body = NormalizeNoreturnStatement(f.Body) },
        Labeled l => l with { Body = NormalizeNoreturnStatement(l.Body) },
        Switch s => s with { Sections = s.Sections.Select(a => a with { Body = a.Body.Select(NormalizeNoreturnStatement).ToList() }).ToList() },
        DeferGuard d => d with { Body = NormalizeNoreturnStatement(d.Body), Cleanup = NormalizeNoreturnStatement(d.Cleanup) },
        _ => stmt,
    };

    private bool NoreturnBodyCanComplete(CStmt stmt) => stmt switch
    {
        Return => true, // returning is forbidden even though it terminates the statement
        ExprStmt e => !CFlowFacts.TerminatesExpression(e.Expr),
        Block b => NoreturnSequenceCanComplete(b.Stmts),
        Seq s => NoreturnSequenceCanComplete(s.Stmts),
        If f => !CFlowFacts.TerminatesExpression(f.Cond)
            && (NoreturnBodyCanComplete(f.Then) || f.Else is null || NoreturnBodyCanComplete(f.Else)),
        While w => !CFlowFacts.TerminatesExpression(w.Cond)
            && !(_ir.ConstEval(w.Cond) is { } c && c != 0 && !HasNoreturnLoopExit(w.Body)),
        For f => !(f.Cond is null || _ir.ConstEval(f.Cond) is { } c && c != 0) || HasNoreturnLoopExit(f.Body),
        Labeled l => NoreturnBodyCanComplete(l.Body),
        DeferGuard d => NoreturnBodyCanComplete(d.Body),
        Switch s => !s.Sections.Any(a => a.Labels.Any(l => l.CaseExpr is null))
            || s.Sections.Any(a => NoreturnSequenceCanComplete(a.Body)),
        _ => true,
    };

    private bool NoreturnSequenceCanComplete(IReadOnlyList<CStmt> stmts)
    {
        foreach (var stmt in stmts)
        {
            if (!NoreturnBodyCanComplete(stmt)) { return false; }
            if (stmt is Return) { return true; }
        }
        return true;
    }

    // A nested loop's break stays inside that loop. Gotos are treated conservatively: proving
    // their target stays inside an infinite loop is outside this bounded bottom-flow change.
    private static bool HasNoreturnLoopExit(CStmt stmt) => HasNoreturnLoopExit(stmt, 0);

    private static bool HasNoreturnLoopExit(CStmt stmt, int nested) => stmt switch
    {
        Break => nested == 0,
        Goto or Return => true,
        Block b => b.Stmts.Any(s => HasNoreturnLoopExit(s, nested)),
        Seq s => s.Stmts.Any(s => HasNoreturnLoopExit(s, nested)),
        If f => HasNoreturnLoopExit(f.Then, nested) || f.Else is { } e && HasNoreturnLoopExit(e, nested),
        DeferGuard d => HasNoreturnLoopExit(d.Body, nested) || HasNoreturnLoopExit(d.Cleanup, nested),
        Labeled l => HasNoreturnLoopExit(l.Body, nested),
        While w => HasNoreturnLoopExit(w.Body, nested + 1),
        DoWhile d => HasNoreturnLoopExit(d.Body, nested + 1),
        For f => HasNoreturnLoopExit(f.Body, nested + 1),
        Switch s => s.Sections.Any(a => a.Body.Any(s => HasNoreturnLoopExit(s, nested + 1))),
        _ => false,
    };

    private CExpr LowerNoreturnPanic(IReadOnlyList<Item> args)
    {
        if (args.Count != 1) { throw new IrUnsupportedException("zig `@panic` expects one message argument"); }
        var message = LowerExprSink(args[0], new CType.Slice(CType.UChar.WithQuals(TypeQual.Const)));
        // Evaluate the message before trapping, including effects in a computed message.
        return new CommaOp(new CExpr[] { message, UnreachableCall() }) { Type = CType.NoReturn };
    }
}
