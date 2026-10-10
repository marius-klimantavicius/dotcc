namespace DotObj;

internal sealed partial class ObjectFile
{
    public Dictionary<long,long> SourceSemanticConstantExpressions {get;}=[];
    private void ReadSourceConstantExpressions()
    {
        using var q=database.Prepare("SELECT constant_id,expression_id,capture_proof FROM source_semantic_constant_expressions ORDER BY constant_id");
        while(q.Read())
        {
            if(!SourceSemanticConstants.TryGetValue(q.Integer(0),out var value) || value.Kind!="source-expression" ||
                !SourceExpressions.TryGetValue(q.Integer(1),out var expression) || expression.Kind!="variable-reference" ||
                q.Text(2)!="original-dependent-variable-expression" || !SourceSemanticConstantExpressions.TryAdd(q.Integer(0),q.Integer(1)))
                throw new InvalidDataException("Dependent source value lost its original variable expression bridge");
        }
        foreach(var value in SourceSemanticConstants.Values)
            if(SourceSemanticConstantExpressions.ContainsKey(value.Id)!=(value.Kind=="source-expression"))
                throw new InvalidDataException("Dependent source value is missing its required original expression");
    }
    private void VerifySourceConstantExpression(SourceSemanticConstant value)
    {
        if(!SourceSemanticConstantExpressions.TryGetValue(value.Id,out long id) || value.OperatorName.Length!=0 || value.ValueText.Length!=0 ||
            SourceSemanticConstantOperands.GetValueOrDefault(value.Id,[]).Length!=0 || SourceExpressions[id].SourceTypeId is not long type ||
            SourceConstantTypeIdentity(type,0)!=SourceConstantTypeIdentity(value.SourceTypeId,0))
            throw new InvalidDataException("Dependent source variable expression changed its original value type or invented an evaluated value");
    }
    private string SourceVariableExpressionIdentity(long expression,int depth)
    {
        if(depth>128 || !SourceExpressionDeclarationReferences.TryGetValue(expression,out long id))throw new InvalidDataException("Original source variable identity cycle/declaration");
        var value=SourceExpressionDeclarations[id];
        if(value.Kind!="variable")throw new InvalidDataException("Original source variable identity has a nonvariable declaration");
        string result=SourceIdentityFrame(value.QualifiedName)+SourceIdentityFrame(value.OwnerType is long owner?SourceArgumentTypeIdentity(owner,depth+1):"")+
            (value.HasTemplate?"1":"0")+(value.IsPrototype?"1":"0")+SourceIdentityFrame(value.TemplateOriginKind);
        foreach(var argument in SourceExpressionDeclarationArguments.GetValueOrDefault(id,[]))
        {
            var facts=SourceConstraintArgumentDetails[("declaration",id,0,argument.Ordinal)];
            result+=SourceIdentityFrame((argument.Kind=="pack-boundary"?"pack-boundary":argument.Kind=="type"?"t"+SourceArgumentTypeIdentity(argument.TypeId!.Value,depth+1):
                argument.Kind=="constant"?"c"+SourceConstantIdentity(argument.ConstantId!.Value,depth+1):"m"+SourceTemplateIdentity(argument.TemplateId!.Value,depth+1))+(facts.Pack?":pack":""));
        }
        return result;
    }
    private bool SourceVariableExpressionContainsParameter(long expression,int depth)
    {
        if(depth>128)throw new InvalidDataException("Original source variable dependency cycle/depth");
        long id=SourceExpressionDeclarationReferences[expression];var value=SourceExpressionDeclarations[id];
        return value.OwnerType is long owner && SourceTypeContainsParameter(owner,depth+1) ||
            SourceExpressionDeclarationArguments.GetValueOrDefault(id,[]).Any(argument=>argument.Kind=="type"?SourceTypeContainsParameter(argument.TypeId!.Value,depth+1):
                argument.Kind=="constant"?ContainsSourceParameter(argument.ConstantId!.Value,depth+1):argument.Kind=="template");
    }
    private void VerifySourceConstantExpressionDeclarations()
    {
        foreach(long expression in SourceSemanticConstantExpressions.Values)
            AddSourceConstraintContract("source-variable:"+SourceVariableExpressionIdentity(expression,0),
                SourceConstraintDeclarationIdentity(SourceExpressionDeclarationReferences[expression],[],[]));
    }
}

internal sealed record SourceAggregateConstant(long Count,long? OriginalType,long? BackingExpression,long Flags,bool TemplateConst);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceAggregateConstant> SourceAggregateConstants {get;}=[];
    private void ReadSourceAggregateConstants()
    {
        using var q=database.Prepare("SELECT constant_id,element_count,original_type_id,backing_expression_id,flag_bits,added_template_const,capture_proof FROM source_aggregate_constants ORDER BY constant_id");
        while(q.Read())
        {
            var value=new SourceAggregateConstant(q.Integer(1),q.OptionalInteger(2),q.OptionalInteger(3),q.Integer(4),q.Integer(5)==1);
            if(!SourceSemanticConstants.TryGetValue(q.Integer(0),out var constant) || constant.Kind!="aggregate" || value.Count<0 ||
                value.OriginalType is long original && !SourceTypes.ContainsKey(original) || value.BackingExpression is long expression && !SourceExpressions.ContainsKey(expression) ||
                value.Flags is <0 or >65535 || (value.Flags&(1L<<15))!=0 || q.Integer(5) is not(0 or 1) ||
                q.Text(6)!="original-aggregate-constant" || !SourceAggregateConstants.TryAdd(q.Integer(0),value))
                throw new InvalidDataException("Original aggregate constant lost its initializer type, flags or expression");
        }
        foreach(var value in SourceSemanticConstants.Values)
            if(SourceAggregateConstants.ContainsKey(value.Id)!=(value.Kind=="aggregate"))throw new InvalidDataException("Original aggregate constant is missing its required initializer metadata");
    }
    private void VerifySourceAggregateConstant(SourceSemanticConstant value)
    {
        var original=SourceAggregateConstants[value.Id];var type=Original(SourceTypeById(value.SourceTypeId));
        var operands=SourceSemanticConstantOperands.GetValueOrDefault(value.Id,[]);
        if(type.Kind is not("record" or "array") || value.ValueText.Length!=0 || value.OperatorName.Length!=0 || operands.Length!=original.Count ||
            type.Kind=="array" && type.ElementCount is long count && original.Count>count)
            throw new InvalidDataException("Original aggregate initializer has a different element list or nonaggregate type");
    }
    private string SourceAggregateIdentity(long id,int depth)
    {
        var value=SourceAggregateConstants[id];
        return SourceIdentityFrame(value.Flags.ToString(System.Globalization.CultureInfo.InvariantCulture))+(value.TemplateConst?"1":"0")+
            SourceIdentityFrame(value.OriginalType is long type?SourceArgumentTypeIdentity(type,depth+1):"")+
            string.Concat(SourceSemanticConstantOperands.GetValueOrDefault(id,[]).Select(child=>SourceIdentityFrame(SourceConstantIdentity(child,depth+1))));
    }
}
