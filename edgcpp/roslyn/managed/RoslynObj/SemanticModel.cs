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
            DependentOperation dependent=>dependent.Template?.DeclarationSymbol,
            TypeOperandOperation operand=>operand.OperandType?.DeclarationSymbol,
            ConstantOperation constant=>constant.Constant.DeclarationSymbol ?? constant.Constant.Value switch
            {
                AddressValue address=>address.BaseEntity as Symbol,
                ConstantValue<MemberPointerValueDetails> member=>member.Payload.Member as Symbol,
                _=>null
            },
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
            WITH literal_target(constant_id,target_id) AS (
                SELECT c.id,a.base_entity_id FROM constant c JOIN address_value a ON a.id=c.value_id WHERE a.base_entity_id=$owner
                UNION SELECT c.id,m.member_id FROM constant c JOIN member_pointer_value m ON m.id=c.value_id WHERE m.member_id=$owner
                UNION SELECT c.id,a.base_entity_id FROM constant c JOIN label_difference_value d ON d.id=c.value_id JOIN address_value a ON a.id=d.from_value_id OR a.id=d.to_value_id WHERE a.base_entity_id=$owner
            )
            SELECT id AS owner_id,target_id,reference_kind_id AS role_id,0 AS usage FROM reference_expression WHERE target_id=$owner
            UNION SELECT r.id,t.symbol_id,r.reference_kind_id,0 FROM reference_expression r JOIN type t ON t.id=r.target_id WHERE t.symbol_id=$owner
            UNION SELECT r.id,p.id,r.reference_kind_id,0 FROM reference_expression r JOIN template_parameter p ON p.placeholder_type_id=r.target_id WHERE p.id=$owner
            UNION SELECT c.id,e.symbol_id,NULL,0 FROM constant_expression c JOIN enumerator e ON e.constant_id=c.constant_id WHERE e.symbol_id=$owner
            UNION SELECT c.id,p.id,NULL,0 FROM constant_expression c JOIN template_parameter p ON p.placeholder_constant_id=c.constant_id WHERE p.id=$owner
            UNION SELECT e.id,t.target_id,NULL,0 FROM literal_target t JOIN constant_expression e ON e.constant_id=t.constant_id
            UNION SELECT t.constant_id,t.target_id,NULL,3 FROM literal_target t WHERE NOT EXISTS (SELECT 1 FROM constant_expression e WHERE e.constant_id=t.constant_id)
            UNION SELECT d.id,s.id,NULL,0 FROM dependent_expression d JOIN template_symbol s ON s.descriptor_id=d.template_id WHERE s.id=$owner
            UNION SELECT d.id,p.id,NULL,0 FROM dependent_expression d JOIN template_parameter p ON p.placeholder_template_id=d.template_id WHERE p.id=$owner
            UNION SELECT i.id,s.id,NULL,3 FROM template_instance i JOIN template_symbol s ON s.descriptor_id=i.template_id WHERE s.id=$owner
            UNION SELECT id,target_routine_id,NULL,1 FROM call_expression WHERE target_routine_id=$owner
            UNION SELECT id,source_target_routine_id,NULL,2 FROM call_expression WHERE source_target_routine_id=$owner
            UNION SELECT s.owner_id,t.symbol_id,s.role_id,3 FROM semantic_reference s JOIN type t ON t.id=s.target_id WHERE t.symbol_id=$owner
                AND NOT EXISTS (SELECT 1 FROM reference_expression r WHERE r.id=s.owner_id AND r.target_id=s.target_id)
            UNION SELECT s.owner_id,p.id,s.role_id,3 FROM semantic_reference s JOIN template_parameter p ON p.placeholder_type_id=s.target_id OR p.placeholder_constant_id=s.target_id OR p.placeholder_template_id=s.target_id WHERE p.id=$owner
                AND NOT EXISTS (SELECT 1 FROM reference_expression r WHERE r.id=s.owner_id AND r.target_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM constant_expression c WHERE c.id=s.owner_id AND c.constant_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM dependent_expression d WHERE d.id=s.owner_id AND d.template_id=s.target_id)
            UNION SELECT s.owner_id,t.id,s.role_id,3 FROM semantic_reference s JOIN template_symbol t ON t.descriptor_id=s.target_id WHERE t.id=$owner
                AND NOT EXISTS (SELECT 1 FROM dependent_expression d WHERE d.id=s.owner_id AND d.template_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM template_instance i WHERE i.id=s.owner_id AND i.template_id=s.target_id)
            UNION SELECT s.owner_id,s.target_id,s.role_id,3 FROM semantic_reference s WHERE s.target_id=$owner
                AND NOT EXISTS (SELECT 1 FROM reference_expression r WHERE r.id=s.owner_id AND r.target_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM constant_expression c JOIN enumerator e ON e.constant_id=c.constant_id WHERE c.id=s.owner_id AND e.symbol_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM dependent_expression d WHERE d.id=s.owner_id AND d.template_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM template_instance i WHERE i.id=s.owner_id AND i.template_id=s.target_id)
                AND NOT EXISTS (SELECT 1 FROM call_expression c WHERE c.id=s.owner_id AND (c.target_routine_id=s.target_id OR c.source_target_routine_id=s.target_id))
                AND NOT EXISTS (SELECT 1 FROM literal_target t WHERE t.constant_id=s.owner_id AND t.target_id=s.target_id)
            ORDER BY owner_id,usage,role_id
            """;
        return owner.List(query.ReplaceLineEndings(" "),symbol.Id,r=>new SymbolReference(owner.Entity(r.Long("owner_id")),symbol,(SymbolReferenceRole)r.Int("usage"),owner.OptionalKind(r.OptionalLong("role_id"))));
    }
    public IReadOnlyList<Symbol> FindSymbols(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        // Parameterization is supplied by the exact atom ID, never C++ lookup.
        var atom=owner.Store.FindAtom(name);
        return atom is null ? [] : owner.List("SELECT s.* FROM symbol s JOIN entity_source_correspondence e ON e.owner_id=s.id JOIN source_correspondence c ON c.id=e.source_correspondence_id WHERE CASE WHEN c.unnamed_entity_given_fabricated_name=1 THEN NULL WHEN c.name_has_been_mangled=1 THEN c.unmangled_name_or_mangled_encoding_id ELSE c.current_name_id END=$owner ORDER BY s.id",atom.Value,r=>owner.Symbol(r.Long("id")),"symbol");
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
        CppType type=>type.Source,Constant constant=>constant.Source,TemplateDescriptor template=>template.Source,
        _=>null
    };
}
