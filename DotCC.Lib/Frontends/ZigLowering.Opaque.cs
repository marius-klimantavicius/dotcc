#nullable enable

using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Opaque definitions have no field table or emitted aggregate declaration.
    // The existing owner-qualified container names retain named identity; inline
    // occurrences additionally retain their captured generic environment.
    private sealed record InlineOpaque(string Name, InlineCaptures Captures);
    private readonly Dictionary<Item, List<InlineOpaque>> _inlineOpaques = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

    private CType ReifyInlineOpaque(Item occurrence, Item? members)
    {
        var captures = CaptureInlineEnvironment(occurrence);
        if (!_inlineOpaques.TryGetValue(occurrence, out var instances))
            _inlineOpaques[occurrence] = instances = new();
        if (instances.FirstOrDefault(i => i.Captures.SameEnvironment(captures)) is { } existing)
            return new CType.Opaque(existing.Name);
        if (members is not null)
            throw new IrUnsupportedException("zig: inline opaque members require a named opaque declaration");
        var name = QualifyTypeName("__opaque_" + _inlineOpaques.Count + "_" + instances.Count);
        instances.Add(new InlineOpaque(name, captures));
        return new CType.Opaque(name);
    }

    private CStmt LowerLocalOpaque(string name, Item? members)
    {
        // Local containers use a fresh nominal identity, including in generic instances.
        if (members is not null)
            throw new IrUnsupportedException("zig: local opaque members are unsupported");
        var mangled = LocalContainerName(name);
        if (!_localContainers.Add(mangled))
            throw new IrUnsupportedException($"zig: duplicate in-function container `{name}`");
        _localContainerShadows.Add((name, _containerTypes.TryGetValue(name, out var previous) ? previous : null));
        _containerTypes[name] = new CType.Opaque(mangled);
        return new Seq(new List<CStmt>());
    }

    private static bool IsOpaqueNullPointer(CExpr? value) => value switch
    {
        NullPtr => true,
        Paren p => IsOpaqueNullPointer(p.Inner),
        ComptimeFold fold => IsOpaqueNullPointer(fold.Resolved),
        _ => false,
    };

    private static void RequireOpaqueValueType(CType type)
    {
        switch (type.Unqualified)
        {
            case CType.VoidType { ZigAnyopaque: true }:
                throw new IrUnsupportedException("opaque type 'anyopaque' has no value or layout");
            case CType.Opaque opaque:
                throw new IrUnsupportedException("opaque type '" + opaque.Name + "' has no value or layout");
            case CType.Array array: RequireOpaqueValueType(array.Element); break;
            case CType.Optional optional: RequireOpaqueValueType(optional.Inner); break;
            case CType.ErrorUnion error: RequireOpaqueValueType(error.Payload); break;
            case CType.Tuple tuple: foreach (var element in tuple.Elements) RequireOpaqueValueType(element); break;
            // Pointers and callback signatures carry addresses, never pointee values.
        }
    }

    // C# erases only the target spelling of these addresses. Source coercions
    // must still distinguish independently declared opaque pointees, including
    // inside callback signatures. An explicit @ptrCast has the destination IR
    // type already; ordinary conversion to anyopaque remains a valid erasure.
    private static void RequireOpaquePointerCoercion(CType target, CType source)
    {
        static bool ContainsOpaque(CType type) => type.Unqualified switch
        {
            CType.Opaque => true,
            CType.Pointer pointer => ContainsOpaque(pointer.Pointee),
            CType.Func fn => ContainsOpaque(fn.Return) || fn.Params.Any(ContainsOpaque),
            CType.ErrorUnion error => ContainsOpaque(error.Payload),
            CType.Optional optional => ContainsOpaque(optional.Inner),
            _ => false,
        };
        static bool Compatible(CType target, CType source)
        {
            target = target.Unqualified;
            source = source.Unqualified;
            return (target, source) switch
            {
                (CType.Pointer { Pointee.Unqualified: CType.VoidType }, CType.Pointer) => true,
                (CType.Pointer a, CType.Pointer b) => Compatible(a.Pointee, b.Pointee),
                (CType.Func a, CType.Func b) => a.Params.Count == b.Params.Count
                    && Compatible(a.Return, b.Return) && a.Params.Zip(b.Params).All(p => Compatible(p.First, p.Second)),
                (CType.ErrorUnion a, CType.ErrorUnion b) => Compatible(a.Payload, b.Payload),
                (CType.Optional a, CType.Optional b) => Compatible(a.Inner, b.Inner),
                _ => target == source,
            };
        }
        // Null/undefined expressions have no pointer type to compare here.
        if (target.Unqualified is not (CType.Pointer or CType.Func)
            || source.Unqualified is not (CType.Pointer or CType.Func)) return;
        if ((ContainsOpaque(target) || ContainsOpaque(source)) && !Compatible(target, source))
            throw new IrUnsupportedException("incompatible opaque pointer types: expected "
                + target.Describe() + ", found " + source.Describe());
    }
}
