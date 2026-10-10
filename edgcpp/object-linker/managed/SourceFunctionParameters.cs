namespace DotObj;
internal sealed record SourceFunctionParameterDeclaration(long Routine,long Binder,long Ordinal,long? DeclaredType);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceFunctionParameterDeclaration> SourceFunctionParameterDeclarations {get;}=[];
    private void ReadSourceFunctionParameterDeclarations()
    {
        using var q=database.Prepare("SELECT declaration_id,routine_declaration_id,binder_id,parameter_ordinal,declared_source_type_id,capture_proof FROM source_function_parameter_declarations ORDER BY declaration_id");
        while(q.Read())
        {
            long id=q.Integer(0);var value=new SourceFunctionParameterDeclaration(q.Integer(1),q.Integer(2),q.Integer(3),q.OptionalInteger(4));
            if(!SourceExpressionDeclarations.ContainsKey(id) || !SourceExpressionDeclarations.ContainsKey(value.Routine) ||
                !SourceParameterBinders.ContainsKey(value.Binder) || value.Ordinal<0 || value.DeclaredType is long declared && !SourceTypes.ContainsKey(declared) ||
                q.Text(5)!="original-function-parameter-list" || !SourceFunctionParameterDeclarations.TryAdd(id,value))
                throw new InvalidDataException("Source function parameter lacks its original declaration-list owner");
        }
    }
    private void VerifySourceFunctionParameterDeclarations()
    {
        var unique=new HashSet<(long Routine,long Ordinal)>();
        foreach(var pair in SourceFunctionParameterDeclarations)
        {
            var value=pair.Value;var declaration=SourceExpressionDeclarations[pair.Key];var owner=SourceExpressionDeclarations[value.Routine];
            var binder=SourceParameterBinders[value.Binder];var parameters=SourceBinderParameters.GetValueOrDefault(value.Binder,[]);
            if(pair.Key==value.Routine || declaration.ScopeKind!="function-parameter" || declaration.Kind!="variable" ||
                declaration.OwnerType is not null || declaration.HasTemplate || declaration.InitializerKind!="none" ||
                declaration.OriginalInitKind!=0 || declaration.OriginalDynamicInitKind is not null ||
                declaration.InitializerExpression is not null || declaration.InitializerConstant is not null ||
                owner.ScopeKind is not("nonlocal" or "namespace") || owner.Kind!="routine" || binder.Kind!="routine-prototype" || !binder.Outside || binder.Parent is not null ||
                binder.RoutineType!=owner.Type || value.Ordinal>=parameters.Count ||
                SourceTypeSignature(declaration.Type)!=SourceTypeSignature(parameters[(int)value.Ordinal].Type) ||
                !unique.Add((value.Routine,value.Ordinal)))
                throw new InvalidDataException("Source function parameter differs from its exact original routine, ordinal or adjusted type");
        }
        if(SourceExpressionDeclarations.Values.Any(value=>(value.ScopeKind=="function-parameter")!=SourceFunctionParameterDeclarations.ContainsKey(value.Id)))
            throw new InvalidDataException("Source function parameter is missing required original declaration ownership");
    }
}
