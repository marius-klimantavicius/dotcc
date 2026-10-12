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
        return node switch {CallOperation call=>call.TargetMethod,ReferenceOperation reference=>reference.Target as Symbol,_=>null};
    }
    public CppType? GetType(SemanticNode node) { Check(node); return (node as Operation)?.Type; }
    public Operation? GetOperation(SemanticNode node) { Check(node); return node as Operation; }
    public IReadOnlyList<SemanticReferenceDetails> GetReferences(Symbol symbol)
    {
        Check(symbol);
        return owner.List("SELECT * FROM semantic_reference WHERE target_id=$owner ORDER BY owner_id,role_id,ordinal",symbol.Id,r=>new SemanticReferenceDetails(owner,r));
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
