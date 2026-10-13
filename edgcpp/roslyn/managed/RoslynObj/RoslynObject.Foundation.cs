namespace RoslynObj;

/// <summary>Owns one immutable translation-unit snapshot. Independent objects are never linked or deduplicated.</summary>
public sealed partial class RoslynObject : IDisposable
{
    internal SnapshotStore Store { get; }
    public ObjectManifest Manifest { get; }
    private readonly BoundedCache<(string Table, long Id), WeakReference<object>> identities;
    private readonly object identityLock = new();

    private RoslynObject(SnapshotStore store, ObjectManifest manifest, ReadOptions options)
    {
        Store = store;
        Manifest = manifest;
        identities = new(options.CachedRowCount);
    }

    internal T Cached<T>(string table, long id, Func<Row, T> create) where T : class
    {
        Store.EnsureActive();
        lock (identityLock)
        {
            if (identities.TryGet((table, id), out var reference) && reference.TryGetTarget(out var value))
                return (T)value;
            var result = create(Store.Required(table, id));
            identities.Set((table, id), new(result));
            return result;
        }
    }

    internal string? Text(long? id) => id is null ? null : Store.Required("text_atom", id.Value).Text("value");

    internal PagedList<T> List<T>(string sql, long owner, Func<Row, T> create, string? table = null)
        => new(Store, sql, owner, create, table);

    public ReaderStatistics Statistics => Store.Statistics;
    public void Dispose()
    {
        Store.Dispose();
        lock (identityLock) identities.Clear();
    }
}

/// <summary>Equality is local EDG identity within one artifact, never a name or inferred C++ equivalence.</summary>
public abstract class SemanticEntity : IEquatable<SemanticEntity>
{
    internal RoslynObject Object { get; }
    internal long Id { get; }
    private readonly string artifact;
    internal SemanticEntity(RoslynObject owner, long id)
    {
        Object = owner; Id = id; artifact = owner.Manifest.ArtifactIdentity;
    }
    public virtual IReadOnlyList<SemanticKind> DeclarationModifiers => Object.List("SELECT * FROM owner_decl_modifier WHERE owner_id=$owner ORDER BY kind_id",Id,r=>Object.Kind(r.Long("kind_id")));
    public virtual IReadOnlyList<Attribute> Attributes => Object.List("SELECT a.* FROM owner_attribute r JOIN attribute a ON a.id=r.attribute_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Attribute(r.Long("id")),"attribute");
    public IReadOnlyList<MicrosoftAttribute> MicrosoftAttributes => Object.List("SELECT a.* FROM owner_ms_attribute r JOIN ms_attribute a ON a.id=r.attribute_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.MicrosoftAttribute(r.Long("id")),"ms_attribute");
    public IReadOnlyList<SynthesizedOriginDetails> SynthesizedOrigins => Object.List("SELECT * FROM synthesized_origin WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new SynthesizedOriginDetails(Object,r));
    public IReadOnlyList<ConstraintMetadataDetails> Constraints => Object.List("SELECT * FROM constraint_metadata WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new ConstraintMetadataDetails(Object,r));
    public T? As<T>() where T : SemanticEntity => this as T;
    public bool Equals(SemanticEntity? other) => other is not null && Id == other.Id && artifact == other.artifact;
    public override bool Equals(object? obj) => obj is SemanticEntity other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(artifact, Id);
    public static bool operator ==(SemanticEntity? left, SemanticEntity? right) => Equals(left, right);
    public static bool operator !=(SemanticEntity? left, SemanticEntity? right) => !Equals(left, right);
}
