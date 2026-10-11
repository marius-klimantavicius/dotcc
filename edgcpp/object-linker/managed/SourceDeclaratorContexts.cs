namespace DotObj;

// Recorded source context, including optional navigation projections. The raw
// EDG graph owns observed contexts; this relation grants no calling convention.
internal sealed record SourceDeclaratorContext(long RoutineType,long ScopeOrdinal,string Kind,
    long? Declaration,long? Record,long? Header,long ArgumentCount,long BorrowedCount);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceDeclaratorContext> SourceDeclaratorContexts {get;}=[];
    public Dictionary<long,SortedDictionary<long,long>> SourceDeclaratorBorrowedArguments {get;}=[];
    public HashSet<long> SourceDeclaratorClassWitnesses {get;}=[];
    private void ReadSourceDeclaratorContexts()
    {
        var scopes=new HashSet<(long,long)>();
        var contextCounts=new Dictionary<long,long>();
        using(var q=database.Prepare("SELECT id,routine_source_type_id,scope_ordinal,kind,selected_source_declaration_id,source_record_id,header_source_record_id,argument_count,borrowed_argument_count,line,column,capture_proof FROM source_declarator_contexts ORDER BY id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceDeclaratorContext(q.Integer(1),q.Integer(2),q.Text(3),
                    q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.Integer(7),q.Integer(8));
                if(id<=0 || !SourceRoutines.ContainsKey(value.RoutineType) || value.ScopeOrdinal<0 ||
                    value.ArgumentCount<0 || value.BorrowedCount<0 || value.BorrowedCount>value.ArgumentCount ||
                    value.Kind is not("routine" or "class" or "class-partial" or "observed") || q.Integer(9)<0 || q.Integer(10)<0 ||
                    q.Text(11) is not("observed-routine-declarator-instantiation-context" or "observed-function-instantiation-declarator-context") ||
                    q.Text(11)=="observed-function-instantiation-declarator-context" && value.Kind is not("routine" or "observed") ||
                    !scopes.Add((value.RoutineType,value.ScopeOrdinal)) || !SourceDeclaratorContexts.TryAdd(id,value))
                    throw new InvalidDataException("Anonymous routine declarator lacks its original instantiation factory context");
                if(value.Kind=="observed")
                {
                    if(value.Declaration is long observedDeclaration &&
                        (!SourceExpressionDeclarations.TryGetValue(observedDeclaration,out var observedRoutine) || observedRoutine.Kind!="routine") ||
                        value.Record is long observedRecord && !SourceRecords.ContainsKey(observedRecord) ||
                        value.Header is long observedHeader && !SourceClassTemplateHeaders.ContainsKey(observedHeader))
                        throw new InvalidDataException("Observed declarator context has invalid typed projection edges");
                    using var root=database.Prepare("SELECT node_id FROM edg_graph_roots WHERE role='source-declarator-context' AND ordinal=?");
                    root.Bind(id-1);
                    if(!root.Read())throw new InvalidDataException("Observed declarator context is missing its retained EDG graph root");
                }
                else if(value.Kind=="routine" ? value.Record is not null || value.Header is not null ||
                    value.Declaration is not long declaration || !SourceExpressionDeclarations.TryGetValue(declaration,out var original) || original.Kind!="routine" :
                    value.Declaration is not null || value.Record is not long record || !SourceRecords.ContainsKey(record) ||
                    value.Header is not long header || !SourceClassTemplateHeaders.ContainsKey(header))
                    throw new InvalidDataException("Routine declarator context has invalid typed declaration edges");
                contextCounts[value.RoutineType]=contextCounts.GetValueOrDefault(value.RoutineType)+1;
                if(value.Record is long ownedRecord)SourceDeclaratorClassWitnesses.Add(ownedRecord);
            }
        using(var q=database.Prepare("SELECT context_id,argument_ordinal,source_type_id FROM source_declarator_borrowed_arguments ORDER BY context_id,argument_ordinal"))
            while(q.Read())
            {
                long contextId=q.Integer(0),ordinal=q.Integer(1),type=q.Integer(2);
                if(!SourceDeclaratorContexts.TryGetValue(contextId,out var context) || ordinal<0 || !SourceTypes.ContainsKey(type))
                    throw new InvalidDataException("Anonymous routine borrow lacks its original declarator context");
                if(!SourceDeclaratorBorrowedArguments.TryGetValue(contextId,out var selected))SourceDeclaratorBorrowedArguments[contextId]=selected=[];
                if(!selected.TryAdd(ordinal,type))throw new InvalidDataException("Duplicate original declarator type argument");
                // Preserve EDG's recorded relation; shared original recipes
                // do not need a unique consuming lexical binder.

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
