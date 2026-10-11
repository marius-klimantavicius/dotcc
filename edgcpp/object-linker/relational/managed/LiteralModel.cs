namespace DotObj.Relational;

internal sealed record LiteralObject(long Id,long Type,long? Symbol,long ByteCount);
internal sealed record LiteralInitializer(long CopyBytes,long ZeroBytes);

internal sealed partial class Unit
{
    public bool HasLiterals {get;private set;}
    public Dictionary<long,LiteralObject> Literals {get;}=[];
    public Dictionary<long,LiteralInitializer> LiteralInitializers=>Body.LiteralInitializers;

    private void ReadLiterals()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='literal_objects'"))
            HasLiterals=q.Read();
        if(!HasLiterals)return;
        using(var q=database.Prepare("SELECT id,type_id,selected_symbol_id,byte_count FROM literal_objects ORDER BY id"))
            while(q.Read())Literals.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.OptionalInteger(2),q.Integer(3)));
    }

    public IEnumerable<(long Offset,byte Value)> LiteralBytes(long id)
    {
        using var q=database.Prepare("SELECT byte_offset,value FROM literal_bytes WHERE literal_id="+id.ToString(System.Globalization.CultureInfo.InvariantCulture)+" ORDER BY byte_offset");
        while(q.Read())yield return(q.Integer(0),checked((byte)q.Integer(1)));
    }

    private void ReadLiteralInitializers(long? function)
    {
        if(!HasLiterals)return;
        using var q=database.Prepare("SELECT l.node_id,l.copy_byte_count,l.zero_byte_count FROM nodes n JOIN literal_initializers l ON l.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY l.node_id");
        while(q.Read())LiteralInitializers.Add(q.Integer(0),new(q.Integer(1),q.Integer(2)));
    }
}
