namespace DotObj;

// A source-only factory witness. The selected original instance owns every
// argument; this relation never grants an executable calling convention.
internal sealed record SourceDeclaratorContext(long RoutineType,long ScopeOrdinal,string Kind,
    long? Declaration,long? Record,long? Header,long ArgumentCount,long BorrowedCount);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceDeclaratorContext> SourceDeclaratorContexts {get;}=[];
    public Dictionary<long,SortedDictionary<long,long>> SourceDeclaratorBorrowedArguments {get;}=[];
    public HashSet<long> SourceDeclaratorClassWitnesses {get;}=[];
    private List<SourceTemplateArgument> SourceDeclaratorArguments(SourceDeclaratorContext context) => context.Kind=="routine"
        ? SourceExpressionDeclarationArguments.GetValueOrDefault(context.Declaration!.Value,[])
        : context.Kind=="class-partial" ? SourceClassSpecializationArguments.GetValueOrDefault(context.Record!.Value,[])
        : SourceTemplateArguments.GetValueOrDefault(context.Record!.Value,[]);
    private void ReadSourceDeclaratorContexts()
    {
        var scopes=new HashSet<(long,long)>();
        var routineDeclarationTypeIds=SourceExpressionDeclarations.Values.Where(declaration=>declaration.Kind=="routine")
            .Select(declaration=>declaration.Type).ToHashSet();
        var contextCounts=new Dictionary<long,long>();
        using(var q=database.Prepare("SELECT id,routine_source_type_id,scope_ordinal,kind,selected_source_declaration_id,source_record_id,header_source_record_id,argument_count,borrowed_argument_count,line,column,capture_proof FROM source_declarator_contexts ORDER BY id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceDeclaratorContext(q.Integer(1),q.Integer(2),q.Text(3),
                    q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.Integer(7),q.Integer(8));
                if(id<=0 || !SourceRoutines.ContainsKey(value.RoutineType) || value.ScopeOrdinal<0 ||
                    value.ArgumentCount<=0 || value.BorrowedCount<=0 || value.BorrowedCount>value.ArgumentCount ||
                    value.Kind is not("routine" or "class" or "class-partial") || q.Integer(9)<0 || q.Integer(10)<0 ||
                    q.Text(11) is not("observed-routine-declarator-instantiation-context" or "observed-function-instantiation-declarator-context") ||
                    q.Text(11)=="observed-function-instantiation-declarator-context" && value.Kind!="routine" ||
                    !scopes.Add((value.RoutineType,value.ScopeOrdinal)) || !SourceDeclaratorContexts.TryAdd(id,value))
                    throw new InvalidDataException("Anonymous routine declarator lacks its original instantiation factory context");
                if(routineDeclarationTypeIds.Contains(value.RoutineType))
                    throw new InvalidDataException("Anonymous routine context cannot replace a selected original routine declaration");
                if(value.Kind=="routine")
                {
                    if(value.Record is not null || value.Header is not null || value.Declaration is not long declarationId ||
                        !SourceExpressionDeclarations.TryGetValue(declarationId,out var declaration) || declaration.Kind!="routine" ||
                        declaration.Type==value.RoutineType || !declaration.HasTemplate || declaration.IsPrototype ||
                        declaration.TemplateOriginKind!="function-template" || declaration.TemplatePrototype is not long prototype ||
                        !SourceExpressionDeclarations.TryGetValue(prototype,out var original) || original.Kind!="routine" ||
                        !original.IsPrototype || !original.HasTemplate || original.TemplateOriginKind!="function-template")
                        throw new InvalidDataException("Routine declarator context lost its exact original function template instance");
                }
                else
                {
                    if(value.Declaration is not null || value.Record is not long record || value.Header is not long header ||
                        !SourceRecords.TryGetValue(record,out var instance) || instance.IsPrototypeInstantiation ||
                        !SourceRecordInstantiations.TryGetValue(record,out var relation) || relation.ResolutionKind!="class-template" ||
                        relation.PrototypeSourceRecordId!=header || !SourceClassTemplateHeaders.TryGetValue(header,out var original) ||
                        (value.Kind=="class-partial")!=(original.Primary is not null) ||
                        (value.Kind=="class-partial")!=relation.HasSelectedPartialSpecialization ||
                        value.Kind=="class-partial" && (!SourceClassSpecializations.TryGetValue(record,out var partial) || partial.Selected!=header))
                        throw new InvalidDataException("Routine declarator context lost its exact original class template instance");
                }
                if(SourceDeclaratorArguments(value).Count!=value.ArgumentCount)
                    throw new InvalidDataException("Routine declarator context original argument count disagrees with its selected instance");
                contextCounts[value.RoutineType]=contextCounts.GetValueOrDefault(value.RoutineType)+1;
                if(value.Record is long ownedRecord)SourceDeclaratorClassWitnesses.Add(ownedRecord);
            }
        using(var q=database.Prepare("SELECT context_id,argument_ordinal,source_type_id FROM source_declarator_borrowed_arguments ORDER BY context_id,argument_ordinal"))
            while(q.Read())
            {
                long contextId=q.Integer(0),ordinal=q.Integer(1),type=q.Integer(2);
                if(!SourceDeclaratorContexts.TryGetValue(contextId,out var context) || ordinal<0 || !SourceTypes.ContainsKey(type))
                    throw new InvalidDataException("Anonymous routine borrow lacks its original declarator context");
                var arguments=SourceDeclaratorArguments(context);
                if(ordinal>=arguments.Count || arguments[checked((int)ordinal)].Ordinal!=ordinal ||
                    arguments[checked((int)ordinal)].Kind!="type" || arguments[checked((int)ordinal)].TypeId!=type)
                    throw new InvalidDataException("Anonymous routine borrow differs from its exact ordered original type argument");
                if(!SourceDeclaratorBorrowedArguments.TryGetValue(contextId,out var selected))SourceDeclaratorBorrowedArguments[contextId]=selected=[];
                if(!selected.TryAdd(ordinal,type))throw new InvalidDataException("Duplicate original declarator type argument");
                var routine=SourceRoutines[context.RoutineType];
                var roots=routine.Parameters.Append(routine.ReturnTypeId).Concat(SourceParameterBinders
                    .Where(pair=>pair.Value.RoutineType==context.RoutineType).SelectMany(pair=>SourceBinderParameters.GetValueOrDefault(pair.Key,[]))
                    .SelectMany(parameter=>parameter.DeclaredType is long declared?new[]{parameter.Type,declared}:new[]{parameter.Type}));
                if(!roots.Any(root=>SourceRoutineTypeContains(root,type,[])))
                    throw new InvalidDataException("Anonymous routine borrow is not used by its original signature");
                var references=new HashSet<long>();CollectBorrowedTypeParameterReferences(type,[],references);
                if(references.Count==0)throw new InvalidDataException("Anonymous routine borrow has no retained original lexical recipe");
                foreach(long reference in references)
                {
                    if(!SourceExpressionParameterReferences.TryGetValue(reference,out var parameter))
                        throw new InvalidDataException("Anonymous routine borrow lost its original parameter-reference owner");
                    long? current=parameter.Binder;var visited=new HashSet<long>();
                    while(current is long binder)
                    {
                        if(!visited.Add(binder) || !SourceParameterBinders.TryGetValue(binder,out var scope))
                            throw new InvalidDataException("Anonymous routine borrow has an invalid original lexical scope");
                        if(scope.RoutineType==context.RoutineType)
                            throw new InvalidDataException("Anonymous routine borrow was rebound to the consuming declarator prototype");
                        current=scope.Parent;
                    }
                }
            }
        foreach(var pair in SourceDeclaratorContexts)
            if(SourceDeclaratorBorrowedArguments.GetValueOrDefault(pair.Key,[]).Count!=pair.Value.BorrowedCount)
                throw new InvalidDataException("Anonymous routine original borrowed argument list is incomplete");
        using(var q=database.Prepare("SELECT type_id,declarator_context_count FROM source_routine_types ORDER BY type_id"))
            while(q.Read())
                if(q.Integer(1)<0 || contextCounts.GetValueOrDefault(q.Integer(0))!=q.Integer(1))
                    throw new InvalidDataException("Anonymous routine is missing its required original declarator contexts");
    }
}
