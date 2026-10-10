namespace DotObj;

internal sealed record SourceBitfieldWidth(long? ConstantId, long? DeclaredWidth, string Kind);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceBitfieldWidth> SourceBitfieldWidths { get; } = [];

    private void ReadAndVerifySourceBitfieldWidths()
    {
        using var rows = database.Prepare("SELECT source_field_id,constant_id,declared_width,width_kind,capture_proof FROM source_bitfield_widths ORDER BY source_field_id");
        while (rows.Read())
        {
            long id = rows.Integer(0);
            var width = new SourceBitfieldWidth(rows.OptionalInteger(1), rows.OptionalInteger(2), rows.Text(3));
            if (!SourceFields.TryGetValue(id, out var field) || field.BitWidth is null ||
                width.Kind is not ("evaluated" or "dependent") || width.DeclaredWidth is < 0 ||
                (width.Kind == "dependent") != (width.DeclaredWidth is null) ||
                width.ConstantId is long constant && !SourceSemanticConstants.ContainsKey(constant) ||
                rows.Text(4) != "original-field-width" || !SourceBitfieldWidths.TryAdd(id, width))
                throw new InvalidDataException("Original bitfield width lost its exact source field or constant recipe");
            var owner = SourceRecords[field.SourceRecordId];
            if (width.Kind == "dependent")
            {
                if (!owner.IsDependent || owner.LayoutKnown || width.ConstantId is not long symbolic ||
                    !ContainsSourceParameter(symbolic, 0))
                    throw new InvalidDataException("Dependent bitfield width claims evaluated storage or lacks its symbolic recipe");
            }
            else
            {
                if (field.BitWidth > width.DeclaredWidth || width.DeclaredWidth == 0 && field.SourceName.Length != 0)
                    throw new InvalidDataException("Original declared bitfield width differs from its stored value bits");
                if (width.ConstantId is long resolved)
                {
                    var value = SourceSemanticConstants[resolved];
                    if (value.Kind != "integer" || !System.Numerics.BigInteger.TryParse(value.ValueText,
                        System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var integer) ||
                        integer != width.DeclaredWidth)
                        throw new InvalidDataException("Evaluated bitfield width disagrees with its original integer constant");
                }
            }
        }
        foreach (var field in SourceFields.Values)
            if ((field.BitWidth is not null) != SourceBitfieldWidths.ContainsKey(field.Id))
                throw new InvalidDataException("Source bitfield is missing its required original width metadata");
    }

    internal string SourceBitfieldWidthIdentity(long fieldId)
        => !SourceBitfieldWidths.TryGetValue(fieldId, out var width) ? "not-bitfield"
            : width.Kind == "evaluated" ? "width:" + width.DeclaredWidth!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "dependent:" + SourceConstantIdentity(width.ConstantId!.Value, 0);
}
