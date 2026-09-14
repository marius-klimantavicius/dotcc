using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    private static string ResolveLibraryClassName(string? name, EmitMode emit)
    {
        if (name is null) return "DotCcLib";
        if (emit is not (EmitMode.ManagedLib or EmitMode.SharedLib))
            throw new CompileException("--class-name requires managed-library or shared-library output; set it at link time for objects");
        var identifier = name.StartsWith('@') ? name[1..] : name;
        static bool Start(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
        if (identifier.Length == 0 || !Start(identifier[0])
            || identifier.Any(c => !Start(c) && c is not (>= '0' and <= '9')))
            throw new CompileException("--class-name must be a single ASCII C# identifier, optionally prefixed with @");
        if (identifier is "Libc" or "Cond" or "CBool" or "VaArg" or "VaList" or "System"
            or "DotCcEntryPoint" or "DotCcPointers" or "DotCcLiterals" or "DotCcGlobals" or "DotCcFunctions" or "DotCcFunctionPointers" or "DotCcProgram"
            or "DotCcExports" or "DotCcImports" or "DotCcStaticImports")
            throw new CompileException($"--class-name '{name}' conflicts with a generated runtime or infrastructure name");
        // The C-expression legalizer intentionally leaves null/default alone
        // because it also handles literal expressions. A type name must escape
        // those tokens and contextual type-name keywords as well.
        return name.StartsWith('@') || identifier is "null" or "default" or "var" or "dynamic"
            or "nint" or "nuint" or "record" or "required" or "file" or "scoped"
            ? "@" + identifier : EmitHelpers.Id(identifier);
    }

    private static void CheckLibraryClassCollision(string name, IEnumerable<string> types, IEnumerable<string> definitions)
    {
        var identifier = name.TrimStart('@');
        if (types.Concat(definitions).Any(other => other.TrimStart('@') == identifier))
            throw new CompileException($"--class-name '{identifier}' conflicts with a translated type or symbol");
    }
}
