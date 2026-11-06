#nullable enable

using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CType SourcePointerIntegerWidth(Item syntax, CType type)
    {
        if (UsesSourceStdlib && (syntax.Content is Zig.TyPointer or Zig.TyPtrConst or Zig.TyPointerAlign or Zig.TyPtrConstAlign
                or Zig.TyManyPtr or Zig.TyManyPtrConst or Zig.TyManyPtrAlign or Zig.TyManyPtrConstAlign)
            && type.Unqualified is CType.Pointer
            { Pointee.Unqualified: CType.Prim { Integer: true, Name: not "_Bool" } } pointer
            && BitcastTypeLocation(syntax, type).Element?.IntegerBits is { } bits)
            return pointer with { ZigSourcePointeeBits = bits, Quals = type.Quals, AlignmentOverride = type.AlignmentOverride };
        return type;
    }

    // C integer names and LP64 word names are distinct source types. Their
    // shared widths alone cannot establish an iN/uN name.
    private static bool UnambiguousIntegerName(CType.Prim integer, int bits)
        => integer.Name != "char" && bits is not (16 or 32 or 64);

    // Only ordinary declared containers have a source name here. Reified and
    // anonymous types need Zig's instantiation/site spelling, never an IR mangle.
    private void RememberSourceTypeName(string identity, string? declaration, string? parent = null)
    {
        var path = _module?.Path ?? (_importerDir is not null && _fileStem is not null
            ? System.IO.Path.Combine(_importerDir, _fileStem + ".zig") : null);
        var owner = parent is not null ? _shared.SourceTypeNames.GetValueOrDefault(parent)
            : path is not null ? _moduleGraph?.SourceTypeNamespace(path, _moduleName) : null;
        if (owner is not null)
            _shared.SourceTypeNames[identity] = declaration is not null ? owner + "." + declaration : owner;
    }

    private string? SourceTypeName(Item argument)
    {
        if (ZigTypeSpelling(argument) is { } primitive) return primitive;
        // These prefixes are explicit source syntax, including constness. A
        // reflected/aliased pointer is not reconstructed from its erased carrier.
        var composed = argument.Content switch
        {
            Zig.Grouped g => SourceTypeName(g.Arg1),
            Zig.TySlice s => SourceTypeName(s.Arg2) is { } e ? "[]" + e : null,
            Zig.TySliceConst s => SourceTypeName(s.Arg3) is { } e ? "[]const " + e : null,
            Zig.TyPointer p => SourceTypeName(p.Arg1) is { } e ? "*" + e : null,
            Zig.TyPtrConst p => SourceTypeName(p.Arg2) is { } e ? "*const " + e : null,
            Zig.TyManyPtr p => SourceTypeName(p.Arg1) is { } e ? "[*]" + e : null,
            Zig.TyManyPtrConst p => SourceTypeName(p.Arg2) is { } e ? "[*]const " + e : null,
            Zig.TyOptional o => SourceTypeName(o.Arg1) is { } e ? "?" + e : null,
            _ => null,
        };
        if (composed is not null) return composed;
        if (!TryTypeAliasRhs(argument, out var type)) return null;
        if (UsesSourceStdlib && type.Unqualified is CType.Func { ZigSourceFunctionName: { } signature } function
            && (!function.ZigSourcePointer || function.ZigSourcePointerAlignment is null or <= 0))
            return (function.ZigOptional ? "?" : "")
                + (function.ZigSourcePointer ? function.ZigSourcePointerConst ? "*const " : "*" : "") + signature;
        if (UsesSourceStdlib && type.Unqualified is CType.Pointer { ZigSourceSize: "one", ZigSourceAlignment: 0 } pointer
            && !pointer.Pointee.IsVolatile)
        {
            // A source single-pointer alias retains its constructor attributes
            // and its nominal child identity. Neither comes from pointer size.
            var child = pointer.Pointee.Unqualified switch
            {
                CType.Named named => _shared.SourceTypeNames.GetValueOrDefault(named.Name),
                CType.Opaque opaque => _shared.SourceTypeNames.GetValueOrDefault(opaque.Name),
                CType.VoidType empty => empty.ZigAnyopaque ? "anyopaque" : "void",
                _ => null,
            };
            if (child is not null)
                return (pointer.ZigOptional ? "?" : "") + "*" + (pointer.Pointee.IsConst ? "const " : "") + child;
        }
        if (UsesSourceStdlib && type.Unqualified is CType.VoidType erased)
            return erased.ZigAnyopaque ? "anyopaque" : "void";
        if (UsesSourceStdlib && type.Unqualified is CType.Named { Name: "Unit" }) return "void";
        // A reflected integer must carry its declared width and fixed-integer
        // name proof. C/word aliases with only an ambiguous width stay unknown.
        if (UsesSourceStdlib && type.Unqualified is CType.Prim { Integer: true, Name: not "_Bool" } integer
            && DeclaredBitsOfTypeArg(argument) is { } bits && bits is >= 0 and <= 128
            && UnambiguousIntegerName(integer, bits))
            return (integer.Signed ? "i" : "u") + bits.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var identity = type.Unqualified switch
        {
            CType.Named named => named.Name,
            CType.Enum enumeration => enumeration.Name,
            CType.Opaque opaque => opaque.Name,
            _ => null,
        };
        return identity is not null ? _shared.SourceTypeNames.GetValueOrDefault(identity) : null;
    }

    private string? SourceFunctionName(Zig.TyFn function)
    {
        // Only retain signatures proved by the defining annotation. An
        // unsupported parameter or convention leaves reflection unavailable.
        var parameters = new List<string>();
        foreach (var parameter in Flatten(function.Arg2))
        {
            var syntax = parameter.Content switch
            {
                Zig.FnTypeParamUnnamed unnamed => unnamed.Arg0,
                Zig.FnTypeParamNamed named => named.Arg2,
                _ => null,
            };
            if (syntax is null || SourceTypeName(syntax) is not { } name) return null;
            parameters.Add(name);
        }
        if (function.Arg5 is not null || SourceTypeName(function.Arg6) is not { } returned) return null;
        var convention = "";
        if (function.Arg4?.Content is Zig.CallConv callConvention)
        {
            var value = callConvention.Arg2;
            while (value.Content is Zig.Grouped grouped) value = grouped.Arg1;
            var name = value.Content is Zig.EnumLit literal ? Tok(literal.Arg1)
                : TryEvalComptimeTag(value, out var selectedConvention, out _) ? selectedConvention : null;
            if (name == "c") convention = " callconv(.c)";
            else if (name != "auto") return null;
        }
        return "fn (" + string.Join(", ", parameters) + ")" + convention + " " + returned;
    }
}
