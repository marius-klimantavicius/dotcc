namespace DotObj.Relational;

internal sealed record ArrayInitializer(long Node,bool ZeroInitialize);
internal sealed record ArrayElement(long Index,long Repeat,long Value);

internal sealed partial class Unit
{
    public bool HasArrayInitializers {get;private set;}
    public Dictionary<long,ArrayInitializer> ArrayInitializers=>Body.ArrayInitializers;
    public Dictionary<long,List<ArrayElement>> ArrayElements=>Body.ArrayElements;

    private void ReadArrayFeatures()
    {
        using var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='array_initializers'");
        HasArrayInitializers=q.Read();
    }

    private void ReadArrayInitializers(long? function)
    {
        if(!HasArrayInitializers)return;
        using(var q=database.Prepare("SELECT a.node_id,a.zero_initialize FROM nodes n JOIN array_initializers a ON a.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY a.node_id"))
            while(q.Read())ArrayInitializers.Add(q.Integer(0),new(q.Integer(0),q.Integer(1)!=0));
        using(var q=database.Prepare("SELECT a.initializer_node_id,a.array_index,a.repeat_count,a.value_node_id FROM nodes n JOIN array_initializer_elements a ON a.initializer_node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY a.initializer_node_id,a.ordinal"))
            while(q.Read())Add(ArrayElements,q.Integer(0),new ArrayElement(q.Integer(1),q.Integer(2),q.Integer(3)));
    }

    public bool IsArray(long id)=>VaListRole(id) is null && Original(id).Kind=="array";
}
