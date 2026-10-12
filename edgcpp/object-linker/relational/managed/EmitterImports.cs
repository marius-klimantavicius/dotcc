using System.Text;

namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly bool requiredProviders;
    private sealed record ProviderImport(SymbolRef Symbol,string Name,string? Accessor);
    private readonly Dictionary<string,ProviderImport> providerImports=new(StringComparer.Ordinal);
    private bool providerStorageNamesPrepared;

    private ProviderImport? RequireProvider(SymbolRef selected)
    {
        if(selected.Symbol.Kind=="source-function")
            throw new InvalidDataException("Executable reference requires a recorded selected ABI entry for source function "+
                selected.Symbol.Id+" ("+selected.Symbol.LinkName+") in "+selected.Unit.Path);
        if(!requiredProviders || selected.Symbol.Definition)return null;
        var symbol=selected.Symbol;
        if(symbol.Linkage!="external" || string.IsNullOrEmpty(symbol.LinkName))
            throw new InvalidDataException("Required provider needs an exact external link identity: "+symbol.Name);
        if(symbol.Kind is not ("function" or "global" or "rtti-object"))
            throw new InvalidDataException("Required provider declaration is not supported for symbol kind "+symbol.Kind+": "+symbol.LinkName);
        if(!providerImports.TryGetValue(symbol.LinkName,out var import))
        {
            // Reserve all required callable names while collecting program
            // references; storage properties/getters are named afterwards.
            string name=symbol.Kind=="function"?names.Allocate(symbol.Name,"import_"+symbol.Id):"";
            import=new(selected,name,null);
            providerImports.Add(symbol.LinkName,import);
            var signature=selected.Unit.Original(symbol.Type);
            if(symbol.Kind=="function" && signature.Kind!="function")
                throw new InvalidDataException("Required provider is missing its recorded function signature: "+symbol.LinkName);
            if(signature.Variadic)needsVarargsRuntime=true;
            if(symbol.Kind!="function" && providerStorageNamesPrepared)import=NameProviderStorage(import);
        }
        if(import.Name.Length!=0)symbolNames.TryAdd(selected,import.Name);
        return import;
    }

    private ProviderImport NameProviderStorage(ProviderImport import)
    {
        var symbol=import.Symbol.Symbol;
        string name=names.AllocateProperty(symbol.Name,"import_"+symbol.Id);
        import=import with {Name=name,Accessor=names.Allocate("__address_"+name.TrimStart('@'),"provider_address")};
        providerImports[symbol.LinkName]=import;
        symbolNames.TryAdd(import.Symbol,name);
        return import;
    }

    private void PrepareProviderStorageNames()
    {
        foreach(var import in providerImports.Values.Where(import=>import.Symbol.Symbol.Kind!="function").ToArray())NameProviderStorage(import);
        PrepareVirtualTableProviderNames();
        providerStorageNamesPrepared=true;
    }

    private void UseSymbol(Unit unit,long id)
    {
        var selected=program.Resolve(unit,id);
        UseType(selected.Unit,selected.Symbol.Type);
        if(RequireProvider(selected) is not null && selected.Unit.Original(selected.Symbol.Type).ObjectType is long receiver)
            UseType(selected.Unit,receiver);
    }

    private bool HasProviderAddress(Unit unit,long id)=>requiredProviders &&
        !program.Resolve(unit,id).Symbol.Definition && unit.Symbols[id].Kind is "global" or "rtti-object";

    // Definitions carry the selected receiver parameter. Imported callable
    // declarations carry EDG's this_class through types.object_type_id.
    private string ReceiverType(SymbolRef selected)
    {
        if(selected.Unit.FunctionDetails.TryGetValue(selected.Id,out var details) && details.Receiver is long receiver)
            return Type(selected.Unit,selected.Unit.Symbols[receiver].Type);
        if(RequireProvider(selected) is not null && selected.Unit.Original(selected.Symbol.Type).ObjectType is long owner)
            return Pointer(selected.Unit,owner);
        throw new InvalidDataException("Selected callable receiver facts are missing for "+selected.Symbol.LinkName);
    }

    private string DeclarationExceptionPolicy(SymbolRef selected)
    {
        if(selected.Unit.FunctionDetails.TryGetValue(selected.Id,out var details))return details.ExceptionPolicy;
        if(RequireProvider(selected) is not null)return selected.Unit.Original(selected.Symbol.Type).ExceptionPolicy;
        throw new InvalidDataException("No definition for return cleanup destructor "+selected.Symbol.LinkName);
    }

    private string ProviderDeclaration(ProviderImport import)
    {
        var selected=import.Symbol;var unit=selected.Unit;
        if(import.Accessor is not null)
            return "public static partial "+Pointer(unit,selected.Symbol.Type)+" "+import.Accessor+"();";
        var signature=unit.Original(selected.Symbol.Type);
        if(signature.Result is not long result)
            throw new InvalidDataException("Required provider return type is missing for "+selected.Symbol.LinkName);
        var parameters=unit.TypeParameters.GetValueOrDefault(signature.Id,[])
            .Select((type,index)=>Type(unit,type)+" argument"+index).ToList();
        if(signature.ObjectType is long receiver && signature.ParametersIncludeReceiver!=true)parameters.Insert(0,Pointer(unit,receiver)+" receiver");
        if(signature.ReturnStorage=="caller-provided")parameters.Insert(0,Type(unit,result)+"* result");
        if(signature.Variadic)parameters.Add("params "+VaSpanType+" arguments");
        return "public static partial "+(signature.ReturnStorage=="caller-provided"?"void":Type(unit,result))+" "+import.Name+"("+string.Join(", ",parameters)+");";
    }

    private void WriteRequiredProviders()
    {
        foreach(var import in providerImports.Values)
        {
            Line(1,"// Required provider for EDG link identity "+StringLiteral(import.Symbol.Symbol.LinkName));
            if(import.Accessor is not null)
                Line(1,"// The provider owns this storage and must return the same valid address for the program lifetime.");
            Line(1,ProviderDeclaration(import));
            if(import.Accessor is not null)
                Line(1,"public static "+Pointer(import.Symbol.Unit,import.Symbol.Symbol.Type)+" "+import.Name+" => "+import.Accessor+"();");
        }
        WriteRequiredVirtualTableProviders();
    }

    private void WriteImportsManifest(string directory)
    {
        static string Cell(string value)=>value.Replace("\\","\\\\",StringComparison.Ordinal)
            .Replace("\t","\\t",StringComparison.Ordinal).Replace("\r","\\r",StringComparison.Ordinal).Replace("\n","\\n",StringComparison.Ordinal);
        using var writer=new StreamWriter(Path.Combine(directory,"Imports.tsv"),false,new UTF8Encoding(false));
        writer.WriteLine("link_name\tkind\tsource_name\tunit\tsymbol_id\ttype_id\texception_policy\tdeclaration\tvirtual_table_id\taddress_point_byte_offset\tentry_size");
        foreach(var import in providerImports.Values)
        {
            var selected=import.Symbol;var symbol=selected.Symbol;
            writer.WriteLine(string.Join('\t',new[]{symbol.LinkName,symbol.Kind,symbol.Name,selected.Unit.Path,
                symbol.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),symbol.Type.ToString(System.Globalization.CultureInfo.InvariantCulture),
                symbol.Kind=="function"?selected.Unit.Original(symbol.Type).ExceptionPolicy:"",ProviderDeclaration(import),"","",""}.Select(Cell)));
        }
        foreach(var import in virtualTableImports.Values)
            writer.WriteLine(string.Join('\t',new[]{import.Table.LinkName,"virtual-table",import.Unit.Types[import.Table.Owner].Name,
                import.Unit.Path,"",import.Table.Owner.ToString(System.Globalization.CultureInfo.InvariantCulture),"",
                VirtualTableProviderDeclaration(import),import.Table.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                import.Table.AddressPoint.ToString(System.Globalization.CultureInfo.InvariantCulture),
                import.Table.EntrySize.ToString(System.Globalization.CultureInfo.InvariantCulture)}.Select(Cell)));
    }
}
