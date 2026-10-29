using System;
using System.Text.Json;

namespace DotCC;

public enum CLongDoubleRepresentation { Binary64, X87Storage16 }

/// <summary>Explicit source ABI choices scoped to one compiler request.
/// X87Storage16 supports layout and byte copies, without floating arithmetic.</summary>
public sealed record CSourceAbi(CLongDoubleRepresentation LongDouble = CLongDoubleRepresentation.Binary64);

public sealed partial class CPreprocessingOptions
{
    public CSourceAbi SourceAbi { get; }

    private static CSourceAbi ReadSourceAbi(JsonElement root)
    {
        if (!root.TryGetProperty("sourceAbi", out var abi)) return new();
        Fields(abi, "longDouble");
        return new(RequiredString(abi, "longDouble") switch
        {
            "binary64" => CLongDoubleRepresentation.Binary64,
            "x87-storage16" => CLongDoubleRepresentation.X87Storage16,
            _ => throw new CompileException("sourceAbi.longDouble must be binary64 or x87-storage16"),
        });
    }

    private void WriteSourceAbiProfile(Utf8JsonWriter json)
    {
        if (SourceAbi.LongDouble == CLongDoubleRepresentation.Binary64) return;
        json.WriteStartObject();
        json.WriteStartObject("sourceAbi");
        json.WriteString("longDouble", "x87-storage16");
        json.WriteEndObject();
        json.WriteEndObject();
    }
}
