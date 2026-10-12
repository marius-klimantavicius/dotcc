using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace RoslynObj;

public sealed partial class RoslynObject
{
    public const int SupportedFormatMajor = 1;
    public const int SupportedFormatMinor = 0;
    private static readonly IReadOnlyDictionary<long, SemanticKind> catalog = ReadCatalog();
    private static readonly HashSet<string> supportedCapabilities =
    ["normalized_semantic_graph_v1", "exact_target_values_v1", "edg_partial_eh_v1", "source_semantic_graph_v1"];
    public TranslationUnit TranslationUnit { get; private set; } = null!;

    public static RoslynObject Open(string path, ReadOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        options ??= new();
        options.Validate();
        var store = new SnapshotStore(path, options);
        try
        {
            var header = store.Required("object_header", 1);
            if (header.Int("format_major") != SupportedFormatMajor || header.Int("format_minor") > SupportedFormatMinor)
                throw new IncompatibleObjectException("This RoslynObj format version is not supported.");
            if (!header.Bool("complete")) throw new IncompatibleObjectException("The object has no complete semantic snapshot.");
            foreach (var kind in store.Query("SELECT * FROM kind ORDER BY id"))
                if (catalog.TryGetValue(kind.Long("id"), out var expected)
                    && (expected.Family != kind.Text("family") || expected.Name != kind.Text("name")))
                    throw new IncompatibleObjectException("The object kind catalog is incompatible with this reader.");
            foreach (var capability in store.Query("SELECT t.value FROM build_capability c JOIN text_atom t ON t.id=c.name_id WHERE c.required=1 AND COALESCE(c.enabled,1)<>0"))
                if (!supportedCapabilities.Contains(capability.Text("value")))
                    throw new IncompatibleObjectException($"Required capability '{capability.Text("value")}' is unsupported.");
            string Atom(string name) => store.Required("text_atom", header.Long(name)).Text("value");
            var target = store.Required("target_profile", header.Long("target_profile_id"));
            var profile = catalog[header.Long("capture_profile_id")].Name;
            var manifest = new ObjectManifest(header.Int("format_major"), header.Int("format_minor"),
                header.Text("artifact_id"), Atom("producer_id"), Atom("revision_id"), Atom("edg_version_id"),
                profile, profile == "source_semantic" ? "source_semantic" : "lowered",
                new(catalog[target.Long("abi_id")].Name, target.Int("addressable_unit_bits"),
                    target.Bool("little_endian") ? "little" : "big", target.Long("pointer_bits"),
                    BigInteger.Parse(target.Text("pointer_alignment"), CultureInfo.InvariantCulture)));
            var result = new RoslynObject(store, manifest, options);
            result.TranslationUnit = result.Unit(header.Long("translation_unit_id"));
            return result;
        }
        catch { store.Dispose(); throw; }
    }

    internal static SemanticKind CatalogKind(long id) => catalog.TryGetValue(id, out var kind) ? kind
        : throw new IncompatibleObjectException("A required semantic kind is not supported by this reader.");
    internal SemanticKind Kind(long id) => CatalogKind(id);
    internal SemanticKind? OptionalKind(long? id) => id is null ? null : Kind(id.Value);
    internal T? Link<T>(long? id, Func<long, T> resolve) where T : class => id is null ? null : resolve(id.Value);
    internal T Detail<T>(string table, long id, Func<Row, T> create) where T : SemanticDetail => create(Store.Required(table, id));
    internal string? Name(long? id) => id is null ? null : Text(Store.Required("name", id.Value).OptionalLong("spelling_id"));
    internal static BigInteger Number(string value) => BigInteger.Parse(value, CultureInfo.InvariantCulture);

    private static IReadOnlyDictionary<long, SemanticKind> ReadCatalog()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoslynObj.kinds.tsv")!;
        using var reader = new StreamReader(stream);
        var result = new Dictionary<long, SemanticKind>();
        reader.ReadLine();
        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split('\t');
            var id = int.Parse(fields[0], CultureInfo.InvariantCulture);
            result.Add(id, new(fields[1], id, fields[2]));
        }
        return result;
    }
}
