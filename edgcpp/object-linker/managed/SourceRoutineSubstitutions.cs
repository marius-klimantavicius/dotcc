namespace DotObj;
internal sealed record SourceRoutineTypeSubstitution(long Declaration,long ArgumentCount);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceRoutineTypeSubstitution> SourceRoutineTypeSubstitutions {get;}=[];
    public Dictionary<long,SortedDictionary<long,long>> SourceRoutineBorrowedArguments {get;}=[];
    private void ReadSourceRoutineSubstitutions()
    {
        using(var q=database.Prepare("SELECT routine_source_type_id,selected_source_declaration_id,borrowed_argument_count,capture_proof FROM source_routine_type_substitutions ORDER BY routine_source_type_id"))
            while(q.Read())
            {
                long owner=q.Integer(0);var value=new SourceRoutineTypeSubstitution(q.Integer(1),q.Integer(2));
                if(!SourceRoutines.ContainsKey(owner) || value.ArgumentCount<=0 ||
                    !SourceExpressionDeclarations.TryGetValue(value.Declaration,out var declaration) || declaration.Kind!="routine" ||
                    declaration.Type!=owner || !declaration.HasTemplate || declaration.IsPrototype ||
                    declaration.TemplateOriginKind!="function-template" || declaration.TemplatePrototype is not long prototype ||
                    !SourceExpressionDeclarations.TryGetValue(prototype,out var original) || original.Kind!="routine" || !original.HasTemplate ||
                    !original.IsPrototype || original.TemplateOriginKind!="function-template" ||
                    q.Text(3)!="original-routine-type-argument-substitution" || !SourceRoutineTypeSubstitutions.TryAdd(owner,value))
                    throw new InvalidDataException("Borrowed routine type lacks its exact selected original template declaration");
            }
        using(var q=database.Prepare("SELECT routine_source_type_id,argument_ordinal,source_type_id FROM source_routine_borrowed_arguments ORDER BY routine_source_type_id,argument_ordinal"))
            while(q.Read())
            {
                long owner=q.Integer(0),ordinal=q.Integer(1),type=q.Integer(2);
                if(!SourceRoutineTypeSubstitutions.TryGetValue(owner,out var substitution) || ordinal<0 || !SourceTypes.ContainsKey(type))
                    throw new InvalidDataException("Borrowed source argument lacks its exact original routine substitution");
                var arguments=SourceExpressionDeclarationArguments.GetValueOrDefault(substitution.Declaration,[]);
                if(ordinal>=arguments.Count || arguments[checked((int)ordinal)].Ordinal!=ordinal ||
                    arguments[checked((int)ordinal)].Kind!="type" || arguments[checked((int)ordinal)].TypeId!=type)
                    throw new InvalidDataException("Borrowed source argument differs from its exact ordered original type argument");
                if(!SourceRoutineBorrowedArguments.TryGetValue(owner,out var selected))SourceRoutineBorrowedArguments[owner]=selected=[];
                if(!selected.TryAdd(ordinal,type))throw new InvalidDataException("Duplicate borrowed original type argument");
                var routine=SourceRoutines[owner];
                var roots=routine.Parameters.Append(routine.ReturnTypeId).Concat(SourceParameterBinders
                    .Where(pair=>pair.Value.RoutineType==owner).SelectMany(pair=>SourceBinderParameters.GetValueOrDefault(pair.Key,[]))
                    .SelectMany(parameter=>parameter.DeclaredType is long declared?new[]{parameter.Type,declared}:new[]{parameter.Type}));
                if(!roots.Any(root=>SourceRoutineTypeContains(root,type,[])))
                    throw new InvalidDataException("Borrowed original argument is not used by its consuming routine signature");
                var references=new HashSet<long>();CollectBorrowedTypeParameterReferences(type,[],references);
                if(references.Count==0)throw new InvalidDataException("Borrowed original argument has no retained lexical parameter recipe");
                foreach(long reference in references)
                {
                    if(!SourceExpressionParameterReferences.TryGetValue(reference,out var parameter))
                        throw new InvalidDataException("Borrowed original argument lost its parameter-reference ownership");
                    long? current=parameter.Binder;var visited=new HashSet<long>();
                    while(current is long binder)
                    {
                        if(!visited.Add(binder) || !SourceParameterBinders.TryGetValue(binder,out var scope))
                            throw new InvalidDataException("Borrowed original argument has an invalid lexical owner graph");
                        if(scope.RoutineType==owner)
                            throw new InvalidDataException("Borrowed original argument was rebound to its consuming routine prototype");
                        current=scope.Parent;
                    }
                }
            }
        using(var q=database.Prepare("SELECT type_id,has_borrowed_arguments FROM source_routine_types ORDER BY type_id"))
            while(q.Read())
                if(q.Integer(1) is not(0 or 1) || (q.Integer(1)==1)!=SourceRoutineTypeSubstitutions.ContainsKey(q.Integer(0)))
                    throw new InvalidDataException("Original routine substitution is missing its required borrowed-argument provenance");
        foreach(var pair in SourceRoutineTypeSubstitutions)
            if(SourceRoutineBorrowedArguments.GetValueOrDefault(pair.Key,[]).Count!=pair.Value.ArgumentCount)
                throw new InvalidDataException("Original routine borrowed-argument list is incomplete");
    }
    private bool SourceRoutineTypeContains(long root,long selected,HashSet<long> visited)
    {
        if(root==selected)return true;
        if(!visited.Add(root))return false;
        var type=SourceTypes[root];
        return type.Kind is "typedef" or "qualified" or "pointer" or "lvalue-reference" or "rvalue-reference" or "array" &&
            type.UnderlyingTypeId is long underlying && SourceRoutineTypeContains(underlying,selected,visited);
    }
    private void CollectBorrowedTypeParameterReferences(long id,HashSet<long> visited,HashSet<long> references)
    {
        if(!visited.Add(id))return;
        if(SourceTypeOperators.TryGetValue(id,out var operation) && operation.Expression is long expression)
            CollectBorrowedExpressionParameterReferences(expression,[],references);
        var type=SourceTypes[id];
        if(type.Kind is "typedef" or "qualified" or "pointer" or "lvalue-reference" or "rvalue-reference" or "array" &&
            type.UnderlyingTypeId is long underlying)CollectBorrowedTypeParameterReferences(underlying,visited,references);
    }
    private void CollectBorrowedExpressionParameterReferences(long id,HashSet<long> visited,HashSet<long> references)
    {
        if(!visited.Add(id))return;
        if(SourceExpressions[id].Kind=="parameter-reference")references.Add(id);
        foreach(var edge in SourceExpressionEdges.GetValueOrDefault(id,[]))CollectBorrowedExpressionParameterReferences(edge.Child,visited,references);
    }
}
