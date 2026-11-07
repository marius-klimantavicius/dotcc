#nullable enable
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // A named enum tag is a real comptime receiver, just like a bound enum
    // parameter. Restrict this probe to declared tags; don't evaluate arbitrary
    // runtime expressions while deciding whether a call returns a type.
    private CType? TypeReturningValueReceiverType(Item receiver)
    {
        if (receiver.Content is Zig.Ident name && _symbols.Resolve(Tok(name.Arg0)) is { } value)
            return value.Type;
        if (receiver.Content is Zig.Field tag && MemberBaseType(tag.Arg0)?.Unqualified is CType.Enum type)
        {
            var owner = _moduleGraph?.OwnerOfContainer(type.Name) ?? this;
            if (owner._enumMembers.TryGetValue(type.Name, out var members)
                && members.ContainsKey(SourceMemberName(tag.Arg2)))
                return type;
        }
        return null;
    }
}
