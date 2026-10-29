using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCC;

// These are facts from typed source IR, not inferred C# spellings or export intent.
internal sealed record SourceAbiForeignRestriction(string Role, string Name, string TargetName);
internal sealed record SourceAbiForeignProof(bool X87Storage16, bool Complete,
    IReadOnlyList<SourceAbiForeignRestriction> Restrictions);

internal static class SourceAbiForeignMetadata
{
    internal const string ModelPrefix = "//!!dotcc-obj source-abi:";
    internal const string VersionPrefix = "//!!dotcc-obj foreign-abi:";
    internal const string Version = VersionPrefix + "1";
    internal const string RestrictionPrefix = "//!!dotcc-obj foreign-abi-restriction:";

    internal static string Serialize(CSourceAbi abi, IEnumerable<SourceAbiForeignRestriction> restrictions)
    {
        var text = new StringBuilder(ModelPrefix).Append(abi.LongDouble == CLongDoubleRepresentation.X87Storage16
            ? "x87-storage16\n" : "binary64\n").Append(Version).Append('\n');
        foreach (var restriction in restrictions.OrderBy(r => r.Role, StringComparer.Ordinal).ThenBy(r => r.Name, StringComparer.Ordinal))
            text.Append(RestrictionPrefix).Append(restriction.Role).Append(' ').Append(Encode(restriction.Name))
                .Append(' ').Append(Encode(restriction.TargetName)).Append(" x87-storage16\n");
        return text.ToString();
    }

    internal static SourceAbiForeignProof Read(string source, string path)
    {
        string? model = null;
        bool complete = false;
        var restrictions = new Dictionary<(string Role, string Name), SourceAbiForeignRestriction>();
        foreach (var line in source.Split('\n'))
        {
            if (line.StartsWith(ModelPrefix, StringComparison.Ordinal))
            {
                if (model is not null) throw Bad("duplicate source ABI model");
                model = line[ModelPrefix.Length..];
                if (model is not ("binary64" or "x87-storage16")) throw Bad("unsupported source ABI model");
            }
            else if (line.StartsWith(VersionPrefix, StringComparison.Ordinal))
            {
                if (complete || line != Version) throw Bad("invalid foreign ABI proof version");
                complete = true;
            }
            else if (line.StartsWith(RestrictionPrefix, StringComparison.Ordinal))
            {
                var parts = line[RestrictionPrefix.Length..].Split(' ');
                if (parts.Length != 4 || parts[0] is not ("import" or "definition") || parts[3] != "x87-storage16")
                    throw Bad("invalid foreign ABI restriction");
                try
                {
                    var restriction = new SourceAbiForeignRestriction(parts[0], Decode(parts[1]), Decode(parts[2]));
                    if (!restrictions.TryAdd((restriction.Role, restriction.Name), restriction))
                        throw Bad("duplicate foreign ABI restriction");
                }
                catch (Exception error) when (error is FormatException or DecoderFallbackException)
                { throw Bad("invalid foreign ABI symbol"); }
            }
        }
        if ((complete && model is null) || (restrictions.Count != 0 && model != "x87-storage16"))
            throw Bad("foreign ABI restrictions lack their source ABI model");
        // Older ordinary objects have no new model/proof. Their existing native
        // import behavior remains available. Explicit new storage objects must
        // carry proof if they later acquire an active foreign binding.
        return new(model == "x87-storage16", complete, restrictions.Values.ToArray());

        CompileException Bad(string reason) => new(reason + " in object '" + path + "'; regenerate objects");
    }

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    private static string Decode(string value)
    {
        var name = new UTF8Encoding(false, true).GetString(Convert.FromBase64String(value));
        if (name.Length == 0 || name.Any(c => !char.IsLetterOrDigit(c) && c is not ('_' or '@')))
            throw new FormatException();
        return name;
    }
}
