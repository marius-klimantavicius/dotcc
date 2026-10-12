namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<string,(Unit Unit,long Type)> recordedRecordDefinitions=new(StringComparer.Ordinal);

    private void PrepareRecordedRecordDefinitions()
    {
        foreach(var unit in program.Units)
            foreach(var type in unit.Types.Query("kind='record' AND is_incomplete=0 AND size>0 AND alignment>0 AND linkage='external' AND link_name<>''").Where(type=>!type.Dependent))
                recordedRecordDefinitions.TryAdd(type.LinkName,(unit,type.Id));
    }

    private bool RecordedRecordDefinition(ProgramType type,out (Unit Unit,long Type) definition)=>
        recordedRecordDefinitions.TryGetValue(type.LinkName,out definition) && type.Linkage=="external";

    private bool UnfinalizedCallableResult(Unit unit,ProgramType signature)
    {
        if(signature.ReturnStorage!="value" || signature.Result is not long result)return false;
        var type=unit.Original(result);
        // A linked record definition can supply physical layout. It does not
        // replace the incomplete source callable's ABI with an inferred one.
        return type.Kind=="record" && type.Incomplete;
    }

    private bool OpaqueCallablePointer(Unit unit,long id)
    {
        var pointer=unit.Original(id);
        if(pointer.Kind is not ("pointer" or "lvalue-reference" or "rvalue-reference") || pointer.Pointee is not long target)return false;
        var signature=unit.Original(target);
        return signature.Kind=="function" && UnfinalizedCallableResult(unit,signature);
    }
}
