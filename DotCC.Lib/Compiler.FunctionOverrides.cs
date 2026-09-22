using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DotCC;

internal sealed record FunctionOverrideMetadata(string Name, string Signature, FunctionOverrideTarget? Target,
    string? TranslationUnit, string? DeclarationFile, string? Linkage);

public static partial class Compiler
{
    private const string FunctionOverridePrefix = "//!!dotcc-obj function-contract:";

    private static string SerializeFunctionOverrides(IEnumerable<FunctionOverrideMetadata>? contracts)
    {
        var text = new StringBuilder();
        foreach (var contract in contracts ?? Array.Empty<FunctionOverrideMetadata>())
        {
            using var stream = new System.IO.MemoryStream();
            using (var json = new Utf8JsonWriter(stream))
            {
                json.WriteStartObject(); json.WriteNumber("version", 1); json.WriteString("name", contract.Name); json.WriteString("signature", contract.Signature);
                json.WriteString("kind", contract.Target?.Kind); json.WriteString("target", contract.Target?.Value);
                json.WriteString("translationUnit", contract.TranslationUnit); json.WriteString("declarationFile", contract.DeclarationFile);
                json.WriteString("linkage", contract.Linkage); json.WriteEndObject();
            }
            text.Append(FunctionOverridePrefix).Append(Encoding.UTF8.GetString(stream.ToArray())).Append('\n');
        }
        return text.ToString();
    }

    private static HashSet<string> MergeFunctionOverrides(string text, string path,
        Dictionary<string, (FunctionOverrideMetadata Contract, string Path)> contracts)
    {
        var incoming = new Dictionary<string, FunctionOverrideMetadata>(StringComparer.Ordinal);
        foreach (var line in text.Split('\n').Where(l => l.StartsWith(FunctionOverridePrefix, StringComparison.Ordinal)))
        {
            try
            {
                using var doc = JsonDocument.Parse(line[FunctionOverridePrefix.Length..]);
                var root = doc.RootElement;
                if (root.GetProperty("version").GetInt32() != 1)
                    throw new CompileException("unsupported semantic function contract version in object '" + path + "'");
                var name = root.GetProperty("name").GetString()!;
                var signature = root.GetProperty("signature").GetString()!;
                var kind = root.GetProperty("kind").GetString();
                var target = root.GetProperty("target").GetString();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(signature)
                    || kind is not (null or "intrinsic" or "managedMethod") || (kind is null) != (target is null))
                    throw new CompileException("invalid semantic function override metadata in object '" + path + "'");
                var contract = new FunctionOverrideMetadata(name, signature, kind is null ? null : new(kind, target!),
                    root.GetProperty("translationUnit").GetString(), root.GetProperty("declarationFile").GetString(), root.GetProperty("linkage").GetString());
                if (!incoming.TryAdd(name, contract)) throw new CompileException("duplicate function contract in object '" + path + "': " + name);
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
            { throw new CompileException("invalid semantic function override metadata in object '" + path + "': " + ex.Message, ex); }
        }
        // Older objects did not record contracts. Treat their definitions as
        // original bodies, so they cannot silently replace an overridden body.
        foreach (var line in text.Split('\n').Where(l => l.StartsWith(FragFunction, StringComparison.Ordinal)))
        {
            var name = line[FragFunction.Length..];
            incoming.TryAdd(name, new(name, "unknown", null, null, null, null));
        }
        foreach (var (name, contract) in incoming)
        {
            if (contracts.TryGetValue(name, out var previous)
                && (previous.Contract.Target is not null || contract.Target is not null)
                && (previous.Contract.Target != contract.Target || previous.Contract.Signature != contract.Signature))
                throw new CompileException("conflicting semantic function override for '" + name + "' in '" + previous.Path + "' and '" + path + "'");
            contracts.TryAdd(name, (contract, path));
        }
        return incoming.Values.Where(c => c.Target is not null).Select(c => c.Name).ToHashSet(StringComparer.Ordinal);
    }
}
