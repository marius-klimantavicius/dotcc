using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DotCC;

/// <summary>Unmanaged storage contract for an authored target type; no members are implied.</summary>
public sealed record ExternalTypeLayout(int Size, int Alignment);

/// <summary>A C type name supplied by authored target code rather than a translated declaration.</summary>
public sealed record ExternalTypeOverride(string Name, ExternalTypeLayout? Layout = null);

public sealed partial class CPreprocessingOptions
{
    public IReadOnlyList<ExternalTypeOverride> ExternalTypes { get; }
    internal IEnumerable<string> TypeNames => Compiler.PredefinedTypeNames.Concat(ExternalTypes.Select(t => t.Name));
    private static readonly HashSet<string> ReservedCTypeNames = new((
        "auto break case char const continue default do double else enum extern float for goto if inline int long register restrict return short signed sizeof static struct switch typedef union unsigned void volatile while " +
        "_Alignas _Alignof _Atomic _BitInt _Bool _Complex _Decimal32 _Decimal64 _Decimal128 _Generic _Imaginary _Noreturn _Static_assert _Thread_local alignas alignof bool constexpr false nullptr static_assert thread_local true typeof typeof_unqual")
        .Split(' '), StringComparer.Ordinal);

    private static IReadOnlyList<ExternalTypeOverride> ValidateExternalTypes(IReadOnlyList<ExternalTypeOverride>? types)
    {
        var result = new Dictionary<string, ExternalTypeOverride>(StringComparer.Ordinal);
        foreach (var type in types ?? Array.Empty<ExternalTypeOverride>())
        {
            if (type is null || type.Name is null || !Identifier(type.Name) || EmitHelpers.Id(type.Name) != type.Name
                || type.Name.StartsWith("__", StringComparison.Ordinal)
                || type.Name is "null" or "default" or "var" or "dynamic" or "nint" or "nuint" or "record" or "required" or "file" or "scoped"
                || Compiler.PredefinedTypeNames.Contains(type.Name, StringComparer.Ordinal)
                || ReservedCTypeNames.Contains(type.Name)
                || type.Name is "timespec" or "tm" or "lconv" or "System" or "Libc")
                throw new CompileException("externalTypes name must be a nonreserved type identifier: " + type?.Name);
            if (type.Layout is { } layout && (layout.Size <= 0 || layout.Alignment <= 0 || layout.Alignment > 128
                || (layout.Alignment & (layout.Alignment - 1)) != 0 || layout.Size % layout.Alignment != 0))
                throw new CompileException("externalTypes layout requires positive size, power-of-two alignment (at most 128), and size divisible by alignment: " + type.Name);
            if (result.TryGetValue(type.Name, out var previous))
            {
                if (previous.Layout is not null && type.Layout is not null && previous.Layout != type.Layout)
                    throw new CompileException("conflicting externalTypes layout: " + type.Name);
                if (previous.Layout is not null) continue;
            }
            result[type.Name] = type;
        }
        return Array.AsReadOnly(result.Values.OrderBy(t => t.Name, StringComparer.Ordinal).ToArray());
    }

    private static IEnumerable<ExternalTypeOverride> ReadExternalTypes(JsonElement root)
    {
        if (!root.TryGetProperty("externalTypes", out var types)) yield break;
        foreach (var entry in types.EnumerateArray())
        {
            Fields(entry, "name", "layout");
            ExternalTypeLayout? layout = null;
            if (entry.TryGetProperty("layout", out var value))
            {
                Fields(value, "size", "alignment");
                layout = new(value.GetProperty("size").GetInt32(), value.GetProperty("alignment").GetInt32());
            }
            yield return new(RequiredString(entry, "name"), layout);
        }
    }

    private void WriteExternalTypesProfile(Utf8JsonWriter json)
    {
        foreach (var type in ExternalTypes)
        {
            json.WriteStartObject(); json.WriteString("externalType", type.Name);
            if (type.Layout is { } layout)
            {
                json.WriteNumber("size", layout.Size); json.WriteNumber("alignment", layout.Alignment);
            }
            json.WriteEndObject();
        }
    }
}
