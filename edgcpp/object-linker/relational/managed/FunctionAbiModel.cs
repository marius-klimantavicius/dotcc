namespace DotObj.Relational;

internal sealed record FunctionAbiEntry(long Source,long Entry,string Variant,long CallableType,long? AliasTarget);
internal sealed record VttEntry(long Ordinal,long Table,long AddressPoint);

internal sealed partial class Unit
{
    public Dictionary<long,List<FunctionAbiEntry>> FunctionAbiEntries {get;}=[];
    public Dictionary<long,List<VttEntry>> VttEntries {get;}=[];

    private void ReadFunctionAbiEntries()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='function_abi_entries'"))
            if(q.Read())
            {
                bool hasAliasTarget;
                using(var columns=database.Prepare("SELECT 1 FROM pragma_table_info('function_abi_entries') WHERE name='alias_target_symbol_id'"))hasAliasTarget=columns.Read();
                using(var rows=database.Prepare("SELECT source_symbol_id,entry_symbol_id,variant,callable_type_id,"+(hasAliasTarget?"alias_target_symbol_id":"NULL")+" FROM function_abi_entries ORDER BY source_symbol_id,variant,entry_symbol_id"))
                    while(rows.Read())Add(FunctionAbiEntries,rows.Integer(0),new FunctionAbiEntry(rows.Integer(0),rows.Integer(1),rows.Text(2),rows.Integer(3),rows.OptionalInteger(4)));
            }
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='vtt_entries'"))
            if(q.Read())
                using(var rows=database.Prepare("SELECT symbol_id,ordinal,virtual_table_id,address_point_byte_offset FROM vtt_entries ORDER BY symbol_id,ordinal"))
                    while(rows.Read())Add(VttEntries,rows.Integer(0),new VttEntry(rows.Integer(1),rows.Integer(2),rows.Integer(3)));
    }
}
