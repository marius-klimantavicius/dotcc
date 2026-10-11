namespace DotObj.Relational;

internal sealed record VirtualCall(long Node,long Selected,long Receiver,long ReceiverType,long CallableType,long VptrOffset,long Slot);
internal sealed record VirtualTable(long Id,long Owner,long? Base,long? ConstructionBase,string LinkName,string Linkage,long AddressPoint,long EntrySize,bool Definition);
internal sealed record VirtualEntry(long Table,long Slot,long Selected,long CallableType,long ReceiverType,long? Target,string Kind,string Variant,string AbiName,long ThisDelta,long VcallIndex,long ReturnDelta,long VbaseIndex);
internal sealed record VptrWrite(long Ordinal,string Phase,long? Base,long? Offset,long? Table,string Source,long? ConstructionIndex);

internal sealed partial class Unit
{
    public bool HasVirtualDispatch {get;private set;}
    public Dictionary<long,VirtualTable> VirtualTables {get;}=[];
    public Dictionary<long,List<VirtualEntry>> VirtualEntries {get;}=[];
    public Dictionary<long,VirtualCall> VirtualCalls=>Body.VirtualCalls;
    public Dictionary<long,List<VptrWrite>> VptrWrites=>Body.VptrWrites;

    private void ReadVirtualTables()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='virtual_calls'"))HasVirtualDispatch=q.Read();
        if(!HasVirtualDispatch)return;
        using(var q=database.Prepare("SELECT id,owner_type_id,base_subobject_id,construction_base_subobject_id,link_name,linkage,address_point_byte_offset,entry_size,is_definition FROM virtual_tables ORDER BY id"))
            while(q.Read())VirtualTables.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.OptionalInteger(2),q.OptionalInteger(3),q.Text(4),q.Text(5),q.Integer(6),q.Integer(7),q.Integer(8)!=0));
        using(var q=database.Prepare("SELECT table_id,slot_index,selected_symbol_id,callable_type_id,receiver_type_id,target_symbol_id,entry_kind,target_variant,abi_entry_name,this_delta,vcall_index,return_delta,vbase_index FROM virtual_table_entries ORDER BY table_id,slot_index"))
            while(q.Read())Add(VirtualEntries,q.Integer(0),new VirtualEntry(q.Integer(0),q.Integer(1),q.Integer(2),q.Integer(3),q.Integer(4),q.OptionalInteger(5),q.Text(6),q.Text(7),q.Text(8),q.Integer(9),q.Integer(10),q.Integer(11),q.Integer(12)));
    }

    private void ReadVirtualBody(long? function)
    {
        if(!HasVirtualDispatch)return;
        using(var q=database.Prepare("SELECT v.node_id,v.selected_symbol_id,v.receiver_node_id,v.receiver_type_id,v.callable_type_id,v.vptr_byte_offset,v.slot_index FROM nodes n JOIN virtual_calls v ON v.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY v.node_id"))
            while(q.Read())VirtualCalls.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Integer(2),q.Integer(3),q.Integer(4),q.Integer(5),q.Integer(6)));
        using(var q=database.Prepare("SELECT function_id,ordinal,phase,base_subobject_id,vptr_byte_offset,table_id,source_kind,construction_index FROM vptr_writes WHERE "+FunctionFilter(function)+" ORDER BY function_id,ordinal"))
            while(q.Read())Add(VptrWrites,q.Integer(0),new VptrWrite(q.Integer(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.Text(6),q.OptionalInteger(7)));
    }
}
