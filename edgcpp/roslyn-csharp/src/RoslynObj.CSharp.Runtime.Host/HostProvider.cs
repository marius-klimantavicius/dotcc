namespace RoslynObj.CSharp.Runtime.Host;

/// <summary>Explicit host provider identity. No native imports are implemented in T1.</summary>
public static class HostProvider
{
    public const string Name = "none";
    public static IReadOnlyList<string> NativeImports => Array.Empty<string>();
}
