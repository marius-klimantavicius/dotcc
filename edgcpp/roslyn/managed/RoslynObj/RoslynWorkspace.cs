namespace RoslynObj;

/// <summary>A collection of independent snapshots. It performs no cross-unit C++ resolution.</summary>
public sealed class RoslynWorkspace : IDisposable
{
    private readonly IReadOnlyList<RoslynObject> objects;
    private RoslynWorkspace(IReadOnlyList<RoslynObject> objects) => this.objects = objects;
    public IReadOnlyList<RoslynObject> Objects => objects;
    public IReadOnlyList<TranslationUnit> TranslationUnits => Array.AsReadOnly(objects.Select(o => o.TranslationUnit).ToArray());
    public static RoslynWorkspace Open(IEnumerable<string> paths, ReadOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(paths);
        var result = new List<RoslynObject>();
        try
        {
            foreach (var path in paths) result.Add(RoslynObject.Open(path, options));
            return new(Array.AsReadOnly(result.ToArray()));
        }
        catch { foreach (var obj in result) obj.Dispose(); throw; }
    }
    public void Dispose() { foreach (var obj in objects) obj.Dispose(); }
}
