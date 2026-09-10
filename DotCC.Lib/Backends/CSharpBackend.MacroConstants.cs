using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void AddMacroConstants(IrBuilder unit, IDictionary<string, string> declarations)
    {
        foreach (var group in unit.MacroConstants.GroupBy(m => m.Name).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var name = EmitHelpers.Id(group.Key);
            var fields = group.Select(m => MacroField(name, m.Value)).Distinct(StringComparer.Ordinal).ToArray();
            // Preserve omitted/conflicting metadata too, so linking cannot
            // accidentally export another TU's incompatible definition.
            declarations.Add(Compiler.MacroConstantPrefix + name, fields.Length == 1 ? fields[0] ?? "" : "");
        }
    }

    private string? MacroField(string name, CExpr? value)
    {
        if (value == null) return null;
        if (value is LitStr text)
        {
            string decoded;
            try { decoded = new UTF8Encoding(false, true).GetString(EmitHelpers.StringByteValues(text.Segments).Select(b => (byte)b).ToArray()); }
            catch (DecoderFallbackException) { return null; }
            var literal = new StringBuilder("\"");
            foreach (var ch in decoded)
            {
                if (ch >= ' ' && ch <= '~' && ch is not ('"' or '\\')) literal.Append(ch);
                else literal.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
            }
            return $"    public const string {name} = {literal}\";\n";
        }
        if (value.Type.Unqualified is not CType.Prim p || p.Name is "_Float128" || p.Bytes > 8) return null;
        var expression = Expr(value);
        // Spliced negative integer values may use unary expressions, which
        // remain compile-time C# expressions. Preserve exact signedness/width.
        return $"    public const {Cs(value.Type)} {name} = unchecked(({Cs(value.Type)})({expression}));\n";
    }
}
