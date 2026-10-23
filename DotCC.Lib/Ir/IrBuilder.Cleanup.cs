#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private CStmt BuildGnuCleanupDeclaration(Item attributes, Item declaration)
    {
        string? cleanup = null;
        void Read(Item item)
        {
            switch (item.Content)
            {
                case C.AttrListCons list: Read(list.Arg0); Read(list.Arg2); break;
                case C.AttrIdent name when Tok(name.Arg0).Trim('_') == "unused": break;
                case C.AttrCall call when Tok(call.Arg0).Trim('_') == "cleanup"
                    && call.Arg2.Content is C.Var function:
                    if (cleanup is not null)
                        throw new IrUnsupportedException("duplicate GNU cleanup attribute");
                    cleanup = Tok(function.Arg0);
                    break;
                default: throw new IrUnsupportedException("unsupported GNU local declaration attribute");
            }
        }
        if (attributes.Content is not C.GnuFunctionAttrs list)
            throw new IrUnsupportedException("unsupported GNU local declaration attributes");
        Read(list.Arg3);
        var built = BuildDeclStmt(declaration);
        if (cleanup is null) return built;
        if (built is not DeclStmt locals || locals.Decls.Count == 0)
            throw new IrUnsupportedException("GNU cleanup requires automatic scalar or aggregate objects");
        var functionSymbol = _symbols.Resolve(cleanup);
        if (functionSymbol is not { Kind: SymKind.Func, Type: CType.Func signature }
            || signature.Variadic || signature.Params.Count != 1)
            throw new IrUnsupportedException("GNU cleanup requires a declared one-parameter function");
        _referencedFuncs.Add(cleanup);
        var actions = new List<CStmt>();
        foreach (var local in locals.Decls.Reverse())
        {
            var pointerType = new CType.Pointer(local.Sym.Type);
            if (signature.Params[0].Unqualified is not CType.Pointer parameter
                || (parameter.Pointee.Unqualified != CType.Void
                    && parameter.Pointee.Unqualified != local.Sym.Type.Unqualified))
                throw new IrUnsupportedException("GNU cleanup parameter must accept the declared object's address");
            local.Sym.AddressTaken = true;
            var address = new Unary(UnOp.AddrOf, new VarRef(local.Sym) { Type = local.Sym.Type })
                { Type = pointerType };
            actions.Add(new ExprStmt(new Call(cleanup, new CExpr[] { address }, signature.Params, functionSymbol)
                { Type = signature.Return }));
        }
        return locals with { ScopeCleanup = new Block(actions) };
    }

    private static List<CStmt> RewriteScopeCleanups(List<CStmt> statements)
    {
        for (var index = statements.Count - 1; index >= 0; index--)
        {
            if (statements[index] is not DeclStmt { ScopeCleanup: { } cleanup } declaration) continue;
            var tail = new Block(statements.Skip(index + 1).ToArray());
            statements.RemoveRange(index, statements.Count - index);
            statements.Add(declaration with { ScopeCleanup = null });
            statements.Add(new DeferGuard(tail, cleanup, OnErrorOnly: false) { Pos = declaration.Pos });
        }
        return statements;
    }
}
