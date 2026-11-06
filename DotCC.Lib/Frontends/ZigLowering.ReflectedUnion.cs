#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private int _reflectedUnionCount;

    private CType ReifyUnionBuiltin(string context, Zig.BuiltinCall call)
    {
        var args = Flatten(call.Arg2);
        if (args.Count != 5) throw new CompileException("zig: @Union expects layout, tag type, names, types, attributes");
        var layout = ReifiedStructLayout(args[0]);
        var tagType = IsComptimeNull(args[1]) ? null : (LowerType(args[1]).Unqualified as CType.Enum
            ?? throw new CompileException("zig: @Union tag type must be an enum or null"));
        if (tagType is not null && layout != AggregateLayout.Default)
            throw new CompileException("zig: tagged @Union requires auto layout");
        if (layout == AggregateLayout.Packed) throw new IrUnsupportedException("zig: reflected packed union storage is not modeled");
        var names = ReifiedFieldNames(context, args[2]);
        var types = ReifiedList(context, args[3], names.Count, "field_types", LowerType);
        var attrs = TryFoldTypeInfoList(StripAddrOf(args[4]), out var list) && list.Attrs is { } attributes
            ? attributes.ToList() : ReifiedList(context, args[4], names.Count, "field_attrs", ReadTypeBodyAttribute);
        if (attrs.Count != names.Count) throw new CompileException("zig: @Union field attribute count differs from field count");
        var fields = new List<StructField>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < names.Count; i++)
        {
            if (!seen.Add(names[i])) throw new CompileException("zig: duplicate union field " + names[i]);
            var attr = attrs[i];
            if (attr.Comptime || attr.HasDefault) throw new CompileException("zig: union field attributes cannot contain a default or comptime field");
            var align = attr.Align ?? 0;
            if (align < 0 || align > int.MaxValue || align != 0 && (align & (align - 1)) != 0)
                throw new CompileException("zig: invalid union field alignment");
            // A `type` payload is compile-time data, with source alignment one;
            // it never acquires CLR storage. Keep other explicit storage cuts.
            if (tagType is not null && align != 0 && !(align == 1 && types[i].Unqualified is CType.ZigMetaType))
                throw new IrUnsupportedException("zig: tagged @Union with explicit field alignment is not modeled");
            fields.Add(new(names[i], types[i], Alignment: (int)align));
        }
        var name = QualifyTypeName("__ReflectedUnion" + _reflectedUnionCount++);
        var type = new CType.Named(name);
        _containerTypes[name] = type;
        if (tagType is null) RegisterAggregate(name, fields, isUnion: true, layout);
        else
        {
            ValidateUnionTagFields(tagType, names);
            if (!MembersOfEnum(tagType)!.Select(m => m.Name).SequenceEqual(names))
                throw new CompileException("zig: @Union field order does not match tag enum field order");
            FinishUnion(name, fields.Select(f => (f.Name, f.Type.Unqualified is CType.VoidType ? null : (CType?)f.Type)).ToList(), tagType);
            _unions[name] = _unions[name] with { SourceFields = fields };
        }
        return type;
    }

    private CExpr LowerUnionInitBuiltin(IReadOnlyList<Item> args)
    {
        if (args.Count != 3) throw new CompileException("zig: @unionInit expects type, field name, value");
        var type = LowerType(args[0]);
        if (type.Unqualified is not CType.Named named) throw new CompileException("zig: @unionInit requires a union type");
        var name = ComptimeStringArg(args[1]) ?? throw new CompileException("zig: @unionInit field name must be comptime-known");
        if (_unions.TryGetValue(named.Name, out var union))
        {
            if (!union.Variants.TryGetValue(name, out var payload)) throw new CompileException("zig: union has no field " + name);
            if (payload is null)
                return BuildVoidUnionValue(union, name, LowerExprSink(args[2], CType.Void));
            RejectNoreturnStorage(payload);
            var value = LowerExprSink(args[2], payload);
            var storageType = new CType.Named(union.PayloadTypeName!);
            var storage = new StructInit([new(name, payload, value)]) { Type = storageType };
            return new StructInit([
                new(union.TagFieldName, union.TagType, ResolveEnumLit(name, union.TagType)),
                new(union.PayloadFieldName, storageType, storage)]) { Type = type };
        }
        if (!_ir.StructIsUnion.GetValueOrDefault(named.Name)) throw new CompileException("zig: @unionInit requires a union type");
        var fieldType = _ir.StructFieldType(type, name) ?? throw new CompileException("zig: union has no field " + name);
        var fieldValue = LowerExprSink(args[2], fieldType);
        return new StructInit(fieldType.Unqualified is CType.VoidType ? [] : [new(name, fieldType, fieldValue)]) { Type = type };
    }

    private CExpr BuildVoidUnionValue(ZigUnionInfo union, string name, CExpr effect)
    {
        var type = new CType.Named(union.Name);
        if (IsNoreturn(effect)) return CoerceNoreturn(effect, type);
        if (effect.Type.Unqualified is not CType.VoidType)
            throw new CompileException("zig: union void field requires a void value");
        var initialized = BuildVoidVariant(union, name);
        return effect is DefaultLit or ZigUndefinedValue ? initialized
            : new StatementExpression(new Block([new ExprStmt(effect)]), initialized) { Type = type };
    }
}
