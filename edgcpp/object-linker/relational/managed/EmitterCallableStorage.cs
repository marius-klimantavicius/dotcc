namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<string,(Unit Unit,ProgramType Type)> recordedRecordDefinitions=new(StringComparer.Ordinal);

    private void PrepareRecordedRecordDefinitions()
    {
        foreach(var unit in program.Units)
            foreach(var type in unit.Types.Values.Where(type=>type.Kind=="record" && !type.Incomplete && !type.Dependent &&
                type.Size is >0 && type.Alignment is >0 && type.Linkage=="external" && type.LinkName.Length!=0))
                recordedRecordDefinitions.TryAdd(type.LinkName,(unit,type));
    }

    private bool RecordedRecordDefinition(ProgramType type,out (Unit Unit,ProgramType Type) definition)=>
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
