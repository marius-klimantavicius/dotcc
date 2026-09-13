using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

/// <summary>Name the anonymous struct/union type of a field, including promoted members.</summary>
public sealed record FieldTypeNameOverride(string Field, string Name, bool RequireMatch = false);

public sealed partial class CPreprocessingOptions
{
    private static IReadOnlyList<FieldTypeNameOverride> ValidateFieldTypeNames(IReadOnlyList<FieldTypeNameOverride>? rules)
    {
        var result = new List<FieldTypeNameOverride>();
        var selectors = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rule in rules ?? Array.Empty<FieldTypeNameOverride>())
        {
            var selector = rule.Field.Replace("::", ".", StringComparison.Ordinal);
            var path = selector.Split('.');
            if (path.Length < 2 || path.Any(p => !Identifier(p)))
                throw new CompileException("fieldTypeNames field must be a type followed by a field path: " + rule.Field);
            if (!selectors.Add(selector)) throw new CompileException("duplicate fieldTypeNames selector: " + selector);
            if (!Identifier(rule.Name) || EmitHelpers.Id(rule.Name) != rule.Name
                || rule.Name is "null" or "default" or "var" or "dynamic" or "nint" or "nuint" or "record" or "required" or "file" or "scoped"
                || rule.Name.StartsWith("__", StringComparison.Ordinal))
                throw new CompileException("fieldTypeNames name must be a nonreserved ASCII identifier: " + rule.Name);
            result.Add(rule with { Field = selector });
        }
        return result.AsReadOnly();
    }
}
