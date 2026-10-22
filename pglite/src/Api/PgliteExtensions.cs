// Manual .NET adaptation of @electric-sql/pglite src/pglite.ts extension lifecycle (Apache-2.0).
namespace Managed.PGlite;

/// <summary>The engine already contains the translated PL/pgSQL and Snowball modules.
/// Managed extensions provide .NET lifecycle and namespace objects. Native and Wasm
/// payloads cannot be loaded into the translated engine.</summary>
public enum PgliteExtensionKind { Managed, BuiltInTranslated, Native, Wasm }

public sealed record PgliteExtensionSetup(object? Namespace = null, Action? Initialize = null, Action? Close = null);

public sealed record PgliteExtension(string Name,
    Func<PgliteDatabase, PgliteExtensionSetup>? Setup = null,
    PgliteExtensionKind Kind = PgliteExtensionKind.Managed)
{
    public static PgliteExtension Live { get; } = new("live", database => new(database.Live));
    public static PgliteExtension Plpgsql { get; } = new("plpgsql", Kind: PgliteExtensionKind.BuiltInTranslated);
    public static PgliteExtension Snowball { get; } = new("snowball", Kind: PgliteExtensionKind.BuiltInTranslated);

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrEmpty(Name);
        if (Name.Contains('\0')) throw new ArgumentException("Extension names cannot contain NUL.");
        if (Kind is PgliteExtensionKind.Native or PgliteExtensionKind.Wasm)
            throw new NotSupportedException($"Extension '{Name}' requires a {Kind} payload. Only translated built-ins and managed .NET extensions are supported.");
        if (!Enum.IsDefined(Kind)) throw new ArgumentOutOfRangeException(nameof(Kind));
        if (Kind == PgliteExtensionKind.BuiltInTranslated && Name is not ("plpgsql" or "snowball"))
            throw new NotSupportedException($"'{Name}' is not a built-in translated extension.");
    }
}
