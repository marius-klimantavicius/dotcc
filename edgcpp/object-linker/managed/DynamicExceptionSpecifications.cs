namespace DotObj;

internal sealed record DynamicExceptionType(long DeclaredSourceTypeId, long MatchSourceTypeId);
internal sealed record DynamicExceptionSpecification(long SourceRoutineTypeId, long AllowedTypeCount,
    DynamicExceptionType[] Types);

internal sealed partial class ObjectFile
{
    public Dictionary<long, DynamicExceptionSpecification> DynamicExceptionSpecifications { get; } = [];
    public Dictionary<long, long> DynamicExceptionBoundaries { get; } = [];

    private void ReadSourceDynamicSpecifications()
    {
        var items = new Dictionary<long,List<DynamicExceptionType>>();
        using (var query=database.Prepare("SELECT source_routine_type_id,ordinal,declared_source_type_id,match_source_type_id FROM source_dynamic_exception_types ORDER BY source_routine_type_id,ordinal"))
            while(query.Read())
            {
                long type=query.Integer(0);
                if(!items.TryGetValue(type,out var list))items.Add(type,list=[]);
                if(query.Integer(1)!=list.Count)throw new InvalidDataException("Dynamic exception type list has non-dense original ordering");
                list.Add(new(query.Integer(2),query.Integer(3)));
            }
        using (var query=database.Prepare("SELECT source_routine_type_id,kind,allowed_type_count,capture_proof FROM source_dynamic_exception_specifications ORDER BY source_routine_type_id"))
            while(query.Read())
            {
                long id=query.Integer(0),count=query.Integer(2);
                if(SourceLanguage!="c++" || !SourceRoutines.TryGetValue(id,out var routine) || routine.ExceptionPolicy!="dynamic" ||
                    query.Text(1)!="throw-list" || count<0 || count!=items.GetValueOrDefault(id,[]).Count ||
                    query.Text(3)!="observed-resolved-source-specification" ||
                    !DynamicExceptionSpecifications.TryAdd(id,new(id,count,items.GetValueOrDefault(id,[]).ToArray())))
                    throw new InvalidDataException("Dynamic exception specification lacks its exact original kind, ordered types or capture");
            }
        if(items.Keys.Any(id=>!DynamicExceptionSpecifications.ContainsKey(id)) ||
            !DynamicExceptionSpecifications.Keys.ToHashSet().SetEquals(SourceRoutines.Values.Where(routine=>routine.ExceptionPolicy=="dynamic").Select(routine=>routine.TypeId)))
            throw new InvalidDataException("Dynamic routine policy is missing its original throw specification");
    }

    private void VerifySourceDynamicSpecifications()
    {
        foreach(var specification in DynamicExceptionSpecifications.Values)
            foreach(var item in specification.Types)
            {
                var source=Original(SourceTypeById(item.DeclaredSourceTypeId));
                long match=source.Kind is "lvalue-reference" or "rvalue-reference" ? source.UnderlyingTypeId!.Value : source.Id;
                if(Original(SourceTypeById(item.MatchSourceTypeId)).Id!=Original(SourceTypeById(match)).Id)
                    throw new InvalidDataException("Dynamic exception matching type differs from its original declaration");
            }
    }

    // Stock types.c::is_nothrow_spec considers empty throw() and noexcept
    // equally restrictive callable contracts. Keep their execution policies
    // distinct: only the legacy body invokes std::unexpected on a violation.
    private string SourceCallableExceptionPolicy(RoutineType routine) =>
        routine.ExceptionPolicy == "dynamic" && DynamicExceptionSpecifications.TryGetValue(routine.TypeId, out var specification) &&
        specification.AllowedTypeCount == 0 ? "terminate" : routine.ExceptionPolicy;

    private string DynamicSpecificationSignature(long sourceRoutineTypeId,int depth)
    {
        if(!DynamicExceptionSpecifications.TryGetValue(sourceRoutineTypeId,out var specification) || specification.AllowedTypeCount == 0)return "";
        // Preserve original order in metadata; callable compatibility uses the
        // type set, so spelling order and duplicate accepted entries do not
        // make otherwise identical lists into different contracts.
        return ":throws("+string.Join(',',specification.Types.Select(item=>SourceTypeSignature(item.MatchSourceTypeId,depth,false,false))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))+")";
    }

    private void ReadDynamicExceptionBoundaries()
    {
        using var query=database.Prepare("SELECT function_entity_id,source_routine_type_id,capture_proof FROM function_dynamic_exception_boundaries ORDER BY function_entity_id");
        while(query.Read())
        {
            long function=query.Integer(0),source=query.Integer(1);
            if(!Functions.ContainsKey(function) || !DynamicExceptionSpecifications.TryGetValue(source,out var specification) ||
                Routines[Entities[function].RoutineTypeId].ExceptionPolicy!="dynamic" ||
                !EntitySourceRoutines.TryGetValue(OriginalFunction(function),out var original) || original.TypeId!=source ||
                query.Text(2)!="observed-throw-spec-function-boundary" || !DynamicExceptionBoundaries.TryAdd(function,source))
                throw new InvalidDataException("Dynamic exception boundary lacks its observed execution and exact original routine specification");
            foreach(var type in specification.Types)_=ExceptionTypeIdentity(type.MatchSourceTypeId);
        }
        if(!DynamicExceptionBoundaries.Keys.ToHashSet().SetEquals(Functions.Keys.Where(id=>Routines[Entities[id].RoutineTypeId].ExceptionPolicy=="dynamic")))
            throw new InvalidDataException("Executable dynamic exception specification lacks an owned boundary");
    }
}
