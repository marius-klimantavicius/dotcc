namespace DotObj;
internal sealed record SourceProxyType(long OriginalType,long Occurrence);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceProxyType> SourceProxyTypes {get;}=[];
    private void ReadSourceProxyTypes()
    {
        using var q=database.Prepare("SELECT source_record_id,original_type_id,source_occurrence,capture_proof FROM source_proxy_types ORDER BY source_record_id");
        while(q.Read())
        {
            if(!SourceRecords.TryGetValue(q.Integer(0),out var record) || !SourceTypes.ContainsKey(q.Integer(1)) ||
                record.SourceTypeId==q.Integer(1) || !record.IsDependent || record.IsPrototypeInstantiation || record.LayoutKnown ||
                SourceTypes[record.SourceTypeId].ExecutionRepresentationId is not null ||
                q.Integer(2)<0 || q.Text(3)!="original-class-proxy-of-type" || !SourceProxyTypes.TryAdd(q.Integer(0),new(q.Integer(1),q.Integer(2))))
                throw new InvalidDataException("Original proxy class lost its exact dependent source type or claims an execution layout");
        }
    }
    private void VerifySourceProxyTypes()
    {
        foreach(var pair in SourceProxyTypes)
        {
            var record=SourceRecords[pair.Key];var target=SourceTypeById(pair.Value.OriginalType);
            bool unknown=target.Kind=="dependent" && SourceDependentTypes[target.Id].Kind=="unknown";
            string expected=unknown?"source-proxy-local:"+SourceIdentityFrame(UnitId)+SourceIdentityFrame(pair.Value.Occurrence.ToString(System.Globalization.CultureInfo.InvariantCulture)):
                "source-proxy:"+SourceIdentityFrame(SourceArgumentTypeIdentity(target.Id,0));
            if(target.Kind!="dependent" && target.SourceForm!="type-operator" ||
                !SourceTypeContainsParameter(target.Id,0) || SourceRecordInstantiations.ContainsKey(record.Id) ||
                record.IdentityKey!=expected)
                throw new InvalidDataException("Original proxy identity differs from its exact template parameter or type operator");
        }
        foreach(var record in SourceRecords.Values)
            if((record.IdentityKey.StartsWith("source-proxy:",StringComparison.Ordinal) || record.IdentityKey.StartsWith("source-proxy-local:",StringComparison.Ordinal))!=SourceProxyTypes.ContainsKey(record.Id))
                throw new InvalidDataException("Original proxy class lacks its required source-type relation");
    }
}
