using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using DotCC.FunctionalTests;

if (args.Length != 5)
    throw new ArgumentException("Usage: audit <Release.dll> <generated-source-dir> <before.json> <output.json> <maximum-helper-parameters>");

string assemblyPath = Path.GetFullPath(args[0]);
string sourceDirectory = Path.GetFullPath(args[1]);
int maximum = int.Parse(args[4], System.Globalization.CultureInfo.InvariantCulture);
byte[] image = File.ReadAllBytes(assemblyPath);
var context = new AssemblyLoadContext("aggregate-product-audit", isCollectible: true);
try
{
    var assembly = context.LoadFromAssemblyPath(assemblyPath);
    if (assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true)
        throw new InvalidOperationException("Getter audit requires an optimized Release assembly");
    var factories = assembly.GetTypes().Where(type => type.Name.StartsWith("__DotccAggregate", StringComparison.Ordinal))
        .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName)
            .Select(method => new {
                type = type.FullName, method = method.Name, parameters = method.GetParameters().Length,
                il_bytes = method.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0
            }))
        .OrderByDescending(method => method.parameters).ThenBy(method => method.type, StringComparer.Ordinal).ToArray();
    // Warm the entire emitted getter set before the acceptance measurement.
    // Per-getter warmup alone does not finish process-wide tier promotion.
    var warmupPayloads = AggregateFactoryAudit.Inspect(image, assembly);
    var payloads = AggregateFactoryAudit.Inspect(image, assembly);
    var factoryBodies = AggregateFactoryAudit.InspectFactoryIl(assembly);
    var factoryMeasurements = AggregateFactoryAudit.InspectFactories(assembly);
    int maxParameters = factories.Select(method => method.parameters).DefaultIfEmpty().Max();
    string[] sources = Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.TopDirectoryOnly);
    using var before = JsonDocument.Parse(File.ReadAllBytes(args[2]));
    var report = new {
        assembly = assemblyPath,
        assembly_sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image)),
        source_directory = sourceDirectory,
        source_files = sources.Length,
        source_bytes = sources.Sum(path => new FileInfo(path).Length),
        main_bytes = new FileInfo(Path.Combine(sourceDirectory, "GhosttyVt.cs")).Length,
        before = before.RootElement.Clone(),
        helper_method_count = factories.Length,
        helper_type_count = factories.Select(method => method.type).Distinct().Count(),
        max_parameters = maxParameters,
        maximum_helper_il_bytes = factories.Select(method => method.il_bytes).DefaultIfEmpty().Max(),
        maximum_allowed_parameters = maximum,
        bounded_parameters = maxParameters <= maximum,
        factories,
        global_getter_warmup_rounds = 1,
        global_getter_warmup_runtime_allocated_bytes = warmupPayloads.Sum(payload => payload.AllocatedBytesAfterWarmup),
        payload_getters = payloads.Count,
        getter_logical_bytes = payloads.Sum(payload => payload.Bytes),
        maximum_getter_il_bytes = payloads.Select(payload => payload.IlBytes).DefaultIfEmpty().Max(),
        getters = payloads,
        zero_argument_factory_probes = factoryMeasurements.Count,
        zero_argument_factory_probes_passed = factoryMeasurements.Count(factory => factory.Status == "pass"),
        zero_argument_factory_probes_not_passed = factoryMeasurements.Count(factory => factory.Status != "pass"),
        factory_measurements = factoryMeasurements,
        factory_helper_il = factoryBodies,
        runtime_tiering = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default",
        measurement_code = "Auditor measurement methods use NoInlining|AggressiveOptimization before allocation counting; emitted getter/factory attributes and process tiering unchanged",
        allocation_measurement = "JIT: 256 warmup iterations; 10000 reads/copies per getter; 1000 factory calls for >=4096-byte values, otherwise 10000; every returned byte consumed in no-inline checksum. Reflection/delegates/destinations/type initialization excluded",
        factory_probe_scope = "Every zero-argument Create with closed unmanaged return <=1MiB; unsupported/errors explicit and fail audit; factories taking Args are IL/callgraph audited but not dynamically invoked",
        il_policy = "No newarr, ordinary reference constructors, known native/managed allocation service calls, unresolved/indirect calls in factory/helper graph. Exception constructors are separately recorded guarded failure paths; successful executed paths must allocate zero bytes",
        qualification = "PE RVA and executed JIT getter/factory audit; not NativeAOT execution or terminal qualification"
    };
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[3]))!);
    File.WriteAllText(args[3], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    AggregateFactoryAudit.RequireEmbeddedAllocationFree(payloads);
    AggregateFactoryAudit.RequireAllocationFreeFactories(factoryMeasurements, factoryBodies);
    if (maxParameters > maximum)
        throw new InvalidOperationException($"Aggregate helper parameters {maxParameters} exceed bound {maximum}");
    Console.WriteLine($"factory audit: {factories.Length} methods, max parameters {maxParameters}; {payloads.Count} RVA getters and {factoryMeasurements.Count} zero-argument factories allocation-free PASS");
}
finally { context.Unload(); }
