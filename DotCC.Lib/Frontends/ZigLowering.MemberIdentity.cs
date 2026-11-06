using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private static string MemberToken(Item token) => token.Content is LALR.CC.Reduction { Children: [var only] }
        ? MemberToken(only) : token.Content as string ?? throw new IrUnsupportedException("expected a member token lexeme");

    private static bool QuotedMemberToken(Item token) => MemberToken(token).StartsWith("@\"", StringComparison.Ordinal);

    private static string SourceMemberName(Item token)
    {
        var raw = MemberToken(token);
        return raw.StartsWith("@\"", StringComparison.Ordinal) ? SourceMemberString(raw[1..]) : raw;
    }

    private static string SourceMemberString(string quoted)
        => ZigMemberIdentityRegistry.Decode(EmitHelpers.StringByteValues(new[] { NormalizeZigByteEscapes(quoted) })
            .Select(b => checked((byte)b)).ToArray());

    private List<EnumMember> AnnotateEnumMembers(string owner, List<EnumMember> members)
    {
        var identities = _shared.MemberIdentities.Register(owner, ZigMemberKind.Enum,
            members.Select(m => (m.Name, (CType?)null)));
        return members.Select((m, i) => m with { TargetName = identities[i].TargetName }).ToList();
    }

    private IReadOnlyList<StructField>? SourceUnionFields(CType type)
    {
        if (type.Unqualified is not CType.Named named || !_unions.TryGetValue(named.Name, out var union)) return null;
        var identities = _shared.MemberIdentities.Register(named.Name, ZigMemberKind.Variant,
            union.Variants.Select(v => (v.Key, v.Value)));
        if (union.SourceFields is { } fields) return fields;
        return identities.Select(m => new StructField(m.SourceName, m.PayloadType ?? CType.Void)).ToList();
    }

    /// <summary>Quoted member reads stop here: they cannot fall into normalized const/method navigation.</summary>
    private CExpr LowerQuotedMemberRead(Zig.Field field)
    {
        var name = SourceMemberName(field.Arg2);
        if (field.Arg0.Content is Zig.Ident id && _symbols.Resolve(Tok(id.Arg0)) is null
            && TryLookupContainerType(Tok(id.Arg0), out var type))
        {
            if (type.Unqualified is CType.Enum en) return ResolveEnumLit(name, en, allowValueDeclaration: true);
            throw new IrUnsupportedException($"zig: quoted declaration member '{name}' is unsupported (only enum members and value fields are modeled)");
        }
        if (field.Arg0.Content is Zig.Field)
        {
            if (TryResolveModuleNestedType(field.Arg0) is { Type.Unqualified: CType.Enum imported, Owner: var owner })
                return owner.ResolveEnumLit(name, imported, allowValueDeclaration: true);
            if (TryResolveQualifiedNestedType(field.Arg0)?.Unqualified is CType.Enum nested) return ResolveEnumLit(name, nested, allowValueDeclaration: true);
        }
        var receiver = LowerExpr(field.Arg0);
        if (receiver.Type.Unqualified is CType.Tuple tuple
            && int.TryParse(name, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index)
            && index < tuple.Elements.Count)
            return TupleElement(receiver, index);
        return ExactValueMember(receiver, name, "quoted member");
    }

    private CExpr ExactValueMember(CExpr receiver, string name, string operation)
    {
        var arrow = receiver.Type.Unqualified is CType.Pointer;
        if (TryContainerName(receiver.Type, out var container) && _unions.TryGetValue(container, out var union)
            && union.Variants.TryGetValue(name, out var payload))
        {
            if (payload is null) return new DefaultLit { Type = CType.Void };
            var storage = new Member(receiver, union.PayloadFieldName, arrow)
                { Type = new CType.Named(union.PayloadTypeName!), IsLValue = true };
            return new Member(storage, name, false) { Type = payload, IsLValue = true };
        }
        var fieldType = _ir.StructFieldType(receiver.Type, name)
            ?? throw new IrUnsupportedException($"zig `{operation}`: {receiver.Type.Describe()} has no field '{name}'");
        return new Member(receiver, name, arrow) { Type = fieldType, IsLValue = true };
    }

    /// <summary>Only immutable literal storage can be cached as a name. Mutable array views and
    /// runtime/effectful expressions keep the existing demand-time name checks.</summary>
    private void RememberImmutableLiteralMemberName(Symbol symbol, bool isConst, bool inferred, CExpr initializer)
    {
        if (!isConst || !inferred) return;
        LitStr? literal = initializer as LitStr;
        if (literal is null && initializer is SliceNew { Element.Unqualified: CType.Prim { Integer: true, Bytes: 1 } } slice
            && LiteralMemberNamePointer(slice.Ptr) && LiteralMemberNameBound(slice.Len)
            && _ir.EvalComptimeValue(initializer) is IrBuilder.CtSlice value
            && value.Offset >= 0 && value.Length >= 0 && value.Offset <= value.Backing.Elems.Length
            && value.Length <= value.Backing.Elems.Length - value.Offset
            && _ir.SpliceComptimeValue(value) is SliceNew { Ptr: LitStr bytes })
            literal = bytes;
        if (literal is not null && DecodeComptimeName(EmitHelpers.StringByteValues(literal.Segments)
                .Select(b => checked((byte)b)).ToArray()) is { } name)
            _constStringLocals[symbol] = name;
    }

    private static bool LiteralMemberNamePointer(CExpr expression) => expression switch
    {
        LitStr => true,
        Paren p => LiteralMemberNamePointer(p.Inner),
        Cast c when c.Type.Unqualified is CType.Pointer { Pointee.Unqualified: CType.Prim { Integer: true, Bytes: 1 } }
            => LiteralMemberNamePointer(c.Operand),
        Binary { Op: BinOp.Add or BinOp.Sub } b => LiteralMemberNamePointer(b.Left) && LiteralMemberNameBound(b.Right),
        _ => false,
    };

    private static bool LiteralMemberNameBound(CExpr expression) => expression switch
    {
        LitInt => true,
        Paren p => LiteralMemberNameBound(p.Inner),
        Cast c when c.Type.Unqualified is CType.Prim { Integer: true, Bytes: <= 8 } => LiteralMemberNameBound(c.Operand),
        Binary { Op: BinOp.Add or BinOp.Sub } b => LiteralMemberNameBound(b.Left) && LiteralMemberNameBound(b.Right),
        _ => false,
    };
}
