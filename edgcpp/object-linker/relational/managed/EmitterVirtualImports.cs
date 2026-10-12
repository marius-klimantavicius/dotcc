namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed record VirtualTableImport(Unit Unit,VirtualTable Table,string Accessor);
    private readonly Dictionary<string,VirtualTableImport> virtualTableImports=new(StringComparer.Ordinal);
    private Unit? virtualCalleeUnit;
    private HashSet<long> virtualCalleeMetadata=[];
    private HashSet<long> virtualDestructionMetadata=[];

    private void PrepareVirtualCalleeMetadata(Unit unit)
    {
        var candidates=new HashSet<long>();
        foreach(var call in unit.VirtualCalls.Values)
            foreach(var edge in unit.Edges.GetValueOrDefault(call.Node,[]).Where(edge=>edge.Role=="operand" && edge.Ordinal==0))
                if(unit.Nodes.TryGetValue(edge.Child,out var node) && node.Kind=="function")candidates.Add(node.Id);
        // A shared designator still needs its provider if any recorded parent
        // evaluates it directly, including an ordinary call or address use.
        if(candidates.Count!=0)
            foreach(var pair in unit.Edges)
                foreach(var edge in pair.Value)
                    if(candidates.Contains(edge.Child) &&
                        !(unit.VirtualCalls.ContainsKey(pair.Key) && edge.Role=="operand" && edge.Ordinal==0))
                        candidates.Remove(edge.Child);
        var destructionCandidates=new HashSet<long>();
        var destructionNodes=new HashSet<long>();
        foreach(var node in unit.Nodes.Values.Where(node=>node.Kind=="delete" && node.Operator=="virtual-destroy"))
            foreach(var edge in unit.Edges.GetValueOrDefault(node.Id,[]).Where(edge=>edge.Role=="destroy"))
                if(unit.Nodes[edge.Child].Initialization is long initialization)
                {
                    destructionCandidates.Add(initialization);destructionNodes.Add(edge.Child);
                }
        // A source destructor action retained by virtual delete describes the
        // logical selection; the supplied D0 execution owns its actual call.
        // Retain a provider when the same action also has another recorded use.
        foreach(var node in unit.Nodes.Values)
            if(node.Initialization is long initialization && !destructionNodes.Contains(node.Id))destructionCandidates.Remove(initialization);
        foreach(long initialization in unit.Destructions.Values.SelectMany(actions=>actions))destructionCandidates.Remove(initialization);
        foreach(var initialization in unit.ConstructorInitializers.Values.SelectMany(actions=>actions))destructionCandidates.Remove(initialization.Initialization);
        virtualCalleeUnit=unit;virtualCalleeMetadata=candidates;virtualDestructionMetadata=destructionCandidates;
    }

    private VirtualTableImport RequireVirtualTableProvider(Unit unit,VirtualTable table)
    {
        if(!requiredProviders || table.Definition || table.Linkage!="external" || string.IsNullOrEmpty(table.LinkName))
            throw new InvalidDataException("Required virtual table provider needs an exact external declaration: "+table.LinkName);
        if(table.EntrySize!=8)
            throw new InvalidDataException("Required virtual table provider entry size is not yet supported: "+table.EntrySize);
        string key=VirtualKey(unit,table);
        if(!virtualTableImports.TryGetValue(key,out var import))
        {
            import=new(unit,table,"");virtualTableImports.Add(key,import);
            if(providerStorageNamesPrepared)import=NameVirtualTableProvider(import);
        }
        else if(import.Table.EntrySize!=table.EntrySize)
            throw new InvalidDataException("Conflicting recorded virtual table provider entry sizes: "+table.LinkName);
        return import;
    }

    private VirtualTableImport NameVirtualTableProvider(VirtualTableImport import)
    {
        string accessor=names.Allocate("__address_vtable_"+import.Unit.Types[import.Table.Owner].Name+"_"+
            import.Table.AddressPoint.ToString(System.Globalization.CultureInfo.InvariantCulture),"virtual_table_address");
        import=import with {Accessor=accessor};
        virtualTableImports[VirtualKey(import.Unit,import.Table)]=import;
        return import;
    }

    private void PrepareVirtualTableProviderNames()
    {
        foreach(var import in virtualTableImports.Values.ToArray())NameVirtualTableProvider(import);
    }

    private static string VirtualTableProviderDeclaration(VirtualTableImport import)=>
        "public static partial nint* "+import.Accessor+"();";

    private void WriteRequiredVirtualTableProviders()
    {
        foreach(var import in virtualTableImports.Values)
        {
            Line(1,"// Required provider for EDG virtual table "+StringLiteral(import.Table.LinkName)+
                ", selected address point "+import.Table.AddressPoint+" bytes, entry size "+import.Table.EntrySize+" bytes.");
            Line(1,"// Return the already selected address point; no byte offset is added by this program.");
            Line(1,"// The provider owns the storage and must preserve its address for the program lifetime.");
            Line(1,"// Dispatched entries must match the generated delegate* managed signatures and recorded slots.");
            Line(1,VirtualTableProviderDeclaration(import));
        }
    }
}
