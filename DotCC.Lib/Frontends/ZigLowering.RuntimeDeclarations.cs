#nullable enable

using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Namespace navigation deliberately leaves file fields lazy. A named type
    // retained in runtime code (including behind a callback pointer) additionally
    // needs its real declaration in the managed product. This is declaration
    // demand, not a request to lower every method of that namespace.
    internal void CompleteRuntimeDeclarations()
    {
        if (!UsesSourceStdlib) return;
        var names = new Queue<string>();
        var visited = new HashSet<string>(System.StringComparer.Ordinal);
        var expressions = new HashSet<CExpr>(System.Collections.Generic.ReferenceEqualityComparer.Instance);
        void Type(CType type)
        {
            switch (type.Unqualified)
            {
                case CType.Named { IsExternal: false } named:
                    if (visited.Add(named.Name)) names.Enqueue(named.Name);
                    break;
                case CType.Pointer pointer: Type(pointer.Pointee); break;
                case CType.Array array: Type(array.Element); break;
                case CType.Slice slice: Type(slice.Element); break;
                case CType.Optional optional: Type(optional.Inner); break;
                case CType.ErrorUnion error: Type(error.Payload); break;
                case CType.Tuple tuple: foreach (var element in tuple.Elements) Type(element); break;
                case CType.Func function:
                    Type(function.Return);
                    foreach (var parameter in function.Params) Type(parameter);
                    break;
            }
        }
        void Expression(CExpr expression)
        {
            if (!expressions.Add(expression)) return;
            Type(expression.Type);
            // The shared C expression walker handles ordinary operands. Follow
            // the Zig-only carriers as well; their result may be void while a
            // receiver, element or operand still names a real runtime type.
            void Many(IEnumerable<CExpr> expressions) { foreach (var child in expressions) Expr(child); }
            switch (expression)
            {
                case StatementExpression statement: Statement(statement.Body); break;
                case SwitchExpr choice:
                    Expr(choice.Subject);
                    foreach (var arm in choice.Arms) Expr(arm.Value);
                    break;
                case NullCoalesce coalesce: Expr(coalesce.Left); Expr(coalesce.Right); break;
                case ErrUnionOk success: Expr(success.Payload); break;
                case ErrUnionErr error: Expr(error.Code); break;
                case ZigTry tried: Expr(tried.Inner); break;
                case ZigCatch caught: Expr(caught.Union); Expr(caught.Fallback); break;
                case SliceNew slice: Type(slice.Element); Expr(slice.Ptr); Expr(slice.Len); break;
                case TupleNew tuple: Many(tuple.Elements); break;
                case TupleIndex tuple: Expr(tuple.Tuple); break;
                case ArrayByValReturn array: Type(array.Element); Expr(array.Source); break;
                case ZigMemCall memory: Type(memory.Element); Many(memory.Args); break;
                case ZigListCall list: Expr(list.Recv); Many(list.Args); break;
                case AllocCall allocation: Type(allocation.Element); Expr(allocation.Receiver); Expr(allocation.Count); Expr(allocation.FbaCtx); break;
                case FreeCall free: Type(free.Element); Expr(free.Receiver); Expr(free.SliceExpr); Expr(free.FbaCtx); break;
                case CreateCall create: Type(create.Element); Expr(create.Receiver); Expr(create.FbaCtx); break;
                case DestroyCall destroy: Type(destroy.Element); Expr(destroy.Receiver); Expr(destroy.Ptr); Expr(destroy.FbaCtx); break;
                case ReallocCall resize: Type(resize.Element); Expr(resize.Receiver); Expr(resize.OldSlice); Expr(resize.NewCount); Expr(resize.FbaCtx); break;
                case ResizeCall resize: Type(resize.Element); Expr(resize.Receiver); Expr(resize.OldSlice); Expr(resize.NewCount); Expr(resize.FbaCtx); break;
                case RemapCall resize: Type(resize.Element); Expr(resize.Receiver); Expr(resize.OldSlice); Expr(resize.NewCount); Expr(resize.FbaCtx); break;
                case SizeOfExpr size: Type(size.Of); break;
            }
        }
        void Expr(CExpr? expression)
        {
            if (expression is not null) Backends.VaListLifetimeValidator.Walk(expression, Expression);
        }
        void Statement(CStmt? statement)
        {
            switch (statement)
            {
                case Block block: foreach (var child in block.Stmts) Statement(child); break;
                case Seq sequence: foreach (var child in sequence.Stmts) Statement(child); break;
                case DeclStmt declaration:
                    foreach (var local in declaration.Decls) { Type(local.Sym.Type); Expr(local.Init); }
                    break;
                case ArrayDecl array:
                    Type(array.Element); Expr(array.CountExpr);
                    if (array.Inits is { } values) foreach (var value in values) Expr(value);
                    break;
                case ExprStmt expression: Expr(expression.Expr); break;
                case Return result: Expr(result.Value); break;
                case Labeled label: Statement(label.Body); break;
                case If branch: Expr(branch.Cond); Statement(branch.Then); Statement(branch.Else); break;
                case While loop: Expr(loop.Cond); Statement(loop.Body); break;
                case DoWhile loop: Statement(loop.Body); Expr(loop.Cond); break;
                case For loop: Statement(loop.Init); Expr(loop.Cond); Expr(loop.Post); Statement(loop.Body); break;
                case Switch choice:
                    Expr(choice.Subject);
                    foreach (var section in choice.Sections)
                    {
                        foreach (var label in section.Labels) { Expr(label.CaseExpr); Expr(label.HiExpr); }
                        foreach (var child in section.Body) Statement(child);
                    }
                    break;
                case CaseLabelStmt label: Expr(label.CaseExpr); Statement(label.Body); break;
                case SetjmpGuard guard: Expr(guard.Env); Statement(guard.TryBody); Statement(guard.CatchBody); break;
                case SetjmpCapture capture: Expr(capture.Env); Expr(capture.Target); Statement(capture.Body); break;
                case DeferGuard guard: Statement(guard.Body); Statement(guard.Cleanup); break;
                case ZigErrorThrow error: Expr(error.Code); break;
            }
        }
        foreach (var definition in _ir.Types.Where(t => !_ir.PrunableTypes.Contains(t.Name)).ToArray())
            Type(new CType.Named(definition.Name));
        var functions = new HashSet<Symbol>();
        var globals = new HashSet<Symbol>();
        do
        {
            foreach (var function in _ir.Functions.ToArray())
                if (functions.Add(function.Sym)) { Type(function.Sym.Type); Statement(function.Body); }
            foreach (var global in _ir.Globals.ToArray())
                if (globals.Add(global.Sym)) { Type(global.Sym.Type); Expr(global.Init); }
            while (names.TryDequeue(out var name))
            {
                var owner = _moduleGraph?.OwnerOfContainer(name) ?? this;
                owner.EnsureNestedBody(name);
                owner.RaiseIfPoisoned(name);
                if (_ir.StructFieldsOf(name) is { } fields)
                    foreach (var field in fields) Type(field.Type);
            }
            // Real field defaults can name callbacks. Retain their bodies by the
            // ordinary source worklist before closing the declaration graph.
            _moduleGraph?.DrainAll();
        } while (_ir.Functions.Any(f => !functions.Contains(f.Sym)) || _ir.Globals.Any(g => !globals.Contains(g.Sym)));
    }
}
