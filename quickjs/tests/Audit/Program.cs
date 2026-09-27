using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

// Read metadata without loading or executing the inspected assembly.
var framework = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (string path in ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "").Split(Path.PathSeparator))
    if (Path.GetFileNameWithoutExtension(path) != "QuickJs.Audit") framework.Add(Path.GetFileNameWithoutExtension(path));
var intended = new Dictionary<string, (string Assembly, string[] Additional)>
{
    ["product"] = ("TranslatedQuickJs", []),
    ["facade"] = ("Managed.QuickJs", ["TranslatedQuickJs"]),
    ["consumer"] = ("ManagedConsumer", ["Managed.QuickJs", "TranslatedQuickJs"]),
    ["harness"] = ("QuickJs.Behavior", ["Managed.QuickJs", "TranslatedQuickJs"])
};
var allowedNative = new HashSet<string>(["libc", "kernel32.dll", "psapi.dll"], StringComparer.OrdinalIgnoreCase);
var reports = new List<object>();
try
{
    if (args.Length == 0 || args.Length % 2 != 0) throw new ArgumentException("Usage: QuickJs.Audit (ROLE ASSEMBLY_PATH)+");
    for (int i = 0; i < args.Length; i += 2)
    {
        string role = args[i], path = Path.GetFullPath(args[i + 1]);
        if (!intended.TryGetValue(role, out var contract)) throw new ArgumentException("Unknown audit role: " + role);
        using var file = File.OpenRead(path);
        using var pe = new PEReader(file);
        if (!pe.HasMetadata) throw new InvalidOperationException("Managed metadata absent: " + path);
        MetadataReader metadata = pe.GetMetadataReader();
        if (!metadata.IsAssembly) throw new InvalidOperationException("Input is not an assembly: " + path);
        string assembly = metadata.GetString(metadata.GetAssemblyDefinition().Name);
        if (assembly != contract.Assembly) throw new InvalidOperationException($"Expected {contract.Assembly}, found {assembly}: {path}");
        var references = new List<string>();
        foreach (AssemblyReferenceHandle handle in metadata.AssemblyReferences)
        {
            var reference = metadata.GetAssemblyReference(handle);
            string name = metadata.GetString(reference.Name);
            if (!framework.Contains(name) && !contract.Additional.Contains(name, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Non-BCL/unapproved assembly reference {assembly} -> {name}: {path}");
            references.Add(name);
        }
        var imports = new List<object>();
        foreach (TypeDefinitionHandle handle in metadata.TypeDefinitions)
        {
            string name = metadata.GetString(metadata.GetTypeDefinition(handle).Name);
            if (name is "DotCcImports" or "DotCcStaticImports")
                throw new InvalidOperationException($"Compiler-emitted required native import bindings present: {name}: {path}");
        }
        foreach (MethodDefinitionHandle handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);
            if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0) continue;
            MethodImport import = method.GetImport();
            string library = metadata.GetString(metadata.GetModuleReference(import.Module).Name);
            string symbol = metadata.GetString(import.Name);
            if (!allowedNative.Contains(library)) throw new InvalidOperationException($"Unapproved P/Invoke {assembly} -> {library}!{symbol}: {path}");
            imports.Add(new { library, symbol });
        }
        reports.Add(new { role, path, assembly, references = references.Order().ToArray(), imports });
    }
    Console.WriteLine(JsonSerializer.Serialize(reports, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine("AUDIT FAIL " + error.Message);
    return 1;
}
