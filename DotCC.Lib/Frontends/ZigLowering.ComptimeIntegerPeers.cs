#nullable enable
using System.Numerics;
using System.Globalization;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;
internal sealed partial class ZigLowering
{
    private ZigResultLocation? ProvenUnsignedIntegerPeer(Item leftSource, CExpr left, Item rightSource, CExpr right)
    {
        if (!UsesSourceStdlib
            || left.Type.Unqualified is not CType.Prim { Integer: true, Signed: false, IsComptimeInt: false, Name: not "_Bool" }
            || right.Type.Unqualified is not CType.Prim { Integer: true, Signed: false, IsComptimeInt: false, Name: not "_Bool" }
            || IsSourceComptimeInteger(leftSource, left) || IsSourceComptimeInteger(rightSource, right)) return null;
        var leftBits = BitcastSourceLocation(leftSource, left).IntegerBits;
        var rightBits = BitcastSourceLocation(rightSource, right).IntegerBits;
        if (leftBits is not (> 0 and <= 128) || rightBits is not (> 0 and <= 128)) return null;
        // Both source integer domains are unsigned, so the wider one contains
        // every value of the narrower one. Keep its source width and carrier.
        return leftBits > rightBits ? new(left.Type, leftBits) : new(right.Type, rightBits);
    }

    // A named/computed comptime integer adopts its declared runtime peer just
    // as a literal does. Fold only a proved constant that fits the source width.
    private bool IsSourceComptimeInteger(Item source, CExpr? value = null, int depth = 0)
    {
        if (depth > 32) return false;
        if (value?.Type.Unqualified is CType.Prim { IsComptimeInt: true }) return true;
        if (source.Content is Zig.Ident id)
        {
            var name = Tok(id.Arg0);
            var symbol = _symbols.Resolve(name);
            if (symbol is not null && (symbol.Type.Unqualified is CType.Prim { IsComptimeInt: true }
                || _comptimeIntLocals.Contains(symbol))) return true;
            // A local/parameter shadows a same-named namespace declaration.
            if (symbol is { IsGlobal: false }) return false;
            if (!_lazyValueConsts.TryGetValue(name, out var declaration)) return false;
            if (declaration.typeItem?.Content is Zig.Ident type && Tok(type.Arg0) == "comptime_int") return true;
            return declaration.typeItem is null && IsSourceComptimeInteger(declaration.rhs, depth: depth + 1);
        }
        if (source.Content is Zig.Field field)
        {
            var member = SourceMemberName(field.Arg2);
            if (ResolveModulePath(field.Arg0)?.Lowering is { } module
                && module._lazyValueConsts.TryGetValue(member, out var moduleConstant))
            {
                using var definition = new DefinitionBindingScope(module);
                return moduleConstant.typeItem is null
                    ? module.IsSourceComptimeInteger(moduleConstant.rhs, depth: depth + 1)
                    : IsComptimeIntType(moduleConstant.typeItem);
            }
            if (MemberBaseType(field.Arg0) is { } type && ContainerTypeName(type) is { } container)
            {
                var owner = _shared.ContainerConstOwners.GetValueOrDefault(container) ?? this;
                if (!owner._containerConsts.TryGetValue(container, out var constants) || !constants.TryGetValue(member, out var constant))
                    return false;
                if (constant.typeItem is not null) return IsComptimeIntType(constant.typeItem);
                using var definition = new DefinitionBindingScope(owner);
                using var context = owner.EnterContainer(container);
                using var seeds = owner.EnterReifiedSeeds(container);
                return owner.IsSourceComptimeInteger(constant.rhs, depth: depth + 1);
            }
            return false;
        }
        bool Untyped(Item item) => IsSourceComptimeInteger(item, depth: depth + 1);
        return source.Content switch
        {
            Zig.IntLit or Zig.CharLit => true,
            Zig.Grouped g => Untyped(g.Arg1),
            Zig.PreNeg n => Untyped(n.Arg1),
            Zig.PreBitNot n => Untyped(n.Arg1),
            Zig.Add b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.Sub b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.Mul b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.DivOp b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.ModOp b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.BitAnd b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.BitOr b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.BitXor b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.Shl b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.Shr b => Untyped(b.Arg0) && Untyped(b.Arg2),
            Zig.BuiltinCall b when Tok(b.Arg0) is "@sizeOf" or "@bitSizeOf" or "@alignOf" or "@offsetOf" => true,
            _ => false,
        };
    }

    private CExpr CoerceComptimeIntegerPeer(Item source, CExpr value, Item peerSource, CExpr peer,
        bool arithmetic, out ZigResultLocation? peerLocation)
    {
        peerLocation = null;
        if (!UsesSourceStdlib || !IsSourceComptimeInteger(source, value)
            || IsSourceComptimeInteger(peerSource, peer)
            || peer.Type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not ("_Bool" or "char") })
            return value;
        var bits = DeclaredBitsOfValue(peerSource) ?? DeclaredBitsOfLowered(peer)
            ?? BitcastLoweredLocation(peer).IntegerBits;
        if (bits is not (> 0 and <= 128) || _ir.EvalComptimeValue(value) is not IrBuilder.CtInt constant)
            return value;
        var sourceType = peerSource.Content is Zig.BuiltinCall builtin && Tok(builtin.Arg0) == "@intFromBool"
            ? InferredTagCarrier(bits.Value) : ZigIntOperandType(peerSource) ?? peer.Type;
        if (sourceType.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false } integer) return value;
        var minimum = integer.Signed ? -(BigInteger.One << (bits.Value - 1)) : BigInteger.Zero;
        var maximum = (BigInteger.One << (integer.Signed ? bits.Value - 1 : bits.Value)) - 1;
        var number = BigInteger.CreateChecked(constant.Value);
        if (number < minimum || number > maximum)
        {
            // Zig permits comparisons against out-of-range constants. Ordinary
            // arithmetic still requires the constant to fit its declared peer.
            if (!arithmetic) return value;
            throw new CompileException($"zig: type '{(integer.Signed ? "i" : "u")}{bits}' cannot represent integer value '{constant.Value.ToString(CultureInfo.InvariantCulture)}'"
                + $" ({_module?.Path ?? _fileStem}:{source.Position.Line}, function '{_currentFnName}')");
        }
        var converted = _ir.SpliceComptimeValue(new IrBuilder.CtInt(constant.Value, sourceType))!;
        peerLocation = new ZigResultLocation(sourceType, bits);
        _shared.BitcastWidths.Expressions[converted] = peerLocation;
        return converted;
    }
}
