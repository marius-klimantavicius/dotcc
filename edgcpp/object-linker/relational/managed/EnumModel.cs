namespace DotObj.Relational;

internal sealed record EnumDetails(long Type,bool Scoped,bool ExplicitBase,long? DeclaredBase,bool Definition,
    long? File,long? Line,long? Column);
internal sealed record EnumValue(long Id,long Type,long Ordinal,string Name,string Value,long ValueType,
    long? File,long? Line,long? Column);

internal sealed partial class Unit
{
    public bool HasEnums {get;private set;}
    public Dictionary<long,EnumDetails> Enums {get;}=[];
    public Dictionary<long,EnumValue> EnumValues {get;}=[];

    private void ReadEnums()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='enums'"))
            HasEnums=q.Read();
        if(!HasEnums)return;
        using(var q=database.Prepare("SELECT type_id,is_scoped,has_explicit_base,declared_base_type_id,is_definition,file_id,line,column FROM enums ORDER BY type_id"))
            while(q.Read())Enums.Add(q.Integer(0),new(q.Integer(0),q.Integer(1)!=0,q.Integer(2)!=0,q.OptionalInteger(3),q.Integer(4)!=0,q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7)));
        using(var q=database.Prepare("SELECT id,enum_type_id,ordinal,name,value_text,value_type_id,file_id,line,column FROM enum_values ORDER BY enum_type_id,ordinal"))
            while(q.Read())EnumValues.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Integer(2),q.Text(3),q.Text(4),q.Integer(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8)));
    }
}
