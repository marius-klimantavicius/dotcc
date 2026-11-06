#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<string> _comptimeNamesInProgress = new(System.StringComparer.Ordinal);
    private static readonly UTF8Encoding ComptimeNameEncoding = new(false, true);

    /// <summary>Evaluate a demanded reflection name in its ordinary lexical/module context.
    /// Unlike the pass-0 string recorder, this may resolve a computed const, but never supplies
    /// an answer from a same-spelled binding when a runtime symbol owns the reference.</summary>
    private string? TryComptimeByteName(Item item)
    {
        while (item.Content is Zig.Grouped grouped) { item = grouped.Arg1; }
        // A file const's source belongs to the file, even when the name is demanded from
        // a function or a top-level comptime session. Keep caller locals out of its RHS.
        if (item.Content is Zig.Ident id && Tok(id.Arg0) is var name
            && _symbols.Resolve(name) is null or { IsGlobal: true }
            && _topLevelConstRhs.TryGetValue(name, out var rhs))
        {
            if (!_comptimeNamesInProgress.Add(name))
                throw new IrUnsupportedException($"zig: comptime name '{name}' has a dependency loop");
            using var ownerScope = new FnStateScope(this);
            using var comptime = EnterComptime();
            try
            {
                // A typed labeled initializer must execute in its own source-
                // ordered session, including statements that determine extents.
                if (UsesSourceStdlib && rhs.Content is Zig.LabeledBlock
                    && _lazyValueConsts.TryGetValue(name, out var declaration) && declaration.typeItem is not null)
                    return LowerLazyValueConst(name) is { } stored ? ComptimeByteNameValue(stored) : null;
                return TryComptimeByteName(rhs);
            }
            finally { _comptimeNamesInProgress.Remove(name); }
        }

        using var hoist = EnterFreshHoist();
        var lowered = LowerExpr(item);
        // Discarding hoisted statements or an unevaluated call would erase effects. Only
        // already-known comptime values and pure reads/projections enter the interpreter.
        if (_hoist is { Count: > 0 }) { return null; }
        return ComptimeByteNameValue(lowered);
    }

    private string? ComptimeByteNameValue(CExpr lowered)
    {
        if (!IsComptimeNameRead(lowered)) return null;
        if (lowered is LitStr literal)
            return DecodeComptimeName(DotCC.EmitHelpers.StringByteValues(literal.Segments).Select(b => (byte)b).ToArray());

        var element = lowered.Type.Unqualified switch
        {
            CType.Array a => a.Element,
            CType.Pointer { Pointee.Unqualified: CType.Array a } => a.Element,
            CType.Slice s => s.Element,
            _ => null,
        };
        if (element?.Unqualified is not CType.Prim { Integer: true, Bytes: 1 }) { return null; }
        var sliceType = new CType.Slice(CType.UChar.WithQuals(TypeQual.Const));
        var slice = lowered.Type.Unqualified is CType.Slice ? lowered : CoerceToSlice(lowered, sliceType);
        if (!IsComptimeNameRead(slice) || _ir.EvalComptimeValue(slice) is not IrBuilder.CtSlice value
            || value.Offset < 0 || value.Length < 0 || value.Offset > value.Backing.Elems.Length
            || value.Length > value.Backing.Elems.Length - value.Offset)
            return null;
        var bytes = new byte[(int)value.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            if (value.Backing.Elems[value.Offset + i] is not IrBuilder.CtInt octet || octet.Value < 0 || octet.Value > 255)
                return null;
            bytes[i] = (byte)octet.Value;
        }
        return DecodeComptimeName(bytes);
    }

    private static string? DecodeComptimeName(byte[] bytes)
    {
        try { return ComptimeNameEncoding.GetString(bytes); }
        catch (DecoderFallbackException) { return null; }
    }

    private bool IsComptimeNameRead(CExpr expression) => expression switch
    {
        LitStr or LitInt or LitFloat or LitBool or DefaultLit or NullPtr or EnumConstRef or SizeOfExpr or OffsetOf => true,
        VarRef v => v.Sym.Kind == SymKind.Func || v.Sym.IsConstexpr || _ir.ComptimeGlobals.ContainsKey(v.Sym)
            || _ir.ConstGlobalInits.TryGetValue(v.Sym, out var init) && IsComptimeNameRead(init),
        ComptimeFold { Live: true, Inner: var inner } => IsComptimeNameRead(inner),
        ComptimeFold { Resolved: { } resolved } => IsComptimeNameRead(resolved),
        Paren p => IsComptimeNameRead(p.Inner),
        Cast c => IsComptimeNameRead(c.Operand),
        Unary { Op: UnOp.AddrOf or UnOp.Deref or UnOp.Plus or UnOp.Neg or UnOp.BitNot or UnOp.LogNot } u
            => IsComptimeNameRead(u.Operand),
        Binary b => IsComptimeNameRead(b.Left) && IsComptimeNameRead(b.Right),
        CondExpr c => IsComptimeNameRead(c.Cond) && IsComptimeNameRead(c.Then) && IsComptimeNameRead(c.Else),
        Member m => IsComptimeNameRead(m.Base),
        DotCC.Ir.Index i => IsComptimeNameRead(i.Base) && IsComptimeNameRead(i.Idx),
        SliceNew s => IsComptimeNameRead(s.Ptr) && IsComptimeNameRead(s.Len),
        StackArray a => a.Elems.All(IsComptimeNameRead),
        PinnedArray { Elems: { } elems } => elems.All(IsComptimeNameRead),
        TupleNew t => t.Elements.All(IsComptimeNameRead),
        TupleIndex t => IsComptimeNameRead(t.Tuple),
        StructInit s => s.Members.All(m => IsComptimeNameRead(m.Value)),
        _ => false,
    };

    private string ComptimeReadDiagnostic(CExpr expression, int depth = 0)
    {
        if (depth >= 12) return expression.GetType().Name;
        string Child(CExpr value) => ComptimeReadDiagnostic(value, depth + 1);
        return expression switch
        {
            Member member => $"{member.Field}: {Child(member.Base)}",
            StructInit aggregate => string.Join("; ", aggregate.Members.Where(m => !IsComptimeNameRead(m.Value))
                .Take(3).Select(m => $"{m.Name}: {Child(m.Value)}")),
            VarRef variable when _ir.ConstGlobalInits.TryGetValue(variable.Sym, out var init)
                => $"{variable.Sym.Name}: {Child(init)}",
            VarRef variable => $"{variable.Sym.Name} ({variable.Sym.Kind})",
            Cast cast => Child(cast.Operand),
            Paren paren => Child(paren.Inner),
            Unary unary => $"{unary.Op}: {Child(unary.Operand)}",
            SliceNew slice => IsComptimeNameRead(slice.Ptr) ? Child(slice.Len) : Child(slice.Ptr),
            Call call => $"call {call.Callee}",
            _ => expression.GetType().Name,
        };
    }
}
