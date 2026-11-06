#nullable enable

using System.Collections.Generic;
using System.Numerics;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

/// <summary>Curated <c>std.testing</c> assertions for <c>dotcc zig test</c>. Like <c>std.mem</c> /
/// <c>std.debug.print</c>, this is a curated-paths resolver, not a general <c>std</c> model — only the
/// two most common assertions are lowered; anything else is a loud, specific cut.
///
/// <para>Each assertion lowers to a call of the runtime <see cref="DotCC.Libc.ZigTesting"/> helper,
/// typed as an error union over <c>void</c> (<c>ErrUnion&lt;Unit&gt;</c>). A test body reaches them
/// through <c>try std.testing.expect(…)</c>: on failure the returned <c>Err</c> makes <c>try</c>
/// propagate a <c>ZigErrorReturn</c> to the test function's boundary, which the generated test runner
/// observes as a failing test.</para></summary>
internal sealed partial class ZigLowering
{
    private bool NeedsSourceTestingCall(string method, IReadOnlyList<Item> arguments)
    {
        if (!UsesSourceStdlib) return false;
        if (method == "expectEqualSlices" && arguments.Count == 3)
            // CLR enums and CBool do not satisfy the adapter's IEquatable<T>
            // constraint. Source testing retains its actual element comparison.
            return LowerType(arguments[0]).Unqualified is not CType.Prim { Name: not "_Bool" };
        if (method == "expectError" && arguments.Count == 2)
            // Source !void has its proved zero-payload carrier, not ErrUnion<T>.
            return TypeOfOperand(arguments[1]).Type.Unqualified is CType.ErrorUnion { Payload.Unqualified: CType.VoidType };
        if (method != "expectEqual" || arguments.Count != 2
            || TryFoldTypeEquality(arguments[0], arguments[1]) is not null) return false;
        // Literal enum peers retain the existing result-location path below.
        if (UnwrapGrouped(arguments[0]).Content is Zig.EnumLit || UnwrapGrouped(arguments[1]).Content is Zig.EnumLit)
            return false;
        // This probe is unevaluated. The chosen call lowers each real argument
        // once through its existing call-argument evaluation scope.
        using var hoist = EnterThrowawayHoist();
        var expected = LowerTestingEqualityArgument(arguments[0], null);
        var actual = LowerTestingEqualityArgument(arguments[1], null);
        foreach (var value in new[] { expected, actual })
        {
            if (value.Type.Unqualified is CType.Pointer or CType.Func or CType.Slice or CType.Array or CType.Optional) return true;
            if (value.Type.Unqualified is CType.Named named && _ir.StructFieldsOf(named.Name) is not null
                && _ir.StructIsUnion.TryGetValue(named.Name, out var union) && !union) return true;
        }
        NormalizeTestingEqualityPeers(arguments, ref expected, ref actual);
        return expected.Type.Unqualified != actual.Type.Unqualified;
    }

    private CExpr LowerTestingEqualityArgument(Item source, CType.Enum? literalPeer)
    {
        var value = literalPeer is not null ? LowerExprSink(source, literalPeer) : LowerExpr(source);
        return UsesSourceStdlib && IsZigBoolValue(value) && value.Type.Unqualified != CType.Bool
            ? new Cast(CType.Bool, value) { Type = CType.Bool } : value;
    }

    private void NormalizeTestingEqualityPeers(IReadOnlyList<Item> source, ref CExpr expected, ref CExpr actual)
    {
        var originalExpected = expected;
        expected = CoerceComptimeIntegerPeer(source[0], expected, source[1], actual, arithmetic: true, out _);
        actual = CoerceComptimeIntegerPeer(source[1], actual, source[0], originalExpected, arithmetic: true, out _);
        expected = CoerceTestingIntegerConstant(source[0], expected, source[1], actual);
        actual = CoerceTestingIntegerConstant(source[1], actual, source[0], originalExpected);
    }

    private CExpr CoerceTestingIntegerConstant(Item source, CExpr value, Item peerSource, CExpr peer)
    {
        // Upstream expectEqual is inline: a known typed constant still coerces
        // by value at its call site. Do not turn it into a runtime i32 parameter
        // before comparing a cached `const limit: usize = 0` with a runtime usize.
        if (!UsesSourceStdlib || value.Type.Unqualified is not CType.Prim { Integer: true, Name: not ("_Bool" or "char") }
            || peer.Type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not ("_Bool" or "char") }
            || IsSourceComptimeInteger(peerSource, peer) || !IsComptimeNameRead(value)
            || _ir.EvalComptimeValue(value) is not IrBuilder.CtInt constant) return value;
        var bits = DeclaredBitsOfValue(peerSource) ?? DeclaredBitsOfLowered(peer)
            ?? BitcastLoweredLocation(peer).IntegerBits;
        var target = ZigIntOperandType(peerSource) ?? peer.Type;
        if (bits is not (> 0 and <= 128) || target.Unqualified is not CType.Prim { Integer: true } integer)
            return value;
        var number = BigInteger.CreateChecked(constant.Value);
        var minimum = integer.Signed ? -(BigInteger.One << (bits.Value - 1)) : BigInteger.Zero;
        var maximum = (BigInteger.One << (integer.Signed ? bits.Value - 1 : bits.Value)) - 1;
        if (number < minimum || number > maximum) return value;
        var converted = _ir.SpliceComptimeValue(new IrBuilder.CtInt(constant.Value, target))!;
        _shared.BitcastWidths.Expressions[converted] = new(target, bits);
        return converted;
    }

    /// <summary>Lower a curated <c>std.testing</c> call. <c>expect(ok: bool)</c> and
    /// <c>expectEqual(expected, actual)</c> are modeled; any other member is a clear cut.</summary>
    private CExpr LowerStdTestingCall(string method, IReadOnlyList<Item> argItems)
    {
        var errUnionVoid = new CType.ErrorUnion(CType.Void);
        switch (method)
        {
            case "expect":
                if (argItems.Count != 1)
                {
                    throw new IrUnsupportedException(
                        $"zig `std.testing.expect` expects one argument (ok: bool); got {argItems.Count}");
                }
                var cond = LowerExprSink(argItems[0], CType.Bool);
                return new Call("ZigTesting.expect", new List<CExpr> { cond }, new List<CType> { CType.Bool })
                {
                    Type = errUnionVoid,
                };
            case "expectEqual":
                if (argItems.Count != 2)
                {
                    throw new IrUnsupportedException(
                        $"zig `std.testing.expectEqual` expects two arguments (expected, actual); got {argItems.Count}");
                }
                // Type values have no runtime carrier. Compare their source
                // identity and logical widths through the existing type-equality
                // path, retaining the ordinary assertion error on a mismatch.
                if (UsesSourceStdlib && TryFoldTypeEquality(argItems[0], argItems[1]) is { } equalTypes)
                    return equalTypes
                        ? new ErrUnionOk(null) { Type = errUnionVoid }
                        : new ErrUnionErr(LowerErrorLit("TestExpectedEqual")) { Type = errUnionVoid };
                // A bare enum literal adopts the other operand's source enum.
                // Probe its type without executing it, then lower each operand
                // once in source order. The literal has no runtime carrier.
                var expectedEnum = UnwrapGrouped(argItems[0]).Content is Zig.EnumLit
                    ? TypeOfOperand(argItems[1]).Type.Unqualified as CType.Enum : null;
                var actualEnum = UnwrapGrouped(argItems[1]).Content is Zig.EnumLit
                    ? TypeOfOperand(argItems[0]).Type.Unqualified as CType.Enum : null;
                var equalityValues = new List<CExpr>(2);
                using (var evaluation = new CallArgumentScope(this, equalityValues, _hoistImpureSeen))
                {
                    evaluation.Add(LowerTestingEqualityArgument(argItems[0], expectedEnum));
                    evaluation.Add(LowerTestingEqualityArgument(argItems[1], actualEnum));
                }
                var expected = equalityValues[0];
                var actual = equalityValues[1];
                NormalizeTestingEqualityPeers(argItems, ref expected, ref actual);
                return new Call("ZigTesting.expectEqual", new List<CExpr> { expected, actual })
                {
                    Type = errUnionVoid,
                };
            case "expectError":
                if (argItems.Count != 2)
                {
                    throw new IrUnsupportedException(
                        $"zig `std.testing.expectError` expects two arguments (expected_error, actual); got {argItems.Count}");
                }
                // `error.X` lowers to its flat code (CType.ErrorSet → ushort); the actual result must
                // be an error union (the un-`try`'d result of a fallible call), whose code is compared.
                var expectedErr = LowerExpr(argItems[0]);
                var actualResult = LowerExpr(argItems[1]);
                if (actualResult.Type.Unqualified is not CType.ErrorUnion)
                {
                    throw new IrUnsupportedException(
                        "zig `std.testing.expectError` expects an error-union second argument (a fallible "
                        + $"call's result, not `try`'d), got {actualResult.Type.Describe()}");
                }
                return new Call("ZigTesting.expectError", new List<CExpr> { expectedErr, actualResult })
                {
                    Type = errUnionVoid,
                };
            case "expectEqualStrings":
                if (argItems.Count != 2)
                {
                    throw new IrUnsupportedException(
                        $"zig `std.testing.expectEqualStrings` expects two arguments (expected, actual); got {argItems.Count}");
                }
                var strSink = new CType.Slice(CType.UChar.WithQuals(TypeQual.Const));   // []const u8
                var strA = LowerExprSink(argItems[0], strSink);
                var strB = LowerExprSink(argItems[1], strSink);
                return new Call("ZigTesting.expectEqualStrings", new List<CExpr> { strA, strB })
                {
                    Type = errUnionVoid,
                };
            case "expectEqualSlices":
                if (argItems.Count != 3)
                {
                    throw new IrUnsupportedException(
                        $"zig `std.testing.expectEqualSlices` expects three arguments (T, expected, actual); got {argItems.Count}");
                }
                // First argument is the element TYPE (like `std.mem.eql`); C# infers the generic `T`
                // from the coerced slice operands, so it is not passed through to the emitted call.
                var elem = LowerType(argItems[0]).Unqualified;
                var sliceSink = new CType.Slice(elem.WithQuals(TypeQual.Const));
                var slA = LowerExprSink(argItems[1], sliceSink);
                var slB = LowerExprSink(argItems[2], sliceSink);
                return new Call("ZigTesting.expectEqualSlices", new List<CExpr> { slA, slB })
                {
                    Type = errUnionVoid,
                };
            default:
                throw new IrUnsupportedException(
                    $"zig `std.testing.{method}` is not supported yet — `dotcc zig test` models `expect`, "
                    + "`expectEqual`, `expectError`, `expectEqualStrings`, and `expectEqualSlices` "
                    + "(road-to-zig-std; more assertions grow on demand)");
        }
    }
}
