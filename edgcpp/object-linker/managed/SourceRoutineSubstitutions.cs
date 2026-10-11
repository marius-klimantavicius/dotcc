namespace DotObj;
internal sealed record SourceRoutineTypeSubstitution(long Declaration,long ArgumentCount,string ArgumentOwnerKind);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceRoutineTypeSubstitution> SourceRoutineTypeSubstitutions {get;}=[];
    public Dictionary<long,SortedDictionary<long,long>> SourceRoutineBorrowedArguments {get;}=[];
    private void ReadSourceRoutineSubstitutions()
    {
        using(var q=database.Prepare("SELECT routine_source_type_id,selected_source_declaration_id,borrowed_argument_count,capture_proof,argument_owner_kind FROM source_routine_type_substitutions ORDER BY routine_source_type_id"))
            while(q.Read())
            {
                long owner=q.Integer(0);var value=new SourceRoutineTypeSubstitution(q.Integer(1),q.Integer(2),q.Text(4));
                if(!SourceRoutines.ContainsKey(owner) || value.ArgumentCount<=0 ||
                    !SourceExpressionDeclarations.TryGetValue(value.Declaration,out var declaration) || declaration.Kind!="routine" ||
                    value.ArgumentOwnerKind is not("function-template" or "class-template" or "class-partial") ||
                    q.Text(3)!="original-routine-type-argument-substitution" || !SourceRoutineTypeSubstitutions.TryAdd(owner,value))
                    throw new InvalidDataException("Routine substitution has an invalid recorded declaration, kind or count");

            }
        using(var q=database.Prepare("SELECT routine_source_type_id,argument_ordinal,source_type_id FROM source_routine_borrowed_arguments ORDER BY routine_source_type_id,argument_ordinal"))
            while(q.Read())
            {
                long owner=q.Integer(0),ordinal=q.Integer(1),type=q.Integer(2);
                if(!SourceRoutineTypeSubstitutions.TryGetValue(owner,out var substitution) || ordinal<0 || !SourceTypes.ContainsKey(type))
                    throw new InvalidDataException("Borrowed source argument lacks its exact original routine substitution");
                if(!SourceRoutineBorrowedArguments.TryGetValue(owner,out var selected))SourceRoutineBorrowedArguments[owner]=selected=[];
                if(!selected.TryAdd(ordinal,type))throw new InvalidDataException("Duplicate borrowed original type argument");
                // Preserve EDG's recorded relation; shared original recipes
                // do not need a unique consuming lexical binder.

            }
        using(var q=database.Prepare("SELECT type_id,has_borrowed_arguments FROM source_routine_types ORDER BY type_id"))
            while(q.Read())
                if(q.Integer(1) is not(0 or 1) || (q.Integer(1)==1)!=SourceRoutineTypeSubstitutions.ContainsKey(q.Integer(0)))
                    throw new InvalidDataException("Original routine substitution is missing its required borrowed-argument provenance");
        foreach(var pair in SourceRoutineTypeSubstitutions)
            if(SourceRoutineBorrowedArguments.GetValueOrDefault(pair.Key,[]).Count!=pair.Value.ArgumentCount)
                throw new InvalidDataException("Original routine borrowed-argument list is incomplete");
    }
}
