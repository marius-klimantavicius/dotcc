namespace DotObj.Relational;

internal sealed partial class Unit
{
    public string? VaListRole(long id)
    {
        var type=Types[id];
        while(true)
        {
            if(type.SemanticRole is "va-list" or "va-list-storage")return type.SemanticRole;
            if(type.Kind is not("qualified" or "typedef"))return null;
            type=Types[type.Underlying??throw new InvalidDataException("Type wrapper has no target")];
        }
    }
}
