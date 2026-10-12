namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit Unit,long Type),string> arrayNames=[];
    private readonly Dictionary<string,(Unit Unit,long Type,string Name)> arrayLayouts=new(StringComparer.Ordinal);
    private bool needsLocalArrayStorage;
    private string? localArrayStorageName;

    private void PrepareArrays()
    {
        foreach(var unit in program.Units)
            foreach(var type in unit.Types.Values.Where(type=>unit.IsArray(type.Id) && type.Kind=="array" && executableTypes.Contains((unit,type.Id))))
                PrepareArrayType(unit,type.Id);
        if(needsLocalArrayStorage)localArrayStorageName=names.Allocate("__LocalArrayStorage","local_array_storage");
    }

    private void PrepareArrayType(Unit unit,long id)
    {
        var type=unit.Original(id);
        if(type.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
        {
            PrepareArrayType(unit,type.Pointee!.Value);return;
        }
        if(!unit.IsArray(type.Id) || arrayNames.ContainsKey((unit,type.Id)))return;
        if(IsZeroExtentArray(unit,type.Id))return;
        long elementId=type.Element??throw new InvalidDataException("Array element type missing");
        PrepareArrayType(unit,elementId);
        string element=Type(unit,elementId);
        string key=element+":"+type.Count+":"+type.Size+":"+type.Alignment;
        if(!arrayLayouts.TryGetValue(key,out var layout))
        {
            string name=names.Allocate("__array_"+element+"_"+type.Count,"array_storage");
            layout=(unit,type.Id,name);arrayLayouts.Add(key,layout);
        }
        arrayNames.Add((unit,type.Id),layout.Name);
    }

    private static void CheckArrayLayout(Unit unit,ProgramType type)
    {
        if(IsZeroExtentArray(unit,type.Id))
            throw new InvalidDataException("Zero-extent array non-field storage emission is not yet implemented: type "+type.Id);
        if(type.Dependent || type.Incomplete || type.Count is not >0 || type.Size is not >0 || type.Size>int.MaxValue || type.Alignment is not >0)
            throw new InvalidDataException("Fixed array storage requires recorded positive count, size and alignment: "+type.Name);
        if(type.Element is not long element)throw new InvalidDataException("Array element type missing");
        var entry=unit.Original(element);
        if(entry.Kind=="record" || unit.VaListRole(element) is not null)
            throw new InvalidDataException("Record or va_list array elements are not yet implemented");
    }

    private void WriteArrayTypes()
    {
        foreach(var layout in arrayLayouts.Values)
        {
            var unit=layout.Unit;var type=unit.Types[layout.Type];CheckArrayLayout(unit,type);
            Line(1,"// Fixed array storage: "+Type(unit,type.Element!.Value)+"["+type.Count+"].");
            Line(1,"[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = "+type.Size+")]");
            Line(1,"public struct "+layout.Name);Line(1,"{");
            Line(2,"[global::System.Runtime.InteropServices.FieldOffset(0)]");
            Line(2,"public "+Type(unit,type.Element!.Value)+" First;");Line(1,"}");
        }
        if(localArrayStorageName is not string owner)return;
        Line(1,"private ref struct "+owner);Line(1,"{");
        Line(2,"private void* allocation;");Line(2,"public readonly void* Address { get; }");
        Line(2,"public "+owner+"(nuint size, nuint alignment)");Line(2,"{");
        Line(3,"allocation = global::System.Runtime.InteropServices.NativeMemory.Alloc(checked(size + alignment - 1));");
        Line(3,"if (allocation == null) throw new global::System.OutOfMemoryException();");
        Line(3,"Address = (void*)(((nuint)allocation + alignment - 1) & ~(alignment - 1));");Line(2,"}");
        Line(2,"public void Dispose()");Line(2,"{");
        Line(3,"global::System.Runtime.InteropServices.NativeMemory.Free(allocation);");Line(3,"allocation = null;");Line(2,"}");Line(1,"}");
    }

    private static string AlignedArrayAddress(string storage,long alignment)=>
        "(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref "+storage+"[0]) + "+(alignment-1)+"U) & ~(nuint)"+(alignment-1)+"U)";

    private void WriteGlobalArray(SymbolRef global)
    {
        var unit=global.Unit;var type=unit.Original(global.Symbol.Type);CheckArrayLayout(unit,type);
        string name=SymbolName(unit,global.Id),storage=names.Allocate("__"+name+"_storage","global_array_storage");
        string create=names.Allocate("__"+name+"_initialize","initialize_global_array");
        long bytes=checked(type.Size!.Value+type.Alignment!.Value-1);
        if(bytes>int.MaxValue)throw new InvalidDataException("Pinned global array size exceeds supported managed storage");
        Line(1,"private static readonly byte[] "+storage+" = global::System.GC.AllocateArray<byte>("+bytes+", pinned: true);");
        Line(1,"public static "+Type(unit,type.Id)+"* "+name+" => ("+Type(unit,type.Id)+"*)"+AlignedArrayAddress(storage,type.Alignment!.Value)+";");
        if(!unit.Initializers.TryGetValue(global.Id,out long initialization))
            throw new InvalidDataException("Exporter defect: global array initialization actions missing for "+global.Symbol.Name);
        string? data=WriteConstantArrayData(unit,unit.Nodes[initialization],type,name);
        Line(1,"private static void "+create+"()");Line(1,"{");
        if(data is not null)
            Line(2,data+".CopyTo(new global::System.Span<"+Type(unit,type.Element!.Value)+">(("+Type(unit,type.Element.Value)+"*)"+name+", "+type.Count+"));");
        else new FunctionWriter(this,unit,null).WriteArrayInitializer(unit.Nodes[initialization],name,2);
        Line(1,"}");globalInitializerNames.Add(create);
    }

    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,string> localArrayOwners=[];

        private void WriteLocalArrayStorage(int indent)
        {
            foreach(var pair in localArrayOwners)
            {
                var type=unit.Original(unit.Symbols[pair.Key].Type);
                if(IsZeroExtentArray(unit,type.Id) && unit.Nodes.Values.FirstOrDefault(node=>node.Symbol==pair.Key && node.Kind=="initialize") is Node declaration)
                    throw unit.Missing(declaration,"zero-extent array non-field storage emission is not yet implemented");
                CheckArrayLayout(unit,type);
                Line(indent,"using var "+pair.Value+" = new "+emitter.localArrayStorageName+"("+type.Size+"U, "+type.Alignment+"U);");
                Line(indent,Type(type.Id)+"* "+Name(pair.Key)+" = ("+Type(type.Id)+"*)"+pair.Value+".Address;");
            }
        }

        private string ArrayAddress(Node node)
        {
            if(ZeroExtentFieldAddress(node) is string zeroField)return zeroField;
            if(node.Kind=="string-literal" && node.LiteralObject is long literal)
                return "(("+NodeType(node)+"*)"+emitter.literalNames[(unit,literal)]+")";
            if(node.Kind=="variable" && node.Symbol is long symbol)return Name(symbol);
            if(node.Kind=="operation" && node.Operator is "indirect" or "ref-indirect")return Expression(Child(node,"operand"));
            if(node.Kind=="operation" && node.Operator=="lvalue-adjust")return ArrayAddress(Child(node,"operand"));
            if(node.Kind=="operation" && node.Operator is "subscript" or "dot-field" or "points-to-field")return "(&"+Expression(node)+")";
            throw unit.Missing(node,"array address emission is not yet implemented for this expression");
        }

        private string ArrayDecay(Node node)
        {
            long id=node.ExecutionType??node.Type??throw unit.Missing(node,"array decay operand type missing");
            var type=unit.Original(id);
            if(!unit.IsArray(id))throw unit.Missing(node,"array decay requires recorded array type");
            return "(("+Type(type.Element??throw unit.Missing(node,"array element type missing"))+"*)"+ArrayAddress(node)+")";
        }

        private void WriteArrayDeclaration(Node node,int indent)
        {
            long variable=node.Symbol??throw unit.Missing(node,"array declaration symbol missing");
            if(node.Initialization is long selected)
            {
                var initialization=unit.Initializations[selected];
                CheckInitialization(initialization);
                if(initialization.Destructor is not null)throw unit.Missing(node,"array destruction emission is not yet implemented");
                if(initialization.Kind=="none")return;
                if(initialization.Kind=="zero")
                {
                    Line(indent,"global::System.Runtime.InteropServices.NativeMemory.Clear("+Name(variable)+", "+unit.Original(unit.Symbols[variable].Type).Size+"U);");return;
                }
                if(initialization.Kind is not("constant" or "expression" or "aggregate") || initialization.Value is not long value)
                    throw unit.Missing(node,"array initialization requires explicit array initializer actions");
                WriteArrayInitializer(unit.Nodes[value],Name(variable),indent);return;
            }
            if(unit.Children(node,"value").FirstOrDefault() is Node expression)WriteArrayInitializer(expression,Name(variable),indent);
        }

        public void WriteArrayInitializer(Node node,string address,int indent)
        {
            if(unit.LiteralInitializers.ContainsKey(node.Id)){WriteLiteralInitializer(node,address,indent);return;}
            if(!unit.ArrayInitializers.TryGetValue(node.Id,out var initialization))
                throw unit.Missing(node,"exporter defect: array initializer actions missing");
            var type=unit.Original(node.Type??throw unit.Missing(node,"array initializer type missing"));
            if(IsZeroExtentArray(unit,type.Id) && unit.ArrayElements.GetValueOrDefault(node.Id,[]).Count==0 &&
                node.Initialization is null && node.Lifetime is null)return;
            CheckArrayLayout(unit,type);
            string element=Type(type.Element!.Value);
            if(initialization.ZeroInitialize)
                Line(indent,"global::System.Runtime.InteropServices.NativeMemory.Clear("+address+", "+type.Size+"U);");
            foreach(var action in unit.ArrayElements.GetValueOrDefault(node.Id,[]))
            {
                if(action.Index<0 || action.Repeat<=0 || action.Index>type.Count-action.Repeat)
                    throw unit.Missing(node,"exporter defect: array initializer range exceeds recorded storage");
                string index=action.Index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                int level=indent;
                if(action.Repeat>1)
                {
                    index="__array_index_"+node.Id+"_"+action.Index;
                    Line(indent,"for (long "+index+" = "+action.Index+"; "+index+" < "+checked(action.Index+action.Repeat)+"; ++"+index+")");
                    Line(indent,"{");level++;
                }
                string destination="(("+element+"*)"+address+")["+index+"]";
                var value=unit.Nodes[action.Value];
                if(!WriteAggregateComponent(value,destination,level,action.Repeat))
                {
                    if(unit.IsArray(type.Element!.Value))WriteArrayInitializer(value,"(&"+destination+")",level);
                    else Line(level,destination+" = "+ExpandedExpression(value,level)+";");
                }
                if(action.Repeat>1)Line(indent,"}");
            }
        }
    }
}
