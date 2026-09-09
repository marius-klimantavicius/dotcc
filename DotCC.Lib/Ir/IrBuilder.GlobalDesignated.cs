using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private void BuildGlobalStructDesignated(Item typeItem, Item nameItem, Item memberList)
    {
        var type = ResolveType(typeItem);
        var position = SrcPos.From(nameItem);
        var declaration = RegisterScalarGlobal(new Symbol
        {
            Name = Tok(nameItem), Kind = SymKind.Var, Type = type,
            Storage = Storage.Static, IsGlobal = true,
        }, position);
        if (declaration is null) return;
        DefineRegisteredGlobal(declaration, BuildStructDesignated(type, memberList), hasInitializer: true, position);
    }
}
