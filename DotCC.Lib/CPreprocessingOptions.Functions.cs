using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotCC;

/// <summary>C types are resolved in the selected declaration's typedef environment.</summary>
public sealed record FunctionSignature(string ReturnType, IReadOnlyList<string> ParameterTypes, bool Variadic = false);
/// <summary>An intrinsic name or fully qualified static managed method name.</summary>
public sealed record FunctionOverrideTarget(string Kind, string Value);
public sealed record FunctionOverride(string Name, FunctionSignature Signature, FunctionOverrideTarget Target,
    string? Linkage = null, string? TranslationUnit = null, string? DeclarationFile = null,
    bool RequireMatch = false, string Origin = "API");

public sealed partial class CPreprocessingOptions
{
    public IReadOnlyList<FunctionOverride> FunctionOverrides { get; }

    private static IReadOnlyList<FunctionOverride> ValidateFunctionOverrides(IReadOnlyList<FunctionOverride>? rules, string? profilePath)
    {
        var result = new List<FunctionOverride>();
        var selectors = new HashSet<(string, string?, string?, string?)>();
        var directory = profilePath is null ? Environment.CurrentDirectory : Path.GetDirectoryName(Path.GetFullPath(profilePath))!;
        foreach (var rule in rules ?? Array.Empty<FunctionOverride>())
        {
            if (!Identifier(rule.Name) || rule.Name.StartsWith("__dotcc_", StringComparison.Ordinal)
                || rule.Name.StartsWith("__builtin_", StringComparison.Ordinal)
                || rule.Name.StartsWith("__atomic_", StringComparison.Ordinal)
                || rule.Name.StartsWith("__sync_", StringComparison.Ordinal)
                || rule.Name is "setjmp" or "longjmp" or "malloc" or "free" or "dlsym"
                    or "va_start" or "va_end" or "va_copy" or "va_arg"
                    or "printf" or "fprintf" or "sprintf" or "snprintf" or "scanf" or "fscanf" or "sscanf"
                    or "wprintf" or "fwprintf" or "swprintf" or "wscanf" or "fwscanf" or "swscanf")
                throw FunctionError(rule, "invalid or reserved function name");
            if (rule.Linkage is not (null or "external" or "internal"))
                throw FunctionError(rule, "linkage must be external or internal");
            if (rule.Signature is null || string.IsNullOrWhiteSpace(rule.Signature.ReturnType)
                || rule.Signature.ParameterTypes is null || rule.Signature.ParameterTypes.Any(string.IsNullOrWhiteSpace))
                throw FunctionError(rule, "a return type and parameter type list are required");
            if (rule.Signature.Variadic) throw FunctionError(rule, "variadic replacements are not supported");
            if (rule.Target is null || rule.Target.Kind is not ("intrinsic" or "managedMethod"))
                throw FunctionError(rule, "target kind must be intrinsic or managedMethod");
            if (rule.Target.Kind == "intrinsic" && FunctionOverrideIntrinsic.Find(rule.Target.Value) is null)
                throw FunctionError(rule, "unknown intrinsic: " + rule.Target.Value);
            if (rule.Target.Kind == "managedMethod" && (rule.Target.Value is null || !Regex.IsMatch(rule.Target.Value,
                @"\Aglobal::@?[A-Za-z_][A-Za-z_0-9]*(\.@?[A-Za-z_][A-Za-z_0-9]*)+\z", RegexOptions.CultureInvariant)))
                throw FunctionError(rule, "managedMethod requires a fully qualified global::Type.Method name");
            var copy = rule with
            {
                Signature = rule.Signature with { ParameterTypes = Array.AsReadOnly(rule.Signature.ParameterTypes.ToArray()) },
                TranslationUnit = rule.TranslationUnit is null ? null : Path.GetFullPath(rule.TranslationUnit, directory),
                DeclarationFile = rule.DeclarationFile is null ? null : Path.GetFullPath(rule.DeclarationFile, directory),
            };
            if (!selectors.Add((copy.Name, copy.Linkage, copy.TranslationUnit, copy.DeclarationFile)))
                throw FunctionError(copy, "duplicate function selector");
            result.Add(copy);
        }
        return result.AsReadOnly();
    }

    private static IEnumerable<FunctionOverride> ReadFunctionOverrides(JsonElement root, string profilePath)
    {
        if (!root.TryGetProperty("functionOverrides", out var entries)) yield break;
        var index = 0;
        foreach (var entry in entries.EnumerateArray())
        {
            Fields(entry, "name", "signature", "target", "linkage", "translationUnit", "declarationFile", "requireMatch");
            var signature = entry.GetProperty("signature");
            Fields(signature, "returnType", "parameterTypes", "variadic");
            var target = entry.GetProperty("target");
            var kind = RequiredString(target, "kind");
            Fields(target, "kind", kind == "managedMethod" ? "method" : "name");
            yield return new(RequiredString(entry, "name"),
                new(RequiredString(signature, "returnType"), Strings(signature.GetProperty("parameterTypes")),
                    signature.TryGetProperty("variadic", out var variadic) && variadic.GetBoolean()),
                new(kind, RequiredString(target, kind == "managedMethod" ? "method" : "name")),
                OptionalString(entry, "linkage"), OptionalString(entry, "translationUnit"), OptionalString(entry, "declarationFile"),
                entry.TryGetProperty("requireMatch", out var required) && required.GetBoolean(),
                $"{profilePath}:functionOverrides[{index++}]");
        }
    }

    private void WriteFunctionOverridesProfile(Utf8JsonWriter writer)
    {
        foreach (var rule in FunctionOverrides)
        {
            // Origin is diagnostic provenance, not part of the semantic hash.
            writer.WriteStartObject(); writer.WritePropertyName("functionOverride");
            writer.WriteStartObject(); writer.WriteString("name", rule.Name);
            writer.WriteString("returnType", rule.Signature.ReturnType); writer.WriteStartArray("parameterTypes");
            foreach (var type in rule.Signature.ParameterTypes) writer.WriteStringValue(type);
            writer.WriteEndArray(); writer.WriteBoolean("variadic", rule.Signature.Variadic);
            writer.WriteString("kind", rule.Target.Kind); writer.WriteString("target", rule.Target.Value);
            writer.WriteString("linkage", rule.Linkage); writer.WriteString("translationUnit", rule.TranslationUnit);
            writer.WriteString("declarationFile", rule.DeclarationFile); writer.WriteBoolean("requireMatch", rule.RequireMatch);
            writer.WriteEndObject(); writer.WriteEndObject();
        }
    }

    internal static CompileException FunctionError(FunctionOverride rule, string message) =>
        new($"function override '{rule.Name}' ({rule.Origin}): {message}");
}
