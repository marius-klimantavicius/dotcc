using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DotCC;

/// <summary>A module-local import alias, corresponding to Zig's --dep alias=module.</summary>
public sealed record ZigModuleDependency(string Name, string Module);

/// <summary>A Zig module root and its own named dependencies. Relative file imports retain this scope.</summary>
public sealed record ZigModuleDefinition(string Name, string Root, IReadOnlyList<ZigModuleDependency> Dependencies);

/// <summary>The original Zig compilation's output kind, independently of DotCC object splitting.</summary>
public enum ZigSourceOutputMode { Exe, Lib, Obj }

/// <summary>The native Zig source optimization context; does not select C# code generation flags.</summary>
public enum ZigSourceOptimizeMode { Debug, Safe, Fast, Small }

/// <summary>Immutable, invocation-local Zig module configuration; independent of C preprocessing.</summary>
public sealed class ZigCompilationOptions
{
    public string RootModule { get; }
    public string? ZigLibDir { get; }
    public IReadOnlyList<ZigModuleDefinition> Modules { get; }
    public ZigSourceOutputMode? OutputMode { get; }
    public ZigSourceOptimizeMode? OptimizeMode { get; }
    private readonly IReadOnlyDictionary<string, ZigModuleDefinition> _modules;

    public ZigCompilationOptions(string rootModule, IReadOnlyList<ZigModuleDefinition> modules, string? zigLibDir = null,
        ZigSourceOutputMode? outputMode = null, ZigSourceOptimizeMode? optimizeMode = null)
    {
        if (outputMode is { } mode && !Enum.IsDefined(mode)) throw Error("unsupported output_mode");
        if (optimizeMode is { } optimize && !Enum.IsDefined(optimize)) throw Error("unsupported optimize_mode");
        OutputMode = outputMode;
        OptimizeMode = optimizeMode;
        RootModule = Required(rootModule, "root_module");
        ZigLibDir = zigLibDir is null ? null : Path.GetFullPath(Required(zigLibDir, "zig_lib_dir"));
        var map = new Dictionary<string, ZigModuleDefinition>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            var name = Required(module.Name, "module name");
            var aliases = new HashSet<string>(StringComparer.Ordinal);
            var dependencies = module.Dependencies.Select(dep =>
            {
                var alias = Required(dep.Name, "dependency name");
                if (alias is "std" or "builtin" or "root") throw Error($"reserved dependency alias '{alias}'");
                if (!aliases.Add(alias)) throw Error($"duplicate dependency '{alias}' in module '{name}'");
                return new ZigModuleDependency(alias, Required(dep.Module, "dependency module"));
            }).ToArray();
            if (!map.TryAdd(name, new(name, Path.GetFullPath(Required(module.Root, "module root")), Array.AsReadOnly(dependencies))))
                throw Error($"duplicate module '{name}'");
        }
        if (!map.ContainsKey(RootModule)) throw Error($"unknown root module '{RootModule}'");
        var roots = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var module in map.Values)
        {
            if (!roots.Add(module.Root)) throw Error($"file '{module.Root}' is the root of more than one module");
            foreach (var dep in module.Dependencies)
                if (!map.ContainsKey(dep.Module)) throw Error($"module '{module.Name}' dependency '{dep.Name}' refers to unknown module '{dep.Module}'");
        }
        _modules = map;
        Modules = Array.AsReadOnly(map.Values.ToArray());
    }

    internal ZigModuleDefinition GetModule(string name) => _modules.TryGetValue(name, out var module)
        ? module : throw Error($"unknown module '{name}'");

    internal ZigModuleDefinition? Dependency(string owner, string alias)
    {
        var dep = GetModule(owner).Dependencies.FirstOrDefault(x => x.Name == alias);
        return dep is null ? null : GetModule(dep.Module);
    }

    /// <summary>Read a strict version-one graph. Paths are relative to this file, never the current directory.</summary>
    public static ZigCompilationOptions Load(string path)
    {
        var full = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(full)!;
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(full));
            var root = json.RootElement;
            Fields(root, "schema_version", "root_module", "zig_lib_dir", "modules", "output_mode", "optimize_mode");
            if (root.GetProperty("schema_version").GetInt32() != 1) throw Error("unsupported schema_version");
            var modules = new List<ZigModuleDefinition>();
            foreach (var module in root.GetProperty("modules").EnumerateArray())
            {
                Fields(module, "name", "root", "dependencies");
                var deps = new List<ZigModuleDependency>();
                if (module.TryGetProperty("dependencies", out var dependencies))
                    foreach (var dep in dependencies.EnumerateArray())
                    {
                        Fields(dep, "name", "module");
                        deps.Add(new(Text(dep, "name"), Text(dep, "module")));
                    }
                modules.Add(new(Text(module, "name"), Path.GetFullPath(Text(module, "root"), directory), deps));
            }
            var lib = root.TryGetProperty("zig_lib_dir", out var libNode) && libNode.ValueKind != JsonValueKind.Null
                ? Path.GetFullPath(Required(libNode.GetString()!, "zig_lib_dir"), directory) : null;
            ZigSourceOutputMode? mode = root.TryGetProperty("output_mode", out var output) ? output.GetString() switch
            {
                "Exe" => ZigSourceOutputMode.Exe,
                "Lib" => ZigSourceOutputMode.Lib,
                "Obj" => ZigSourceOutputMode.Obj,
                _ => throw Error("output_mode must be Exe, Lib or Obj"),
            } : null;
            ZigSourceOptimizeMode? optimizeMode = root.TryGetProperty("optimize_mode", out var optimize) ? optimize.GetString() switch
            {
                "debug" => ZigSourceOptimizeMode.Debug,
                "safe" => ZigSourceOptimizeMode.Safe,
                "fast" => ZigSourceOptimizeMode.Fast,
                "small" => ZigSourceOptimizeMode.Small,
                _ => throw Error("optimize_mode must be debug, safe, fast or small"),
            } : null;
            return new(Text(root, "root_module"), modules, lib, mode, optimizeMode);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException)
        {
            throw Error($"invalid module graph '{full}': {ex.Message}");
        }
    }

    private static void Fields(JsonElement node, params string[] allowed)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in node.EnumerateObject())
        {
            if (!seen.Add(field.Name)) throw Error($"duplicate field '{field.Name}'");
            if (!allowed.Contains(field.Name, StringComparer.Ordinal)) throw Error($"unknown field '{field.Name}'");
        }
    }

    private static string Text(JsonElement node, string name) => Required(node.GetProperty(name).GetString()!, name);
    private static string Required(string value, string name) => !string.IsNullOrWhiteSpace(value) ? value : throw Error($"{name} must be nonempty");
    private static CompileException Error(string message) => new($"Zig modules: {message}");
}
