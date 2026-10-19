using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

// JIT-only metadata reader. Inspected assemblies are never loaded or executed.
var framework = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (string path in ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "").Split(Path.PathSeparator))
    if (Path.GetFileNameWithoutExtension(path) != "Zstd.Audit") framework.Add(Path.GetFileNameWithoutExtension(path));
var contracts = new Dictionary<string, (string Name, string[] Additional)>
{
    ["product"] = ("TranslatedZstd", []),
    ["facade"] = ("Managed.Zstd", ["TranslatedZstd"]),
    ["consumer"] = ("ManagedConsumer", ["Managed.Zstd", "TranslatedZstd"]),
    ["harness"] = ("ManagedTests", ["Managed.Zstd", "TranslatedZstd"])
};
var hostLibraries = new HashSet<string>(["libc", "kernel32.dll", "psapi.dll"], StringComparer.OrdinalIgnoreCase);
var reports = new List<object>();
try
{
    if (args.Length == 0 || args.Length % 2 != 0) throw new ArgumentException("Zstd.Audit (ROLE ASSEMBLY)+");
    for (int i = 0; i < args.Length; i += 2)
    {
        string role = args[i], path = Path.GetFullPath(args[i + 1]);
        if (!contracts.TryGetValue(role, out var contract)) throw new ArgumentException("Unexpected audit role");
        using var file = File.OpenRead(path); using var pe = new PEReader(file);
        if (!pe.HasMetadata) throw new InvalidDataException("Missing managed metadata: " + path);
        var metadata = pe.GetMetadataReader();
        string assembly = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        if (assembly != contract.Name) throw new InvalidDataException("Unexpected assembly: " + assembly);
        var references = new List<string>();
        foreach (var handle in metadata.AssemblyReferences)
        {
            string name = metadata.GetString(metadata.GetAssemblyReference(handle).Name);
            if (!framework.Contains(name) && !contract.Additional.Contains(name, StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("Unapproved assembly reference: " + assembly + " -> " + name);
            references.Add(name);
        }
        foreach (var handle in metadata.TypeDefinitions)
        {
            string name = metadata.GetString(metadata.GetTypeDefinition(handle).Name);
            if (name is "DotCcImports" or "DotCcStaticImports") throw new InvalidDataException("Required native fallback present: " + name);
        }
        var imports = new List<object>();
        foreach (var handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);
            if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0) continue;
            var import = method.GetImport();
            string library = metadata.GetString(metadata.GetModuleReference(import.Module).Name), symbol = metadata.GetString(import.Name);
            string owner = TypeName(metadata, method.GetDeclaringType()), member = metadata.GetString(method.Name);
            bool privateStatic = (method.Attributes & (MethodAttributes.MemberAccessMask | MethodAttributes.Static)) == (MethodAttributes.Private | MethodAttributes.Static);
            bool hostMath = role == "product" && owner == "Managed.Zstd.Native.Zstd+Libc" && library == "libm.so.6"
                && (member, symbol) is ("PosixErf", "erf") or ("PosixErfc", "erfc")
                && metadata.GetBlobBytes(method.Signature) is [0x00, 0x01, 0x0d, 0x0d]
                && privateStatic && (int)import.Attributes == 0x140 && (method.ImplAttributes & MethodImplAttributes.PreserveSig) != 0;
            if (role != "product" || !(hostLibraries.Contains(library) || hostMath) || symbol.StartsWith("ZSTD_", StringComparison.Ordinal)
                || symbol.StartsWith("ZDICT_", StringComparison.Ordinal) || symbol.StartsWith("HUF_", StringComparison.Ordinal) || symbol.StartsWith("FSE_", StringComparison.Ordinal))
                throw new InvalidDataException("Unapproved native import: " + assembly + " -> " + library + "!" + symbol);
            imports.Add(new { library, symbol, owner, member });
        }
        reports.Add(new { role, path, assembly, references, imports });
    }
    Console.WriteLine(JsonSerializer.Serialize(reports, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }

static string TypeName(MetadataReader reader, TypeDefinitionHandle handle)
{
    var type = reader.GetTypeDefinition(handle); string name = reader.GetString(type.Name);
    var parent = type.GetDeclaringType();
    if (!parent.IsNil) return TypeName(reader, parent) + "+" + name;
    string ns = reader.GetString(type.Namespace); return ns.Length == 0 ? name : ns + "." + name;
}
