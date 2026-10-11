namespace DotObj;

internal sealed record SourceExpressionTemporary(long Kind,long? SpelledType,long? Constructor,long? Destructor,long? Constant,bool DependentConstructor,string LifetimeDisposition);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceExpressionTemporary> SourceExpressionTemporaries { get; }=[];
    public Dictionary<long,HashSet<string>> SourceExpressionTemporaryFlags { get; }=[];

    private void ReadSourceExpressionTemporaries()
    {
        using(var q=database.Prepare("SELECT expression_id,initialization_kind,spelled_source_type_id,constructor_declaration_id,destructor_declaration_id,constant_id,is_dependent_constructor,lifetime_disposition,capture_proof FROM source_expression_temporaries ORDER BY expression_id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceExpressionTemporary(q.Integer(1),q.OptionalInteger(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.Integer(6)==1,q.Text(7));
                if(!SourceExpressions.TryGetValue(id,out var expression) || expression.Kind!="temporary-initialization" ||
                    value.Kind<0 || value.SpelledType is long spelled && !SourceTypes.ContainsKey(spelled) ||
                    value.Constructor is long ctor && !SourceExpressionDeclarations.ContainsKey(ctor) ||
                    value.Destructor is long dtor && !SourceExpressionDeclarations.ContainsKey(dtor) ||
                    value.Constant is long constant && !SourceSemanticConstants.ContainsKey(constant) || q.Integer(6) is not(0 or 1) ||
                    value.LifetimeDisposition is not("no-linked-lifetime" or "retained-in-edg-graph") || q.Text(8)!="original-expression-temporary" || !SourceExpressionTemporaries.TryAdd(id,value))
                    throw new InvalidDataException("Invalid original source temporary initialization");
                if(value.LifetimeDisposition=="retained-in-edg-graph" || value.Kind>7)
                {
                    using var root=database.Prepare("SELECT n.id FROM edg_graph_roots r JOIN edg_graph_nodes n ON n.id=r.node_id WHERE r.role='source-expression' AND r.ordinal=? AND n.entry_kind_name='expr-node'");root.Bind(id-1);
                    if(!root.Read())throw new InvalidDataException("Source temporary metadata lacks its retained EDG graph root");
                }
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
        if(value.Constructor is long ctor)VerifyTemporaryRoutine(ctor,"constructor");
        if(value.Destructor is long dtor)VerifyTemporaryRoutine(dtor,"destructor");
        // Additional EDG initialization discriminants remain fully typed in
        // the raw graph; they do not need a second initialization grammar.
        if(value.Kind>7)return;
        bool constant=value.Kind is 2 or 6;
        if((value.Constant is not null)!=constant)
            throw new InvalidDataException("Source temporary constant payload differs from its recorded initialization kind");
        string? role=value.Kind switch {3 or 4=>"initializer",5=>"argument",7=>"source",_=>null};
        if(role is null?edges.Count!=0:edges.Any(edge=>edge.Role!=role) || value.Kind!=5 && edges.Count!=1)
            throw new InvalidDataException("Original source temporary lost its ordered initializer operands");
    }

    private void VerifyTemporaryRoutine(long declaration,string expected)
    {
        var value=SourceExpressionDeclarations[declaration];
        if(value.Kind!="routine" || value.RoutineKind!=expected || !SourceRoutines.ContainsKey(value.Type))
            throw new InvalidDataException("Source temporary has an invalid selected routine reference kind");
    }

}
