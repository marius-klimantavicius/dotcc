namespace Managed.PGlite;

/// <summary>Per-query value conversion and optional PostgreSQL parameter OIDs.
/// A zero OID asks PostgreSQL to infer that parameter's type from the statement.</summary>
public sealed record PgliteQueryOptions
{
    private IReadOnlyList<uint>? parameterTypes;

    /// <summary>An optional registry overriding the session's value codecs.</summary>
    public PgliteCodecs? Codecs { get; init; }

    /// <summary>Notices from this query, delivered on the owner thread after
    /// translated entry. Callback exceptions preserve the healthy core lifetime.</summary>
    public Action<PgliteError>? OnNotice { get; init; }

    public IReadOnlyList<uint>? ParameterTypes
    {
        get => parameterTypes;
        init => parameterTypes = value is null ? null : Array.AsReadOnly(value.ToArray());
    }

    /// <summary>Alias matching the upstream QueryOptions.paramTypes name.</summary>
    public IReadOnlyList<uint>? ParamTypes
    {
        get => parameterTypes;
        init => parameterTypes = value is null ? null : Array.AsReadOnly(value.ToArray());
    }
}

/// <summary>A value with an explicit PostgreSQL type OID. Useful for custom types
/// and parameters whose type cannot be inferred from the statement.</summary>
public sealed record PgliteParameter(object? Value, uint TypeOid);
