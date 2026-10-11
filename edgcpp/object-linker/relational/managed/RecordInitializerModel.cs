namespace DotObj.Relational;

internal sealed record RecordInitializer(long Node,long ZeroBytes);
internal sealed record RecordElement(long? Field,long? Base,long Value);

internal sealed partial class Unit
{
    private bool hasRecordInitializers;
    public Dictionary<long,RecordInitializer> RecordInitializers=>Body.RecordInitializers;
    public Dictionary<long,List<RecordElement>> RecordElements=>Body.RecordElements;

    private void ReadRecordInitializerFeatures()
    {
        using var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='record_initializers'");
        hasRecordInitializers=q.Read();
    }

    private void ReadRecordInitializers(long? function)
    {
        if(!hasRecordInitializers)return;
        using(var q=database.Prepare("SELECT r.node_id,r.zero_byte_count FROM nodes n JOIN record_initializers r ON r.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY r.node_id"))
            while(q.Read())RecordInitializers.Add(q.Integer(0),new(q.Integer(0),q.Integer(1)));
        using(var q=database.Prepare("SELECT r.initializer_node_id,r.field_id,r.base_subobject_id,r.value_node_id FROM nodes n JOIN record_initializer_elements r ON r.initializer_node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY r.initializer_node_id,r.ordinal"))
            while(q.Read())Add(RecordElements,q.Integer(0),new RecordElement(q.OptionalInteger(1),q.OptionalInteger(2),q.Integer(3)));
    }
}
