namespace RoslynObj;

public sealed class SemanticModel
{
    private readonly RoslynObject owner;
    internal SemanticModel(RoslynObject owner)=>this.owner=owner;
    private void Check(SemanticEntity entity)
    {
        if(entity.Object.Manifest.ArtifactIdentity!=owner.Manifest.ArtifactIdentity)
            throw new ArgumentException("The entity belongs to a different translation unit.",nameof(entity));
    }
    public Symbol? GetSymbol(SemanticNode node)
    {
        Check(node);
        return node switch
        {
            Declaration declaration=>declaration.Symbol as Symbol,CallOperation call=>call.TargetMethod,
            ReferenceOperation reference=>reference.Target as Symbol,
            DependentOperation dependent=>dependent.Template,
            TypeOperandOperation operand=>operand.OperandType?.DeclarationSymbol,
            ConstantOperation constant=>owner.Store.Query("SELECT symbol_id FROM enumerator WHERE constant_id=$owner ORDER BY owner_id,ordinal",constant.Constant.Id).FirstOrDefault() is {} row ? owner.Enumerator(row.Long("symbol_id")) : null,
            _=>null
        };
    }
    public CppType? GetType(SemanticNode node)
    {
        Check(node);
        return node is Declaration declaration ? declaration.Symbol switch {TypeSymbol symbol=>symbol.Type,CppType type=>type,Symbol symbol=>symbol.DeclaredType,_=>null} : (node as Operation)?.Type;
    }
    public Operation? GetOperation(SemanticNode node) { Check(node); return node as Operation; }
    public IReadOnlyList<SymbolReference> GetReferences(Symbol symbol)
    {
        Check(symbol);
        const string query = """
            SELECT id AS owner_id,target_id,reference_kind_id AS role_id,0 AS usage FROM reference_expression WHERE target_id=$owner
            UNION SELECT r.id,t.symbol_id,r.reference_kind_id,0 FROM reference_expression r JOIN type t ON t.id=r.target_id WHERE t.symbol_id=$owner
            UNION SELECT c.id,e.symbol_id,NULL,0 FROM constant_expression c JOIN enumerator e ON e.constant_id=c.constant_id WHERE e.symbol_id=$owner
            UNION SELECT id,template_id,NULL,0 FROM dependent_expression WHERE template_id=$owner
            UNION SELECT id,template_id,NULL,3 FROM template_instance WHERE template_id=$owner
            UNION SELECT id,target_routine_id,NULL,1 FROM call_expression WHERE target_routine_id=$owner
            UNION SELECT id,source_target_routine_id,NULL,2 FROM call_expression WHERE source_target_routine_id=$owner
            UNION SELECT s.owner_id,t.symbol_id,s.role_id,3 FROM semantic_reference s JOIN type t ON t.id=s.target_id WHERE t.symbol_id=$owner
                AND NOT EXISTS (SELECT 1 FROM reference_expression r WHERE r.id=s.owner_id AND r.target_id=s.target_id)
            UNION SELECT s.owner_id,s.target_id,s.role_id,3 FROM semantic_reference s WHERE s.target_id=$owner
                AND NOT EXISTS (SELECT 1 FROM reference_expression r WHERE r.id=s.owner_id AND r.target_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM constant_expression c JOIN enumerator e ON e.constant_id=c.constant_id WHERE c.id=s.owner_id AND e.symbol_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM dependent_expression d WHERE d.id=s.owner_id AND d.template_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM template_instance i WHERE i.id=s.owner_id AND i.template_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM call_expression c WHERE c.id=s.owner_id AND (c.target_routine_id=s.target_id OR c.source_target_routine_id=s.target_id))
            ORDER BY owner_id,usage,role_id
            """;
        return owner.List(query.ReplaceLineEndings(" "),symbol.Id,r=>new SymbolReference(owner.Entity(r.Long("owner_id")),symbol,(SymbolReferenceRole)r.Int("usage"),owner.OptionalKind(r.OptionalLong("role_id"))));
    }
    public IReadOnlyList<Symbol> FindSymbols(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        // Parameterization is supplied by the exact atom ID, never C++ lookup.
        var atom=owner.Store.FindAtom(name);
        return atom is null ? [] : owner.List("SELECT s.* FROM symbol s JOIN name n ON n.id=s.name_id WHERE n.spelling_id=$owner ORDER BY s.id",atom.Value,r=>owner.Symbol(r.Long("id")),"symbol");
    }
    public IReadOnlyList<FactAvailability> GetAvailability(SemanticEntity entity)
    {
        Check(entity);
        return owner.List("SELECT * FROM fact_availability WHERE owner_id=$owner ORDER BY fact_id",entity.Id,r=>new FactAvailability(owner.Kind(r.Long("fact_id")).Name,
            owner.Kind(r.Long("state_id")).Name switch {"available"=>AvailabilityState.Available,"absent"=>AvailabilityState.Absent,"not_applicable"=>AvailabilityState.NotApplicable,
                "dependent"=>AvailabilityState.Dependent,"disabled"=>AvailabilityState.Disabled,"lost" or "not_retained"=>AvailabilityState.NotRetained,_=>AvailabilityState.Unsupported},owner.Text(r.OptionalLong("reason_id"))));
    }
}

public enum SymbolReferenceRole { Reference, CallTarget, SourceCallTarget, RetainedRelation }

public sealed record SymbolReference(SemanticEntity Owner,Symbol ReferencedSymbol,SymbolReferenceRole Role,SemanticKind? RecordedRole)
{
    public SourceInfo? Source => Owner switch
    {
        SemanticNode node=>node.Source,Symbol symbol=>symbol.Source,
        CppType type=>type.Source,Constant constant=>constant.Source,
        _=>null
    };
}
