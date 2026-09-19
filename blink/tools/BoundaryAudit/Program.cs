using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length is not (2 or 4) || (args.Length == 4 && args[2] != "--max-methods"))
    throw new ArgumentException("BoundaryAudit <ManagedCore.dll> <report.json> [--max-methods count]");
int methodBudget = args.Length == 4 ? int.Parse(args[3]) : 100_000;
if (methodBudget <= 0) throw new ArgumentOutOfRangeException(nameof(methodBudget));
string binary = Path.GetFullPath(args[0]), reportPath = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
// A failed load must not leave an older successful report at the requested path.
File.Delete(reportPath);
var context = new AuditLoadContext(binary);
Assembly assembly = context.LoadFromAssemblyPath(binary);
Type container = assembly.GetType("Managed.Emulation.BlinkCore", true)!;
const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var scoped = new HashSet<Assembly>();
var inputHashes = new Dictionary<string, string>(StringComparer.Ordinal);
var queue = new Queue<MethodBase>();
var seen = new HashSet<(Guid Module, int Token)>();
var roots = new SortedSet<string>();
var runtimeEntries = new SortedSet<string>();
var externalCalls = new SortedSet<string>();
var nativeCalls = new SortedSet<string>();
var declarations = new SortedSet<string>();
var unresolved = new List<object>();
var errors = new List<object>();
var indirect = new SortedSet<string>();
var virtualSites = new SortedSet<string>();
var stateMachines = new SortedSet<string>();
string Name(MethodBase method) => method.Module.Assembly.GetName().Name + "!" +
    (method.DeclaringType?.FullName ?? "<global>") + "::" + method;
string Site(MethodBase method, int offset) => Name(method) + " @ IL_" + offset.ToString("x4");
string? Import(MethodBase method)
{
    // DllImportAttribute is a sealed framework pseudo-attribute, not an authored
    // target constructor. No target custom attributes are instantiated.
    if (method.GetCustomAttribute<DllImportAttribute>() is not { } import) return null;
    return Name(method) + " => " + import.Value + "!" + (import.EntryPoint ?? method.Name);
}
void Root(MethodBase method)
{
    roots.Add(Name(method)); queue.Enqueue(method);
}
void Scope(Assembly target)
{
    if (!scoped.Add(target)) return;
    foreach (Type type in target.GetTypes())
    {
        if (type.TypeInitializer is { } initializer) Root(initializer);
        foreach (MethodInfo method in type.GetMethods(All))
            if (Import(method) is { } import) declarations.Add(import);
    }
    foreach (Module module in target.GetModules())
    {
        foreach (MethodInfo method in module.GetMethods(All))
            if (Import(method) is { } import) declarations.Add(import);
        // Reflection GetTypes omits <Module>. Read its actual .cctor token,
        // then resolve metadata only; never invoke a module initializer.
        byte[] image = File.ReadAllBytes(module.FullyQualifiedName);
        inputHashes[module.FullyQualifiedName] = Convert.ToHexStringLower(SHA256.HashData(image));
        using var stream = new MemoryStream(image, writable: false);
        using var pe = new PEReader(stream);
        MetadataReader metadata = pe.GetMetadataReader();
        if (metadata.GetGuid(metadata.GetModuleDefinition().Mvid) != module.ModuleVersionId)
            throw new InvalidDataException("Scoped module changed between load and inspection");
        foreach (TypeDefinitionHandle handle in metadata.TypeDefinitions)
        {
            TypeDefinition type = metadata.GetTypeDefinition(handle);
            if (metadata.GetString(type.Name) != "<Module>") continue;
            foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
            {
                if (metadata.GetString(metadata.GetMethodDefinition(methodHandle).Name) != ".cctor") continue;
                Root(module.ResolveMethod(MetadataTokens.GetToken(methodHandle))
                     ?? throw new InvalidDataException("Unresolved module initializer"));
            }
        }
    }
}
Scope(assembly);
// The Host assembly's field accesses can trigger constructors without any call
// edge to them. Include its complete initializer set when it is referenced.
foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
    if (reference.Name == "Managed.Emulation.Host") Scope(context.LoadFromAssemblyName(reference));
foreach (MethodInfo method in container.GetMethods(All)) Root(method);
bool InScope(Assembly candidate)
{
    if (candidate == assembly) return true;
    if (candidate.GetName().Name != "Managed.Emulation.Host") return false;
    if (AssemblyLoadContext.GetLoadContext(candidate) != context)
        throw new InvalidDataException("Host dependency escaped the isolated audit load context");
    Scope(candidate); return true;
}
void Edge(MethodBase caller, MethodBase target)
{
    if (caller.DeclaringType == container && target.DeclaringType?.Name == "Libc") runtimeEntries.Add(Name(target));
    if (Import(target) is { } import) nativeCalls.Add(import);
    if (InScope(target.Module.Assembly)) queue.Enqueue(target);
    else externalCalls.Add(Name(target));
}
while (queue.TryDequeue(out var candidate))
{
    var key = (candidate.Module.ModuleVersionId, candidate.MetadataToken);
    if (seen.Contains(key)) continue;
    if (seen.Count >= methodBudget)
    {
        errors.Add(new { caller = Name(candidate), error = "Method budget exhausted", methodBudget }); break;
    }
    seen.Add(key);
    // Inspect each definition once. Expanding Foo<T> -> Foo<List<T>> otherwise
    // creates an unbounded graph despite having only one finite IL body.
    MethodBase method = candidate.Module.ResolveMethod(candidate.MetadataToken) ?? candidate;
    if (Import(method) is { } import) nativeCalls.Add(import); // includes root imports
    MethodBody? body;
    try
    {
        foreach (CustomAttributeData attribute in method.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName is not
                ("System.Runtime.CompilerServices.AsyncStateMachineAttribute" or
                 "System.Runtime.CompilerServices.IteratorStateMachineAttribute" or
                 "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute")) continue;
            if (attribute.ConstructorArguments.Count != 1 || attribute.ConstructorArguments[0].Value is not Type state)
                throw new InvalidDataException("Invalid state-machine metadata");
            stateMachines.Add(Name(method) + " => " + state.FullName);
            foreach (MethodBase member in state.GetMethods(All).Cast<MethodBase>().Concat(state.GetConstructors(All)))
                if (InScope(member.Module.Assembly)) queue.Enqueue(member);
        }
        body = method.GetMethodBody();
    }
    catch (Exception error) { unresolved.Add(new { caller = Name(method), error = error.Message }); continue; }
    if (body?.GetILAsByteArray() is not { } bytes) continue;
    int position = 0;
    while (position < bytes.Length)
    {
        int offset = position;
        OpCode op;
        int operand, size;
        try { (op, operand, size) = IlDecoder.Read(bytes, ref position); }
        catch (InvalidDataException error)
        {
            errors.Add(new { caller = Name(method), offset, error = error.Message }); break;
        }
        if (op == OpCodes.Calli) indirect.Add(Site(method, offset));
        if (op == OpCodes.Callvirt || op == OpCodes.Ldvirtftn) virtualSites.Add(Site(method, offset));
        if (op.OperandType == OperandType.InlineMethod)
        {
            int token = BitConverter.ToInt32(bytes, operand);
            try
            {
                var target = method.Module.ResolveMethod(token, method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null);
                if (target != null) Edge(method, target);
                else unresolved.Add(new { caller = Name(method), offset, token, error = "No resolved method" });
            }
            catch (Exception error) { unresolved.Add(new { caller = Name(method), offset, token, error = error.Message }); }
        }
    }
}
foreach (var input in inputHashes)
    if (!StringComparer.Ordinal.Equals(input.Value, Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(input.Key)))))
        errors.Add(new { path = input.Key, error = "Scoped module changed during inspection" });
var identities = scoped.OrderBy(a => a.FullName).Select(a => new
{
    identity = a.FullName, path = a.Location,
    sha256 = inputHashes[a.Location],
    modules = a.GetModules().Select(m => new { path = m.FullyQualifiedName, m.ModuleVersionId,
        sha256 = inputHashes[m.FullyQualifiedName] }).ToArray()
}).ToArray();
bool complete = errors.Count == 0 && unresolved.Count == 0;
var report = new
{
    kind = "conservative-static-managed-boundary-inventory", complete, binary,
    sha256 = inputHashes[assembly.Location],
    scopedAssemblies = identities,
    scope = "All declared BlinkCore methods plus type/module initializers in the main and referenced Managed.Emulation.Host assemblies. Direct IL calls/ldftn recursively include these assemblies; attributed async/iterator state-machine methods are included. Target code and initializers are never invoked.",
    limitations = new[] {
        "calli targets, virtual overrides, ordinary delegate/event dispatch, reflection and dynamic code are not resolved; this inventory alone cannot prove runtime isolation.",
        "Async/iterator expansion follows standard metadata attributes only; arbitrary scheduler callbacks and unattributed state machines are not inferred.",
        "Generic method/type definitions are inspected once; concrete generic dispatch specializations are not resolved.",
        "External framework assemblies are boundary targets, not recursively audited. Runtime native declarations and traversed targets are separate inventories.",
        "All top-level translated methods are roots, including unused guest syscall paths. A complete inventory is not an isolation pass." },
    rootCount = roots.Count, roots, methodBudget, inspectedMethodCount = seen.Count,
    directRuntimeEntries = runtimeEntries, externalManagedCalls = externalCalls,
    traversedNativeImports = nativeCalls, allNativeDeclarations = declarations,
    indirectCallSites = indirect, virtualCallSites = virtualSites,
    stateMachineExpansions = stateMachines, unresolved, errors
};
File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Inspected {seen.Count} method definitions; native targets {nativeCalls.Count}, indirect sites {indirect.Count}, unresolved {unresolved.Count}, errors {errors.Count}; complete={complete}.");
Environment.ExitCode = complete ? 0 : 1;

sealed class AuditLoadContext : AssemblyLoadContext
{
    private readonly string directory;
    private readonly AssemblyDependencyResolver resolver;
    public AuditLoadContext(string path) : base("BoundaryAudit", isCollectible: true)
    { directory = Path.GetDirectoryName(path)!; resolver = new(path); }
    protected override Assembly? Load(AssemblyName name)
    {
        // Resolve local inputs before default-context fallback. The Host
        // dependency must never silently bind to an unrelated loaded copy.
        string? path = resolver.ResolveAssemblyToPath(name);
        string local = Path.Combine(directory, name.Name + ".dll");
        if (path == null && File.Exists(local)) path = local;
        if (name.Name == "Managed.Emulation.Host")
        {
            if (path == null) throw new FileNotFoundException("Missing scoped Host assembly", local);
            var actual = AssemblyName.GetAssemblyName(path);
            if (!StringComparer.Ordinal.Equals(actual.FullName, name.FullName))
                throw new FileLoadException("Host assembly identity differs from reference: " + name.FullName, path);
        }
        return path == null ? null : LoadFromAssemblyPath(path);
    }
}
