namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly List<string> globalInitializerNames=[];

    private void WriteGlobalInitializers()
    {
        if(globalInitializerNames.Count==0)return;
        Line(1,"static "+className+"()");Line(1,"{");
        foreach(string initializer in globalInitializerNames)Line(2,initializer+"();");
        Line(1,"}");
    }

    private bool HasPinnedScalarStorage(Unit unit,long symbol)
    {
        if(unit.Symbols[symbol].Kind is not ("global" or "static-local" or "rtti-object"))return false;
        var selected=program.Resolve(unit,symbol);
        return selected.Unit.Original(selected.Symbol.Type).Kind is
            "bool" or "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "i128" or "u128" or
            "f32" or "f64" or "f80" or "f128" or "enum" or "pointer" or "lvalue-reference" or "rvalue-reference" or "nullptr";
    }

    private void WriteGlobalScalar(SymbolRef global)
    {
        var unit=global.Unit;
        var layout=unit.Original(global.Symbol.Type);
        if(layout.Size is not >0 || layout.Alignment is not >0)
            throw new InvalidDataException("Pinned scalar storage requires recorded positive size and alignment: "+global.Symbol.Name);
        long bytes=checked(layout.Size.Value+layout.Alignment.Value-1);
        if(bytes>int.MaxValue)throw new InvalidDataException("Pinned scalar size exceeds supported managed storage");
        string name=SymbolName(unit,global.Id),type=Type(unit,global.Symbol.Type);
        string storage=names.Allocate("__"+name+"_storage","scalar_storage");
        string create=names.Allocate("__"+name+"_initialize","initialize_scalar");
        string value;
        if(unit.Initializers.TryGetValue(global.Id,out long initializer))
            value=new FunctionWriter(this,unit,null).Expression(unit.Nodes[initializer]);
        else if(global.Symbol.Kind=="static-local")
            throw new InvalidDataException("Exporter defect: static-local constant or zero initializer missing: "+global.Symbol.Name);
        else value="default";
        Line(1,"private static readonly byte[] "+storage+" = global::System.GC.AllocateArray<byte>("+bytes+", pinned: true);");
        Line(1,"public static "+type+"* "+name+" => ("+type+"*)"+AlignedArrayAddress(storage,layout.Alignment.Value)+";");
        Line(1,"private static void "+create+"()");Line(1,"{");
        Line(2,"*"+name+" = "+value+";");Line(1,"}");
        globalInitializerNames.Add(create);
    }

    private sealed partial class FunctionWriter
    {
        private long StaticDeclarationSymbol(Node node)
        {
            if(node.Symbol is not long symbol || unit.Symbols[symbol].Kind!="static-local")
                throw unit.Missing(node,"exporter defect: static declaration selected storage symbol missing");
            if(node.Initialization is not null)
                throw unit.Missing(node,"guarded static-local initialization is not yet supported");
            return symbol;
        }

        private void WriteStaticDeclaration(Node node,int indent)
        {
            long symbol=StaticDeclarationSymbol(node);
            // Storage is emitted once under its selected linker identity.
            Line(indent,"// static "+Type(unit.Symbols[symbol].Type)+" "+Name(symbol)+";");
        }
    }
}
