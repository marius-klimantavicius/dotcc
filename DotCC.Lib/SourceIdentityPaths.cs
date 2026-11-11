using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotCC;

/// <summary>Opt-in logical identities. Physical paths remain available for I/O and diagnostics.</summary>
internal sealed class SourceIdentityPaths
{
    private readonly string _root;

    internal SourceIdentityPaths(string root)
    {
        if (string.IsNullOrWhiteSpace(root)) throw new CompileException("--source-root requires a nonempty common source root");
        _root = Path.GetFullPath(root);
        if (!Directory.Exists(_root)) throw new CompileException("--source-root directory does not exist: " + _root);
    }

    internal string File(string path) => Relative(_root, Path.GetFullPath(path), OperatingSystem.IsWindows());

    internal string[] Units(IReadOnlyList<string> paths)
    {
        var units = paths.Select(File).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var unit in units)
            if (!seen.Add(unit)) throw new CompileException("duplicate logical source unit '" + unit
                + "'; use one common --source-root for the whole program");
        return units;
    }

    // Pure lexical seam for testing both contracts on any host. Production first
    // uses Path.GetFullPath, so relative paths and . / .. follow the host platform.
    // A backslash is a separator ONLY for Windows; it is a legal Unix filename byte.
    internal static string Relative(string root, string path, bool windows)
    {
        var (rootVolume, rootParts) = Parts(root);
        var (pathVolume, pathParts) = Parts(path);
        var comparison = windows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(rootVolume, pathVolume, comparison) || pathParts.Length <= rootParts.Length
            || rootParts.Where((part, index) => !string.Equals(part, pathParts[index], comparison)).Any())
            throw new CompileException("source path '" + path + "' is outside --source-root '" + root
                + "' (or on a different drive); stage sources and included disk headers under one common root");
        return string.Join("/", pathParts.Skip(rootParts.Length));

        (string Volume, string[] Components) Parts(string value)
        {
            if (windows) value = value.Replace('\\', '/');
            string volume;
            if (!windows && value.StartsWith('/')) { volume = "/"; value = value[1..]; }
            else if (windows && value.Length >= 3 && char.IsAsciiLetter(value[0]) && value[1] == ':' && value[2] == '/')
            { volume = value[..2]; value = value[3..]; }
            else if (windows && value.StartsWith("//", StringComparison.Ordinal) && !value.StartsWith("//?", StringComparison.Ordinal)
                && !value.StartsWith("//.", StringComparison.Ordinal))
            {
                var unc = value[2..].Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (unc.Length < 2) throw new CompileException("--source-root requires an absolute UNC share path");
                volume = "//" + unc[0] + "/" + unc[1]; value = string.Join("/", unc.Skip(2));
            }
            else throw new CompileException("--source-root requires ordinary absolute platform paths: " + value);
            var parts = new List<string>();
            foreach (var part in value.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (part == ".") continue;
                if (part == "..") { if (parts.Count != 0) parts.RemoveAt(parts.Count - 1); }
                else parts.Add(part);
            }
            return (volume, parts.ToArray());
        }
    }
}
