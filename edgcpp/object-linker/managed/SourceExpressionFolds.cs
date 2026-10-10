using System.Globalization;
namespace DotObj;

internal sealed record SourceExpressionFold(long Token,string Spelling,bool LeftAssociative);
internal sealed partial class ObjectFile
{
    public Dictionary<long,long> SourceExpressionFieldReferences { get; }=[];
    public Dictionary<long,SourceExpressionFold> SourceExpressionFolds { get; }=[];
    private static string SourceFoldOperator(long token)=>token switch
    {
        36=>"&",
        37=>"*",
        38=>"+",
        39=>"-",
        42=>"/",
        43=>"%",
        44=>"<<",
        45=>">>",
        46=>"<",
        47=>">",
        48=>"<=",
        49=>">=",
        50=>"==",
        51=>"!=",
        54=>"^",
        55=>"|",
        56=>"&&",
        57=>"||",
        60=>"=",
        61=>"*=",
        62=>"/=",
        63=>"%=",
        64=>"+=",
        65=>"-=",
        66=>"<<=",
        67=>">>=",
        68=>"&=",
        69=>"^=",
        70=>"|=",
        71=>",",
        216=>".*",
        217=>"->*",
        _=>throw new InvalidDataException("Original fold operator is not a configured C++17 fold token")
    };
    private void ReadSourceExpressionFolds()
    {
        using(var fields=database.Prepare("SELECT expression_id,source_field_id,capture_proof FROM source_expression_field_references ORDER BY expression_id"))
            while(fields.Read())
                if(!SourceExpressions.TryGetValue(fields.Integer(0),out var expression) || expression.Kind!="field-reference" ||
                    !SourceFields.ContainsKey(fields.Integer(1)) || fields.Text(2)!="original-field-reference" ||
                    !SourceExpressionFieldReferences.TryAdd(fields.Integer(0),fields.Integer(1)))throw new InvalidDataException("Original field reference lacks its owned declaration");
        using var q=database.Prepare("SELECT expression_id,operator_token,operator_spelling,is_left_associative,capture_proof FROM source_expression_folds ORDER BY expression_id");
        while(q.Read())
        {
            long id=q.Integer(0);var value=new SourceExpressionFold(q.Integer(1),q.Text(2),q.Integer(3)==1);
            if(!SourceExpressions.TryGetValue(id,out var expression) || expression.Kind!="fold-expression" ||
                q.Integer(3) is not(0 or 1) || q.Text(4)!="original-fold-expression" || value.Spelling!=SourceFoldOperator(value.Token) ||
                !SourceExpressionFolds.TryAdd(id,value))throw new InvalidDataException("Original fold expression lost its exact operator and association");
        }
    }
    private void VerifySourceFoldPresence()
    {
        foreach(var value in SourceExpressions.Values) {
            if((value.Kind=="fold-expression")!=SourceExpressionFolds.ContainsKey(value.Id))
                throw new InvalidDataException("Original fold expression is missing its operator provenance");
            if((value.Kind=="field-reference")!=SourceExpressionFieldReferences.ContainsKey(value.Id))
                throw new InvalidDataException("Original field reference is missing its declaration provenance");
        }
    }
    private void VerifySourceFieldReference(SourceExpression expression)
    {
        var field=SourceFields[SourceExpressionFieldReferences[expression.Id]];
        if((expression.OriginalLvalueTypeId??expression.SourceTypeId) is not long type || SourceTypeSignature(type)!=SourceTypeSignature(field.SourceTypeId))
            throw new InvalidDataException("Original field reference differs from its selected declaration type");
    }
    private string SourceFieldReferenceIdentity(long id,int depth)
    {
        var field=SourceFields[SourceExpressionFieldReferences[id]];var owner=SourceRecords[field.SourceRecordId];
        return SourceIdentityFrame(SourceExpressionTypeKey(owner.SourceTypeId,depth+1))+SourceIdentityFrame(field.Ordinal.ToString(CultureInfo.InvariantCulture));
    }
    private string SourceFoldIdentity(long id)
    {
        var value=SourceExpressionFolds[id];
        return SourceIdentityFrame(value.Token.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(value.Spelling)+(value.LeftAssociative?"1":"0");
    }
}
