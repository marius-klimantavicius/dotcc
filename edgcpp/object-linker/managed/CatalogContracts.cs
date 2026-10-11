namespace DotObj;

// These are comparison facts, not rewritten source metadata. Every locator
// remains unit-local; only semantic keys and typed signature atoms are global.
internal static class CatalogContracts
{
    private static CatalogExecutionType Execution(string type)=>new(type);
    public static void Write(LinkCatalog output,long unit,ObjectFile obj)
    {
        output.Fact(unit,"unit-runtime",0,"",new {
            Varargs=obj.Types.Values.Contains("va-list") || obj.Routines.Values.Any(value=>value.Variadic),
            Float80=obj.Types.Values.Contains("f80"),
            Pointers=obj.Types.Values.Any(ScalarTypes.IsPointer),
            Guards=obj.GuardOperations.Count!=0 || obj.GuardCleanupActions.Count!=0 });
        foreach(var (id,type) in obj.Types.Where(pair=>ScalarTypes.IsArray(pair.Value)))
            output.Fact(unit,"array-type",id,type,Execution(type));
        // Source-only templates, concepts and enum recipes remain in their
        // dotobj graph. They are not global link contracts.
        var originalFields=obj.SourceFields.Values.GroupBy(value=>value.SourceRecordId)
            .ToDictionary(group=>group.Key,group=>group.OrderBy(value=>value.Ordinal).ToArray());
        foreach(var record in obj.Records.Values)
            output.Fact(unit,"record",record.TypeId,record.Key,new {
                record.SourceName,record.Size,record.Alignment,record.LayoutKind,record.RepresentationKind,record.ValueCopyKind,
                CompleteType=record.CompleteTypeId is long complete?Execution(obj.Type(complete)):null,
                Source=record.SourceRecordId is long original?OriginalRecord(obj,original,originalFields.GetValueOrDefault(original,[])):null,
                Fields=obj.RecordFields(record.TypeId).Select(field=>new {
                    field.SourceName,field.ByteOffset,field.BitOffset,field.BitWidth,field.BitSigned,
                    Type=Execution(obj.Type(field.TypeId)),field.FieldRole,field.ObservedAlignment,
                    BaseOrdinal=field.SourceBaseId is long id?obj.SourceBases[id].Ordinal:(long?)null,
                    SourceSignature=field.SourceTypeId is long source?obj.SourceTypeSignature(source):null }).ToArray(),
                Bases=obj.RecordBases(record.TypeId).Select(value=>new {
                    obj.SourceBases[value.SourceBaseId].Ordinal,CompleteType=Execution(obj.Type(value.CompleteBaseTypeId)),
                    PhysicalView=value.PhysicalViewTypeId is long view?Execution(obj.Type(view)):null,value.ByteOffset,value.Extent,
                    FieldOrdinal=value.FieldId is long field?obj.Fields[field].Ordinal:(long?)null }).ToArray(),
                VirtualBases=obj.RecordVirtualBases(record.TypeId).Select(value=>new {
                    CompleteType=Execution(obj.Type(value.CompleteBaseTypeId)),
                    PhysicalView=value.PhysicalViewTypeId is long view?Execution(obj.Type(view)):null,value.ByteOffset,value.Extent,
                    FieldOrdinal=value.FieldId is long field?obj.Fields[field].Ordinal:(long?)null }).ToArray() });
        var deferred=obj.DeferredRoutineAbis.Select(value=>value.SourceRoutineTypeId).ToHashSet();
        foreach(var abi in obj.SourceRoutineAbis.Values)
        {
            var type=obj.SourceTypes[abi.SourceRoutineTypeId];if(type.ExecutionRepresentationId is not long execution)continue;
            var routine=obj.SourceRoutines[type.Id];var slots=obj.RoutineAbiParameters(type.Id);
            var decisions=new SortedDictionary<string,string>(StringComparer.Ordinal);
            if(!obj.IsIncompleteOriginalValue(routine.ReturnTypeId))decisions.Add("result",abi.ReturnPassing);
            foreach(var slot in slots)
                if(slot.SourceOrdinal is long ordinal && !obj.IsIncompleteOriginalValue(routine.Parameters[ordinal]))
                    decisions.Add("parameter:"+ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture),slot.ParameterRole);
            decisions.Add("implicit",string.Join(',',slots.Where(slot=>slot.SourceOrdinal is null && slot.ParameterRole!="result-buffer").Select(slot=>slot.ParameterRole)));
            output.Fact(unit,"routine-abi",type.Id,obj.SourceTypeSignature(type.Id),new {
                Execution=Execution(obj.Type(execution)),Deferred=deferred.Contains(type.Id),Decisions=decisions });
        }
    }
    private static object OriginalRecord(ObjectFile obj,long id,SourceRecordField[] fields)
    {
        var record=obj.SourceRecords[id];
        if(obj.IsLocalSourceRecord(id))return new {LocalObservation=true};
        return new {
            record.IdentityKey,record.SourceName,record.RecordKind,record.IsTriviallyCopyable,record.IsPacked,
            record.LayoutKnown,record.IsDependent,record.IsPrototypeInstantiation,record.MaxMemberAlignment,record.ExplicitAlignment,
            Language=obj.SourceLanguage,Operations=obj.RecordOperations.GetValueOrDefault(id),
            Bases=obj.OriginalBases(id).Select(value=>new {value.Ordinal,value.AccessKind,value.IsVirtual,
                Type=obj.SourceTypeSignature(value.BaseSourceTypeId)}).ToArray(),
            Fields=fields.Select(value=>new {value.SourceName,value.IsMutable,value.HasInitializer,value.IsPacked,value.ExplicitAlignment,
                value.HasNoUniqueAddress,value.BitWidth,BitWidthIdentity=obj.SourceBitfieldWidthIdentity(value.Id),
                Type=obj.SourceTypeSignature(value.SourceTypeId)}).ToArray() };
    }
    public static void ExceptionUse(LinkCatalog output,long unit,ObjectFile obj,long function,Node node)
    {
        output.Fact(unit,"exception-use",node.Id,function.ToString(System.Globalization.CultureInfo.InvariantCulture),new {
            node.Kind,Match=node.Kind=="catch" && obj.SourceHandlers[node.Id].MatchTypeId is long type?obj.SourceTypeSignature(type):null });
        if(node.Kind!="throw")return;
        var expression=obj.ThrowExpressions[node.Id];var descriptor=obj.ExceptionTypes[expression.ExceptionTypeId];
        // One descriptor may occur at multiple throw sites. Use the site locator
        // as the row key; the contract payload is losslessly interned.
        output.Fact(unit,"exception-descriptor",node.Id,descriptor.IdentityKey,new {
            descriptor.PayloadSize,descriptor.PayloadAlignment,
            Destructor=descriptor.SourceDestructorEntityId is long primary?
                new {Key=new Symbol(obj,obj.Entities[primary]).Key,SourceSignature=obj.FunctionSourceSignature(primary)}:null,
            Bases=obj.ExceptionBases[descriptor.Id].Select(value=>new {
                Type=obj.ExceptionTypeIdentity(value.BaseSourceTypeId),value.ByteOffset,value.IsPublic,value.IsAmbiguous }).ToArray() });
    }
}
