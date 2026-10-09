namespace DotObj;

internal sealed record Entity(long Id, string SourceName, string Linkage, string? ExternalKey,
    string Definition, long ReturnTypeId, long? SourceFileId, long Line, long Column, long RoutineTypeId);
internal sealed record Binding(long Id, long FunctionId, string Kind, long? Ordinal,
    long TypeId, string SourceName, long ScopeNodeId, bool IsSynthetic);
internal sealed record Node(long Id, long FunctionId, string Kind, long? TypeId,
    long? EntityId, long? BindingId, string? Operator, string? ConstantText,
    string? ValueCategory, string? EvaluationOrder, long? FieldId = null, long? StorageId = null);
internal sealed record Edge(long ParentId, string Role, long Ordinal, long ChildId);

internal sealed class FunctionBody
{
    public bool RequiresFlatControl { get; set; }
    public HashSet<long> CleanupCapturedBindings { get; } = [];
    public required long RootId { get; init; }
    public required Dictionary<long, Node> Nodes { get; init; }
    public required Dictionary<long, Binding> Bindings { get; init; }
    public required Dictionary<long, List<Edge>> Edges { get; init; }
    public IEnumerable<Edge> Children(long id, string role) => Edges.GetValueOrDefault(id, [])
        .Where(edge => edge.Role == role).OrderBy(edge => edge.Ordinal);
    public Node Child(long id, string role) => Nodes[Children(id, role).Single().ChildId];
}

internal sealed record Symbol(ObjectFile Object, Entity Entity)
{
    public string Key => Entity.Linkage == "external" ? "external:" + Entity.ExternalKey
        : "internal:" + Object.UnitId + ":" + Entity.Id;
    public string Signature => Object.EntityRoutine(Entity.Id);
}
