namespace DotObj;
internal sealed record SourcePackSize(bool IsType,bool IsTemplate,long? Type,long? Expression,long? Template);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourcePackSize> SourcePackSizes {get;}=[];
    private void ReadSourcePackSizes()
    {
        using var q=database.Prepare("SELECT constant_id,is_type,is_template_template,source_type_id,expression_id,template_id,capture_proof FROM source_pack_sizes ORDER BY constant_id");
        while(q.Read())
        {
            var value=new SourcePackSize(q.Integer(1)==1,q.Integer(2)==1,q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5));
            if(!SourceSemanticConstants.TryGetValue(q.Integer(0),out var constant) || constant.Kind!="sizeof-pack" ||
                q.Integer(1) is not(0 or 1) || q.Integer(2) is not(0 or 1) || q.Text(6)!="original-sizeof-pack" ||
                (value.IsTemplate ? value.Template is null || !SourceTemplates.ContainsKey(value.Template.Value) || value.Type is not null || value.Expression is not null :
                 value.IsType ? value.Type is null || !SourceTypes.ContainsKey(value.Type.Value) || value.Template is not null || value.Expression is not null :
                 value.Expression is null || !SourceExpressions.ContainsKey(value.Expression.Value) || value.Type is not null || value.Template is not null) ||
                !SourcePackSizes.TryAdd(q.Integer(0),value))
                throw new InvalidDataException("Original pack size lost its exact type/expression/template operand role");
        }
        foreach(var constant in SourceSemanticConstants.Values)
            if(SourcePackSizes.ContainsKey(constant.Id)!=(constant.Kind=="sizeof-pack"))
                throw new InvalidDataException("Original pack size is missing its required operand metadata");
    }
    private void VerifySourcePackSize(SourceSemanticConstant constant)
    {
        if(constant.ValueText.Length!=0 || constant.OperatorName.Length!=0 ||
            SourceSemanticConstantOperands.GetValueOrDefault(constant.Id,[]).Length!=0)
            throw new InvalidDataException("Original pack size contains an unexpected evaluated value or operand list");
    }
    private string SourcePackSizeIdentity(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Original pack-size identity is cyclic or too deep");
        var value=SourcePackSizes[id];
        return (value.IsType?"1":"0")+(value.IsTemplate?"1":"0")+SourceIdentityFrame(value.Type is long type?SourceArgumentTypeIdentity(type,depth+1):
            value.Template is long template?SourceTemplateIdentity(template,depth+1):SourceStructuralExpressionKey(value.Expression!.Value,depth+1));
    }
}
