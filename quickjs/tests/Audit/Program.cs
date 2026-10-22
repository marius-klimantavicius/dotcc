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
if (args is ["--self-test-host-math"])
{
    byte[] signature = [0x00, 0x01, 0x0d, 0x0d]; // static double(double)
    const string owner = "Managed.Interpreters.QuickJs+Libc";
    foreach (var (member, symbol) in new[] { ("PosixErf", "erf"), ("PosixErfc", "erfc") })
    {
        if (!HostMath("product", owner, member, "libm.so.6", symbol, signature, true, 0x140, true))
            throw new InvalidOperationException("Exact host math binding rejected");
    }
    bool[] rejected = [
        HostMath("consumer", owner, "PosixErf", "libm.so.6", "erf", signature, true, 0x140, true),
        HostMath("product", owner + "Other", "PosixErf", "libm.so.6", "erf", signature, true, 0x140, true),
        HostMath("product", owner, "Other", "libm.so.6", "erf", signature, true, 0x140, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erfc", signature, true, 0x140, true),
        HostMath("product", owner, "PosixErf", "other.so", "erf", signature, true, 0x140, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", [0x00, 0x01, 0x0c, 0x0d], true, 0x140, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", [0x20, 0x01, 0x0d, 0x0d], true, 0x140, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", signature, false, 0x140, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", signature, true, 0x100, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", signature, true, 0x240, true),
        HostMath("product", owner, "PosixErf", "libm.so.6", "erf", signature, true, 0x140, false)];
    if (rejected.Any(value => value)) throw new InvalidOperationException("Inexact host math binding accepted");
    Console.WriteLine("PASS exact shared host math policy: 2 positive, 11 negative controls");
    foreach (var (member, symbol, shape) in new (string, string, byte[])[] {
        ("BackendNativeLgamma", "lgamma_r", [0x00, 0x02, 0x0d, 0x0d, 0x0f, 0x08]),
        ("BackendNativeTgamma", "tgamma", [0x00, 0x01, 0x0d, 0x0d]) })
    {
        if (!HostMath("product", owner, member, "libm.so.6", symbol, shape, true, 0x140, true))
            throw new InvalidOperationException("Exact shared gamma binding rejected");
        byte[] wrongReturn = (byte[])shape.Clone(), wrongParameter = (byte[])shape.Clone(),
            wrongCount = (byte[])shape.Clone(), instance = (byte[])shape.Clone();
        wrongReturn[2] = 0x0c;
        wrongParameter[3] = 0x08;
        wrongCount[1]++;
        instance[0] = 0x20;
        byte[] wrongShape = member == "BackendNativeLgamma"
            ? [0x00, 0x02, 0x0d, 0x0d, 0x0f, 0x09] // uint*, not int*
            : [0x00, 0x02, 0x0d, 0x0d, 0x0f, 0x08]; // unexpected second argument
        bool[] gammaRejected = [
            HostMath("consumer", owner, member, "libm.so.6", symbol, shape, true, 0x140, true),
            HostMath("product", owner + "Other", member, "libm.so.6", symbol, shape, true, 0x140, true),
            HostMath("product", owner, member + "Other", "libm.so.6", symbol, shape, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol + "Other", shape, true, 0x140, true),
            HostMath("product", owner, member, "other.so", symbol, shape, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, wrongReturn, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, wrongParameter, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, wrongCount, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, instance, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, wrongShape, true, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, shape, false, 0x140, true),
            HostMath("product", owner, member, "libm.so.6", symbol, shape, true, 0x100, true),
            HostMath("product", owner, member, "libm.so.6", symbol, shape, true, 0x240, true),
            HostMath("product", owner, member, "libm.so.6", symbol, shape, true, 0x140, false)];
        if (gammaRejected.Any(value => value)) throw new InvalidOperationException("Inexact shared gamma binding accepted");
    }
    Console.WriteLine("PASS exact shared gamma policy: 2 positive, 28 negative controls");
    return 0;
}
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
            string owner = TypeName(metadata, method.GetDeclaringType());
            string member = metadata.GetString(method.Name);
            byte[] signature = metadata.GetBlobBytes(method.Signature);
            bool privateStatic = (method.Attributes & (MethodAttributes.MemberAccessMask | MethodAttributes.Static))
                == (MethodAttributes.Private | MethodAttributes.Static);
            bool hostMath = HostMath(role, owner, member, library, symbol, signature, privateStatic,
                (int)import.Attributes, (method.ImplAttributes & MethodImplAttributes.PreserveSig) != 0);
            if (!allowedNative.Contains(library) && !hostMath)
                throw new InvalidOperationException($"Unapproved P/Invoke {assembly} -> {library}!{symbol} ({owner}.{member}, signature={Convert.ToHexString(signature)}, importAttributes=0x{(int)import.Attributes:X}): {path}");
            imports.Add(new { library, symbol, declaring_type = owner, method = member,
                signature = Convert.ToHexString(signature), import_attributes = (int)import.Attributes,
                shared_host_math = hostMath });
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

static string TypeName(MetadataReader metadata, TypeDefinitionHandle handle)
{
    var type = metadata.GetTypeDefinition(handle);
    string name = metadata.GetString(type.Name);
    var parent = type.GetDeclaringType();
    if (!parent.IsNil) return TypeName(metadata, parent) + "+" + name;
    string ns = metadata.GetString(type.Namespace);
    return ns.Length == 0 ? name : ns + "." + name;
}

static bool HostMath(string role, string owner, string member, string library, string symbol,
    ReadOnlySpan<byte> signature, bool privateStatic, int importAttributes, bool preserveSig) =>
    role == "product" && owner == "Managed.Interpreters.QuickJs+Libc" && library == "libm.so.6"
    && (((member, symbol) is ("PosixErf", "erf") or ("PosixErfc", "erfc") or ("BackendNativeTgamma", "tgamma")
            && signature is [0x00, 0x01, 0x0d, 0x0d])
        || ((member, symbol) is ("BackendNativeLgamma", "lgamma_r")
            && signature is [0x00, 0x02, 0x0d, 0x0d, 0x0f, 0x08]))
    && privateStatic && preserveSig
    // Exact source DllImport defaults: Winapi (0x100) plus SetLastError (0x40).
    && importAttributes == 0x140;
