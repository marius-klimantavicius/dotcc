using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private void BuildGlobalStructDesignated(Item typeItem, Item nameItem, Item memberList)
    {
        _sawThreadLocalSpec = false;
        var type = ResolveType(typeItem);
        var position = SrcPos.From(nameItem);
        var declaration = RegisterScalarGlobal(new Symbol
        {
            Name = Tok(nameItem), Kind = SymKind.Var, Type = type,
            Storage = Storage.Static, IsGlobal = true, IsThreadLocal = _sawThreadLocalSpec,
            Alignment = DeclarationAlignment(typeItem, type),
        }, position);
        if (declaration is null) return;
        var initializer = BuildStaticAggregateInitializer(type, () => BuildStructDesignated(type, memberList));
        ValidateThreadAggregateInitializer(declaration.Symbol, initializer);
        DefineRegisteredGlobal(declaration, initializer, hasInitializer: true, position);
    }

    private void ValidateThreadAggregateInitializer(Symbol symbol, CExpr initializer)
    {
        if (symbol.IsThreadLocal && !ConstantAggregate(initializer))
            throw new IrUnsupportedException("_Thread_local requires a supported constant aggregate initializer");
    }

    private bool ConstantAggregate(CExpr expression) => expression switch
    {
        StructInit aggregate => System.Linq.Enumerable.All(aggregate.Members, field => ConstantAggregate(field.Value)),
        InlineArrayInit array => System.Linq.Enumerable.All(array.Elems, ConstantAggregate),
        // String literals have static storage duration. Each TLS aggregate
        // owns its fields, while its literal pointers can share rooted storage.
        LitStr or LitU16Str or LitU32Str => true,
        Paren parenthesized => ConstantAggregate(parenthesized.Inner),
        Cast cast => ConstantAggregate(cast.Operand),
        _ => ConstEval(expression) is not null,
    };
}
