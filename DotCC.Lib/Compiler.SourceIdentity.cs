using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCC;

public static partial class Compiler
{
    private const string SourceIdentityPrefix = "//!!dotcc-obj source-identity:";
    private const string SourceIdentityVersion = SourceIdentityPrefix + "root-relative-v1";
    private const string LogicalUnitPrefix = "//!!dotcc-obj logical-unit:";

    private static string SerializeSourceIdentity(IReadOnlyList<string> units) => SourceIdentityVersion + "\n"
        + string.Concat(units.Select(unit => LogicalUnitPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(unit)) + "\n"));

    // No physical root is serialized: relocation must retain the exact object.
    // Specifying a root asserts one common logical program. Duplicate unit paths
    // are errors, including objects emitted using separate per-unit roots.
    // Legacy objects carry no such assertion and retain their original identities.
    private static string?[] ValidateObjectSourceIdentities(string[] texts, IReadOnlyList<string> paths)
    {
        var origins = new string?[texts.Length];
        var owners = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < texts.Length; index++)
        {
            var lines = texts[index].Split('\n');
            var versions = lines.Where(line => line.StartsWith(SourceIdentityPrefix, StringComparison.Ordinal)).ToArray();
            var records = lines.Where(line => line.StartsWith(LogicalUnitPrefix, StringComparison.Ordinal)).ToArray();
            if (versions.Length == 0 && records.Length == 0) continue;
            if (versions.Length != 1 || versions[0] != SourceIdentityVersion || records.Length == 0)
                throw new CompileException("invalid source identity metadata in '" + paths[index] + "'; rebuild the object");
            var units = new List<string>();
            foreach (var record in records)
            {
                string unit;
                try { unit = Encoding.UTF8.GetString(Convert.FromBase64String(record[LogicalUnitPrefix.Length..])); }
                catch (FormatException) { throw new CompileException("invalid logical source unit in '" + paths[index] + "'"); }
                if (unit.Length == 0 || unit.StartsWith('/') || unit.Contains('\0')
                    || unit.Split('/').Any(part => part.Length == 0 || part is "." or ".."))
                    throw new CompileException("invalid logical source unit in '" + paths[index] + "'");
                if (!owners.TryAdd(unit, paths[index]))
                    throw new CompileException("duplicate logical source unit '" + unit + "' in '" + owners[unit] + "' and '"
                        + paths[index] + "'; use one common --source-root for the whole program, preserve subdirectories, and rebuild objects");
                units.Add(unit);
            }
            origins[index] = string.Join("\0", units);
        }
        return origins;
    }
}
