using System;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    private static void ValidateInstanceObject(string source, string path, bool instanceMethods)
    {
        const string prefix = "//!!dotcc-obj calling-convention:";
        var records = source.Split('\n').Where(line => line.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
        if (records.Length > 1 || records.Length == 1 && records[0] is not (prefix + "static-v1" or prefix + "instance-v1"))
            throw new CompileException("invalid calling convention metadata in object '" + path + "'");
        var actual = records.Length == 0 ? "static-v1" : records[0][prefix.Length..];
        var expected = instanceMethods ? "instance-v1" : "static-v1";
        if (actual != expected)
            throw new CompileException("calling convention mismatch in object '" + path + "': " + actual + "; link requires " + expected);
    }
}
