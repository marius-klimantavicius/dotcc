using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr LowerAtomicAccess(IReadOnlyList<Item> arguments, bool store)
    {
        var builtin = store ? "@atomicStore" : "@atomicLoad";
        if (arguments.Count != (store ? 4 : 3))
            throw new CompileException($"zig {builtin}: incorrect argument count");
        var type = LowerType(arguments[0]).Unqualified;
        var packed = AtomicPackedCarrier(type);
        (CType.Prim Carrier, int Bits)? enumStorage = UsesSourceStdlib && type is CType.Enum enumeration
            ? AtomicEnumStorage(enumeration, builtin) : null;
        if (type is CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool", Bytes: 1 or 2 or 4 or 8 } integer)
        {
            if (DeclaredBitsOfTypeArg(arguments[0]) != integer.Bytes * 8)
                throw new IrUnsupportedException($"zig {builtin}: integer storage requires an exact 8, 16, 32 or 64 bit source type");
        }
        else if (type is not CType.Pointer && packed is null && enumStorage is null)
            throw new IrUnsupportedException($"zig {builtin}: this atomic storage type is not modeled ({type.Describe()})");

        var orderName = ComptimeExportTag(arguments[^1], "AtomicOrder", builtin)
            ?? throw new CompileException($"zig {builtin}: order must be a comptime-known AtomicOrder");
        var order = orderName switch
        {
            "unordered" or "monotonic" => 0,
            "acquire" when !store => 2,
            "release" when store => 3,
            "seq_cst" => 5,
            _ => throw new CompileException($"zig {builtin}: invalid memory order '{orderName}'"),
        };
        var values = new List<CExpr>();
        using var evaluation = new CallArgumentScope(this, values, _hoistImpureSeen);
        var address = LowerExpr(arguments[1]);
        if (address.Type.Unqualified is not CType.Pointer pointer || !pointer.Pointee.Unqualified.Equals(type))
            throw new CompileException($"zig {builtin}: pointer does not address the requested atomic type");
        if (store && (pointer.Pointee.IsConst
            || address is Unary { Op: UnOp.AddrOf, Operand: var target } && IsConstStorage(target)))
            throw new CompileException("zig @atomicStore: cannot write through a const pointer");
        if (pointer.ZigSourceAlignment is > 0 && pointer.ZigSourceAlignment < _ir.AlignOfConst(type))
            throw new CompileException($"zig {builtin}: pointer alignment is insufficient for atomic storage");
        if (enumStorage is { } storage)
        {
            var carrierPointer = pointer with { Pointee = storage.Carrier.WithQuals(pointer.Pointee.Quals) };
            evaluation.Add(new Cast(carrierPointer, address) { Type = carrierPointer });
        }
        else evaluation.Add(packed is null ? address : AtomicPackedAddress(address, packed));
        if (store)
        {
            var value = LowerExprSink(arguments[2], BitcastTypeLocation(arguments[0], type));
            if (enumStorage is { } storedEnum)
            {
                if (!value.Type.Unqualified.Equals(type))
                    throw new CompileException("zig @atomicStore: operand must have the requested nominal enum type");
                evaluation.Add(new Cast(storedEnum.Carrier, value) { Type = storedEnum.Carrier });
            }
            else evaluation.Add(packed is null ? value : AtomicPackedValue(value, packed, type));
        }
        values.Add(new LitInt(order.ToString(System.Globalization.CultureInfo.InvariantCulture), order) { Type = CType.Int });
        // The existing GNU helpers validate orders and use same-width Interlocked
        // accesses. Their seq-cst barriers are stronger than the weaker Zig orders.
        CExpr result = new Call(store ? "atomic_store_explicit" : "atomic_load_explicit", values)
            { Type = store ? CType.Void : enumStorage?.Carrier ?? packed ?? type };
        if (!store && enumStorage is { } loadedEnum)
        {
            var nominal = new Cast(type, result) { Type = type };
            _shared.BitcastWidths.Expressions[nominal] = new(type, loadedEnum.Bits);
            return nominal;
        }
        return !store && packed is not null ? AtomicPackedValue(result, type, type) : result;
    }
    private CExpr LowerAtomicRmw(IReadOnlyList<Item> arguments)
    {
        const string builtin = "@atomicRmw";
        if (arguments.Count != 5) throw new CompileException("zig @atomicRmw: incorrect argument count");
        var type = LowerType(arguments[0]).Unqualified;
        if (UsesSourceStdlib && type is CType.Enum enumeration) return LowerAtomicEnumExchange(arguments, enumeration);
        var packed = AtomicPackedCarrier(type);
        var integer = type as CType.Prim;
        if (type is not CType.Pointer && packed is null
            && (integer is not { Integer: true, IsComptimeInt: false, Name: not "_Bool", Bytes: 1 or 2 or 4 or 8 }
                || DeclaredBitsOfTypeArg(arguments[0]) != integer.Bytes * 8))
            throw new IrUnsupportedException("zig @atomicRmw: integer storage requires an exact 8, 16, 32 or 64 bit source type");
        var operation = ComptimeExportTag(arguments[2], "AtomicRmwOp", builtin) switch
        {
            "Add" => "__atomic_fetch_add", "Sub" => "__atomic_fetch_sub",
            "And" => "__atomic_fetch_and", "Or" => "__atomic_fetch_or", "Xor" => "__atomic_fetch_xor",
            "Xchg" => "__atomic_exchange_n",
            var other => throw new IrUnsupportedException($"zig @atomicRmw: operation '{other}' is not modeled"),
        };
        if (type is CType.Pointer && operation != "__atomic_exchange_n")
            throw new IrUnsupportedException("zig @atomicRmw: pointer storage only supports Xchg");
        if (packed is not null && operation is not ("__atomic_fetch_add" or "__atomic_fetch_sub"
            or "__atomic_fetch_and" or "__atomic_fetch_or" or "__atomic_fetch_xor" or "__atomic_exchange_n"))
            throw new IrUnsupportedException("zig @atomicRmw: packed storage supports Add/Sub, bitwise operations and Xchg");
        var order = ComptimeExportTag(arguments[4], "AtomicOrder", builtin) switch
        {
            "monotonic" => 0, "acquire" => 2, "release" => 3, "acq_rel" => 4, "seq_cst" => 5,
            var other => throw new CompileException($"zig @atomicRmw: invalid memory order '{other}'"),
        };
        var values = new List<CExpr>();
        using var evaluation = new CallArgumentScope(this, values, _hoistImpureSeen);
        var address = LowerExpr(arguments[1]);
        if (address.Type.Unqualified is not CType.Pointer pointer || !pointer.Pointee.Unqualified.Equals(type))
            throw new CompileException("zig @atomicRmw: pointer does not address the requested atomic type");
        if (pointer.Pointee.IsConst || address is Unary { Op: UnOp.AddrOf, Operand: var target } && IsConstStorage(target))
            throw new CompileException("zig @atomicRmw: cannot write through a const pointer");
        if (pointer.ZigSourceAlignment is > 0 && pointer.ZigSourceAlignment < _ir.AlignOfConst(type))
            throw new CompileException("zig @atomicRmw: pointer alignment is insufficient for atomic storage");
        var location = BitcastTypeLocation(arguments[0], type);
        if (type is CType.Pointer)
            AtomicPointerSourceWidths(BitcastPathLocation(arguments[1])?.Element ?? BitcastLoweredLocation(address).Element, location, builtin);
        if ((BitcastPathLocation(arguments[1])?.Element?.IntegerBits ?? BitcastLoweredLocation(address).Element?.IntegerBits) is { } pointeeBits
            && pointeeBits != location.IntegerBits)
            throw new CompileException("zig @atomicRmw: pointer does not address the requested atomic source type");
        evaluation.Add(packed is null ? address : AtomicPackedAddress(address, packed));
        var value = LowerExprSink(arguments[3], location);
        if (type is CType.Pointer destinationPointer)
        {
            var converted = AtomicPointerOperand(value, destinationPointer, builtin);
            if (value is not (NullPtr or DefaultLit))
                AtomicPointerSourceWidths(BitcastPathLocation(arguments[3]) ?? BitcastSourceLocation(arguments[3], value), location, builtin);
            value = converted;
        }
        else if (packed is not null)
        {
            if (!value.Type.Unqualified.Equals(type))
                throw new CompileException("zig @atomicRmw: operand must have the requested nominal packed type");
            value = AtomicPackedValue(value, packed, type);
        }
        else if (_ir.ConstEval128(value) is { } known)
        {
            var width = integer!.Bytes * 8;
            var minimum = integer.Signed ? -((System.Int128)1 << (width - 1)) : 0;
            var maximum = ((System.Int128)1 << (integer.Signed ? width - 1 : width)) - 1;
            if (known < minimum || known > maximum)
                throw new CompileException("zig @atomicRmw: operand does not fit the atomic integer type");
        }
        else
        {
            if (value.Type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool" } source)
                throw new CompileException("zig @atomicRmw: operand must coerce to the atomic integer type");
            var sourceBits = BitcastSourceLocation(arguments[3], value).IntegerBits
                ?? throw new IrUnsupportedException("zig @atomicRmw: operand's logical source width is not known");
            if (sourceBits > integer!.Bytes * 8 || source.Signed && !integer.Signed
                || !source.Signed && integer.Signed && sourceBits >= integer.Bytes * 8)
                throw new CompileException("zig @atomicRmw: operand must coerce to the atomic integer type");
        }
        evaluation.Add(new Cast(packed ?? type, value) { Type = packed ?? type });
        values.Add(new LitInt(order.ToString(System.Globalization.CultureInfo.InvariantCulture), order) { Type = CType.Int });
        var result = new Call(operation, values) { Type = packed ?? type };
        _shared.BitcastWidths.CallResults[result] = location;
        return packed is not null ? AtomicPackedValue(result, type, type) : result;
    }

    private CExpr LowerAtomicCompareExchange(IReadOnlyList<Item> arguments, string builtin)
    {
        if (arguments.Count != 6)
            throw new CompileException($"zig {builtin}: incorrect argument count");
        var type = LowerType(arguments[0]).Unqualified;
        var packed = UsesSourceStdlib ? AtomicPackedCarrier(type) : null;
        (CType.Prim Carrier, int Bits)? enumStorage = UsesSourceStdlib && type is CType.Enum enumeration
            ? AtomicEnumStorage(enumeration, builtin) : null;
        if (type is CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool", Bytes: 1 or 2 or 4 or 8 } integer)
        {
            if (DeclaredBitsOfTypeArg(arguments[0]) != integer.Bytes * 8)
                throw new IrUnsupportedException($"zig {builtin}: integer storage requires an exact 8, 16, 32 or 64 bit source type");
        }
        else if (type is not CType.Pointer && packed is null && enumStorage is null)
            throw new IrUnsupportedException($"zig {builtin}: this atomic storage type is not modeled ({type.Describe()})");

        var success = ComptimeExportTag(arguments[4], "AtomicOrder", builtin)
            ?? throw new CompileException($"zig {builtin}: success order must be a comptime-known AtomicOrder");
        var failure = ComptimeExportTag(arguments[5], "AtomicOrder", builtin)
            ?? throw new CompileException($"zig {builtin}: failure order must be a comptime-known AtomicOrder");
        if (success is not ("monotonic" or "acquire" or "release" or "acq_rel" or "seq_cst")
            || failure is not ("monotonic" or "acquire" or "seq_cst"))
            throw new CompileException($"zig {builtin}: invalid memory order (success '{success}', failure '{failure}')");
        // Pinned Zig accepts release/acquire, unlike GNU compare-exchange.
        // Validate Zig's named combinations here, then use the stronger seq-cst CAS.
        if (failure == "seq_cst" && success != "seq_cst"
            || failure == "acquire" && success == "monotonic")
            throw new CompileException($"zig {builtin}: failure memory order is stronger than success memory order");

        CExpr result;
        // Keep all operand work inside this expression, including a later operand's
        // orelse/catch statements. The snapshots are ordered pointer/expected/desired.
        // StatementExpression also keeps a CAS in a lazy branch or loop condition local.
        using (EnterFreshHoist())
        {
            var address = LowerExpr(arguments[1]);
            if (address.Type.Unqualified is not CType.Pointer pointer || !pointer.Pointee.Unqualified.Equals(type))
                throw new CompileException($"zig {builtin}: pointer does not address the requested atomic type");
            if (pointer.Pointee.IsConst
                || address is Unary { Op: UnOp.AddrOf, Operand: var target } && IsConstStorage(target))
                throw new CompileException($"zig {builtin}: cannot write through a const pointer");
            if (pointer.ZigSourceAlignment is > 0 && pointer.ZigSourceAlignment < _ir.AlignOfConst(type))
                throw new CompileException($"zig {builtin}: pointer alignment is insufficient for atomic storage");
            var location = BitcastTypeLocation(arguments[0], type);
            if (type is CType.Pointer)
                AtomicPointerSourceWidths(BitcastPathLocation(arguments[1])?.Element ?? BitcastLoweredLocation(address).Element, location, builtin);
            if (location.IntegerBits is { } bits && (BitcastPathLocation(arguments[1])?.Element?.IntegerBits ?? BitcastLoweredLocation(address).Element?.IntegerBits) is { } pointeeBits
                && bits != pointeeBits)
                throw new CompileException($"zig {builtin}: pointer does not address the requested atomic source type");
            CExpr Snapshot(CExpr value, ZigResultLocation shape)
            {
                var symbol = _symbols.Declare(new Symbol { Name = "__anf" + _anfTempCounter++, Kind = SymKind.Var, Type = value.Type });
                RememberBitcastValue(symbol, shape);
                RecordValueBits(symbol, shape.IntegerBits, shape.Element?.IntegerBits);
                _hoist!.Add(new DeclStmt(new List<LocalDecl> { new(symbol, value) }));
                _hoistImpureSeen = false;
                return new VarRef(symbol) { Type = symbol.Type };
            }
            CExpr LowerValue(int index)
            {
                var value = LowerExprSink(arguments[index], location);
                if (type is CType.Pointer destinationPointer)
                {
                    var converted = AtomicPointerOperand(value, destinationPointer, builtin);
                    if (value is not (NullPtr or DefaultLit))
                        AtomicPointerSourceWidths(BitcastPathLocation(arguments[index]) ?? BitcastSourceLocation(arguments[index], value), location, builtin);
                    return converted;
                }
                if (packed is not null)
                {
                    if (!value.Type.Unqualified.Equals(type))
                        throw new CompileException($"zig {builtin}: operand must have the requested nominal packed type");
                    return AtomicPackedValue(value, packed, type);
                }
                if (enumStorage is { } enumValue)
                {
                    if (!value.Type.Unqualified.Equals(type))
                        throw new CompileException($"zig {builtin}: operand must have the requested nominal enum type");
                    return new Cast(enumValue.Carrier, value) { Type = enumValue.Carrier };
                }
                var destination = (CType.Prim)type;
                if (value.Type.Unqualified is not CType.Prim { Integer: true, Name: not "_Bool" } source)
                    throw new CompileException($"zig {builtin}: operand must coerce to the atomic integer type");
                var width = destination.Bytes * 8;
                if (_ir.ConstEval128(value) is { } known)
                {
                    var minimum = destination.Signed ? -((System.Int128)1 << (width - 1)) : 0;
                    var maximum = ((System.Int128)1 << (destination.Signed ? width - 1 : width)) - 1;
                    if (known < minimum || known > maximum)
                        throw new CompileException($"zig {builtin}: operand does not fit the atomic integer type");
                }
                else
                {
                    var sourceWidth = BitcastSourceLocation(arguments[index], value).IntegerBits ?? source.Bytes * 8;
                    if (source.IsComptimeInt || sourceWidth > width || source.Signed && !destination.Signed
                        || !source.Signed && destination.Signed && sourceWidth >= width)
                        throw new CompileException($"zig {builtin}: operand must coerce to the atomic integer type");
                }
                return new Cast(type, value) { Type = type };
            }
            if ((enumStorage?.Carrier ?? packed) is { } atomicCarrier)
            {
                var carrierPointer = pointer with { Pointee = atomicCarrier.WithQuals(pointer.Pointee.Quals) };
                address = new Cast(carrierPointer, address) { Type = carrierPointer };
            }
            address = Snapshot(address, BitcastLoweredLocation(address));
            var expected = Snapshot(LowerValue(2), location);
            var desired = Snapshot(LowerValue(3), location);
            // Existing __sync CAS performs one same-width Interlocked operation and
            // returns its observed value. Full seq-cst barriers fulfill every valid
            // Zig order above; no enum ordinal is passed as a GNU memory order.
            // Weak may coherently use this same non-spurious strong implementation.
            var observed = Snapshot(new Call("__sync_val_compare_and_swap", [address, expected, desired]) { Type = enumStorage?.Carrier ?? packed ?? type }, location);
            CType optional = type is CType.Pointer { ZigOptional: false } nonnull
                ? nonnull with { ZigOptional = true } : new CType.Optional(type);
            var payload = packed is null ? observed : AtomicPackedValue(observed, type, type);
            if (enumStorage is not null) payload = new Cast(type, observed) { Type = type };
            var value = new CondExpr(new Binary(BinOp.Eq, observed, expected) { Type = CType.Bool },
                new DefaultLit { Type = optional }, OptionalConditionalArm(payload, optional)) { Type = optional };
            var once = Snapshot(value, location with { Physical = optional });
            result = new StatementExpression(new Block(new List<CStmt>(_hoist!)), once) { Type = optional };
        }
        if (_hoist is not null) _hoistImpureSeen = true;
        return result;
    }

    private CExpr AtomicPointerOperand(CExpr value, CType.Pointer destination, string builtin)
    {
        if (value is NullPtr or DefaultLit)
        {
            if (!destination.ZigOptional)
                throw new CompileException($"zig {builtin}: null operand requires an optional atomic pointer type");
            return new NullPtr { Type = destination };
        }
        var source = value.Type.Unqualified as CType.Pointer;
        if (source is null
            || !source.Pointee.Unqualified.Equals(destination.Pointee.Unqualified)
            || source.Pointee.IsConst && !destination.Pointee.IsConst
            || source.ZigOptional && !destination.ZigOptional
            || source.ZigSourceSize != destination.ZigSourceSize
            || destination.ZigSourceAlignment is > 0 && destination.ZigSourceAlignment >
                (source.ZigSourceAlignment is > 0 ? source.ZigSourceAlignment.Value : _ir.AlignOfConst(source.Pointee)))
            throw new CompileException($"zig {builtin}: operand must coerce to the atomic pointer type"
                + $" ({_module?.Path ?? _fileStem}, function '{_currentFnName}'; source {value.Type.Describe()}, size {source?.ZigSourceSize}, optional {source?.ZigOptional}, align {source?.ZigSourceAlignment}; destination {destination.Describe()}, size {destination.ZigSourceSize}, optional {destination.ZigOptional}, align {destination.ZigSourceAlignment})");
        return new Cast(destination, value) { Type = destination };
    }

    private static void AtomicPointerSourceWidths(ZigResultLocation? source, ZigResultLocation destination, string builtin)
    {
        // Pointer bits alone cannot distinguish *u7 from *u8. Compare witnessed
        // logical pointee widths through each pointer layer, without CLR guesses.
        for (var expected = destination.Element; expected is not null; expected = expected.Element)
        {
            source = source?.Element;
            if (expected.IntegerBits is not { } bits) continue;
            if (source?.IntegerBits is not { } actual)
                throw new IrUnsupportedException($"zig {builtin}: pointer operand's logical source pointee width is not known");
            if (actual != bits)
                throw new CompileException($"zig {builtin}: pointer operand does not have the requested atomic source pointee type");
        }
    }

    // Reuse the existing source-proven packed integer storage. No CLR field or
    // source carrier width establishes eligibility: every bit must be witnessed.
    private CType? AtomicPackedCarrier(CType type)
    {
        if (type is not CType.Named named || !_ir.ZigStorageLayouts.TryGetValue(named.Name, out var storage)
            || storage.Kind != ZigStorageKind.PackedInteger) return null;
        storage.RequireSupported();
        if (storage.LogicalBits != storage.NativeSize * 8 || storage.NativeAlignment != storage.NativeSize)
            throw new IrUnsupportedException("zig atomic packed storage requires an exact 8, 16, 32 or 64 bit source layout");
        return storage.LogicalBits switch
        {
            8 => CType.UChar, 16 => CType.UShort, 32 => CType.UInt, 64 => CType.ULong,
            _ => throw new IrUnsupportedException("zig atomic packed storage requires an exact 8, 16, 32 or 64 bit source layout"),
        };
    }

    private static CExpr AtomicPackedAddress(CExpr address, CType carrier)
    {
        var pointer = new CType.Pointer(carrier);
        return new Cast(pointer, address) { Type = pointer };
    }

    private CExpr AtomicPackedValue(CExpr value, CType target, CType packedType)
    {
        var storage = _ir.ZigStorageLayouts[((CType.Named)packedType).Name];
        var packing = target.Equals(packedType);
        if (!packing && !value.Type.Unqualified.Equals(packedType))
            throw new CompileException("zig atomic operand must have the requested packed source type");
        var packed = new ZigBitcastValueLayout(storage.LogicalBits, storage.BackingSigned, ZigBitcastRepresentation.Packed);
        var integer = new ZigBitcastValueLayout(storage.LogicalBits, false, ZigBitcastRepresentation.Integer);
        return new BitCast(target, value) { Type = target, ZigLayout = packing ? new(integer, packed) : new(packed, integer) };
    }

}
