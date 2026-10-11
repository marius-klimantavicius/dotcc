namespace DotObj.Relational;

internal sealed record Allocation(long Node,long Type,long Allocator,long? CountValue,long? CountSymbol,long Size,long? Check,long? Failure,long ResultOffset);

internal sealed partial class Unit
{
    private bool hasAllocations;
    public Dictionary<long,Allocation> Allocations=>Body.Allocations;

    private void ReadAllocationFeatures()
    {
        using var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='allocations'");
        hasAllocations=q.Read();
    }

    private void ReadAllocations(long? function)
    {
        if(!hasAllocations)return;
        using var q=database.Prepare("SELECT a.node_id,a.allocated_type_id,a.allocator_symbol_id,a.count_value_node_id,a.count_symbol_id,a.size_node_id,a.length_check_node_id,a.length_failure_symbol_id,a.result_byte_offset FROM nodes n JOIN allocations a ON a.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY a.node_id");
        while(q.Read())Allocations.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Integer(2),q.OptionalInteger(3),q.OptionalInteger(4),q.Integer(5),q.OptionalInteger(6),q.OptionalInteger(7),q.Integer(8)));
    }
}
