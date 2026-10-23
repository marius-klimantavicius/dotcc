using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedConsumer;

internal sealed record RuntimeIdentity(string kind, string framework, string version, string rid,
    string process_path, bool dynamic_code, string runtime_directory, string[] mapped_files);

internal static class ExecutionIdentity
{
    internal static void Print()
    {
        bool dynamicCode = RuntimeFeature.IsDynamicCodeCompiled;
        string[] mapped = OperatingSystem.IsLinux()
            ? File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries).Last())
                .Where(path => path.StartsWith('/') && File.Exists(path)).Distinct().Order().ToArray()
            : [];
        var identity = new RuntimeIdentity("actual-runtime-identity", RuntimeInformation.FrameworkDescription,
            Environment.Version.ToString(), RuntimeInformation.RuntimeIdentifier, Environment.ProcessPath ?? "",
            dynamicCode, dynamicCode ? RuntimeEnvironment.GetRuntimeDirectory() : "", mapped);
        Console.WriteLine(JsonSerializer.Serialize(identity, IdentityJson.Default.RuntimeIdentity));
    }
}

[JsonSerializable(typeof(RuntimeIdentity))]
internal partial class IdentityJson : JsonSerializerContext { }
