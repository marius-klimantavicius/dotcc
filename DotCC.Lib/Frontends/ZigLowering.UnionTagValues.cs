#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr? SourceUnionTagMember(CType type, string member)
        => type.Unqualified is CType.Named named && _unions.TryGetValue(named.Name, out var union)
            && union.Variants.ContainsKey(member) ? ResolveEnumLit(member, union.TagType) : null;

    private CType? SourceUnionTagPeer(CType left, CType right)
    {
        bool OwnTag(CType owner, CType tag) => owner.Unqualified is CType.Named named
            && tag.Unqualified is CType.Enum enumeration && _unions.TryGetValue(named.Name, out var union)
            && union.TagType.Name == enumeration.Name;
        return OwnTag(left, right) ? left : OwnTag(right, left) ? right : null;
    }

    private CExpr? CoerceSourceUnionTag(CExpr value, CType? sink)
    {
        if (sink?.Unqualified is not CType.Named named || !_unions.TryGetValue(named.Name, out var union)
            || value.Type.Unqualified is not CType.Enum tag || tag.Name != union.TagType.Name) return null;
        if (_ir.ConstEval(value) is { } ordinal)
        {
            var member = MembersOfEnum(tag)?.FirstOrDefault(member => member.Value == ordinal)
                ?? throw new CompileException("zig: union tag has no declared variant");
            return BuildVoidVariant(union, member.Name);
        }
        if (union.Variants.Values.Any(payload => payload is not null && !IsVoidValueType(payload)))
            throw new CompileException("zig: a runtime tag cannot construct a union with payload variants");
        return new StructInit(new List<FieldInit> { new(union.TagFieldName, union.TagType, value) }) { Type = sink };
    }
}
