namespace DotObj.Relational;

internal sealed record BaseSubobject(long Id,long Owner,long Type,long? DeclaredType,long Ordinal,long? DirectOrdinal,
    long Offset,bool Direct,bool Virtual,bool OptimizedEmpty,string? Access,long? File,long? Line,long? Column);
internal sealed record ClassConversion(long Node,long Base,string Direction,bool PreservesNull,long? Adjustment,
    string SourceRepresentation,string ResultRepresentation);

internal sealed partial class Unit
{
    public bool HasBaseSubobjects {get;private set;}
    public Dictionary<long,BaseSubobject> BaseSubobjects {get;}=[];
    public Dictionary<long,ClassConversion> ClassConversions=>Body.ClassConversions;

    private void ReadInheritance()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='base_subobjects'"))
            HasBaseSubobjects=q.Read();
        if(!HasBaseSubobjects)return;
        using(var q=database.Prepare("SELECT id,owner_type_id,base_type_id,declared_type_id,ordinal,direct_ordinal,byte_offset,is_direct,is_virtual,is_optimized_empty,access,file_id,line,column FROM base_subobjects ORDER BY owner_type_id,ordinal"))
            while(q.Read())BaseSubobjects.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Integer(2),q.OptionalInteger(3),q.Integer(4),q.OptionalInteger(5),q.Integer(6),q.Integer(7)!=0,q.Integer(8)!=0,q.Integer(9)!=0,q.OptionalText(10),q.OptionalInteger(11),q.OptionalInteger(12),q.OptionalInteger(13)));
    }

    private void ReadClassConversions(long? function)
    {
        if(!HasBaseSubobjects)return;
        using(var q=database.Prepare("SELECT c.node_id,c.base_subobject_id,c.direction,c.preserves_null,c.byte_adjustment,c.source_representation,c.result_representation FROM nodes n JOIN class_conversions c ON c.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY c.node_id"))
            while(q.Read())ClassConversions.Add(q.Integer(0),new(q.Integer(0),q.Integer(1),q.Text(2),q.Integer(3)!=0,q.OptionalInteger(4),q.Text(5),q.Text(6)));
    }
}
