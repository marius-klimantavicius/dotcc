namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly HashSet<(Unit,long)> executableTypes=[];
    private readonly HashSet<(Unit,long)> executableGlobalNodes=[];

    // Follow only recorded program references. Template recipes remain in the
    // object catalog; this traversal does not instantiate or analyze them.
    private void FindExecutableTypes()
    {
        foreach(var global in program.Globals)
        {
            if(virtualCalleeUnit!=global.Unit)PrepareVirtualCalleeMetadata(global.Unit);
            UseType(global.Unit,global.Symbol.Type);
            if(global.Unit.Initializers.TryGetValue(global.Id,out long initializer))
                UseNode(global.Unit,global.Unit.Nodes[initializer],[]);
        }
        foreach(var function in program.Functions)
        {
            var unit=function.Unit;
            UseType(unit,function.Symbol.Type);
            foreach(var local in unit.Symbols.Values.Where(symbol=>symbol.Parent==function.Id))
            {
                UseType(unit,local.Type);
                needsLocalArrayStorage|=local.Kind=="local" && unit.IsArray(local.Type);
            }
            using var body=program.LoadFunction(function);
            PrepareVirtualCalleeMetadata(unit);
            var visited=new HashSet<long>();
            foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function.Id))UseNode(unit,node,visited);
            var byteZeroInitializations=unit.Nodes.Values.Where(node=>IsAllocationByteZero(unit,node))
                .Select(node=>node.Initialization!.Value).ToHashSet();
            foreach(var initialization in unit.Initializations.Values.Where(initialization=>initialization.Function==function.Id))
            {
                if(initialization.Type is long type && !byteZeroInitializations.Contains(initialization.Id))UseType(unit,type);
                if(initialization.Constructor is long constructor)UseSymbol(unit,constructor);
                if(initialization.Destructor is long destructor && !virtualDestructionMetadata.Contains(initialization.Id))UseSymbol(unit,destructor);
            }
            foreach(var member in unit.ConstructorInitializers.GetValueOrDefault(function.Id,[]))
            {
                if(member.Type is long type)UseType(unit,type);
                if(member.Field is long field)UseType(unit,unit.Fields[field].Owner);
                if(member.Base is long selected)UseBase(unit,unit.BaseSubobjects[selected]);
                if(member.CopySource is long source)UseType(unit,unit.Symbols[source].Type);
            }
            foreach(var conversion in unit.ClassConversions.Values.Where(conversion=>unit.Nodes[conversion.Node].Function==function.Id))
                UseBase(unit,unit.BaseSubobjects[conversion.Base]);
            foreach(var write in unit.VptrWrites.GetValueOrDefault(function.Id,[]))
                if(write.Table is long table)UseVirtualTable(unit,table);
            foreach(var site in unit.Throws.Values.Where(site=>unit.Nodes[site.Node].Function==function.Id))
            {
                if(site.Type is long type)UseType(unit,type);
                if(site.Destructor is long destructor)UseSymbol(unit,destructor);
            }
            foreach(var handler in unit.Handlers.Values.SelectMany(handlers=>handlers).Where(handler=>unit.Nodes[handler.CatchNode].Function==function.Id))
                if(handler.Type is long type)UseType(unit,type);
            NeedsExceptionRuntime|=unit.FunctionDetails[function.Id].ExceptionPolicy=="terminate";
            CollectExceptionRequirements(unit,function.Id);
        }
        foreach(var unit in program.Units)CollectExceptionRequirements(unit,null);
    }

    private void UseNode(Unit unit,Node node,HashSet<long> visited)
    {
        if(!visited.Add(node.Id))return;
        // EDG has already accepted this declaration. Its recorded condition
        // and message are source metadata, without executable type/storage use.
        if(node.Kind=="static-assertion")return;
        if(node.Kind=="type-info-address")
        {
            var provider=ResolveTypeInfoObject(unit,node);
            UseType(provider.Unit,provider.Symbol.Type);
        }
        needsNullptrNormalization|=node.ValueNormalization=="nullptr-zero";
        if(node.Function is null)executableGlobalNodes.Add((unit,node.Id));
        bool byteZero=IsAllocationByteZero(unit,node);
        // This selected byte-zero action retains its source runtime array
        // type for navigation. Execution uses the recorded byte count and
        // allocation result, without requiring a fixed array value layout.
        if(node.Type is long type && !byteZero)UseType(unit,type);
        if(node.ExecutionType is long execution)UseType(unit,execution);
        if(node.OperationType is long operation)UseType(unit,operation);
        if(node.OriginalLvalueType is long original)UseType(unit,original);
        if(node.Symbol is long symbol && node.Kind!="type-info-address")
        {
            if(node.Kind=="function" && virtualCalleeUnit==unit && virtualCalleeMetadata.Contains(node.Id))
                UseType(unit,unit.Symbols[symbol].Type);
            else if(node.Kind is "variable" or "function" or "routine-address")UseSymbol(unit,symbol);
            else UseType(unit,unit.Symbols[symbol].Type);
        }
        if(node.Field is long field)
        {
            UseType(unit,unit.Fields[field].Owner);UseType(unit,unit.Fields[field].Type);
        }
        if(node.EnumValue is long enumeration)UseType(unit,unit.EnumValues[enumeration].Type);
        if(node.FloatingValue is long floating)UseType(unit,unit.FloatingValues[floating].Type);
        if(node.LiteralObject is long literal)UseLiteral(unit,literal);
        if(unit.Allocations.TryGetValue(node.Id,out var allocation))
        {
            NeedsExceptionRuntime|=node.Operator=="placement";
            // The allocated array type is source metadata. Runtime execution
            // uses the selected pointer result and supplied size/check graph.
            UseSymbol(unit,allocation.Allocator);
            if(allocation.CountSymbol is long count)UseType(unit,unit.Symbols[count].Type);
            if(allocation.CountValue is long countValue)UseNode(unit,unit.Nodes[countValue],visited);
            UseNode(unit,unit.Nodes[allocation.Size],visited);
            if(allocation.Check is long check)UseNode(unit,unit.Nodes[check],visited);
            if(allocation.Failure is long failure)UseSymbol(unit,failure);
        }
        foreach(var element in unit.ArrayElements.GetValueOrDefault(node.Id,[]))UseNode(unit,unit.Nodes[element.Value],visited);
        foreach(var element in unit.RecordElements.GetValueOrDefault(node.Id,[]))
        {
            if(element.Field is long member)
            {
                UseType(unit,unit.Fields[member].Owner);UseType(unit,unit.Fields[member].Type);
            }
            if(element.Base is long selectedBase)UseBase(unit,unit.BaseSubobjects[selectedBase]);
            UseNode(unit,unit.Nodes[element.Value],visited);
        }
        if(node.Initialization is long selected && unit.Initializations.TryGetValue(selected,out var initialization))
        {
            if(initialization.Type is long initializedType && !byteZero)UseType(unit,initializedType);
            if(initialization.Constructor is long constructor)UseSymbol(unit,constructor);
            if(initialization.Destructor is long destructor && !virtualDestructionMetadata.Contains(initialization.Id))UseSymbol(unit,destructor);
            if(initialization.Value is long value)UseNode(unit,unit.Nodes[value],visited);
            foreach(long argument in unit.InitializationArguments.GetValueOrDefault(selected,[]))UseNode(unit,unit.Nodes[argument],visited);
        }
        if(unit.ClassConversions.TryGetValue(node.Id,out var conversion))UseBase(unit,unit.BaseSubobjects[conversion.Base]);
        if(unit.VirtualCalls.TryGetValue(node.Id,out var call))
        {
            UseType(unit,call.ReceiverType);UseType(unit,call.CallableType);
            if(call.AbiCallableType is long abi)UseType(unit,abi);
        }
        // These edges retain source metadata. Selected execution follows its
        // program references without realizing or evaluating this metadata.
        foreach(var edge in unit.Edges.GetValueOrDefault(node.Id,[]).Where(edge=>edge.Role is not ("selected-type" or "source-condition" or "source-name" or "selected-abi-entry")))
            UseNode(unit,unit.Nodes[edge.Child],visited);
    }

    private static bool IsAllocationByteZero(Unit unit,Node node)=>node.Kind=="allocation-initialize" &&
        node.Initialization is long id && unit.Initializations.TryGetValue(id,out var initialization) && initialization.Kind=="zero";

    private void UseBase(Unit unit,BaseSubobject subobject)
    {
        UseType(unit,subobject.Owner);UseType(unit,subobject.Type);
    }

    private void UseType(Unit unit,long id)
    {
        if(!executableTypes.Add((unit,id)))return;
        var type=unit.Types[id];
        if(type.Dependent)return;
        if(unit.VaListRole(id) is not null)return;
        if(type.Underlying is long underlying)UseType(unit,underlying);
        if(type.Pointee is long pointee)UseType(unit,pointee);
        if(type.Element is long element)UseType(unit,element);
        if(type.Result is long result)UseType(unit,result);
        foreach(long parameter in unit.TypeParameters.GetValueOrDefault(id,[]))UseType(unit,parameter);
        if(type.Kind=="record")
        {
            foreach(var field in unit.Fields.Values.Where(field=>field.Owner==id))UseType(unit,field.Type);
            foreach(var subobject in unit.BaseSubobjects.Values.Where(subobject=>subobject.Owner==id))UseType(unit,subobject.Type);
        }
    }
}
