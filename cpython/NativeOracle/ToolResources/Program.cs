using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 3)
    throw new ArgumentException("ToolResources <actual DotCC.Lib.dll> <resource-name/source-path JSON> <report JSON>");
var assembly = Path.GetFullPath(args[0]);
var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(args[1]))
    ?? throw new InvalidDataException("Missing resource/source specification");
if (expected.Count == 0)
    throw new InvalidDataException("Empty resource/source specification");
using var input = File.OpenRead(assembly);
using var pe = new PEReader(input);
var metadata = pe.GetMetadataReader();
var directory = pe.PEHeaders.CorHeader?.ResourcesDirectory
    ?? throw new InvalidDataException("Missing CLR resources");
var block = pe.GetSectionData(directory.RelativeVirtualAddress).GetContent(0, directory.Size).ToArray();
var rows = new List<object>();
var seen = new HashSet<string>(StringComparer.Ordinal);
foreach (var handle in metadata.ManifestResources)
{
    var resource = metadata.GetManifestResource(handle);
    var name = metadata.GetString(resource.Name);
    if (!name.StartsWith("DotCC.Runtime.", StringComparison.Ordinal) &&
        !name.StartsWith("DotCC.SystemHeaders.", StringComparison.Ordinal))
        continue;
    if (!seen.Add(name) || !expected.TryGetValue(name, out var source) || !resource.Implementation.IsNil)
        throw new InvalidDataException("Unexpected, duplicate or external compiler resource: " + name);
    var offset = checked((int)resource.Offset);
    if (offset < 0 || offset > block.Length - 4)
        throw new InvalidDataException("Invalid resource offset: " + name);
    var length = BitConverter.ToInt32(block, offset);
    if (length < 0 || length > block.Length - offset - 4)
        throw new InvalidDataException("Invalid resource extent: " + name);
    var payload = block.AsSpan(offset + 4, length);
    var sourceBytes = File.ReadAllBytes(source);
    if (!payload.SequenceEqual(sourceBytes))
        throw new InvalidDataException("Actual embedded resource differs from current source: " + name);
    rows.Add(new { resourceName = name, source = Path.GetFullPath(source),
        offsetWithinClrResources = offset, bytes = length,
        sha256 = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant() });
}
if (!seen.SetEquals(expected.Keys))
    throw new InvalidDataException("Missing expected embedded source/header resources: " +
        string.Join(", ", expected.Keys.Except(seen)));
var report = new {
    status = "passed exact actual compiler manifest-resource/source byte identity",
    compilerLibrary = assembly,
    compilerLibrarySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))).ToLowerInvariant(),
    compilerLibraryMvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid),
    clrResourcesRva = directory.RelativeVirtualAddress, clrResourcesBytes = directory.Size,
    runtimeResourceCount = seen.Count(n => n.StartsWith("DotCC.Runtime.", StringComparison.Ordinal)),
    headerResourceCount = seen.Count(n => n.StartsWith("DotCC.SystemHeaders.", StringComparison.Ordinal)),
    sources = rows,
    scope = "Actual compiler PE only; no emitted-product, native, interpreter or runtime-qualification claim"
};
File.WriteAllText(args[2], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"PASS compiler resources: {seen.Count} exact source/header resources; MVID {report.compilerLibraryMvid}");
