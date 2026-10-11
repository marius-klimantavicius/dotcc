namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private bool HasPinnedRecordStorage(Unit unit,long symbol)
    {
        if(unit.Symbols[symbol].Parent is not null)return false;
        var global=program.Resolve(unit,symbol);
        return global.Unit.Initializers.TryGetValue(global.Id,out long initializer) && global.Unit.RecordInitializers.ContainsKey(initializer);
    }

    private void WriteGlobalRecord(SymbolRef global,long initializer)
    {
        string name=SymbolName(global.Unit,global.Id),type=Type(global.Unit,global.Symbol.Type);
        var layout=global.Unit.Original(global.Symbol.Type);
        if(layout.Size is not >0 || layout.Alignment is not >0)
            throw new InvalidDataException("Pinned record storage requires recorded positive size and alignment: "+global.Symbol.Name);
        long bytes=checked(layout.Size.Value+layout.Alignment.Value-1);
        if(bytes>int.MaxValue)throw new InvalidDataException("Pinned global record size exceeds supported managed storage");
        string create=names.Allocate("__"+name+"_initialize","initialize_global_record");
        string value=names.Allocate("__"+name+"_value","global_record_value");
        string storage=names.Allocate("__"+name+"_storage","global_record_storage");
        Line(1,"private static readonly byte[] "+storage+" = "+create+"();");
        Line(1,"public static "+type+"* "+name+" => ("+type+"*)"+AlignedArrayAddress(storage,layout.Alignment.Value)+";");
        Line(1,"private static byte[] "+create+"()");Line(1,"{");
        Line(2,"var storage = global::System.GC.AllocateArray<byte>("+bytes+", pinned: true);");
        Line(2,type+"* "+value+" = ("+type+"*)"+AlignedArrayAddress("storage",layout.Alignment.Value)+";");
        new FunctionWriter(this,global.Unit,null).WriteRecordInitializer(global.Unit.Nodes[initializer],"(*"+value+")",2);
        Line(2,"return storage;");Line(1,"}");
    }

    private sealed partial class FunctionWriter
    {
        public void WriteRecordInitializer(Node node,string destination,int indent)
        {
            if(!unit.RecordInitializers.TryGetValue(node.Id,out var initialization))
                throw unit.Missing(node,"exporter defect: record initializer actions missing");
            if(initialization.ZeroBytes<0)throw unit.Missing(node,"exporter defect: record initialization zero extent is negative");
            // This count is EDG's selected destination extent, which can be
            // smaller than the complete base type or zero for an empty base.
            if(initialization.ZeroBytes>0)
                Line(indent,"global::System.Runtime.InteropServices.NativeMemory.Clear(&"+destination+", "+initialization.ZeroBytes+"U);");
            foreach(var element in unit.RecordElements.GetValueOrDefault(node.Id,[]))
            {
                string target;
                if(element.Field is long field && element.Base is null)
                    target="("+destination+")."+emitter.fieldNames[(unit,field)];
                else if(element.Base is long selected && element.Field is null)
                {
                    if(!emitter.baseProjections.TryGetValue((unit,selected),out string? projection))
                        throw unit.Missing(node,"virtual or unavailable aggregate base projection is not implemented");
                    target="(*"+projection+"(&"+destination+"))";
                }
                else throw unit.Missing(node,"exporter defect: aggregate element requires one selected field or base");
                var value=unit.Nodes[element.Value];
                if(unit.RecordInitializers.ContainsKey(value.Id))WriteRecordInitializer(value,target,indent);
                else if(unit.ArrayInitializers.ContainsKey(value.Id))WriteArrayInitializer(value,"(&"+target+")",indent);
                else if(element.Base is not null)
                    throw unit.Missing(value,"aggregate base copy requires an explicit destination extent");
                else Line(indent,target+" = "+ExpandedExpression(value,indent)+";");
            }
        }
    }
}
