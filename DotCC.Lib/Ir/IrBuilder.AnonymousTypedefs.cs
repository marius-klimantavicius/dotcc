using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // A typedef is an ordinary, TU-local alias, not an aggregate tag. Carry
    // the declaration identity through the IR and object link. Only the final
    // output can decide whether the familiar alias is unambiguous/public.
    internal Dictionary<string, string> AnonymousTypedefs { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _anonymousTypedefOccurrences = new(StringComparer.Ordinal);

    private string AnonymousTypedefName(Item members, string alias)
    {
        var source = SourceFileOrigin.Of(members)?.Identity ?? FunctionUnitIdentity;
        var location = source + "\n" + SourceMappedItem.Physical(members).ByteOffset.ToString(CultureInfo.InvariantCulture)
            + "\nanonymous-typedef";
        var occurrence = _anonymousTypedefOccurrences.GetValueOrDefault(location);
        _anonymousTypedefOccurrences[location] = occurrence + 1;
        var identity = location + "\n" + occurrence.ToString(CultureInfo.InvariantCulture);
        var name = AnonymousTypedefNames.TypePrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
        if (AnonymousTypedefs.TryGetValue(name, out var previous) && previous != alias)
            throw new CompileException("conflicting aliases for anonymous aggregate declaration: " + alias);
        AnonymousTypedefs[name] = alias;
        return AggregateName(name);
    }
}
