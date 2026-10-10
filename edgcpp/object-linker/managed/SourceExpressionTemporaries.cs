namespace DotObj;

internal sealed record SourceExpressionTemporary(long Kind,long? SpelledType,long? Constructor,long? Destructor,long? Constant,bool DependentConstructor);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceExpressionTemporary> SourceExpressionTemporaries { get; }=[];
    public Dictionary<long,HashSet<string>> SourceExpressionTemporaryFlags { get; }=[];

    private void ReadSourceExpressionTemporaries()
    {
        using(var q=database.Prepare("SELECT expression_id,initialization_kind,spelled_source_type_id,constructor_declaration_id,destructor_declaration_id,constant_id,is_dependent_constructor,lifetime_disposition,capture_proof FROM source_expression_temporaries ORDER BY expression_id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceExpressionTemporary(q.Integer(1),q.OptionalInteger(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.Integer(6)==1);
                if(!SourceExpressions.TryGetValue(id,out var expression) || expression.Kind!="temporary-initialization" ||
                    value.Kind is <0 or >7 || value.SpelledType is long spelled && !SourceTypes.ContainsKey(spelled) ||
                    value.Constructor is long ctor && !SourceExpressionDeclarations.ContainsKey(ctor) ||
                    value.Destructor is long dtor && !SourceExpressionDeclarations.ContainsKey(dtor) ||
                    value.Constant is long constant && !SourceSemanticConstants.ContainsKey(constant) || q.Integer(6) is not(0 or 1) ||
                    q.Text(7)!="no-linked-lifetime" || q.Text(8)!="original-expression-temporary" || !SourceExpressionTemporaries.TryAdd(id,value))
                    throw new InvalidDataException("Invalid original source temporary initialization");
            }
        using(var q=database.Prepare("SELECT expression_id,flag FROM source_expression_temporary_flags ORDER BY expression_id,flag"))
            while(q.Read())
            {
                long id=q.Integer(0);string flag=q.Text(1);
                if(!SourceExpressionTemporaries.ContainsKey(id) || flag is not("static-storage" or "after-statement" or "conditional" or "unordered-destruction" or
                    "temporary-lifetime" or "explicit-cast" or "compound-literal" or "braced" or "partial-initialization" or "conditional-result" or
                    "master-result" or "comma-result" or "reused-value" or "initializer-list-object" or "initializer-list-array" or
                    "constexpr-reference-argument" or "implicit-copy-initialization" or "value-initialization" or "sequenced-arguments"))
                    throw new InvalidDataException("Unknown original source temporary initialization flag");
                if(!SourceExpressionTemporaryFlags.TryGetValue(id,out var flags))SourceExpressionTemporaryFlags[id]=flags=[];
                if(!flags.Add(flag))throw new InvalidDataException("Duplicate original source temporary initialization flag");
            }
    }

    private void VerifySourceExpressionTemporary(SourceExpression expression,List<SourceExpressionEdge> edges)
    {
        if(!SourceExpressionTemporaries.TryGetValue(expression.Id,out var value) || expression.SourceTypeId is not long type)throw new InvalidDataException("Original temporary expression lacks its typed initialization recipe");
        bool constructor=value.Kind==5,constant=value.Kind is 2 or 6;
        if((value.Constant is not null)!=constant || !constructor && (value.Constructor is not null || value.DependentConstructor) ||
            constructor && ((value.Constructor is not null)==value.DependentConstructor) ||
            value.DependentConstructor && !SourceTypeContainsParameter(type,0))
            throw new InvalidDataException("Original source temporary has inconsistent selected initialization");
        string? role=value.Kind switch {3 or 4=>"initializer",5=>"argument",7=>"source",_=>null};
        if(role is null?edges.Count!=0:edges.Any(edge=>edge.Role!=role) || value.Kind!=5 && edges.Count!=1)
            throw new InvalidDataException("Original source temporary lost its ordered initializer operands");
        var flags=SourceExpressionTemporaryFlags.GetValueOrDefault(expression.Id,[]);
        if(!constructor && flags.Any(flag=>flag is "implicit-copy-initialization" or "value-initialization" or "sequenced-arguments"))
            throw new InvalidDataException("Source temporary constructor flags lack constructor initialization");
        if(value.Constructor is long ctor)VerifyTemporaryRoutine(ctor,"constructor",type);
        if(value.Destructor is long dtor)VerifyTemporaryRoutine(dtor,"destructor",type);
    }

    private void VerifyTemporaryRoutine(long declaration,string expected,long objectType)
    {
        var value=SourceExpressionDeclarations[declaration];
        while(SourceTypeById(objectType).Kind is "qualified" or "typedef" or "array")objectType=SourceTypeById(objectType).UnderlyingTypeId??throw new InvalidDataException("Source temporary object type has no underlying type");
        if(value.Kind!="routine" || value.RoutineKind!=expected || !SourceRoutines.TryGetValue(value.Type,out var signature) ||
            signature.ObjectTypeId is not long owner || SourceArgumentTypeIdentity(owner,0)!=SourceArgumentTypeIdentity(objectType,0))
            throw new InvalidDataException("Source temporary selected routine lacks its exact original object and special-member role");
    }

    private string SourceTemporaryIdentity(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Source temporary initialization identity depth");
        var value=SourceExpressionTemporaries[id];
        string result=SourceIdentityFrame(value.Kind.ToString(System.Globalization.CultureInfo.InvariantCulture))+
            SourceIdentityFrame(value.SpelledType is long spelled?SourceArgumentTypeIdentity(spelled,depth+1):"")+
            SourceIdentityFrame(value.Constructor is long ctor?SourceStructuralDeclarationKey(ctor,depth+1):"")+
            SourceIdentityFrame(value.Destructor is long dtor?SourceStructuralDeclarationKey(dtor,depth+1):"")+
            SourceIdentityFrame(value.Constant is long constant?SourceConstantIdentity(constant,depth+1):"")+(value.DependentConstructor?"1":"0");
        var flags=SourceExpressionTemporaryFlags.GetValueOrDefault(id,[]).Order(StringComparer.Ordinal).ToArray();
        return result+"F"+SourceIdentityFrame(flags.Length.ToString(System.Globalization.CultureInfo.InvariantCulture))+string.Concat(flags.Select(SourceIdentityFrame));
    }
}
