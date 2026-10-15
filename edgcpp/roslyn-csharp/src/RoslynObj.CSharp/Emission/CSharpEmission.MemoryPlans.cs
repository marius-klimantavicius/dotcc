using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string VolatileMemory = "global::RoslynObj.CSharp.Runtime.TargetVolatileMemory";

        // Called by the occurrence classifier at each node; its existing
        // operand recursion discovers nested actual accesses. Pointer-to-volatile
        // qualification alone is a representation view, not an object fetch.
        private static bool NeedsPlannedMemoryAccess(ExpressionOperation expression) =>
            expression.Details.VolatileFetch || IsVolatileObjectType(expression.Type);

        private static bool TryVolatileStorageRepresentation(CppType type, out CppType representation)
        {
            representation = type;
            if (!IsVolatileObjectType(type)) return false;
            representation = MemoryRepresentationType(type);
            return true;
        }

        private static bool IsVolatileObjectType(CppType type)
        {
            var seen = new HashSet<CppType>();
            for (var current = type; seen.Add(current);)
            {
                if (current is QualifiedType { Qualification.IsVolatile: true }) return true;
                if (current is not TypeReference reference) return false;
                current = reference.UnderlyingType;
            }
            return false;
        }

        // Representation/dispatch inspection only. This never emits a load.
        // Unlike the general execution Unwrap, volatile qualification remains
        // available separately to the explicit fetch/store node below.
        private static CppType MemoryRepresentationType(CppType type)
        {
            var seen = new HashSet<CppType>();
            for (var current = type; seen.Add(current);)
            {
                if (current.Details.IsAtomic || current.Details.SizeKind.Name == "upc_threads")
                    throw Limit("Atomic-qualified/UPC object access requires a separate memory plan.", "volatile");
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if (qualifiers.IsAtomic || qualifiers.AddressSpace is not (null or 0) || qualifiers.IsShared ||
                        qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed || qualifiers.IsNear || qualifiers.IsFar || qualifiers.IsUnaligned)
                        throw Limit("This memory address/qualifier representation is unsupported.", "volatile");
                }
                if (current is not TypeReference reference) return current;
                current = reference.UnderlyingType;
            }
            throw Limit("Cyclic memory type wrappers have no selected representation.", "volatile");
        }

        private static LValuePlan MemoryPlace(CppType type, LValueKind kind, CsExpression operand, FieldSymbol? field = null,
            VariableSymbol? variable = null)
            => new(type, kind, operand, field, IsVolatileObjectType(type), variable);

        private ExpressionPlan ApplyRecordedVolatileFetch(ExpressionOperation expression, ExpressionPlan plan)
        {
            if (!expression.Details.VolatileFetch) return plan;
            if (plan.Value is not CsLoad)
                throw Limit("This volatile fetch occurrence needs a separately exercised value/effect plan.", "volatile");
            var place = plan.Place ?? throw Limit("A recorded volatile fetch needs its actual retained addressable location.", "volatile");
            RequireVolatilePrimitive(expression.Type, place);
            return plan with { Value = new CsLoad(place, VolatileFetch: true),
                Effects = plan.Effects | ExpressionEffects.ReadStorage | ExpressionEffects.VolatileRead | ExpressionEffects.MayThrow };
        }

        private string RequireVolatilePrimitive(CppType type, LValuePlan place)
        {
            if (place.IsBitField) throw Limit("Volatile bitfield access requires an exercised exact-span plan.", "volatile");
            var raw = MemoryRepresentationType(type);
            if (raw is not ArithmeticType arithmetic || arithmetic.Arithmetic.ArithmeticKind.Name != "unsigned_integer" ||
                arithmetic.Arithmetic.Signed != false || arithmetic.Arithmetic.IsBool || arithmetic.Arithmetic.Bits is not (16 or 32) ||
                type.Size != raw.Size || type.Alignment != raw.Alignment || raw.Size != arithmetic.Arithmetic.Bits / 8 || raw.Alignment != raw.Size)
                throw Limit("Only captured naturally aligned unsigned16/unsigned32 volatile value accesses are exercised.", "volatile");
            return arithmetic.Arithmetic.Bits == 16 ? "UInt16" : "UInt32";
        }

        private string RenderMemoryLoad(CsLoad load)
        {
            if (load.VolatileFetch)
            {
                var suffix = RequireVolatilePrimitive(load.Place.Type, load.Place);
                return $"{VolatileMemory}.Load{suffix}((void*)({RenderAddress(load.Place)}))";
            }
            if (load.Place.VolatileTarget)
                throw Limit("A volatile-qualified lvalue value needs a recorded fetch; address-only representation does not imply one.", "volatile");
            return RenderPlace(load.Place);
        }

        private void EmitMemoryStore(CsStore store, CodeWriter writer)
        {
            if (!store.Place.VolatileTarget)
            {
                writer.Line(RenderPlace(store.Place) + " = " + RenderExpression(store.Value) + ";");
                return;
            }
            var suffix = RequireVolatilePrimitive(store.Place.Type, store.Place);
            writer.Line($"{VolatileMemory}.Store{suffix}((void*)({RenderAddress(store.Place)}), {RenderExpression(store.Value)});");
        }

        private static ExpressionEffects StoreEffects(LValuePlan place) => ExpressionEffects.WriteStorage |
            (place.VolatileTarget ? ExpressionEffects.VolatileWrite | ExpressionEffects.MayThrow : ExpressionEffects.None);
        private static CsLoad ReadSelectedPlace(LValuePlan place) => new(place, VolatileFetch: place.VolatileTarget);
    }
}
