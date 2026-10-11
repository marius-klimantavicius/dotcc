namespace DotObj.Relational;

internal sealed partial class Unit
{
    public bool HasExceptionSpecifications {get;private set;}
    public Dictionary<long,List<long>> ExceptionSpecificationTypes {get;}=[];

    private void ReadExceptionSpecifications()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='exception_specification_types'"))
            HasExceptionSpecifications=q.Read();
        if(!HasExceptionSpecifications)return;
        // These are EDG's selected signature facts, including an empty list
        // for throw(). Navigation alone does not realize runtime layouts.
        using(var q=database.Prepare("SELECT function_type_id,allowed_type_id FROM exception_specification_types ORDER BY function_type_id,ordinal"))
            while(q.Read())Add(ExceptionSpecificationTypes,q.Integer(0),q.Integer(1));
    }
}
