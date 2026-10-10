using System.Globalization;
using System.Numerics;
using System.Text;

namespace DotObj;

internal sealed record SourceSemanticConstant(long Id, string Kind, long SourceTypeId, string SourceName,
    string ValueText, long? ParameterDepth, long? ParameterPosition, bool IsPack, string OperatorName,
    long? OwnerSourceTypeId, long? OperandSourceTypeId, long? BuiltinOperatorKind, bool IsStandardAlignof, long? SourceFileId, long Line, long Column);
internal sealed record SourceRecordInstantiation(string ResolutionKind, long? PrototypeSourceRecordId, long? SourceTemplateId, long? MemberOwnerSourceTypeId,
    bool MemberOfUnknownBase, bool QualifiedUnknownBaseMember);
internal sealed record SourceTemplateArgument(long Ordinal, string Kind, long? TypeId, long? ConstantId, long? TemplateId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceSemanticConstant> SourceSemanticConstants { get; } = [];
    public Dictionary<long, long[]> SourceSemanticConstantOperands { get; } = [];
    public Dictionary<long, long> SourceArrayBounds { get; } = [];
    public Dictionary<long, SourceRecordInstantiation> SourceRecordInstantiations { get; } = [];
    public Dictionary<long, List<SourceTemplateArgument>> SourceTemplateArguments { get; } = [];

    private void ReadSourceTemplateMetadata()
    {
        using (var q = database.Prepare("SELECT id,kind,source_type_id,source_name,value_text,parameter_depth,parameter_position,is_pack,operator_name,owner_source_type_id,operand_source_type_id,builtin_operator_kind,is_standard_alignof,source_file_id,line,column FROM source_semantic_constants ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceSemanticConstant(q.Integer(0), q.Text(1), q.Integer(2), q.Text(3), q.Text(4),
                    q.OptionalInteger(5), q.OptionalInteger(6), q.Integer(7) == 1, q.Text(8), q.OptionalInteger(9), q.OptionalInteger(10), q.OptionalInteger(11), q.Integer(12) == 1, q.OptionalInteger(13), q.Integer(14), q.Integer(15));
                if (value.Id <= 0 || !SourceTypes.ContainsKey(value.SourceTypeId) || q.Integer(7) is not (0 or 1) || q.Integer(12) is not (0 or 1) ||
                    value.Kind is not ("integer" or "parameter" or "operation" or "expression" or "member" or "type-operand" or "builtin-trait" or "sizeof" or "alignof" or "datasizeof") || value.Line < 0 || value.Column < 0 ||
                    !SourceSemanticConstants.TryAdd(value.Id, value)) throw new InvalidDataException("Invalid original semantic constant");
            }
        var operands = new Dictionary<long, List<long>>();
        using (var q = database.Prepare("SELECT constant_id,ordinal,operand_id FROM source_semantic_constant_operands ORDER BY constant_id,ordinal"))
            while (q.Read())
            {
                long owner = q.Integer(0), child = q.Integer(2);
                if (!SourceSemanticConstants.ContainsKey(owner) || !SourceSemanticConstants.ContainsKey(child))
                    throw new InvalidDataException("Source constant operand lacks its owned node");
                if (!operands.TryGetValue(owner, out var list)) operands[owner] = list = [];
                if (q.Integer(1) != list.Count) throw new InvalidDataException("Source constant operand order is not contiguous");
                list.Add(child);
            }
        foreach (var pair in operands) SourceSemanticConstantOperands[pair.Key] = pair.Value.ToArray();
        using (var q = database.Prepare("SELECT type_id,constant_id,capture_proof FROM source_array_bounds ORDER BY type_id"))
            while (q.Read())
            {
                if (!SourceTypes.TryGetValue(q.Integer(0), out var array) || array.Kind != "array" || array.ArrayBoundKind != "dependent" || array.ElementCount is not null ||
                    array.ExecutionRepresentationId is not null || !SourceSemanticConstants.ContainsKey(q.Integer(1)) ||
                    q.Text(2) != "original-dependent-bound" || !SourceArrayBounds.TryAdd(array.Id, q.Integer(1)))
                    throw new InvalidDataException("Dependent array bound requires its owned constant and no fabricated execution extent");
            }
        ReadSourceTemplateReferences();
        using (var q = database.Prepare("SELECT source_record_id,prototype_source_record_id,resolution_kind,source_template_id,member_owner_source_type_id,member_of_unknown_base,qualified_unknown_base_member,capture_proof FROM source_record_instantiations ORDER BY source_record_id"))
            while (q.Read())
            {
                var relation = new SourceRecordInstantiation(q.Text(2), q.OptionalInteger(1), q.OptionalInteger(3), q.OptionalInteger(4), q.Integer(5) == 1, q.Integer(6) == 1);
                if (!SourceRecords.TryGetValue(q.Integer(0), out var instance) || !instance.IsDependent || instance.IsPrototypeInstantiation ||
                    q.Integer(5) is not (0 or 1) || q.Integer(6) is not (0 or 1) ||
                    relation.QualifiedUnknownBaseMember && !relation.MemberOfUnknownBase)
                    throw new InvalidDataException("Invalid original dependent class instance relation");
                if (relation.ResolutionKind == "class-template")
                {
                    if (relation.PrototypeSourceRecordId is not long prototypeId || !SourceRecords.TryGetValue(prototypeId, out var prototype) ||
                        !prototype.IsDependent || !prototype.IsPrototypeInstantiation || instance.Id == prototype.Id ||
                        instance.SourceName != prototype.SourceName || instance.RecordKind != prototype.RecordKind ||
                        relation.SourceTemplateId is not null || relation.MemberOwnerSourceTypeId is not null || relation.MemberOfUnknownBase || relation.QualifiedUnknownBaseMember ||
                        q.Text(7) != "original-template-arguments")
                        throw new InvalidDataException("Dependent class instance requires its exact original template prototype");
                }
                else if (relation.ResolutionKind == "dependent-member-template")
                {
                    if (relation.PrototypeSourceRecordId is not null || relation.SourceTemplateId is not null || relation.MemberOwnerSourceTypeId is not long owner ||
                        !SourceTypes.ContainsKey(owner) || !SourceTypeContainsParameter(owner, 0) || instance.SourceName.Length == 0 ||
                        q.Text(7) != "original-nonreal-member-template")
                        throw new InvalidDataException("Dependent member template requires its original owner and no fabricated prototype");
                }
                else if (relation.ResolutionKind == "template-parameter")
                {
                    if (relation.SourceTemplateId is not long template || !SourceTemplates.TryGetValue(template, out var parameter) || instance.SourceName != parameter.SourceName ||
                        relation.PrototypeSourceRecordId is not null || relation.MemberOwnerSourceTypeId is not null ||
                        relation.MemberOfUnknownBase || relation.QualifiedUnknownBaseMember ||
                        q.Text(7) != "original-template-parameter-instantiation")
                        throw new InvalidDataException("Template parameter instance requires its exact original template declaration");
                }
                else throw new InvalidDataException("Unknown source template resolution kind");
                if (!SourceRecordInstantiations.TryAdd(instance.Id, relation))
                    throw new InvalidDataException("Duplicate source template instantiation relation");
            }
        using (var q = database.Prepare("SELECT source_record_id,ordinal,kind,type_id,constant_id,template_id FROM source_template_arguments ORDER BY source_record_id,ordinal"))
            while (q.Read())
            {
                long owner = q.Integer(0);
                var argument = new SourceTemplateArgument(q.Integer(1), q.Text(2), q.OptionalInteger(3), q.OptionalInteger(4), q.OptionalInteger(5));
                if (!SourceRecordInstantiations.ContainsKey(owner) ||
                    !SourceTemplatePayloadValid(argument.Kind, argument.TypeId, argument.ConstantId, argument.TemplateId))
                    throw new InvalidDataException("Source template argument lacks its typed semantic payload");
                if (!SourceTemplateArguments.TryGetValue(owner, out var list)) SourceTemplateArguments[owner] = list = [];
                if (argument.Ordinal != list.Count) throw new InvalidDataException("Source template arguments are not in original order");
                list.Add(argument);
            }
        foreach (var type in SourceTypes.Values)
            if ((type.ArrayBoundKind == "dependent") != SourceArrayBounds.ContainsKey(type.Id))
                throw new InvalidDataException("Dependent array is missing its exact original bound expression");
        foreach (var constant in SourceSemanticConstants.Values) VerifySourceSemanticConstant(constant);
        VerifySourceTemplateReferences();
        foreach (long bound in SourceArrayBounds.Values)
            if (!ContainsSourceParameter(bound, 0)) throw new InvalidDataException("Dependent array bound lost its dependent semantic operand");
    }

    private bool ContainsSourceParameter(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source semantic constant graph is cyclic or too deep");
        return SourceSemanticConstants[id].Kind is "parameter" or "member" ||
            SourceSemanticConstants[id].OperandSourceTypeId is long type && SourceTypeContainsParameter(type, depth + 1) ||
            SourceSemanticConstantOperands.GetValueOrDefault(id, []).Any(child => ContainsSourceParameter(child, depth + 1));
    }

    private void VerifySourceSemanticConstant(SourceSemanticConstant value)
    {
        SourceType valueType = Original(SourceTypeById(value.SourceTypeId));
        bool unknownType = valueType.Kind == "dependent" && SourceDependentTypes[valueType.Id].Kind == "unknown";
        if (unknownType && value.Kind is not ("member" or "expression"))
            throw new InvalidDataException("Only unresolved source member values may retain an unknown result type");
        var shape = unknownType || value.Kind == "type-operand" ? (Bits: 0, Signed: false) : SourceIntegerShape(valueType);
        VerifySourceTypedExpression(value, valueType);
        if ((value.Kind == "member") != (value.OwnerSourceTypeId is not null))
            throw new InvalidDataException("Source constant member owner differs from its semantic kind");
        long[] operands = SourceSemanticConstantOperands.GetValueOrDefault(value.Id, []);
        if (value.Kind == "member")
        {
            SourceType owner = Original(SourceTypeById(value.OwnerSourceTypeId!.Value));
            if (owner.Kind != "dependent" && (owner.Kind != "record" || !SourceRecordsByType[owner.Id].IsDependent) ||
                value.SourceName.Length == 0 || value.ParameterDepth is not null || value.ParameterPosition is not null ||
                value.IsPack || value.ValueText.Length != 0 || value.OperatorName.Length != 0 || operands.Length != 0)
                throw new InvalidDataException("Dependent constant member requires its exact dependent owner and member name");
        }
        else if (value.Kind == "parameter")
        {
            if (value.ParameterDepth is not >= 0 || value.ParameterPosition is not > 0 || value.SourceFileId is null || value.Line <= 0 ||
                value.ValueText.Length != 0 || value.OperatorName.Length != 0 || operands.Length != 0)
                throw new InvalidDataException("Non-type template parameter requires its original declaration and coordinates");
            SourceConstantParameterIdentity(value);
        }
        else
        {
            if (value.ParameterDepth is not null || value.ParameterPosition is not null || value.IsPack || value.SourceName.Length != 0)
                throw new InvalidDataException("Nonparameter source constant has fabricated declaration coordinates");
            if (value.Kind is "type-operand" or "builtin-trait" or "sizeof" or "alignof" or "datasizeof")
            {
                if (value.ValueText.Length != 0) throw new InvalidDataException("Source type query has a fabricated evaluated value");
            }
            else if (value.Kind == "integer")
            {
                if (!BigInteger.TryParse(value.ValueText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number) ||
                    number.ToString(CultureInfo.InvariantCulture) != value.ValueText || operands.Length != 0 || value.OperatorName.Length != 0 ||
                    number < (shape.Signed ? -(BigInteger.One << (shape.Bits - 1)) : BigInteger.Zero) ||
                    number >= (BigInteger.One << (shape.Bits - (shape.Signed ? 1 : 0))))
                    throw new InvalidDataException("Source integer constant exceeds its exact original type");
            }
            else
            {
                bool unary = value.OperatorName is "cast" or "neg" or "plus" or "not" or "bit-not";
                bool binary = value.OperatorName is "add" or "sub" or "mul" or "div" or "rem" or "shl" or "shr" or
                    "bit-and" or "bit-or" or "bit-xor" or "eq" or "ne" or "lt" or "le" or "gt" or "ge" or "and" or "or";
                if (value.ValueText.Length != 0 || (value.Kind == "expression" ? value.OperatorName.Length != 0 || operands.Length != 1
                    : !unary && !binary || operands.Length != (unary ? 1 : 2)))
                    throw new InvalidDataException("Source constant operation lost its exact operator or operand shape");
                if (value.Kind == "expression" && SourceConstantTypeIdentity(value.SourceTypeId, 0) !=
                    SourceConstantTypeIdentity(SourceSemanticConstants[operands[0]].SourceTypeId, 0))
                    throw new InvalidDataException("Source constant expression wrapper changes its operand type");
            }
        }
        SourceConstantIdentity(value.Id, 0); // Includes cycle/depth validation.
    }

    private static string SourceIdentityFrame(string value) => Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture) + ":" + value;
    private string SourceConstantParameterIdentity(SourceSemanticConstant value)
    {
        using var file = database.Prepare($"SELECT path FROM source_files WHERE id={value.SourceFileId}");
        if (!file.Read()) throw new InvalidDataException("Source constant declaration file is absent");
        return file.Text(0) + ":" + value.Line + ":" + value.Column + ":" + value.ParameterDepth + ":" + value.ParameterPosition;
    }
    private string SourceConstantTypeIdentity(long id, int depth)
    {
        SourceType type = SourceTypeById(id);
        return type.Kind == "dependent" && SourceDependentTypes[id].Kind == "unknown" ? "dependent-unknown" : SourceArgumentTypeIdentity(id, depth);
    }
    private string SourceConstantIdentity(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source constant identity cycle/depth");
        SourceSemanticConstant value = SourceSemanticConstants[id];
        string result = value.Kind + ":" + SourceIdentityFrame(SourceConstantTypeIdentity(value.SourceTypeId, depth + 1));
        if (value.OperandSourceTypeId is long operandType) result += SourceIdentityFrame(SourceArgumentTypeIdentity(operandType, depth + 1));
        if (value.Kind == "builtin-trait") result += SourceIdentityFrame(value.BuiltinOperatorKind!.Value.ToString(CultureInfo.InvariantCulture));
        if (value.Kind == "alignof") result += value.IsStandardAlignof ? "std:" : "extension:";
        return value.Kind switch
        {
            "integer" => result + SourceIdentityFrame(value.ValueText),
            "member" => result + SourceIdentityFrame(SourceArgumentTypeIdentity(value.OwnerSourceTypeId!.Value, depth + 1)) + SourceIdentityFrame(value.SourceName),
            "parameter" => result + SourceIdentityFrame(SourceConstantParameterIdentity(value)) + ":" + (value.IsPack ? "1" : "0"),
            _ => result + SourceIdentityFrame(value.OperatorName) + string.Concat(SourceSemanticConstantOperands.GetValueOrDefault(id, [])
                .Select(child => SourceIdentityFrame(SourceConstantIdentity(child, depth + 1))))
        };
    }
    private string SourceArgumentTypeIdentity(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source template type identity cycle/depth");
        SourceType value = SourceTypeById(id);
        if (value.Kind is "typedef" or "qualified")
        {
            string inner = SourceArgumentTypeIdentity(value.UnderlyingTypeId!.Value, depth + 1);
            string qualifiers = (value.IsConst ? "c" : "") + (value.IsVolatile ? "v" : "") + (value.IsRestrict ? "r" : "") + (value.IsAtomic ? "a" : "") +
                string.Concat(SourceTypeQualifiers.GetValueOrDefault(id, []).Select(SourceIdentityFrame));
            return qualifiers.Length == 0 ? inner : "q" + SourceIdentityFrame(qualifiers) + SourceIdentityFrame(inner);
        }
        if (value.Kind == "record") return "r" + SourceIdentityFrame(SourceRecordIdentity(SourceRecordsByType[id].Id, depth + 1));
        if (value.Kind == "dependent")
        {
            var parameter = SourceDependentTypes[id];
            return parameter.Kind switch
            {
                "parameter" => "p" + SourceIdentityFrame(parameter.IdentityKey) + (parameter.IsPack ? "1" : "0"),
                "member" => "m" + SourceIdentityFrame(SourceArgumentTypeIdentity(parameter.OwnerTypeId!.Value, depth + 1)) + SourceIdentityFrame(value.SourceName),
                _ => throw new InvalidDataException("Unknown dependent type cannot identify a template argument")
            };
        }
        if (value.Kind == "array") return "a" + SourceIdentityFrame(SourceArrayBounds.TryGetValue(id, out long bound)
            ? SourceConstantIdentity(bound, depth + 1) : value.ElementCount?.ToString(CultureInfo.InvariantCulture) ?? "?") +
            SourceIdentityFrame(SourceArgumentTypeIdentity(value.UnderlyingTypeId!.Value, depth + 1));
        if (value.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
            return value.Kind + ":" + SourceIdentityFrame(SourceArgumentTypeIdentity(value.UnderlyingTypeId!.Value, depth + 1));
        if (value.Kind is "enum" or "routine" or "member-pointer")
            throw new InvalidDataException("Source template argument identity family lacks an owned contract");
        return value.Kind;
    }
    private string SourceRecordIdentity(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source record template identity cycle/depth");
        if (!SourceRecordInstantiations.TryGetValue(id, out var relation)) return SourceRecords[id].IdentityKey;
        string owner = relation.ResolutionKind == "class-template"
            ? "source-instantiation:" + SourceIdentityFrame(SourceRecordIdentity(relation.PrototypeSourceRecordId!.Value, depth + 1))
            : relation.ResolutionKind == "template-parameter"
            ? "source-parameter-instantiation:" + SourceIdentityFrame(SourceTemplateIdentity(relation.SourceTemplateId!.Value, depth + 1))
            : "source-member-instantiation:" + SourceIdentityFrame(SourceArgumentTypeIdentity(relation.MemberOwnerSourceTypeId!.Value, depth + 1)) +
                SourceIdentityFrame(SourceRecords[id].SourceName);
        return owner + string.Concat(SourceTemplateArguments.GetValueOrDefault(id, []).Select(argument => SourceIdentityFrame(argument.Kind == "type"
                ? "t" + SourceArgumentTypeIdentity(argument.TypeId!.Value, depth + 1) : argument.Kind == "constant"
                ? "c" + SourceConstantIdentity(argument.ConstantId!.Value, depth + 1) : "m" + SourceTemplateIdentity(argument.TemplateId!.Value, depth + 1))));
    }
    private void VerifySourceTemplateMetadata()
    {
        foreach (var record in SourceRecords.Values)
        {
            bool instance = record.IsDependent && !record.IsPrototypeInstantiation && !SourceRecordOwners.ContainsKey(record.Id);
            bool instanceKey = record.IdentityKey.StartsWith("source-instantiation:", StringComparison.Ordinal) ||
                record.IdentityKey.StartsWith("source-member-instantiation:", StringComparison.Ordinal) ||
                record.IdentityKey.StartsWith("source-parameter-instantiation:", StringComparison.Ordinal);
            if (SourceRecordInstantiations.ContainsKey(record.Id) != instance || instanceKey != instance ||
                instance && record.IdentityKey != SourceRecordIdentity(record.Id, 0))
                throw new InvalidDataException("Dependent class identity differs from its exact original template arguments");
        }
    }
}
