namespace DotObj;
internal sealed record SourceEnumIncrement(long Previous,long One,long OriginalType,bool Forced);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceEnumIncrement> SourceEnumIncrements {get;}=[];
    public Dictionary<long,long> SourceEnumIncrementPreviousValues {get;}=[];
    private void ReadSourceEnumIncrements()
    {
        using var q=database.Prepare("SELECT operation_constant_id,previous_constant_id,one_constant_id,original_source_type_id,did_force_dependent_type,capture_proof FROM source_enum_increments ORDER BY operation_constant_id");
        while(q.Read())
        {
            long id=q.Integer(0);var value=new SourceEnumIncrement(q.Integer(1),q.Integer(2),q.Integer(3),q.Integer(4)==1);
            if(!SourceSemanticConstants.TryGetValue(id,out var operation) || operation.Kind!="operation" || operation.OperatorName!="add" ||
                !SourceSemanticConstants.TryGetValue(value.Previous,out var previous) || previous.Kind!="expression" ||
                !SourceSemanticConstants.TryGetValue(value.One,out var one) || one.Kind!="integer" || one.ValueText!="1" ||
                !SourceTypes.ContainsKey(value.OriginalType) || operation.SourceTypeId!=value.OriginalType ||
                !SourceSemanticConstantOperands.GetValueOrDefault(id,[]).SequenceEqual(new[]{value.Previous,value.One}) ||
                SourceSemanticConstantOperands.GetValueOrDefault(value.Previous,[]).Length!=1 ||
                q.Integer(4) is not(0 or 1) || q.Text(5)!="observed-dependent-enumerator-increment")
                throw new InvalidDataException("Dependent enum increment lost its exact observed addition/previous-value graph");
            long backingId=SourceSemanticConstantOperands[value.Previous][0];
            var backing=SourceSemanticConstants[backingId];var previousType=Original(SourceTypeById(previous.SourceTypeId));
            bool unknown=previousType.Kind=="dependent" && SourceDependentTypes[previousType.Id].Kind=="unknown";
            if(backing.SourceTypeId!=value.OriginalType || value.Forced && Original(SourceTypeById(one.SourceTypeId)).Kind!="int" ||
                (value.Forced? !unknown || previous.SourceTypeId==value.OriginalType : previous.SourceTypeId!=value.OriginalType) ||
                !SourceEnumerators.Values.SelectMany(values=>values).Any(enumerator=>enumerator.ConstantId is long constant && SourceConstantContainsEnumIncrement(constant,id,[])) ||
                !SourceEnumIncrements.TryAdd(id,value) || !SourceEnumIncrementPreviousValues.TryAdd(value.Previous,id))
                throw new InvalidDataException("Dependent enum previous value lost its original type or enumerator ownership");
        }
    }
    private bool SourceConstantContainsEnumIncrement(long current,long target,HashSet<long> path)
    {
        if(current==target)return true;
        if(path.Count>128 || !path.Add(current))throw new InvalidDataException("Source enum increment ancestry is cyclic or too deep");
        bool result=SourceSemanticConstantOperands.GetValueOrDefault(current,[]).Any(child=>SourceConstantContainsEnumIncrement(child,target,path));
        path.Remove(current);return result;
    }
}
