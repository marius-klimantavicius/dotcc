#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

/// <summary>Capture forms of <c>if</c>: <c>if (opt) |x|</c>, <c>if (err_union) |x| … else |e|</c>, pointer captures
/// <c>|*x|</c>, as statements and as values, and a comptime-known optional selecting its branch. One concern of the
/// <see cref="ZigLowering"/> binder.</summary>
internal sealed partial class ZigLowering
{
    /// <summary>Lower a captured <c>if</c> statement: <c>if (opt) |x|</c> over an optional, an optional pointer or an
    /// error union (with <c>else |e|</c>). <paramref name="byRef"/> is the by-ref form <c>if (opt) |*x|</c> (task #94):
    /// <c>x</c> points AT the payload in place (a value optional's through <c>ZigMem.OptionalPayload</c>, an optional
    /// pointer's own slot), so the condition is used as the lvalue it names rather than copied.</summary>
    private CStmt LowerIfCapture(Item condItem, string capName, Item thenItem, Item? elseItem, string? errCapName,
        bool byRef = false, bool thenCaptured = true)
    {
        if (errCapName is null && TryOptionalTypeCaptureValue(condItem, out var optionalType))
        {
            if (byRef) throw new IrUnsupportedException("zig: a comptime optional type has no runtime payload to point at");
            if (optionalType is not { } present)
                return elseItem is { } absentArm ? LowerStmt(absentArm) : new Seq(new List<CStmt>());
            using var typeScope = EnterOptionalTypeCapture(capName, present);
            return LowerStmt(thenItem);
        }
        if (errCapName is null && TryComptimeOptionalSliceCond(condItem, out var slicePayload, out var sliceType))
        {
            if (byRef) throw new IrUnsupportedException("zig: a comptime optional slice has no runtime payload to point at");
            if (slicePayload is IrBuilder.CtNull)
                return elseItem is { } arm ? LowerStmt(arm) : new Seq(new List<CStmt>());
            using var scope = EnterSymbolScope();
            BindFoldedSliceCapture(capName, slicePayload, sliceType);
            return LowerStmt(thenItem);
        }
        if (errCapName is null && TryFieldAttrDefault(condItem, out var reflectedDefaultType, out var defaultValue))
        {
            if (byRef) throw new IrUnsupportedException("zig: a reflected comptime default has no runtime optional payload to point at");
            if (defaultValue is null)
                return elseItem is { } absent ? LowerStmt(absent) : new Seq(new List<CStmt>());
            // Presence comes from the field attributes: an optional field whose
            // default is null still has a present defaultValue result.
            var known = _ir.EvalComptimeValue(defaultValue)
                ?? throw new IrUnsupportedException("zig reflected field default must be known at compile time");
            using var scope = EnterSymbolScope();
            if (capName != "_")
            {
                var symbol = _symbols.Declare(new Symbol { Name = capName, Kind = SymKind.Var, Type = reflectedDefaultType.WithQuals(TypeQual.Const) });
                _ir.ComptimeGlobals[symbol] = known;
                if (DefaultValueCall(condItem) is var (_, typeItem))
                    RememberBitcastValue(symbol, BitcastTypeLocation(typeItem, reflectedDefaultType));
            }
            return LowerStmt(thenItem);
        }
        // Comptime fold (S4b): a captured `if` on a comptime-known optional (a `comptime x: ?T` seed)
        // selects the taken branch at lowering time. `null` → the else (empty if absent); a known payload
        // → the then with `x` bound to the literal. Only for the plain optional form (no `else |e|`).
        if (errCapName is null && TryComptimeOptionalCond(condItem, out var copt))
        {
            if (byRef)
            {
                throw new IrUnsupportedException(
                    "zig `if (opt) |*x|` over a comptime-known optional: a comptime value has no runtime payload to point at");
            }
            if (!copt.HasValue) { return elseItem is { } el ? LowerStmt(el) : new Seq(new List<CStmt>()); }
            using var symbolScope = EnterSymbolScope();
            BindFoldedCapture(capName, copt.Value, copt.Inner);
            // The payload's declared width rides the capture (`if (comptime std.math.cast(usize, v)) |x|`: 64 bits),
            // so an `anytype` it is passed to can answer `@typeInfo(@TypeOf(x)).int.bits`.
            // A `field_attrs` entry's `defaultValue(T)` (task #162) is a `T`: its receiver has no runtime value to lower.
            if (capName != "_" && _symbols.Resolve(capName) is { } foldedCap
                && (DefaultValueCall(condItem) is var (_, defaultType) ? DeclaredBitsOfTypeArg(defaultType)
                    // A `@typeInfo` payload's `sentinel()` (task #213) is comptime only: there is no value to lower.
                    : IsTypeInfoSentinelCall(condItem) ? null
                    : DeclaredBitsOfArgument(condItem))
                   is { } capBits)
            {
                RecordValueBits(foldedCap, capBits, null);
            }
            var folded = LowerStmt(thenItem);
            symbolScope.Dispose();
            return folded;
        }

        var cond = LowerExpr(condItem);
        if (!thenCaptured) { RejectIgnoredErrorPayload(cond.Type); }
        var ct = cond.Type.Unqualified;

        // Hoist a side-effecting condition to a single-eval temp (a bare var is already re-readable).
        var pre = new List<CStmt>();
        var condRef = StabilizeCaptureCondition(cond, byRef, pre);

        CExpr test;
        CExpr payloadInit;
        CType payloadType;
        if (ct is CType.Optional opt)
        {
            if (errCapName is not null)
            {
                throw new IrUnsupportedException(
                    "zig `if (optional) |x| … else |e|`: an optional has no error to capture (use a plain `else`)");
            }
            test = new Member(condRef, "HasValue", false) { Type = CType.Bool };
            payloadInit = new Member(condRef, "Value", false) { Type = opt.Inner };
            payloadType = opt.Inner;
            if (byRef)
            {
                // Into a `const` optional, the capture is a `*const T` (a store through it is rejected, task #95).
                payloadType = CapturePointerType(cond, opt.Inner);
                payloadInit = new Call("ZigMem.OptionalPayload", new List<CExpr> { AddressOfLValue(condRef) },
                    new List<CType> { new CType.Pointer(cond.Type) }) { Type = payloadType };
            }
        }
        else if (ct is CType.Pointer or CType.Func)
        {
            if (errCapName is not null)
            {
                throw new IrUnsupportedException(
                    "zig `if (optional pointer) |x| … else |e|`: a pointer optional has no error to capture (use a plain `else`)");
            }
            test = condRef;        // a pointer condition tests non-null
            payloadInit = OptionalPointerPayload(condRef);
            payloadType = OptionalPointerPayloadType(cond.Type);
            if (byRef)
            {
                // `|*p|` of an optional pointer points at the pointer variable itself (a `*const` one when the variable is).
                payloadType = CapturePointerType(cond, OptionalPointerPayloadType(cond.Type));
                payloadInit = AddressOfLValue(condRef) with { Type = payloadType };
            }
        }
        else if (ct is CType.ErrorUnion && byRef)
        {
            throw new IrUnsupportedException("zig `if (error_union) |*x|`: a by-ref capture of an error union's payload is not modeled");
        }
        else if (ct is CType.ErrorUnion eu)
        {
            // Error union (Milestone M, part 3): bind the success payload to `x` in the then-branch,
            // the error to `e` (the runtime `ushort Code`) in the else-branch. We test `__cap.IsErr`
            // (a clean bool) and emit the ERROR branch as the C# `if`, success as `else` — so no `!`
            // is needed. NOTE: this is a value inspection (`.IsErr`), NOT `try`, so it never throws a
            // ZigErrorReturn — the error is handled HERE and does not propagate to the function's
            // boundary catch. The captured error binds as `CType.ErrorSet` (rendered `ushort`, the
            // flat global code), so `e == error.Foo` compares codes (Milestone N) — what un-erased
            // the part-3 cut: a USED named `|e|` is now valid in both compilers.
            var errStmts = new List<CStmt>();
            using (EnterSymbolScope())
            {
                if (errCapName is not null && errCapName != "_")
                {
                    var errSym = _symbols.Declare(new Symbol { Name = errCapName, Kind = SymKind.Var, Type = CType.ErrorSet });
                    errStmts.Add(new DeclStmt(new List<LocalDecl> { new(errSym, new Member(condRef, "Code", false) { Type = CType.ErrorSet }) }));
                }
                if (elseItem is not null) { errStmts.Add(LowerStmt(elseItem)); }
            }

            var okStmts = new List<CStmt>();
            using (EnterSymbolScope())
            {
                if (capName != "_")
                {
                    var okSym = _symbols.Declare(new Symbol { Name = capName, Kind = SymKind.Var, Type = eu.Payload });
                    okStmts.Add(new DeclStmt(new List<LocalDecl> { new(okSym, new Member(condRef, "Value", false) { Type = eu.Payload }) }));
                }
                okStmts.Add(LowerStmt(thenItem));
            }

            var errTest = new Member(condRef, "IsErr", false) { Type = CType.Bool };
            CStmt euIf = new If(errTest, new Block(errStmts), new Block(okStmts));
            if (pre.Count > 0) { pre.Add(euIf); return new Block(pre); }
            return euIf;
        }
        else
        {
            throw new IrUnsupportedException(
                $"zig `if (...) |x|` requires an optional (or error-union) condition (got {ct.Describe()}, {_module?.Path ?? _fileStem}:{condItem.Position.Line}, function '{_currentFnName}')");
        }

        // then-branch: bind the payload at the top, with `x` in scope while lowering the branch.
        var thenStmts = new List<CStmt>();
        using (EnterSymbolScope())
        {
            if (capName != "_")
            {
                var capSym = _symbols.Declare(new Symbol { Name = capName, Kind = SymKind.Var, Type = payloadType });
                _zigConstBindings.Add(capSym);
                RememberCaptureBitcastLocation(capSym, condItem, cond, byRef);
                // The payload's declared width (`if (std.math.cast(isize, v)) |x|`: 64), for an `anytype` it reaches.
                if (DeclaredBitsOfLowered(cond) is { } runtimeCapBits) { RecordValueBits(capSym, runtimeCapBits, null); }
                thenStmts.Add(new DeclStmt(new List<LocalDecl> { new(capSym, payloadInit) }));
            }
            thenStmts.Add(LowerStmt(thenItem));
        }
        var thenBlock = new Block(thenStmts);

        var elseStmt = elseItem is null ? null : LowerStmt(elseItem);

        CStmt ifStmt = new If(test, thenBlock, elseStmt);
        if (pre.Count > 0)
        {
            pre.Add(ifStmt);
            return new Block(pre);
        }
        return ifStmt;
    }
    /// <summary><c>&amp;lvalue</c>, marking the variable at its root address-taken, as <c>&amp;x</c> in source does.</summary>
    private static CExpr AddressOfLValue(CExpr lvalue)
    {
        var root = lvalue;
        while (root is Member m) { root = m.Base; }
        if (root is VarRef { Sym: { Kind: SymKind.Var or SymKind.Param } rootSym }) { rootSym.AddressTaken = true; }
        return new Unary(UnOp.AddrOf, lvalue) { Type = new CType.Pointer(lvalue.Type) };
    }
    /// <summary>Read a value condition once, or retain the original storage of a pointer capture.
    /// A nonrepeatable lvalue needs an address temporary, never a copy of its optional payload.</summary>
    private CExpr StabilizeCaptureCondition(CExpr condition, bool byRef, List<CStmt> into, string? tempName = null)
    {
        if (byRef && !condition.IsLValue)
            throw new IrUnsupportedException("zig by-reference if capture requires addressable optional storage; rvalue optional captures are not modeled");
        if (condition is VarRef || byRef && IsRepeatableLValue(condition)) return condition;
        var init = byRef ? AddressOfLValue(condition) with { Type = CapturePointerType(condition, condition.Type) } : condition;
        // The address type retains readonly storage and pointer payload metadata across the hoist.
        var temp = _symbols.Declare(new Symbol { Name = tempName ?? "__cap", Kind = SymKind.Var, Type = init.Type });
        into.Add(new DeclStmt(new List<LocalDecl> { new(temp, init) }));
        var reference = new VarRef(temp) { Type = temp.Type, IsLValue = true };
        return byRef ? new Unary(UnOp.Deref, reference) { Type = condition.Type, IsLValue = true } : reference;
    }
    private CType.Pointer CapturePointerType(CExpr storage, CType payload)
        => SourcePointer(IsConstStorage(storage) ? payload.WithQuals(TypeQual.Const) : payload) with
        {
            ZigSourceAlignment = UsesSourceStdlib ? CaptureStorageAlignment(storage) : null,
        };

    // Retain explicit alignment on dereferenced slots. Field offsets cannot strengthen a
    // byte-aligned address; stronger nondefault alignment needs offset evidence we don't have.
    private static int? CaptureStorageAlignment(CExpr storage) => storage switch
    {
        VarRef v => v.Type.AlignmentOverride,
        Paren p => CaptureStorageAlignment(p.Inner),
        Unary { Op: UnOp.Deref, Operand.Type.Unqualified: CType.Pointer p } => p.ZigSourceAlignment,
        Member { Arrow: true, Base.Type.Unqualified: CType.Pointer p } => p.ZigSourceAlignment is 0 or 1 ? p.ZigSourceAlignment : null,
        Member m => CaptureStorageAlignment(m.Base) is 0 or 1 ? CaptureStorageAlignment(m.Base) : null,
        _ => null,
    };
    /// <summary>Lower a VALUE-position captured <c>if</c> — <c>if (opt) |x| thenE else elseE</c> (S4a),
    /// the expression sibling of <see cref="LowerIfCapture"/>. A pure ternary can't bind the payload
    /// <c>x</c>, so this hoists (ANF): a result temp is declared before the enclosing statement and
    /// assigned by a real <c>if</c> whose then-branch binds <c>x = opt.Value</c> (or the unwrapped
    /// pointer). <c>else</c> is mandatory (the grammar requires it — a value is yielded on both paths).
    /// The optional / optional-pointer forms mirror <see cref="LowerIfCapture"/>; an error-union value
    /// <c>if</c> is a separate (later) shape. Like the value <c>IfExpr</c> ternary, the branch
    /// expressions retain their own hoists inside the selected branch. Terminating arms carry
    /// noreturn control flow and never assign the result temporary.</summary>
    /// <summary>True when a captured-<c>if</c> condition is a comptime-known OPTIONAL — a bare identifier
    /// (optionally parenthesized) resolving to a <c>comptime x: ?T</c> generic seed in
    /// <see cref="_comptimeOptionalVars"/>. Yields the seed (<c>HasValue</c> / payload <c>Value</c> /
    /// <c>Inner</c> type) so <see cref="LowerIfCapture"/> / <see cref="LowerIfCaptureExpr"/> can fold to
    /// the taken branch at lowering time (road-to-zig-std S4b).</summary>
    /// <summary>True when a call's callee is declared to return <c>?comptime_int</c> (so the call is comptime by its
    /// type): a plain function, a generic's template, or a function of another module, by its return-type AST.</summary>
    private bool ReturnsOptionalComptimeInt(Item call)
    {
        // Answered from the callee's DECLARED return type where it is a generic template (the call itself may
        // already have folded to its payload literal).
        var calleeAst = call.Content switch
        {
            Zig.CallArgs { Arg2: not null } ca => ca.Arg0,
            Zig.CallArgs { Arg2: null } cn => cn.Arg0,
            _ => null,
        };
        if (calleeAst is not null && DeclaredReturnIsOptionalComptimeInt(calleeAst)) { return true; }
        var callee = call.Content switch
        {
            Zig.CallArgs { Arg2: not null } ca => ca.Arg0,
            Zig.CallArgs { Arg2: null } cn => cn.Arg0,
            _ => null,
        };
        if (callee is null) { return false; }
        CExpr lowered;
        using (EnterThrowawayHoist()) { lowered = LowerExpr(call); }
        // The lowered return type of a `?comptime_int` is `Int128?` (comptime_int is the interpreter's 128 bits).
        return lowered.Type?.Unqualified is CType.Optional { Inner: var inner } && inner.Unqualified == CType.Int128;
    }
    /// <summary>True when a callee names a generic whose declared return type is <c>?comptime_int</c>, found through a
    /// module path (<c>std.simd.suggestVectorLength</c>) or by bare name.</summary>
    private bool DeclaredReturnIsOptionalComptimeInt(Item callee)
    {
        (ZigLowering Owner, Symbol Sym)? decl = callee.Content switch
        {
            Zig.Field f when ResolveModulePath(f.Arg0)?.Lowering is { } module => module.ResolveExportedDecl(Tok(f.Arg2)),
            Zig.Ident id => _symbols.Resolve(Tok(id.Arg0)) is { } s ? (this, s) : null,
            _ => null,
        };
        return decl is { } d && d.Owner._genericFns.TryGetValue(d.Sym, out var g)
            && g.RetType.Content is Zig.TyOptional { Arg1: var inner } && IsComptimeIntType(inner);
    }
    private bool TryComptimeOptionalCond(Item condItem, out (bool HasValue, long Value, CType Inner) info)
    {
        info = default;
        var cur = condItem;
        while (cur.Content is Zig.Grouped g) { cur = g.Arg1; }
        // A comptime aggregate parameter can carry an optional field. Its source
        // field type supplies the payload type; null and the value come from the
        // existing interpreter binding, never from a runtime caller's storage.
        if (cur.Content is Zig.Field)
        {
            var root = cur;
            while (root.Content is Zig.Field field) root = field.Arg0;
            if (root.Content is Zig.Ident aggregateName
                && _symbols.Resolve(Tok(aggregateName.Arg0)) is { } aggregateSymbol
                && _ir.ComptimeGlobals.ContainsKey(aggregateSymbol))
            {
                using var hoist = EnterThrowawayHoist();
                var fieldValue = LowerExpr(cur);
                if (fieldValue.Type.Unqualified is CType.Optional { Inner: var fieldInner })
                {
                    if (_ir.EvalComptimeValue(fieldValue) is IrBuilder.CtNull)
                    {
                        info = (false, 0, fieldInner);
                        return true;
                    }
                    if (_ir.EvalComptimeValue(fieldValue) is IrBuilder.CtInt integer
                        && integer.Value >= long.MinValue && integer.Value <= long.MaxValue)
                    {
                        info = (true, (long)integer.Value, fieldInner);
                        return true;
                    }
                }
            }
        }
        // `f_attr.defaultValue(f_type)` over a comptime `field_attrs` entry (std.mem.zeroInit, task #162): checked before a
        // call is lowered below, since the receiver has no runtime value.
        if (TryFieldAttrDefaultValue(cur, out info)) { return true; }
        // `ptrInfo.sentinel()` over a folded `@typeInfo` pointer or array (std.json.static's innerParse, task #213).
        if (TryTypeInfoSentinel(cur, out info)) { return true; }
        // `if (comptime f()) |x|`: a comptime OPTIONAL value (the comptime engine's E2 runs the call now,
        // lowering its body on demand), so `x` is a comptime integer and the branch folds.
        // A call returning `?comptime_int` (`if (std.simd.suggestVectorLength(T)) |block_len|` in std.mem) is
        // comptime by its type, exactly as if it were spelled `comptime`.
        var optionalCall = cur.Content is Zig.PreComptime explicitComptime ? explicitComptime.Arg1 : cur;
        while (optionalCall.Content is Zig.Grouped groupedCall) optionalCall = groupedCall.Arg1;
        var comptimeByType = optionalCall.Content is Zig.CallArgs && ReturnsOptionalComptimeInt(optionalCall);
        if (cur.Content is Zig.PreComptime || comptimeByType)
        {
            CExpr inner;
            using (EnterThrowawayHoist()) { inner = LowerExpr(cur.Content is Zig.PreComptime pc ? pc.Arg1 : cur); }
            // A comptime-only `?comptime_int` call may already be its folded value: the payload literal itself.
            CType payload;
            if (inner.Type?.Unqualified is CType.Optional { Inner: var optionalPayload }) { payload = optionalPayload; }
            else if (comptimeByType) { payload = CType.Long; }
            else { return false; }
            // A `?comptime_int` payload is an untyped number: bind it as a plain `long` literal.
            if (payload.Unqualified == CType.Int128) { payload = CType.Long; }
            switch (_ir.ResolveComptimeFold(inner))
            {
                case DefaultLit:
                    info = (false, 0, payload);
                    return true;
                case { } lit when _ir.ConstEval(lit) is { } v:
                    info = (true, v, payload);
                    return true;
                default:
                    return false;
            }
        }
        return cur.Content is Zig.Ident id
            && _symbols.Resolve(Tok(id.Arg0)) is { } sym
            && _comptimeOptionalVars.TryGetValue(sym, out info);
    }
    /// <summary>Bind a folded captured-<c>if</c> payload <paramref name="capName"/> to its comptime
    /// literal for the taken then-branch (road-to-zig-std S4b). A narrow UNSIGNED inner type
    /// (<c>u8</c>/<c>u16</c>) is bound as <c>int</c> so the folded literal renders as a bare <c>N</c>
    /// (the value always fits) rather than <c>Nu</c> — a <c>uint</c> literal would not implicitly assign
    /// to a <c>byte</c>/<c>ushort</c> sink (CS0266). Wider / signed inners keep their type. <c>_</c>
    /// binds nothing. The caller wraps this in a fresh scope.</summary>
    private void BindFoldedCapture(string capName, long value, CType inner)
    {
        if (capName == "_") { return; }
        var bindType = inner.Unqualified is CType.Prim { Integer: true, Signed: false, Bytes: <= 2 }
            ? CType.Int
            : inner;
        var capSym = _symbols.Declare(new Symbol { Name = capName, Kind = SymKind.Var, Type = bindType });
        _comptimeVars[capSym] = (value, bindType);
    }

    private bool TryComptimeOptionalSliceCond(Item condition, out IrBuilder.ComptimeValue payload, out CType inner)
    {
        payload = null!;
        inner = null!;
        while (condition.Content is Zig.Grouped grouped) condition = grouped.Arg1;
        if (condition.Content is not Zig.Ident id || _symbols.Resolve(Tok(id.Arg0)) is not { } symbol
            || symbol.Type.Unqualified is not CType.Optional { Inner.Unqualified: CType.Slice slice }
            || !_ir.ComptimeGlobals.TryGetValue(symbol, out var value)
            || value is not (IrBuilder.CtNull or IrBuilder.CtSlice)) return false;
        (payload, inner) = (value, slice);
        return true;
    }

    private void BindFoldedSliceCapture(string name, IrBuilder.ComptimeValue payload, CType inner)
    {
        if (name == "_") return;
        var symbol = _symbols.Declare(new Symbol { Name = name, Kind = SymKind.Var, Type = inner.WithQuals(TypeQual.Const) });
        _ir.ComptimeGlobals[symbol] = payload;
    }
    /// <summary>A value-position <c>if (x) |v| a else [|e|] b</c> (<c>ifExprCapture</c>): its payloads read, then
    /// <see cref="LowerIfCaptureExpr"/>.</summary>
    private void RememberCaptureBitcastLocation(Symbol symbol, Item condition, CExpr lowered, bool byRef)
    {
        if (!byRef) RememberValueSentinel(symbol, condition);
        var payload = BitcastSourceLocation(condition, lowered);
        if (lowered.Type.Unqualified is CType.Optional optional)
            payload = payload with { Physical = optional.Inner };
        if (!byRef) RecordValueBits(symbol, payload.IntegerBits, payload.Element?.IntegerBits);
        RememberBitcastValue(symbol, byRef
            ? new ZigResultLocation(symbol.Type, Element: payload)
            : payload with { Physical = symbol.Type });
    }

    private CExpr LowerIfCaptureExprOf(Zig.IfElseExpr ec, CType? sink)
    {
        var (capture, byRef, _) = PayloadParts(ec.Arg4);
        return LowerIfCaptureExpr(ec.Arg2, capture ?? "_", ec.Arg5, ec.Arg8, sink, PayloadParts(ec.Arg7).Name,
            thenCaptured: capture is not null, byRef: byRef);
    }
    /// <summary>zig's "error union payload is ignored": <c>if (eu) a else |e| b</c> with no then-capture is legal only over a
    /// <c>!void</c>; a payload must be captured, if only as <c>|_|</c>.</summary>
    private static void RejectIgnoredErrorPayload(CType condType)
    {
        if (condType.Unqualified is CType.ErrorUnion { Payload: var payload } && !payload.Unqualified.Equals(CType.Void))
        {
            throw new CompileException("zig: error union payload is ignored (capture it with `|_|`)");
        }
    }
    private CExpr LowerIfCaptureExpr(Item condItem, string capName, Item thenItem, Item elseItem, CType? sink = null,
        string? errCapName = null, bool thenCaptured = true, bool byRef = false)
    {
        if (errCapName is null && TryOptionalTypeCaptureValue(condItem, out var optionalType))
        {
            if (byRef) throw new IrUnsupportedException("zig: a comptime optional type has no runtime payload to point at");
            if (optionalType is not { } present) return LowerCaptureBranch(elseItem, sink, _hoist);
            using var typeScope = EnterOptionalTypeCapture(capName, present);
            return LowerCaptureBranch(thenItem, sink, _hoist);
        }
        if (errCapName is null && TryComptimeOptionalSliceCond(condItem, out var slicePayload, out var sliceType))
        {
            if (byRef) throw new IrUnsupportedException("zig: a comptime optional slice has no runtime payload to point at");
            if (slicePayload is IrBuilder.CtNull) return LowerCaptureBranch(elseItem, sink, _hoist);
            using var scope = EnterSymbolScope();
            BindFoldedSliceCapture(capName, slicePayload, sliceType);
            return LowerCaptureBranch(thenItem, sink, _hoist);
        }
        // Comptime fold (S4b): if the condition is a comptime-known optional (a generic instance's
        // `comptime x: ?T` seed), select the taken branch NOW — no runtime test, no hoist. A comptime
        // `null` yields the else; a comptime-known payload yields the then with `x` bound to the literal.
        if (TryComptimeOptionalCond(condItem, out var copt))
        {
            if (byRef)
                throw new IrUnsupportedException("zig `if (opt) |*x|` over a comptime-known optional: a comptime value has no runtime payload to point at");
            if (!copt.HasValue) { return LowerCaptureBranch(elseItem, sink, _hoist); }
            using var foldedScope = EnterSymbolScope();
            BindFoldedCapture(capName, copt.Value, copt.Inner);
            var folded = LowerCaptureBranch(thenItem, sink, _hoist);
            foldedScope.Dispose();
            return folded;
        }

        var buf = RequireHoistable("value `if (opt) |x| … else …`");
        var savedImpure = _hoistImpureSeen;

        var cond = LowerExpr(condItem);
        if (!thenCaptured) { RejectIgnoredErrorPayload(cond.Type); }
        var ct = cond.Type.Unqualified;

        var condRef = StabilizeCaptureCondition(cond, byRef, buf, "__ifcapc" + _anfTempCounter++);

        CExpr test;
        CExpr payloadInit;
        CType payloadType;
        if (ct is CType.Optional opt)
        {
            test = new Member(condRef, "HasValue", false) { Type = CType.Bool };
            payloadInit = new Member(condRef, "Value", false) { Type = opt.Inner };
            payloadType = opt.Inner;
            if (byRef)
            {
                payloadType = CapturePointerType(cond, opt.Inner);
                payloadInit = new Call("ZigMem.OptionalPayload", new List<CExpr> { AddressOfLValue(condRef) },
                    new List<CType> { new CType.Pointer(cond.Type) }) { Type = payloadType };
            }
        }
        else if (ct is CType.Pointer or CType.Func)
        {
            test = condRef;          // a pointer condition tests non-null
            payloadInit = OptionalPointerPayload(condRef);
            payloadType = OptionalPointerPayloadType(cond.Type);
            if (byRef)
            {
                payloadType = CapturePointerType(cond, OptionalPointerPayloadType(cond.Type));
                payloadInit = AddressOfLValue(condRef) with { Type = payloadType };
            }
        }
        else if (ct is CType.ErrorUnion && byRef)
            throw new IrUnsupportedException("zig `if (error_union) |*x|`: a by-ref capture of an error union's payload is not modeled");
        else if (ct is CType.ErrorUnion eu)
        {
            // An error union (`if (r.getSize()) |size| … else |_| …`): the success payload binds in the then arm, the
            // error code (as in the statement form) in the else arm. A value inspection, never a `try`.
            test = new Unary(UnOp.LogNot, new Member(condRef, "IsErr", false) { Type = CType.Bool }) { Type = CType.Bool };
            payloadInit = new Member(condRef, "Value", false) { Type = eu.Payload };
            payloadType = eu.Payload;
        }
        else
        {
            throw new IrUnsupportedException(
                "zig value-position `if (...) |x| ... else ...` requires an optional, optional-pointer or error-union condition"
                + $" ({_module?.Path ?? _fileStem}:{condItem.Position.Line}, function '{_currentFnName}', {cond.Type.Describe()})");
        }
        if (errCapName is not null && ct is not CType.ErrorUnion)
        {
            throw new IrUnsupportedException("zig value `if (x) |v| … else |e| …`: only an error union has an error to capture");
        }

        // then-branch: bind the payload to `x`, then lower the then value (which may use `x`).
        var thenStmts = new List<CStmt>();
        using var thenScope = EnterSymbolScope();
        if (capName != "_")
        {
            var capSym = _symbols.Declare(new Symbol { Name = capName, Kind = SymKind.Var, Type = payloadType });
            _zigConstBindings.Add(capSym);
            RememberCaptureBitcastLocation(capSym, condItem, cond, byRef);
            thenStmts.Add(new DeclStmt(new List<LocalDecl> { new(capSym, payloadInit) }));
        }
        var thenVal = LowerCaptureBranch(thenItem, sink, thenStmts);
        thenScope.Dispose();

        var elseStmts = new List<CStmt>();
        using var elseScope = EnterSymbolScope();
        if (errCapName is not null && errCapName != "_")
        {
            var errSym = _symbols.Declare(new Symbol { Name = errCapName, Kind = SymKind.Var, Type = CType.ErrorSet });
            elseStmts.Add(new DeclStmt(new List<LocalDecl> { new(errSym, new Member(condRef, "Code", false) { Type = CType.ErrorSet }) }));
        }
        var elseVal = LowerCaptureBranch(elseItem, sink, elseStmts);
        elseScope.Dispose();
        CoerceSliceValuePeers(ref thenVal, ref elseVal);
        var resultType = NoreturnPeerType(sink, thenVal, elseVal);
        var tupleSourceBits = TuplePeerSourceBits(thenVal, elseVal);
        thenVal = CoerceTupleValuePeer(thenVal, resultType);
        elseVal = CoerceTupleValuePeer(elseVal, resultType);

        _hoistImpureSeen = savedImpure;
        if (IsNoreturn(thenVal) && IsNoreturn(elseVal))
        {
            thenStmts.Add(NoreturnStatement(thenVal));
            elseStmts.Add(NoreturnStatement(elseVal));
            return new StatementExpression(new Block(new[] { new If(test, new Block(thenStmts), new Block(elseStmts)) }), null)
                { Type = CType.NoReturn };
        }

        // A result temp (declared before the statement), assigned by each branch of a real `if`.
        var resSym = _symbols.Declare(new Symbol { Name = "__ifcap" + _anfTempCounter++, Kind = SymKind.Var, Type = resultType });
        if (tupleSourceBits is not null) _valueTupleElemBits[resSym] = tupleSourceBits;
        buf.Add(new DeclStmt(new List<LocalDecl> { new(resSym, null) }));
        var resRef = new VarRef(resSym) { Type = resultType, IsLValue = true };
        thenStmts.Add(IsNoreturn(thenVal) ? NoreturnStatement(thenVal) : new ExprStmt(new Assign(null, resRef, thenVal) { Type = resultType }));
        elseStmts.Add(IsNoreturn(elseVal) ? NoreturnStatement(elseVal) : new ExprStmt(new Assign(null, resRef, elseVal) { Type = resultType }));
        buf.Add(new If(test, new Block(thenStmts), new Block(elseStmts)));
        return new VarRef(resSym) { Type = resultType };
    }
    /// <summary>One arm's value of a value-position capture <c>if</c>, at the result type when there is one.
    /// A labeled value block (<c>if (p.peek(0)) |b| init: { …; break :init .left; } else null</c>, std.fmt's
    /// Placeholder.parse) needs statements: they go to <paramref name="into"/>, ahead of the arm's
    /// assignment, and the value is the block's result temp.</summary>
    private CExpr LowerCaptureBranch(Item item, CType? sink, List<CStmt>? into)
    {
        if (item.Content is Zig.Block)
            return new StatementExpression(new Block(new[] { LowerNoReturnProngBlock(item) }), null) { Type = CType.NoReturn };
        if (item.Content is Zig.ReturnExpr || IsJumpArm(item))
        {
            // Lower the jump's operands with their own statement hoist; it must execute only
            // in this arm, and return from the enclosing Zig function, never from a delegate.
            var statement = Hoisted(() => LowerStmt(item));
            return new StatementExpression(new Block(new[] { statement }), null) { Type = CType.NoReturn };
        }
        if (!IsLabeledValue(item))
        {
            // A branch of the hoisted capture `if` keeps what it hoists inside itself (task #203); a folded one (`into` is
            // the enclosing buffer) is the whole value, so its hoists belong there.
            if (into is null || ReferenceEquals(into, _hoist)) { return sink is { } s ? LowerExprSink(item, s) : LowerExpr(item); }
            var (branchValue, branchHoisted) = LowerArmIsolated(() => sink is { } bs ? LowerExprSink(item, bs) : LowerExpr(item));
            into.AddRange(branchHoisted);
            return branchValue;
        }
        if (into is null) { throw new IrUnsupportedException("a labeled value block as a folded `if` arm needs a statement position"); }
        Symbol? result = null;
        into.Add(LowerLabeledValue(item, sink, temp =>
        {
            result = temp;
            return new Seq(new List<CStmt>());
        }));
        return result is { } r ? new VarRef(r) { Type = r.Type }
            : throw new IrUnsupportedException("internal: a labeled value block produced no result");
    }
}
