namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private SymbolRef ResolveTypeInfoObject(Unit unit,Node node)
    {
        if(node.Symbol is not long symbol || !unit.Symbols.TryGetValue(symbol,out var declaration) || declaration.Kind!="rtti-object")
            throw unit.Missing(node,"exporter defect: selected RTTI object symbol missing");
        var provider=program.Resolve(unit,symbol);
        if(!provider.Symbol.Definition)
            throw unit.Missing(node,"Unresolved RTTI provider for "+declaration.LinkName);
        if(provider.Symbol.Kind=="rtti-object" && !provider.Unit.Initializers.ContainsKey(provider.Id))
            throw unit.Missing(node,"RTTI provider initializer is not recorded for "+provider.Symbol.LinkName);
        return provider;
    }

    private sealed partial class FunctionWriter
    {
        private string TypeInfoAddress(Node node)
        {
            var provider=emitter.ResolveTypeInfoObject(unit,node);
            if(!provider.Unit.IsArray(provider.Symbol.Type) && !emitter.HasPinnedRecordStorage(provider.Unit,provider.Id) &&
               !emitter.HasPinnedScalarStorage(provider.Unit,provider.Id))
                throw unit.Missing(node,"RTTI provider requires recorded addressable persistent storage for "+provider.Symbol.LinkName);
            return "unchecked(("+NodeType(node)+")"+emitter.SymbolName(provider.Unit,provider.Id)+")";
        }
    }
}
