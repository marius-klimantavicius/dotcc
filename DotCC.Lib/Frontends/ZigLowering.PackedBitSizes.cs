#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Source evidence belongs to this invocation's IR, including imported modules.
    // A physical carrier (u3 -> byte) never establishes a packed field's width.
    private sealed class PackedBitSizeSources
    {
        public Dictionary<string, PackedSource> Packed { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, PackedSource> Enums { get; } = new(StringComparer.Ordinal);
    }

    private static readonly ConditionalWeakTable<IrBuilder, PackedBitSizeSources> PackedBitSizeEvidence = new();

    private void CapturePackedBitSizeSources(IEnumerable<(string? Name, object? Content, string? Parent)> declarations)
    {
        var evidence = PackedBitSizeEvidence.GetOrCreateValue(_ir);
        foreach (var (name, content, _) in declarations)
        {
            if (name is null) continue;
            switch (content)
            {
                case Zig.PackedStructDecl:
                    evidence.Packed[name] = new(this, null, null);
                    break;
                case Zig.PackedStructDeclBacked backed:
                    evidence.Packed[name] = new(this, backed.Arg6, null);
                    break;
                case Zig.EnumDeclTyped typed:
                    evidence.Enums[name] = new(this, typed.Arg5, null);
                    break;
            }
        }
    }

    /// <summary>Native logical width of a source-proved packed struct, independently of storage layout.
    /// Unrecorded declarations, unknown widths, non-packed children and recursive layouts stay unsupported.</summary>
    private int? PackedBitSizes(string name)
    {
        var remainingFields = 4096;
        return PackedBitSizes(name, new HashSet<string>(StringComparer.Ordinal), 0, ref remainingFields);
    }

    private int? PackedBitSizes(string name, HashSet<string> active, int depth, ref int remainingFields)
    {
        if (depth >= 32 || !_ir.PackedStructs.Contains(name)
            || _ir.StructFieldsOf(name) is not { } fields
            || !PackedBitSizeEvidence.TryGetValue(_ir, out var evidence)
            || !evidence.Packed.TryGetValue(name, out var source) || !active.Add(name)) return null;
        try
        {
            using var environment = source.Owner.EnterPackedSource(name, source);
            var isUnion = _ir.StructIsUnion.GetValueOrDefault(name);
            if (isUnion && !source.Owner._storagePackedUnionSources.Contains(name)) return null;
            long total = 0;
            foreach (var field in fields)
            {
                if (--remainingFields < 0 || field.IsAnonBitField) return null;
                var type = field.Type.Unqualified;
                int? bits = field.BitWidth;
                if (bits is null)
                {
                    bits = type switch
                    {
                        CType.Prim { Name: "_Bool" } => 1,
                        CType.Prim { Integer: true, IsComptimeInt: false } =>
                            _structFieldBits.TryGetValue((name, field.Name), out var declared) ? declared
                            : _shared.BitcastWidths.Fields.TryGetValue((name, field.Name), out var shape) ? shape.IntegerBits : null,
                        CType.Prim { Integer: false } p when p == CType.Float || p == CType.Double => p.Bytes * 8,
                        CType.Float128Type => 128,
                        CType.Pointer => 64,
                        CType.VoidType => 0,
                        CType.Enum e => PackedEnumBitSize(e),
                        CType.Named nested => PackedBitSizes(nested.Name, active, depth + 1, ref remainingFields),
                        _ => null,
                    };
                }
                if (bits is null or < 0) return null;
                if (isUnion && total != 0 && total != bits.Value) return null;
                total = isUnion ? bits.Value : total + bits.Value;
                if (total > int.MaxValue) return null;
            }
            if (source.Backing is { } backing)
            {
                // Lower first so a constructed backing records its logical width. Never use its carrier size.
                if (source.Owner.LowerType(backing).Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false }
                    || source.Owner.DeclaredBitsOfTypeArg(backing) is not { } backingBits) return null;
                if (backingBits != total)
                    throw new IrUnsupportedException($"zig `@bitSizeOf({name})`: packed backing width {backingBits} does not match field width {total}");
            }
            return (int)total;
        }
        finally { active.Remove(name); }
    }

    private int? PackedEnumBitSize(CType.Enum type)
    {
        if (TypeBodyEnumWidth(type.Name) is { } reified) return reified;
        if (!PackedBitSizeEvidence.TryGetValue(_ir, out var evidence)
            || !evidence.Enums.TryGetValue(type.Name, out var source)) return null;
        // Native Zig forbids inferred enum tags inside packed structs.
        if (source.Backing is null) return null;
        using var scope = source.Owner.EnterPackedSource(type.Name, source);
        _ = source.Owner.LowerType(source.Backing);
        return source.Owner.DeclaredBitsOfTypeArg(source.Backing);
    }
}
