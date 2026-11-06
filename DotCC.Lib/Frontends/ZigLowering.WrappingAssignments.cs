#nullable enable
using DotCC.Ir;
using Item = LALR.CC.LexicalGrammar.Item;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CStmt WrappingCompoundAssign(Item targetItem, BinOp operation, Item valueItem)
    {
        if (targetItem.Content is Zig.Ident identifier && _symbols.Resolve(Tok(identifier.Arg0)) is { } symbol
            && _comptimeVars.TryGetValue(symbol, out var current))
        {
            var lowered = WrappingCompoundAssignExpr(targetItem, operation, valueItem);
            if (lowered is not Assign assignment || ComptimeIntValue(assignment.Value) is not { } value)
                throw new IrUnsupportedException("zig wrapping comptime variable requires a compile-time-known integer RHS");
            System.Numerics.BigInteger exact = operation switch
            {
                BinOp.Add => (System.Numerics.BigInteger)current.Value + value,
                BinOp.Sub => (System.Numerics.BigInteger)current.Value - value,
                BinOp.Mul => (System.Numerics.BigInteger)current.Value * value,
                _ => throw new IrUnsupportedException("invalid wrapping comptime operation"),
            };
            var integer = (CType.Prim)current.Type.Unqualified;
            var bits = assignment.ZigWrappingBits ?? integer.Bytes * 8;
            var modulus = System.Numerics.BigInteger.One << bits;
            var normalized = exact & (modulus - 1);
            if (integer.Signed && normalized >= (modulus >> 1)) normalized -= modulus;
            if (normalized < long.MinValue || normalized > long.MaxValue)
                throw new IrUnsupportedException("zig wrapping comptime variable exceeds the supported 64-bit signed binding");
            _comptimeVars[symbol] = ((long)normalized, current.Type);
            return new Seq([]);
        }
        return RejectConstStore(targetItem)
           ?? Hoisted(() => new ExprStmt(WrappingCompoundAssignExpr(targetItem, operation, valueItem)));
    }

    private CExpr WrappingCompoundAssignExpr(Item targetItem, BinOp operation, Item valueItem)
    {
        if (targetItem.Content is Zig.Ident identifier && _symbols.Resolve(Tok(identifier.Arg0)) is { } symbol
            && _zigConstBindings.Contains(symbol))
            throw new CompileException("zig: cannot assign to constant '" + symbol.Name + "'");
        RejectConstStore(targetItem);
        var hoistsBefore = _hoist?.Count ?? 0;
        var assignment = CompoundAssignExpr(targetItem, operation, valueItem);
        if (assignment is not Assign assign) return assignment;
        if (assign.Target.Type.Unqualified is not CType.Prim { Integer: true } integer)
            throw new IrUnsupportedException("zig wrapping assignment requires a fixed-width scalar integer target");
        var bits = BitcastSourceLocation(targetItem, assign.Target).IntegerBits
            ?? DeclaredBitsOfValue(targetItem) ?? DeclaredBitsOfLowered(assign.Target) ?? integer.Bytes * 8;
        if (bits <= 0 || bits > integer.Bytes * 8)
            throw new IrUnsupportedException("zig wrapping assignment has an invalid logical integer width");
        if (bits < integer.Bytes * 8 && (assign.Target.Type.IsAtomic || assign.Target.Type.IsVolatile))
            throw new IrUnsupportedException("zig narrow wrapping assignment to atomic/volatile storage is unsupported");
        if (bits < integer.Bytes * 8 && (_hoist?.Count ?? 0) != hoistsBefore)
            throw new IrUnsupportedException("zig narrow wrapping assignment with statement-hoisted operands is unsupported; bind the RHS first");
        // A typed RHS must coerce to the target without truncation or a signedness change.
        var valueBits = BitcastSourceLocation(valueItem, assign.Value).IntegerBits
            ?? DeclaredBitsOfValue(valueItem) ?? DeclaredBitsOfLowered(assign.Value);
        if (valueBits is { } width && assign.Value.Type.Unqualified is CType.Prim { Integer: true } valueInteger
            && (width > bits || valueInteger.Signed != integer.Signed && (valueInteger.Signed || width >= bits)))
            throw new CompileException("zig wrapping assignment RHS cannot coerce to the target integer type");
        if ((IsIntegerLiteral(valueItem) || valueItem.Content is Zig.PreNeg negation && IsIntegerLiteral(negation.Arg1))
            && ComptimeIntValue(assign.Value) is { } literal)
        {
            var minimum = integer.Signed ? -(System.Numerics.BigInteger.One << (bits - 1)) : System.Numerics.BigInteger.Zero;
            var maximum = (System.Numerics.BigInteger.One << (integer.Signed ? bits - 1 : bits)) - 1;
            if ((System.Numerics.BigInteger)literal < minimum || (System.Numerics.BigInteger)literal > maximum)
                throw new CompileException("zig wrapping assignment integer literal does not fit the target type");
        }
        return assign with { ZigWrappingBits = bits < integer.Bytes * 8 ? bits : null };
    }
}
