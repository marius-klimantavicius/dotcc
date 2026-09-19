#nullable enable
using System;
using System.Linq;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private CType? _staticFlexibleInitializerType;

    private CExpr BuildStaticAggregateInitializer(CType type, Func<CExpr> build)
    {
        var previous = _staticFlexibleInitializerType;
        _staticFlexibleInitializerType = type.Unqualified;
        CExpr initializer;
        try { initializer = build(); }
        finally { _staticFlexibleInitializerType = previous; }
        if (initializer is not StructInit aggregate || type.Unqualified is not CType.Named) return initializer;
        var field = StructFieldsOf(type).LastOrDefault();
        if (!field.IsFlexibleArray) return initializer;
        var tail = aggregate.Members.FirstOrDefault(member => member.Name == field.Name);
        if (tail.Value is not InlineArrayInit values) return initializer;
        var header = aggregate with { Members = aggregate.Members.Where(member => member.Name != field.Name).ToArray() };
        return new FlexibleAggregateInit(header, field.Name, values.Element, values.Elems) { Type = type };
    }

    private bool IsStaticFlexibleMember(CType owner, StructField field) =>
        field.IsFlexibleArray && owner.Unqualified == _staticFlexibleInitializerType
        && owner.Unqualified is CType.Named named && !_structIsUnion.GetValueOrDefault(named.Name)
        && StructFieldsOf(owner).Last().Name == field.Name;

    private bool TryBuildStaticFlexibleMember(CType owner, StructField field, Item initializer, out CExpr value)
    {
        value = null!;
        if (!IsStaticFlexibleMember(owner, field)) return false;
        var group = initializer.Content switch
        {
            C.InitListOne or C.InitListCons or C.InitListTrail => new InitGroup(ParseInitList(initializer)),
            _ => throw new IrUnsupportedException("static flexible array member initializer requires braces"),
        };
        return TryBuildStaticFlexibleMember(owner, field, group, out value);
    }

    private bool TryBuildStaticFlexibleMember(CType owner, StructField field, Init initializer, out CExpr value)
    {
        value = null!;
        if (!IsStaticFlexibleMember(owner, field)) return false;
        if (field.Type.Unqualified is not CType.Array { Element: var element }
            || element.Unqualified is CType.Array || initializer is not InitGroup group)
            throw new IrUnsupportedException("static flexible array initializer shape");
        // Infer this object's tail extent from the initializer. The declared
        // aggregate and flexible field retain their original C size and offset.
        var elements = BuildArrayElems(element, null, group.Items);
        value = new InlineArrayInit(element, elements) { Type = field.Type };
        return true;
    }
}
