namespace DotObj;

internal sealed partial class ObjectFile
{
    private void VerifySourceTypedExpression(SourceSemanticConstant value)
    {
        long[] children = SourceSemanticConstantOperands.GetValueOrDefault(value.Id, []);
        bool typeQuery = value.Kind is "sizeof" or "alignof" or "datasizeof";
        if ((value.Kind == "builtin-trait") != (value.BuiltinOperatorKind is not null) ||
            value.IsStandardAlignof && value.Kind != "alignof" ||
            value.OperandSourceTypeId is not null && value.Kind != "type-operand" && !typeQuery)
            throw new InvalidDataException("Source expression contains mismatched type-query metadata");
        if (value.Kind == "type-operand")
        {
            if (value.OperandSourceTypeId is not long type || !SourceTypes.ContainsKey(type) ||
                children.Length != 0 || value.OperatorName.Length != 0)
                throw new InvalidDataException("Original type operand requires its exact type payload and nonvalue expression type");
        }
        else if (value.Kind == "builtin-trait")
        {
            if(value.OperatorName.Length==0)
                throw new InvalidDataException("Source builtin trait lacks its recorded operator name");
        }
        else if (typeQuery)
        {
            if (value.OperatorName.Length != 0 || (value.OperandSourceTypeId is long type
                ? !SourceTypes.ContainsKey(type) || children.Length != 0
                : children.Length != 1))
                throw new InvalidDataException("Original size/alignment query lost its type-versus-expression operand or unsigned result");
        }
    }
}
