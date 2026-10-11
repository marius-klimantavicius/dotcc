namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<string,(Unit Unit,VirtualTable Table)> virtualDefinitions=new(StringComparer.Ordinal);
    private readonly Dictionary<(Unit Unit,long Id),string> virtualTableNames=[];
    private readonly Dictionary<(Unit Unit,long Id),string> virtualCallNames=[];
    private bool needsVirtualRuntime;

    private static string VirtualKey(Unit unit,VirtualTable table)=>
        (table.Linkage=="external"?"external:"+table.LinkName:unit.Path+":"+table.Id)+":"+table.AddressPoint;

    private void PrepareVirtualCatalog()
    {
        foreach(var unit in program.Units)
            foreach(var table in unit.VirtualTables.Values.Where(table=>table.Definition))
                virtualDefinitions.TryAdd(VirtualKey(unit,table),(unit,table));
    }

    private (Unit Unit,VirtualTable Table) ResolveVirtualTable(Unit unit,long id)
    {
        var table=unit.VirtualTables[id];
        return virtualDefinitions.TryGetValue(VirtualKey(unit,table),out var selected)?selected:
            throw new InvalidDataException("No definition for virtual table "+table.LinkName+" at address point "+table.AddressPoint);
    }

    private void UseVirtualTable(Unit unit,long id)
    {
        var selected=ResolveVirtualTable(unit,id);unit=selected.Unit;var table=selected.Table;
        if(virtualTableNames.ContainsKey((unit,table.Id)))return;
        virtualTableNames.Add((unit,table.Id),names.Allocate("__vtable_"+unit.Types[table.Owner].Name,"virtual_table"));
        UseType(unit,table.Owner);
        foreach(var entry in unit.VirtualEntries.GetValueOrDefault(table.Id,[]))
        {
            UseType(unit,entry.CallableType);UseType(unit,entry.ReceiverType);
            if(entry.Target is long target)
            {
                if(entry.Kind!="ordinary")
                {
                    UseType(unit,unit.Symbols[target].Type);continue;
                }
                var function=program.Resolve(unit,target);
                UseType(function.Unit,function.Symbol.Type);
                if(function.Unit.FunctionDetails.TryGetValue(function.Id,out var details) && details.Receiver is long receiver)
                    UseType(function.Unit,function.Unit.Symbols[receiver].Type);
            }
        }
    }

    private string VirtualPointer(Unit unit,long callable,long receiver)
    {
        var signature=unit.Original(callable);
        if(signature.Variadic)throw new InvalidDataException("Variadic virtual dispatch is not yet implemented");
        var parameters=unit.TypeParameters.GetValueOrDefault(signature.Id,[]).Select(id=>Type(unit,id)).Prepend(Type(unit,receiver)+"*");
        if(signature.ReturnStorage=="caller-provided")parameters=parameters.Prepend(Type(unit,signature.Result!.Value)+"*");
        return "delegate* managed<"+string.Join(", ",parameters.Append(signature.ReturnStorage=="caller-provided"?"void":Type(unit,signature.Result!.Value)))+">";
    }

    private (string Result,string Parameters) VirtualSignature(Unit unit,long callable)
    {
        var signature=unit.Original(callable);
        if(signature.Variadic)throw new InvalidDataException("Variadic virtual dispatch is not yet implemented");
        var parameters=unit.TypeParameters.GetValueOrDefault(signature.Id,[])
            .Select((id,index)=>Type(unit,id)+" argument"+index).ToList();
        if(signature.ReturnStorage=="caller-provided")
        {
            parameters.Insert(0,Type(unit,signature.Result!.Value)+"* result");
        }
        return(signature.ReturnStorage=="caller-provided"?"void":Type(unit,signature.Result!.Value),string.Join(", ",parameters));
    }

    private void WriteVirtualTables()
    {
        foreach(var pair in virtualTableNames)
        {
            var unit=pair.Key.Unit;var table=unit.VirtualTables[pair.Key.Id];string name=pair.Value;
            if(table.ConstructionBase is not null)throw new InvalidDataException("Construction virtual table emission is not yet implemented: "+table.LinkName);
            if(table.EntrySize!=8)throw new InvalidDataException("Virtual table target entry size is not yet supported: "+table.EntrySize);
            var entries=unit.VirtualEntries.GetValueOrDefault(table.Id,[]);
            if(entries.Count==0 || entries.Any(entry=>entry.Slot<0))throw new InvalidDataException("Virtual table function slots missing: "+table.LinkName);
            string create=names.Allocate(name+"_create","create_virtual_table");
            var callbacks=new List<(VirtualEntry Entry,string? Name)>();
            foreach(var entry in entries)
            {
                if(entry.Variant!="ordinary" || entry.VcallIndex!=0 || entry.ReturnDelta!=0 || entry.VbaseIndex!=0)
                    throw new InvalidDataException("Virtual destructor, virtual-base or covariant thunk emission is not yet implemented: "+entry.AbiName);
                string? callback=entry.Kind=="null"?null:names.Allocate("__virtual_entry_"+unit.Symbols[entry.Selected].Name,"virtual_entry");
                callbacks.Add((entry,callback));
                if(callback is not null)WriteVirtualEntry(unit,entry,callback);
            }
            Line(1,"private static readonly nint[] "+name+" = "+create+"();");
            Line(1,"private static nint[] "+create+"()");Line(1,"{");
            Line(2,"var entries = global::System.GC.AllocateArray<nint>("+(entries.Max(entry=>entry.Slot)+1)+", pinned: true);");
            foreach(var callback in callbacks.Where(callback=>callback.Name is not null))
                Line(2,"entries["+callback.Entry.Slot+"] = (nint)("+VirtualPointer(unit,callback.Entry.CallableType,callback.Entry.ReceiverType)+")&"+callback.Name+";");
            Line(2,"return entries;");Line(1,"}");
        }
    }

    private void WriteVirtualEntry(Unit unit,VirtualEntry entry,string name)
    {
        var signature=VirtualSignature(unit,entry.CallableType);
        bool indirect=unit.Original(entry.CallableType).ReturnStorage=="caller-provided";
        var parameters=unit.TypeParameters.GetValueOrDefault(unit.Original(entry.CallableType).Id,[])
            .Select((id,index)=>Type(unit,id)+" argument"+index).ToList();
        parameters.Insert(0,Type(unit,entry.ReceiverType)+"* receiver");
        if(indirect)parameters.Insert(0,Type(unit,unit.Original(entry.CallableType).Result!.Value)+"* result");
        Line(1,"private static "+signature.Result+" "+name+"("+string.Join(", ",parameters)+")");Line(1,"{");
        if(entry.Kind is "pure" or "deleted")
        {
            needsVirtualRuntime=true;
            Line(2,"global::Edg.Runtime.CppVirtual."+(entry.Kind=="pure"?"PureVirtual":"DeletedVirtual")+"();");
            if(signature.Result!="void")Line(2,"return default;");
        }
        else
        {
            long targetId=entry.Target??throw new InvalidDataException("Exporter defect: virtual entry target missing");
            var target=program.Resolve(unit,targetId);
            string targetName=SymbolName(unit,targetId);
            long targetReceiver=target.Unit.FunctionDetails[target.Id].Receiver??throw new InvalidDataException("Virtual target receiver missing: "+target.Symbol.Name);
            string receiverType=Type(target.Unit,target.Unit.Symbols[targetReceiver].Type);
            string receiver="("+receiverType+")"+(entry.ThisDelta==0?"receiver":"((byte*)receiver + "+entry.ThisDelta+")");
            var arguments=Enumerable.Range(0,unit.TypeParameters.GetValueOrDefault(unit.Original(entry.CallableType).Id,[]).Count).Select(index=>"argument"+index).ToList();
            arguments.Insert(0,receiver);if(indirect)arguments.Insert(0,"result");
            string call=targetName+"("+string.Join(", ",arguments)+")";
            Line(2,signature.Result=="void"?call+";":"return ("+signature.Result+")"+call+";");
        }
        Line(1,"}");
    }

    private void WriteVirtualCallHelpers(Unit unit,long? function)
    {
        foreach(var call in unit.VirtualCalls.Values.Where(call=>unit.Nodes[call.Node].Function==function))
        {
            string name=names.Allocate("__virtual_"+unit.Types[call.ReceiverType].Name+"_"+unit.Symbols[call.Selected].Name,"virtual_call");
            virtualCallNames.Add((unit,call.Node),name);
            var signature=VirtualSignature(unit,call.CallableType);
            string pointer=VirtualPointer(unit,call.CallableType,call.ReceiverType);
            string receiverType=Type(unit,call.ReceiverType)+"*";
            Line(1,"private readonly struct "+name);Line(1,"{");
            Line(2,"private readonly "+receiverType+" receiver;");Line(2,"private readonly "+pointer+" target;");
            Line(2,"public "+name+"("+receiverType+" receiver)");Line(2,"{");
            Line(3,"this.receiver = receiver;");
            Line(3,"target = ("+pointer+")(*(nint**)((byte*)receiver + "+call.VptrOffset+"))["+call.Slot+"];");Line(2,"}");
            var arguments=Enumerable.Range(0,unit.TypeParameters.GetValueOrDefault(unit.Original(call.CallableType).Id,[]).Count).Select(index=>"argument"+index).ToList();
            arguments.Insert(0,"receiver");
            if(unit.Original(call.CallableType).ReturnStorage=="caller-provided")arguments.Insert(0,"result");
            Line(2,"public "+signature.Result+" Invoke("+signature.Parameters+") => target("+string.Join(", ",arguments)+");");
            Line(1,"}");
        }
    }

    private void ReleaseVirtualCallNames(Unit unit,long function)
    {
        foreach(var call in unit.VirtualCalls.Values.Where(call=>unit.Nodes[call.Node].Function==function))virtualCallNames.Remove((unit,call.Node));
    }

    private void WriteVirtualRuntime(string directory)
    {
        if(!needsVirtualRuntime)return;
        using var resource=typeof(Emitter).Assembly.GetManifestResourceStream("DotObj.Relational.CppVirtual.cs")
            ??throw new InvalidDataException("Embedded virtual runtime missing");
        using var reader=new StreamReader(resource);File.WriteAllText(Path.Combine(directory,"CppVirtual.cs"),reader.ReadToEnd());
    }

    private sealed partial class FunctionWriter
    {
        private string VirtualCallExpression(Node node,VirtualCall call,string? destination=null)
        {
            var operands=unit.Children(node,"operand").ToArray();
            if(operands.Length<2 || operands[1].Id!=call.Receiver)
                throw unit.Missing(node,"exporter defect: virtual receiver operand relation missing");
            bool indirect=unit.Original(call.CallableType).ReturnStorage=="caller-provided";
            if(indirect && destination is null)throw unit.Missing(node,"virtual class result requires explicit initialization destination");
            Node receiver=unit.Nodes[call.Receiver];
            string value=Expression(receiver);
            if(unit.Original(receiver.ExecutionType??receiver.Type!.Value).Kind is not ("pointer" or "lvalue-reference" or "rvalue-reference"))value="(&"+value+")";
            value="("+Type(call.ReceiverType)+"*)"+value;
            var arguments=operands.Skip(2).Select(Expression).ToList();
            if(indirect)arguments.Insert(0,"(&"+destination+")");
            return "new "+emitter.virtualCallNames[(unit,node.Id)]+"("+value+").Invoke("+string.Join(", ",arguments)+")";
        }

        private void WriteVptrWrites(string phase,int indent)
        {
            foreach(var write in unit.VptrWrites.GetValueOrDefault(function!.Value.Id,[]).Where(write=>write.Phase==phase))
            {
                if(write.Source!="table" || write.ConstructionIndex is not null || write.Offset is not long offset || write.Table is not long table)
                    throw new InvalidDataException("Construction-table or dynamic virtual-base vptr initialization is not yet implemented");
                long receiver=unit.FunctionDetails[function!.Value.Id].Receiver??throw new InvalidDataException("Vptr initialization receiver missing");
                var selected=emitter.ResolveVirtualTable(unit,table);
                string name=emitter.virtualTableNames[(selected.Unit,selected.Table.Id)];
                Line(indent,"*(nint**)((byte*)"+Name(receiver)+" + "+offset+") = (nint*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref "+name+"[0]);");
            }
        }
    }
}
