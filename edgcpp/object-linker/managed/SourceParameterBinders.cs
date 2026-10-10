namespace DotObj;
internal sealed record SourceParameterBinder(string Kind,long? Parent,long? RequiresExpression,long? RoutineType,bool Outside,long Count);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceParameterBinder> SourceParameterBinders {get;}=[];
    public Dictionary<long,List<SourceExpressionParameter>> SourceBinderParameters {get;}=[];
    public Dictionary<long,long> SourceRequiresBodyBinders {get;}=[];
    public Dictionary<long,(long OriginalType,long Binder)> SourceRoutinePrototypeOrigins {get;}=[];
    private void ReadSourceParameterBinders()
    {
        using(var q=database.Prepare("SELECT id,kind,parent_binder_id,requires_expression_id,routine_source_type_id,outside_parameter_list,parameter_count,capture_proof FROM source_parameter_binders ORDER BY id"))
            while(q.Read())
            {
                var value=new SourceParameterBinder(q.Text(1),q.OptionalInteger(2),q.OptionalInteger(3),q.OptionalInteger(4),q.Integer(5)==1,q.Integer(6));
                if(value.Kind is not("requires" or "routine-prototype") || q.Integer(5) is not(0 or 1) || value.Count<0 ||
                    (value.Kind=="requires")!=(value.RequiresExpression is not null) || (value.Kind=="routine-prototype")!=(value.RoutineType is not null) ||
                    value.RequiresExpression is long expression && (!SourceExpressions.TryGetValue(expression,out var owner) || owner.Kind!="requires") ||
                    value.RoutineType is long routine && !SourceRoutines.ContainsKey(routine) || q.Text(7)!="original-parameter-scope" ||
                    !SourceParameterBinders.TryAdd(q.Integer(0),value))
                    throw new InvalidDataException("Original parameter binder lacks its exact requires or routine prototype owner");
                if(value.RequiresExpression is long requires && value.Outside && !SourceRequiresBodyBinders.TryAdd(requires,q.Integer(0)))
                    throw new InvalidDataException("Requires expression has competing original body scopes");
            }
        using(var q=database.Prepare("SELECT binder_id,ordinal,source_type_id,declared_source_type_id,source_name,qualifier_bits,is_pack FROM source_binder_parameters ORDER BY binder_id,ordinal"))
            while(q.Read())
            {
                long binder=q.Integer(0);var parameter=new SourceExpressionParameter(q.Integer(2),q.OptionalInteger(3),q.Text(4),q.Integer(5),q.Integer(6)==1);
                if(!SourceParameterBinders.ContainsKey(binder) || !SourceTypes.ContainsKey(parameter.Type) ||
                    parameter.DeclaredType is long declared && !SourceTypes.ContainsKey(declared) || parameter.Qualifiers<0 || q.Integer(6) is not(0 or 1))
                    throw new InvalidDataException("Original parameter binder lost a typed parameter declaration");
                if(!SourceBinderParameters.TryGetValue(binder,out var parameters))SourceBinderParameters[binder]=parameters=[];
                if(q.Integer(1)!=parameters.Count)throw new InvalidDataException("Original binder parameter ordinals are not dense");
                parameters.Add(parameter);
            }
    }
    private void VerifySourceRoutinePrototypeOrigins()
    {
        var declared=new Dictionary<long,string>();
        using(var q=database.Prepare("SELECT type_id,prototype_origin FROM source_routine_types ORDER BY type_id"))
            while(q.Read())
            {
                string origin=q.Text(1);
                if(origin is not("original" or "lambda-conversion-copy"))throw new InvalidDataException("Unknown source routine prototype origin");
                declared.Add(q.Integer(0),origin);
            }
        using(var q=database.Prepare("SELECT routine_source_type_id,original_routine_source_type_id,original_binder_id,kind,capture_proof FROM source_routine_prototype_origins ORDER BY routine_source_type_id"))
            while(q.Read())
            {
                long id=q.Integer(0),original=q.Integer(1),binder=q.Integer(2);
                if(id==original || !SourceRoutines.TryGetValue(id,out var copy) || !SourceRoutines.TryGetValue(original,out var source) ||
                    declared.GetValueOrDefault(id)!="lambda-conversion-copy" || declared.GetValueOrDefault(original)!="original" ||
                    !SourceParameterBinders.TryGetValue(binder,out var scope) || scope.Kind!="routine-prototype" || !scope.Outside || scope.Parent is not null || scope.RoutineType!=original ||
                    copy.ObjectTypeId is not null || copy.ObjectIsConst || copy.ObjectIsVolatile || copy.ObjectRefQualifier!="none" || source.ObjectTypeId is null ||
                    copy.ReturnTypeId!=source.ReturnTypeId || !copy.Parameters.SequenceEqual(source.Parameters) || copy.Variadic!=source.Variadic ||
                    copy.ExceptionPolicy!=source.ExceptionPolicy || copy.ParameterMode!=source.ParameterMode ||
                    q.Text(3)!="lambda-conversion-copy" || q.Text(4)!="observed-lambda-conversion-prototype" ||
                    !SourceRoutinePrototypeOrigins.TryAdd(id,(original,binder)))
                    throw new InvalidDataException("Generated lambda prototype lost its exact original receiver-cleared signature and lexical recipe");
            }
        if(declared.Any(pair=>(pair.Value=="lambda-conversion-copy")!=SourceRoutinePrototypeOrigins.ContainsKey(pair.Key)))
            throw new InvalidDataException("Generated lambda prototype lacks its required original call-operator provenance");
    }
    private void VerifySourceParameterBinders()
    {
        VerifySourceRoutinePrototypeOrigins();
        foreach(var pair in SourceParameterBinders)
        {
            var value=pair.Value;var parameters=SourceBinderParameters.GetValueOrDefault(pair.Key,[]);
            if(parameters.Count!=value.Count)throw new InvalidDataException("Original parameter binder list is incomplete");
            var visited=new HashSet<long>();long? current=pair.Key;
            while(current is long id)
            {
                if(!visited.Add(id) || !SourceParameterBinders.TryGetValue(id,out var parent))
                    throw new InvalidDataException("Original parameter scope parent graph is missing or cyclic");
                current=parent.Parent;
            }
            if(value.RequiresExpression is long expression)
            {
                if(!parameters.SequenceEqual(SourceExpressionParameters.GetValueOrDefault(expression,[])) ||
                    value.Parent!=(SourceExpressionBinders.TryGetValue(expression,out long lexical)?lexical:(long?)null))
                    throw new InvalidDataException("Requires parameter binder differs from its original expression declarations or lexical owner");
            }
            else
            {
                var routine=SourceRoutines[value.RoutineType!.Value];
                if(routine.Parameters.Length!=parameters.Count || routine.Parameters.Where((type,index)=>type!=(parameters[index].DeclaredType??parameters[index].Type)).Any())
                    throw new InvalidDataException("Routine prototype binder differs from its exact original parameter signature");
            }
        }
        foreach(var group in SourceParameterBinders.GroupBy(pair=>(pair.Value.Kind,pair.Value.Parent,pair.Value.RequiresExpression,pair.Value.RoutineType)))
        {
            if(group.GroupBy(pair=>pair.Value.Outside).Any(phases=>phases.Count()!=1) ||
                group.Skip(1).Any(pair=>!SourceBinderParameters.GetValueOrDefault(group.First().Key,[]).SequenceEqual(SourceBinderParameters.GetValueOrDefault(pair.Key,[]))))
                throw new InvalidDataException("Original parameter prototype phases disagree");
        }
        foreach(var expression in SourceExpressions.Values.Where(value=>value.Kind=="requires"))
            if(!SourceRequiresBodyBinders.ContainsKey(expression.Id))throw new InvalidDataException("Requires expression lacks original outside-parameter-list scope");
    }
    private void VerifySourceParameterReference(long expression)
    {
        var reference=SourceExpressionParameterReferences[expression];long selected=SourceExpressionBinders.GetValueOrDefault(expression);long levels=reference.Levels;
        if(selected!=0)
        {
            if(levels==0)
                while(selected!=0 && !SourceParameterBinders[selected].Outside)selected=SourceParameterBinders[selected].Parent??0;
            else if(!SourceParameterBinders[selected].Outside)--levels;
            while(levels-->0 && selected!=0)selected=SourceParameterBinders[selected].Parent??0;
        }
        if(selected==0 || selected!=reference.Binder || reference.Ordinal>=SourceBinderParameters.GetValueOrDefault(selected,[]).Count)
            throw new InvalidDataException("Source parameter reference does not resolve through its exact lexical binders");
        var parameter=SourceBinderParameters[selected][checked((int)reference.Ordinal)];var value=SourceExpressions[expression];
        if((value.OriginalLvalueTypeId??value.SourceTypeId) is not long type || SourceTypeSignature(type)!=SourceParameterValueSignature(parameter))
            throw new InvalidDataException("Source parameter reference lost its exact original adjusted or lvalue type");
    }
    private string SourceParameterValueSignature(SourceExpressionParameter parameter)
    {
        // il_def::a_param_type.qualifiers retains the top-level qualifiers
        // removed from a function type. expr::make_param_ref_operand uses the
        // original param-id value type (func_def.c checks the same bitset).
        // Restore those exact qualifiers; never erase a mismatch on the use.
        long bits=parameter.Qualifiers;
        if((bits&~127L)!=0)throw new InvalidDataException("Source parameter value needs an owned extended qualifier contract");
        string prefix=(bits%2!=0?"const:":"")+((bits&2)!=0?"volatile:":"")+((bits&4)!=0?"restrict:":"")+((bits&8)!=0?"atomic:":"");
        var extras=new List<string>();
        if((bits&16)!=0)extras.Add("nullable");
        if((bits&32)!=0)extras.Add("nonnull");
        if((bits&64)!=0)extras.Add("null-unspecified");
        return prefix+string.Concat(extras.Order(StringComparer.Ordinal).Select(value=>value+":"))+SourceTypeSignature(parameter.Type);
    }
    private string SourceParameterReferenceContextKey(long expression)
    {
        var reference=SourceExpressionParameterReferences[expression];long current=SourceExpressionBinders.GetValueOrDefault(expression);string result="";
        var seen=new HashSet<long>();
        while(current!=0)
        {
            if(!seen.Add(current))throw new InvalidDataException("Source parameter reference context cycle");
            var value=SourceParameterBinders[current];result+=SourceIdentityFrame(value.Kind)+(value.Outside?"1":"0");
            if(current==reference.Binder)return result;
            current=value.Parent??0;
        }
        throw new InvalidDataException("Source parameter reference context does not reach its owner");
    }
}
