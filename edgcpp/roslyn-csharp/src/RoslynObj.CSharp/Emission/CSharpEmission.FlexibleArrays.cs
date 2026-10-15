using System.Numerics;
using System.Text;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly Dictionary<FieldSymbol, (ArrayType Array, BigInteger Offset)?> flexibleFields = [];

        private bool TryFlexibleTail(FieldSymbol field, out ArrayType array, out BigInteger offset)
        {
            if (!flexibleFields.TryGetValue(field, out var retained))
            {
                // This is field representation classification, not a fetch.
                // Retain volatile access handling at its recorded occurrence.
                var raw = MemoryRepresentationType(field.Type);
                retained = null;
                if (raw is ArrayType candidate && candidate.Array.BoundKind.Name == "incomplete")
                {
                    var record = field.ContainingType as RecordType
                        ?? throw Limit("An incomplete array field has no captured record owner.", "array");
                    var layout = record.Layout ?? throw Limit("A flexible tail needs captured field offsets.", "array");
                    var entry = layout.Fields.SingleOrDefault(item => item.Field.Equals(field))
                        ?? throw Limit("The selected flexible field is absent from its captured layout.", "array");
                    if (record.Kind.Name == "tk_union" || candidate.VlaDimension is not null || candidate.Size is not null
                        || entry.Offset is not { } displacement || record.Size is not { } headerSize
                        || displacement > headerSize || displacement <= 0
                        || entry.BitOffset is { } bitOffset && bitOffset != 0 || field.Details.IsBitField
                        || layout.Fields.Any(item => !item.Field.Equals(field) && item.Offset >= displacement)
                        || candidate.ElementType.Size is not { } stride || stride <= 0
                        || candidate.ElementType.Alignment is not { } alignment || alignment <= 0 || alignment > 8
                        || displacement % alignment != 0)
                        throw Limit("Only a captured terminal incomplete array within the positive header boundary with complete ordinary elements is admitted.", "array");
                    retained = (candidate, displacement);
                }
                flexibleFields.Add(field, retained);
            }
            array = retained?.Array!;
            offset = retained?.Offset ?? BigInteger.Zero;
            return retained is not null;
        }

        private bool IsFlexibleTailField(FieldSymbol field) => TryFlexibleTail(field, out _, out _);

        private string FlexibleTailAddress(FieldSymbol field, string receiver)
        {
            if (!IsFlexibleTailField(field)) throw Limit("A selected field has no flexible-tail address projection.", "array");
            return $"{TypeName(field.ContainingType)}.{FieldName(field)}({receiver})";
        }

        private bool TryEmitFlexibleTail(FieldSymbol field, StringBuilder output)
        {
            if (!TryFlexibleTail(field, out var array, out var offset)) return false;
            // No CLR element is inserted into the header. The caller supplies
            // the captured object address, and native pointer arithmetic uses
            // the complete element representation's exact stride.
            output.Append("    public static ").Append(TypeName(array.ElementType)).Append("* ").Append(FieldName(field))
                .Append('(').Append(TypeName(field.ContainingType)).Append("* __self) => (")
                .Append(TypeName(array.ElementType)).Append("*)((byte*)__self + ").Append(offset).AppendLine(");");
            return true;
        }

        private bool NeedsFlexibleTailPlan(ExpressionOperation expression)
            => expression is OperatorOperation { OperatorKind.Name: "eok_dot_field" or "eok_points_to_field" } member
                && member.Operands.LastOrDefault() is ReferenceOperation { Target: FieldSymbol field }
                && IsFlexibleTailField(field);
    }
}
