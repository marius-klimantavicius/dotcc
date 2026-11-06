#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<string> _deferredTypeAliasesInProgress = new(StringComparer.Ordinal);

    // Item identity supplies nominal source identity; each site's captured environment supplies
    // specialization identity. Failed entries retain their diagnostic, never an incomplete layout.
    private sealed class DeferredInlineStruct(string name, InlineCaptures captures)
    {
        internal readonly string Name = name;
        internal readonly InlineCaptures Captures = captures;
        internal ExceptionDispatchInfo? Failure;
    }

    private sealed record InlineCaptures(
        IReadOnlyList<TypeSeed> Types,
        IReadOnlyList<ValueSeed> Values,
        IReadOnlyList<(string name, bool hasValue, long value, CType inner)> Optionals,
        List<(string name, IrBuilder.ComptimeValue value, CType type)> Aggregates,
        IReadOnlyList<(string name, ZigLowering owner, Symbol fn)> Functions,
        string ValueKey)
    {
        internal bool SameEnvironment(InlineCaptures other) => Types.SequenceEqual(other.Types)
            && Values.SequenceEqual(other.Values) && Optionals.SequenceEqual(other.Optionals)
            && Functions.SequenceEqual(other.Functions) && ValueKey == other.ValueKey
            && Aggregates.Select(a => (a.name, a.type)).SequenceEqual(other.Aggregates.Select(a => (a.name, a.type)));
    }

    private readonly Dictionary<Item, List<DeferredInlineStruct>> _deferredInlineStructs = new(ReferenceEqualityComparer.Instance);
    private int _deferredInlineStructCount;

    private CType ReifyDeferredInlineStruct(Item occurrence, Item? members, AggregateLayout layout)
    {
        var captures = CaptureInlineEnvironment(occurrence);
        if (!_deferredInlineStructs.TryGetValue(occurrence, out var instances))
            _deferredInlineStructs[occurrence] = instances = new();
        if (instances.FirstOrDefault(i => i.Captures.SameEnvironment(captures)) is { } existing)
        {
            existing.Failure?.Throw();
            return new CType.Named(existing.Name); // includes a recursive @This pointer during registration
        }
        var name = QualifyTypeName($"__AnonStruct{_deferredInlineStructCount++}");
        var entry = new DeferredInlineStruct(name, captures);
        instances.Add(entry);
        var type = new CType.Named(name);
        _containerTypes[name] = type;
        if ((_currentContainer ?? _typeBodyInstance) is { } parent) { _containerParents[name] = parent; }
        _reifiedSeeds[name] = (captures.Types, captures.Values, captures.Optionals);
        if (captures.Functions.Count > 0) { _reifiedFunctionSeeds[name] = captures.Functions; }
        if (captures.Aggregates.Count > 0) { _reifiedAggregateSeeds[name] = captures.Aggregates; }
        _shared.ContainerConstOwners[name] = this;
        if (layout == AggregateLayout.Packed)
        {
            var backing = occurrence.Content is Zig.InlineLayoutStructTypeBacked backed ? backed.Arg3 : null;
            PackedBitSizeEvidence.GetOrCreateValue(_ir).Packed[name] = new(this, backing, captures);
        }
        try
        {
            using var container = EnterContainer(name);
            var (fields, methods, consts, containers) = members is { } body ? SplitMembers(body)
                : (new List<Item>(), new List<Item>(), new List<Item>(), new List<Item>());
            var nested = CollectNestedContainers(name, containers);
            var nestedMethods = new List<(string container, Item fnDef)>();
            foreach (var (nestedName, content, parentName) in nested)
            {
                RegisterContainerName(nestedName, content, nestedMethods);
                ScopeNestedContainer(nestedName, content, parentName);
                _shared.PendingNestedBodies[nestedName] = (content, nestedMethods, name, this);
            }
            _currentContainer = name;
            methods = DeclareTypeReturningMembers(name, methods);
            RegisterContainerConsts(name, consts);
            foreach (var (nestedName, _, _) in nested) { EnsureNestedBody(nestedName); }
            _currentContainer = name;
            RegisterStruct(name, fields, layout);
            foreach (var (owner, method) in nestedMethods.Concat(methods.Select(m => (name, m))))
            {
                var declared = TryDeclareReifiedMethod(owner, method, name);
                _currentContainer = name;
                if (declared is not { } m || IsFnTemplate(m.sym)) { continue; }
                var pending = new PendingReifiedMethod(m.sym, owner, m.ps, m.body,
                    captures.Types, captures.Values, captures.Optionals, captures.Functions);
                // Root-created anonymous containers need the same demand rule as imported ones.
                _shared.DormantMethodBodies[m.sym] = () => _pendingReifiedMethods.Add(pending);
            }
            return type;
        }
        catch (Exception failure)
        {
            entry.Failure = ExceptionDispatchInfo.Capture(failure);
            throw;
        }
    }

    private InlineCaptures CaptureInlineEnvironment(Item occurrence)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var work = new Stack<Item>();
        work.Push(occurrence);
        while (work.TryPop(out var item))
        {
            if (item.Content is Zig.Ident id) { names.Add(Tok(id.Arg0)); }
            if (item.Content is not { } content) { continue; }
            foreach (var child in InlineChildren(content)) { if (child is not null) { work.Push(child); } }
        }
        var types = new List<TypeSeed>();
        var values = new List<ValueSeed>();
        var optionals = new List<(string, bool, long, CType)>();
        var aggregates = new List<(string, IrBuilder.ComptimeValue, CType)>();
        var functions = new List<(string, ZigLowering, Symbol)>();
        var key = new StringBuilder().Append(_typeBodyInstance).Append('|')
            .Append(_currentInstantiation?.Instance.Name).Append('|')
            .Append(_currentContainer is { } lexical ? ReifiedAncestor(lexical) : null).Append('|');
        var frozen = new Dictionary<IrBuilder.ComptimeValue, IrBuilder.ComptimeValue>(ReferenceEqualityComparer.Instance);
        foreach (var name in names.OrderBy(n => n, StringComparer.Ordinal))
        {
            var symbol = _symbols.Resolve(name);
            if (_typeAliases.TryGetValue(name, out var alias))
            {
                types.Add(new TypeSeed(name, alias, _declaredIntBits.TryGetValue(name, out var bits) ? bits : null,
                    _declaredPtrSize.GetValueOrDefault(name), _declaredSentinel.GetValueOrDefault(name)));
            }
            else if (_fnAliases.TryGetValue(name, out var fn)) { functions.Add((name, fn.Owner, fn.Sym)); }
            else if (symbol is not null && _comptimeOptionalVars.TryGetValue(symbol, out var optional))
                optionals.Add((name, optional.HasValue, optional.Value, optional.Inner));
            else if (symbol is not null && _comptimeVars.TryGetValue(symbol, out var scalar))
                values.Add(new ValueSeed(name, scalar.Value, scalar.Type, DeclaredBitsOfLowered(new VarRef(symbol) { Type = symbol.Type })));
            else if (symbol is { IsGlobal: false, IsConstexpr: true })
                values.Add(new ValueSeed(name, symbol.ConstValue, symbol.Type,
                    DeclaredBitsOfLowered(new VarRef(symbol) { Type = symbol.Type })));
            else
            {
                // Only an existing comptime binding is captured. Runtime symbols are never evaluated
                // into a closure; a demanded reference to one remains an unsupported runtime capture.
                CExpr? expression = _comptimeValues.GetValueOrDefault(name);
                if (symbol is not null && (_ir.ComptimeGlobals.ContainsKey(symbol) || _typeBodyValueSession?.Frame.ContainsKey(symbol) == true))
                    expression = new VarRef(symbol) { Type = symbol.Type };
                else if (symbol is not null && _unfoldedConstInits.TryGetValue(symbol, out var constantInit))
                    expression = constantInit;
                if (expression is null || _ir.EvalComptimeValue(expression) is not { } value) { continue; }
                value = FreezeInlineValue(value, frozen);
                aggregates.Add((name, value, expression.Type));
                key.Append(name.Length).Append(':').Append(name).Append('=');
                AppendInlineValue(key, value);
            }
        }
        return new InlineCaptures(types, values, optionals, aggregates, functions, key.ToString());
    }

    private static IrBuilder.ComptimeValue FreezeInlineValue(IrBuilder.ComptimeValue value,
        Dictionary<IrBuilder.ComptimeValue, IrBuilder.ComptimeValue> copies)
    {
        if (copies.TryGetValue(value, out var existing)) { return existing; }
        switch (value)
        {
            case IrBuilder.CtArray a:
                var array = new IrBuilder.CtArray(new IrBuilder.ComptimeValue[a.Elems.Length], a.Element, a.Type);
                copies[value] = array;
                for (var i = 0; i < a.Elems.Length; i++) { array.Elems[i] = FreezeInlineValue(a.Elems[i], copies); }
                return array;
            case IrBuilder.CtStruct s:
                var result = new IrBuilder.CtStruct(new Dictionary<string, IrBuilder.ComptimeValue>(), s.Type) { Active = s.Active };
                copies[value] = result;
                foreach (var (name, field) in s.Fields) { result.Fields[name] = FreezeInlineValue(field, copies); }
                return result;
            case IrBuilder.CtSlice s: return s with { Backing = (IrBuilder.CtArray)FreezeInlineValue(s.Backing, copies) };
            case IrBuilder.CtElemPtr p: return p with { Backing = (IrBuilder.CtArray)FreezeInlineValue(p.Backing, copies) };
            default: return value;
        }
    }

    // A full structural spelling avoids treating a short generic digest as proof of equality.
    private static void AppendInlineValue(StringBuilder key, IrBuilder.ComptimeValue value)
    {
        switch (value)
        {
            case IrBuilder.CtInt i: key.Append('i').Append(i.Value.ToString(CultureInfo.InvariantCulture)).Append(';'); break;
            case IrBuilder.CtBool b: key.Append(b.Value ? "true;" : "false;"); break;
            case IrBuilder.CtFloat f: key.Append('f').Append(BitConverter.DoubleToInt64Bits(f.Value)).Append(';'); break;
            case IrBuilder.CtNull: key.Append("null;"); break;
            case IrBuilder.CtType t: key.Append('t').Append((int)t.Value.Quals).Append(':').Append(t.Value.Describe()).Append(';'); break;
            case IrBuilder.CtEnumLiteral e: key.Append('e').Append(e.Name.Length).Append(':').Append(e.Name); break;
            case IrBuilder.CtArray a:
                key.Append('['); foreach (var element in a.Elems) { AppendInlineValue(key, element); } key.Append(']'); break;
            case IrBuilder.CtStruct s:
                key.Append('{').Append(s.Active).Append(':');
                foreach (var (name, field) in s.Fields.OrderBy(f => f.Key, StringComparer.Ordinal))
                { key.Append(name.Length).Append(':').Append(name); AppendInlineValue(key, field); }
                key.Append('}'); break;
            case IrBuilder.CtSlice s:
                key.Append('<').Append(s.Offset).Append(':').Append(s.Length).Append(':'); AppendInlineValue(key, s.Backing); key.Append('>'); break;
            case IrBuilder.CtUndefined undefined: key.Append('u').Append(undefined.Type.Describe()).Append(';'); break;
            default: throw new IrUnsupportedException("zig: anonymous container capture has an unsupported comptime value");
        }
    }

    // Exhaustive generated-record child access keeps capture discovery compatible with trimming/AOT.
    // Unknown future Zig productions fail visibly instead of silently dropping a capture.
    private static Item?[] InlineChildren(object content) => content switch
    {
        Zig.DeclsCons n => [n.Arg0, n.Arg1],
        Zig.DeclsOne n => [n.Arg0],
        Zig.PubFn n => [n.Arg0, n.Arg1],
        Zig.InlineFn n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.NoinlineFn n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.PubVar n => [n.Arg0, n.Arg1],
        Zig.ExportFn n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ExportVar n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.PubContainer n => [n.Arg0, n.Arg1],
        Zig.TestDeclNamed n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TestDeclIdent n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TestDeclAnon n => [n.Arg0, n.Arg1],
        Zig.TopComptime n => [n.Arg0, n.Arg1],
        Zig.TopField n => [n.Arg0, n.Arg1],
        Zig.ExternFnProto n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.ExternVar n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.ExternConst n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.OpaqueDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.StructDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ExternStructDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.PackedStructDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.PackedStructDeclBacked n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10, n.Arg11],
        Zig.EnumDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.EnumDeclTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.UnionHead n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.UnionArgEnum n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.UnionArgTag n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.UnionDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.FnDef n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.CallConv n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.ParamsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ParamsOne n => [n.Arg0],
        Zig.ParamsTrail n => [n.Arg0, n.Arg1],
        Zig.Param n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ParamComptime n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.ParamVariadic n => [n.Arg0],
        Zig.ParamUnnamed n => [n.Arg0],
        Zig.ConstDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.ConstDeclTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.VarDecl n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.VarDeclTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.VarDeclThreadLocal n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ConstDeclTypedMods n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.VarDeclTypedMods n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ConstDeclMods n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.VarDeclMods n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.DeclModAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.DeclModLinksection n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.DeclModAlignLink n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.Block n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtsCons n => [n.Arg0, n.Arg1],
        Zig.StmtsOne n => [n.Arg0],
        Zig.StmtIf n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.ElseStmt n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtComptimeIf n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.StmtComptimeIfElse n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.ThenExpr n => [n.Arg0],
        Zig.AssignArm n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtIfThen n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.WhileStmtThen n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ForStmtThen n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.WhileStmt n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ForStmt n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.StmtSwitch n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.WhileContAssign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.WhileContExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.WhileContBlock n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.AopAssign n => [n.Arg0],
        Zig.AopAdd n => [n.Arg0],
        Zig.AopSub n => [n.Arg0],
        Zig.AopMul n => [n.Arg0],
        Zig.AopDiv n => [n.Arg0],
        Zig.AopMod n => [n.Arg0],
        Zig.AopShl n => [n.Arg0],
        Zig.AopShr n => [n.Arg0],
        Zig.AopBitAnd n => [n.Arg0],
        Zig.AopBitOr n => [n.Arg0],
        Zig.AopBitXor n => [n.Arg0],
        Zig.AopAddWrap n => [n.Arg0],
        Zig.AopSubWrap n => [n.Arg0],
        Zig.AopMulWrap n => [n.Arg0],
        Zig.AopAddSat n => [n.Arg0],
        Zig.AopSubSat n => [n.Arg0],
        Zig.AopMulSat n => [n.Arg0],
        Zig.StmtBreak n => [n.Arg0, n.Arg1],
        Zig.StmtContinue n => [n.Arg0, n.Arg1],
        Zig.StmtBreakValue n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtBreakLabelValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.StmtBreakLabel n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.StmtContinueLabel n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.StmtContinueLabelValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.StmtDefer n => [n.Arg0, n.Arg1],
        Zig.StmtErrdefer n => [n.Arg0, n.Arg1],
        Zig.LabeledLoop n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.LabeledBlockStmt n => [n.Arg0],
        Zig.InlineLoop n => [n.Arg0, n.Arg1],
        Zig.StmtReturn n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtReturnVoid n => [n.Arg0, n.Arg1],
        Zig.ReturnStructType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.ReturnPackedStructType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.ReturnPackedStructTypeBacked n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9],
        Zig.ReturnExternStructType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.ReturnStructMember n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ComptimeVarDecl n => [n.Arg0, n.Arg1],
        Zig.ComptimeBlock n => [n.Arg0, n.Arg1],
        Zig.ComptimeLoop n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StmtDestructure n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.StmtAssign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.StmtExpr n => [n.Arg0, n.Arg1],
        Zig.DestructBindsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.DestructBindsOne n => [n.Arg0],
        Zig.DestructBindConst n => [n.Arg0, n.Arg1],
        Zig.DestructBindConstTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.DestructBindVar n => [n.Arg0, n.Arg1],
        Zig.DestructBindVarTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.DestructBindLValue n => [n.Arg0],
        Zig.OrElse n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CatchOp n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CatchCapture n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.OrElseArm n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CatchArm n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CatchCaptureArm n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.FbBlock n => [n.Arg0],
        Zig.VoidValue n => [n.Arg0, n.Arg1],
        Zig.IfElseExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.SwitchExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.SwitchExprEmpty n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.LabeledBlock n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.LabeledSwitch n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.LoopBodyExpr n => [n.Arg0],
        Zig.LoopElse n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.LabeledLoopExpr n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BoolOr n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BoolAnd n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpEq n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpNe n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpLt n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpGt n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpLe n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CmpGe n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BitAnd n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BitXor n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BitOr n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Shl n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Shr n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Add n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Sub n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Concat n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.AddWrap n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.SubWrap n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.AddSat n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.SubSat n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Mul n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.DivOp n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ModOp n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.MulWrap n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.MulSat n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ErrSetMerge n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Repeat n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.PreNot n => [n.Arg0, n.Arg1],
        Zig.PreNeg n => [n.Arg0, n.Arg1],
        Zig.PreNegWrap n => [n.Arg0, n.Arg1],
        Zig.PreBitNot n => [n.Arg0, n.Arg1],
        Zig.PreAddrOf n => [n.Arg0, n.Arg1],
        Zig.PreTry n => [n.Arg0, n.Arg1],
        Zig.PreComptime n => [n.Arg0, n.Arg1],
        Zig.PreNosuspend n => [n.Arg0, n.Arg1],
        Zig.TypedStructInit n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TyOptional n => [n.Arg0, n.Arg1],
        Zig.TyPointer n => [n.Arg0, n.Arg1],
        Zig.TyPtrConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TyPointerPointer n => [n.Arg0, n.Arg1],
        Zig.TyPointerPointerConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TyCPtr n => [n.Arg0, n.Arg1],
        Zig.TyCPtrConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TyManyPtr n => [n.Arg0, n.Arg1],
        Zig.TyManyPtrConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TySentPtr n => [n.Arg0, n.Arg1],
        Zig.TySentPtrConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TySentPtrAlign n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TySentPtrConstAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TySentSlice n => [n.Arg0, n.Arg1],
        Zig.TySentSliceConst n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TySlice n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TySliceConst n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TyVolatile n => [n.Arg0, n.Arg1],
        Zig.TyPointerAlign n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TyPtrConstAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TyManyPtrAlign n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TyManyPtrConstAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TySliceAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TySliceConstAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.TySentSliceExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.TySentSliceConstExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.TySentSliceAlignExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.TySentSliceConstAlignExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.TySentPtrExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.TySentPtrConstExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.TyArray n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.TySentArray n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.TyTuple n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineOpaqueType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineStructType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineEnumType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineEnumTypeTyped n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.InlineUnion n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineLayoutStructType n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.InlineLayoutStructTypeBacked n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.TyFn n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.LayoutExtern n => [n.Arg0],
        Zig.LayoutPacked n => [n.Arg0],
        Zig.EnumTagInt n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ErrUnion n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TupleTypesCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TupleTypesOne n => [n.Arg0],
        Zig.TupleTypesTrail n => [n.Arg0, n.Arg1],
        Zig.FnTypeParamsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.FnTypeParamsOne n => [n.Arg0],
        Zig.FnTypeParamsTrail n => [n.Arg0, n.Arg1],
        Zig.FnTypeParamUnnamed n => [n.Arg0],
        Zig.FnTypeParamNamed n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.FnTypeParamComptime n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.FnTypeParamComptimeUnnamed n => [n.Arg0, n.Arg1],
        Zig.FnTypeParamVariadic n => [n.Arg0],
        Zig.Field n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.Deref n => [n.Arg0, n.Arg1],
        Zig.Unwrap n => [n.Arg0, n.Arg1],
        Zig.Index n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.SliceRange n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.SliceOpen n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.SliceRangeSentinel n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.SliceOpenSentinel n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.CallArgs n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.Ident n => [n.Arg0],
        Zig.IntLit n => [n.Arg0],
        Zig.FloatLit n => [n.Arg0],
        Zig.StrLit n => [n.Arg0],
        Zig.Grouped n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.BuiltinCall n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.NullLit n => [n.Arg0],
        Zig.UndefinedLit n => [n.Arg0],
        Zig.TrueLit n => [n.Arg0],
        Zig.FalseLit n => [n.Arg0],
        Zig.CharLit n => [n.Arg0],
        Zig.EnumLit n => [n.Arg0, n.Arg1],
        Zig.AnonStructInit n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.AnonStructInitEmpty n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StructMemberExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.ErrorLit n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ErrorSet n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.ErrSetOne n => [n.Arg0],
        Zig.ErrSetOneTrailing n => [n.Arg0, n.Arg1],
        Zig.ErrSetCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ArgsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ArgsOne n => [n.Arg0],
        Zig.ArgsTrail n => [n.Arg0, n.Arg1],
        Zig.PtrAlign n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.PtrAllowzero n => [n.Arg0],
        Zig.VoidArg n => [n.Arg0, n.Arg1],
        Zig.ForObjsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ForObj n => [n.Arg0],
        Zig.ForObjFrom n => [n.Arg0, n.Arg1],
        Zig.ForObjRange n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ForCapsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ForCapture n => [n.Arg0, n.Arg1],
        Zig.ProngsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ProngsOne n => [n.Arg0],
        Zig.Prong n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.InlineProng n => [n.Arg0, n.Arg1],
        Zig.ProngAssign n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.ProngComptimeBlock n => [n.Arg0, n.Arg1],
        Zig.Payload n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.PayloadTag n => [n.Arg0, n.Arg1],
        Zig.CaseValsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CaseValsOne n => [n.Arg0],
        Zig.CaseElse n => [n.Arg0],
        Zig.CaseRangeOne n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.CaseRangeCons n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.CaseValsTrail n => [n.Arg0, n.Arg1],
        Zig.CaseRangeTrail n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.FieldDeclsCons n => [n.Arg0, n.Arg1],
        Zig.FieldDeclsOne n => [n.Arg0],
        Zig.MemberField n => [n.Arg0, n.Arg1],
        Zig.MemberFieldLast n => [n.Arg0],
        Zig.MemberMethod n => [n.Arg0, n.Arg1],
        Zig.MemberInlineMethod n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.MemberNoinlineMethod n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.MemberExternFn n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.MemberExternVar n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.MemberExternConst n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.MemberComptime n => [n.Arg0, n.Arg1],
        Zig.MemberTest n => [n.Arg0],
        Zig.TestBlockNamed n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TestBlockIdent n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.TestBlockAnon n => [n.Arg0, n.Arg1],
        Zig.MemberConst n => [n.Arg0, n.Arg1],
        Zig.MemberContainer n => [n.Arg0, n.Arg1],
        Zig.StructField n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.StructFieldDefault n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.StructFieldAligned n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.StructFieldAlignedDefault n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8],
        Zig.EnumFieldsCons n => [n.Arg0, n.Arg1],
        Zig.EnumFieldsOne n => [n.Arg0],
        Zig.EnumMemberFieldLast n => [n.Arg0],
        Zig.EnumMemberField n => [n.Arg0, n.Arg1],
        Zig.EnumField n => [n.Arg0],
        Zig.EnumFieldInit n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.UnionVariantsCons n => [n.Arg0, n.Arg1],
        Zig.UnionVariantsOne n => [n.Arg0],
        Zig.UnionMemberVariant n => [n.Arg0, n.Arg1],
        Zig.UnionMemberVariantLast n => [n.Arg0],
        Zig.UnionVariantPayload n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.UnionVariantVoid n => [n.Arg0],
        Zig.FieldInitsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.FieldInitsOne n => [n.Arg0],
        Zig.FieldInitsTrail n => [n.Arg0, n.Arg1],
        Zig.FieldInit n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.FieldInitPositional n => [n.Arg0],
        Zig.AsmExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4],
        Zig.AsmVolatileExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.AsmTailNone n => [],
        Zig.AsmTailOut n => [n.Arg0, n.Arg1],
        Zig.AsmTailIn n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.AsmTailClobbers n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.AsmItemsNone n => [],
        Zig.AsmItemsOne n => [n.Arg0],
        Zig.AsmItemsCons n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.AsmItemExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.AsmItemReturn n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ReturnExpr n => [n.Arg0, n.Arg1],
        Zig.FbBreak n => [n.Arg0],
        Zig.FbBreakLabel n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.FbBreakLabelValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.FbBreakValue n => [n.Arg0, n.Arg1],
        Zig.FbContinue n => [n.Arg0],
        Zig.FbContinueLabel n => [n.Arg0, n.Arg1, n.Arg2],
        Zig.FbContinueLabelValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3],
        Zig.IfThenExpr n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5],
        Zig.EmptyStmt n => [n.Arg0],
        Zig.TyIf n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6],
        Zig.WhileValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7],
        Zig.ForValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10],
        Zig.InlineForValue n => [n.Arg0, n.Arg1, n.Arg2, n.Arg3, n.Arg4, n.Arg5, n.Arg6, n.Arg7, n.Arg8, n.Arg9, n.Arg10, n.Arg11],
        _ when content.GetType().DeclaringType == typeof(Zig) => throw new IrUnsupportedException("zig: anonymous container capture walker does not recognize " + content.GetType().Name),
        _ => Array.Empty<Item?>(),
    };
}
