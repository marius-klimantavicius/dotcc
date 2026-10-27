namespace DotCC;

/// <summary>Runtime source included in final C# output. Auto follows source/object language provenance.</summary>
public enum RuntimeProfile { All, C, Auto }

/// <summary>C# emission and final output options. Layout options are normally set at link time for object fragments.</summary>
/// <param name="ExportEnum">Exact/glob selectors for file-scope C enumerators, exported as underlying integer constants.</param>
/// <param name="DeduplicateStatic">Merge statically bound functions with equivalent typed proofs, including inline functions.</param>
/// <param name="AggregateData">Opt in to immutable byte payloads for fully constant aggregates of at least 1024 bytes
/// in the LP64 little-endian target layout. Destinations retain independent, writable storage.
/// Set at source emission (including EmitObject); already emitted objects cannot be optimized at link time.</param>
public sealed record CSharpOutputOptions(bool NestTypes = false, RuntimeProfile Runtime = RuntimeProfile.All,
    bool DeduplicateInline = false, System.Collections.Generic.IReadOnlyList<string>? ExportInline = null,
    bool LiteralPool = false, bool StateContext = false,
    bool InstanceMethods = false, System.Collections.Generic.IReadOnlyList<string>? ExportEnum = null,
    bool DeduplicateStatic = false, bool PruneUnusedInline = false, bool AggregateData = false);
