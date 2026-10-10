namespace DotObj;

internal sealed record GeneratedArrayInitializer(long FunctionEntityId, long OwnerFunctionEntityId,
    long SourceAllocationObligationId, long SourceElementTypeId, long ElementTypeId,
    long? ConstructorEntityId, long ObjectParameterBindingId, long ModelBindingId,
    long ModelInitializationNodeId, long ModelCopyNodeId, long? ConstructorCallNodeId,
    long ModelLoweredInitKind, string ModelSiteKind, bool ModelZeroStorage, bool ModelPartiallyInitialized);

internal sealed partial class ObjectFile
{
    public Dictionary<long, GeneratedArrayInitializer> GeneratedArrayInitializers { get; } = [];

    private void ReadGeneratedArrayInitializers()
    {
        using var query=database.Prepare("SELECT function_entity_id,owner_function_entity_id,source_allocation_obligation_id,source_element_type_id,element_type_id,constructor_entity_id,object_parameter_binding_id,model_binding_id,model_initialization_node_id,model_copy_node_id,constructor_call_node_id,model_lowered_init_kind,model_site_kind,model_zero_storage,model_partially_initialized,capture_proof FROM generated_array_initializers ORDER BY function_entity_id");
        while(query.Read())
        {
            var value=new GeneratedArrayInitializer(query.Integer(0),query.Integer(1),query.Integer(2),query.Integer(3),query.Integer(4),
                query.OptionalInteger(5),query.Integer(6),query.Integer(7),query.Integer(8),query.Integer(9),query.OptionalInteger(10),
                query.Integer(11),query.Text(12),query.Integer(13)==1,query.Integer(14)==1);
            string element=Type(value.ElementTypeId);
            if(SourceLanguage!="c++" || !Entities.TryGetValue(value.FunctionEntityId,out var helper) ||
                helper.Linkage!="internal" || helper.Definition!="strong" || Type(helper.ReturnTypeId)!="void" ||
                !Functions.ContainsKey(helper.Id) || Routines[helper.RoutineTypeId].Variadic ||
                !Parameters.GetValueOrDefault(helper.Id,[]).SequenceEqual(["ptr("+element+")"]) ||
                ParameterRoles[(helper.Id,0)].Role!="initialization-object" ||
                !ScalarTypes.IsRecord(element) || RecordsByKey[element].RepresentationKind!="complete" ||
                !SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId,out var allocation) ||
                !allocation.IsArray || !IsSourceLifetimeExecution(SourceLifetimes[allocation.LifetimeId],value.OwnerFunctionEntityId) ||
                DestructionElementType(allocation.AllocatedSourceTypeId).ExecutionTypeId!=value.ElementTypeId ||
                SourceTypeSignature(DestructionElementType(allocation.AllocatedSourceTypeId).Id)!=SourceTypeSignature(value.SourceElementTypeId) ||
                SourceTypeById(value.SourceElementTypeId).ExecutionTypeId!=value.ElementTypeId ||
                (value.ConstructorEntityId is null)!=(value.ConstructorCallNodeId is null) ||
                value.ObjectParameterBindingId==value.ModelBindingId || value.ModelLoweredInitKind is <0 or >5 ||
                value.ModelSiteKind is not ("scope-entry" or "dynamic-init" or "lowered-assignment") ||
                value.ModelSiteKind=="scope-entry" && value.ModelLoweredInitKind!=3 ||
                query.Integer(13) is not (0 or 1) || query.Integer(14) is not (0 or 1) ||
                value.ModelZeroStorage!=(value.ModelLoweredInitKind==3 || value.ModelLoweredInitKind==2 && value.ModelPartiallyInitialized) ||
                value.ModelPartiallyInitialized && value.ModelLoweredInitKind!=2 ||
                query.Text(15)!="observed-array-zero-initializer-factory" ||
                !GeneratedArrayInitializers.TryAdd(helper.Id,value))
                throw new InvalidDataException("Generated array initializer lacks its exact factory, source allocation or model contract");
            if(value.ConstructorEntityId is long constructor &&
                (SourceFunctionKinds.GetValueOrDefault(OriginalFunction(constructor))!="constructor" ||
                 Type(Entities[constructor].ReturnTypeId)!="void" ||
                 !Parameters.GetValueOrDefault(constructor,[]).SequenceEqual(["ptr("+element+")"])))
                throw new InvalidDataException("Generated array initializer selected constructor has incompatible element ABI");
            RequireScalar($"SELECT count(*) FROM bindings WHERE id={value.ObjectParameterBindingId} AND function_id={helper.Id} AND kind='parameter' AND ordinal=0 AND is_synthetic=1",1);
            RequireScalar($"SELECT count(*) FROM bindings WHERE id={value.ModelBindingId} AND function_id={helper.Id} AND kind='local' AND is_synthetic=1 AND type_id={value.ElementTypeId}",1);
            RequireScalar($"SELECT count(*) FROM source_declarations WHERE binding_id IN ({value.ModelBindingId},{value.ObjectParameterBindingId})",0);
            RequireScalar($"SELECT count(*) FROM nodes WHERE id={value.ModelInitializationNodeId} AND function_id={helper.Id} AND kind='record-storage-init'",1);
            RequireScalar($"SELECT count(*) FROM nodes WHERE id={value.ModelCopyNodeId} AND function_id={helper.Id} AND kind='record-transfer'",1);
            RequireScalar($"SELECT count(*) FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id WHERE a.constructor_entity_id={helper.Id} AND a.source_allocation_obligation_id={allocation.Id} AND n.function_id={value.OwnerFunctionEntityId} AND a.element_type_id={value.ElementTypeId}",1);
            RequireScalar($"SELECT count(*) FROM array_allocation_operations WHERE constructor_entity_id={helper.Id}",1);
            RequireScalar($"SELECT count(*) FROM nodes WHERE kind='call' AND entity_id={helper.Id}",0);
            RequireScalar($"SELECT count(*) FROM nodes WHERE kind='function-address' AND entity_id={helper.Id}",1);
            if(value.ConstructorCallNodeId is long call)
                RequireScalar($"SELECT count(*) FROM nodes WHERE id={call} AND function_id={helper.Id} AND kind='call' AND entity_id={value.ConstructorEntityId}",1);
        }
        RejectRows("SELECT p.entity_id FROM parameters p LEFT JOIN generated_array_initializers g ON g.function_entity_id=p.entity_id WHERE p.parameter_role='initialization-object' AND g.function_entity_id IS NULL", "Synthetic initialization parameter lacks its generated factory origin");
    }

    private void ReadGeneratedArrayModelInitializations()
    {
        foreach(var value in GeneratedArrayInitializers.Values)
            if(!RecordStorageInitializations.TryAdd(value.ModelInitializationNodeId,new(value.ModelBindingId,value.ModelZeroStorage)))
                throw new InvalidDataException("Generated array model has overlapping initialization provenance");
    }

    private void VerifyGeneratedArrayModelCopies()
    {
        foreach(var value in GeneratedArrayInitializers.Values)
            if(!RecordStorageCopies.TryGetValue(value.ModelCopyNodeId,out var copy) || copy.Kind!="array-zero-model" ||
                RecordsByKey[Type(value.ElementTypeId)].SourceRecordId!=copy.SourceRecordId)
                throw new InvalidDataException("Generated array model copy lacks its complete typed storage proof");
        foreach(var pair in RecordStorageCopies.Where(pair=>pair.Value.Kind=="array-zero-model"))
            if(!GeneratedArrayInitializers.Values.Any(value=>value.ModelCopyNodeId==pair.Key))
                throw new InvalidDataException("Array zero model storage copy lacks an owned generated helper");
    }

    public void VerifyGeneratedArrayInitializerBody(long functionId,FunctionBody body)
    {
        foreach(var origin in GeneratedArrayInitializers.Values.Where(origin=>origin.OwnerFunctionEntityId==functionId))
        {
            var operation=ArrayAllocationOperations.Values.Single(operation=>operation.ConstructorEntityId==origin.FunctionEntityId);
            var leaf=body.Nodes.Values.Single(node=>node.Kind=="function-address" && node.EntityId==origin.FunctionEntityId);
            long current=leaf.Id;
            for(int depth=0;;++depth)
            {
                if(depth>128)throw new InvalidDataException("Generated initializer callback conversion exceeds nesting limit");
                var parents=body.Edges.Values.SelectMany(edges=>edges).Where(edge=>edge.ChildId==current).ToArray();
                if(parents.Length!=1)throw new InvalidDataException("Generated initializer callback escapes its exact vector argument");
                var edge=parents[0];var parent=body.Nodes[edge.ParentId];
                if(parent.Kind=="call" && parent.Id==operation.NodeId && edge.Role=="argument" && edge.Ordinal==3)break;
                if(parent.Kind!="convert" || edge.Role!="operand")throw new InvalidDataException("Generated initializer callback has an unowned use");
                current=parent.Id;
            }
        }
        if(!GeneratedArrayInitializers.TryGetValue(functionId,out var value))return;
        bool Parameter(Node node)=>node.Kind=="binding" && node.BindingId==value.ObjectParameterBindingId;
        Node initialization=body.Nodes[value.ModelInitializationNodeId],copy=body.Nodes[value.ModelCopyNodeId];
        Node target=body.Child(copy.Id,"target"),source=body.Child(copy.Id,"value");
        if(target.Kind!="dereference" || !Parameter(body.Child(target.Id,"operand")) ||
            source.Kind!="binding" || source.BindingId!=value.ModelBindingId ||
            body.Child(initialization.Id,"target") is not {Kind:"binding"} model || model.BindingId!=value.ModelBindingId)
            throw new InvalidDataException("Generated array model copy differs from its exact object/model operands");
        if(value.ConstructorCallNodeId is long callId)
        {
            var arguments=body.Children(callId,"argument").ToArray();
            if(arguments.Length!=1 || !Parameter(body.Nodes[arguments[0].ChildId]))
                throw new InvalidDataException("Generated array constructor call does not use its exact destination parameter");
        }
        var operations=new List<long>();bool returned=false;
        void Walk(Node node,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("Generated array initializer body exceeds nesting limit");
            if(node.Kind=="block") { foreach(var edge in body.Children(node.Id,"statement"))Walk(body.Nodes[edge.ChildId],depth+1);return; }
            if(node.Kind=="declare" && node.BindingId==value.ModelBindingId)return;
            if(node.Kind=="expr-stmt") { Walk(body.Child(node.Id,"value"),depth+1);return; }
            if(node.Kind=="sequence") { foreach(var edge in body.Children(node.Id,"item"))Walk(body.Nodes[edge.ChildId],depth+1);return; }
            if(node.Kind=="return" && !body.Edges.GetValueOrDefault(node.Id,[]).Any()) { if(returned)throw new InvalidDataException("Generated array initializer has repeated return scaffolding");returned=true;return; }
            if(returned || (node.Id!=value.ModelInitializationNodeId && node.Id!=value.ModelCopyNodeId && node.Id!=value.ConstructorCallNodeId))
                throw new InvalidDataException("Generated array initializer body contains an unowned operation");
            operations.Add(node.Id);
        }
        Walk(body.Nodes[body.RootId]);
        long[] expected=value.ConstructorCallNodeId is long constructorCall?
            [value.ModelInitializationNodeId,value.ModelCopyNodeId,constructorCall]:[value.ModelInitializationNodeId,value.ModelCopyNodeId];
        if(!operations.SequenceEqual(expected))throw new InvalidDataException("Generated array initializer changed zero/copy/constructor ordering");
        VerifyGeneratedArraySemanticZero(value,body,initialization);
    }

    private void VerifyGeneratedArraySemanticZero(GeneratedArrayInitializer value,FunctionBody body,Node initialization)
    {
        var minusOneOffsets=new HashSet<long>();
        bool IsDataMemberPointer(long? source)
        {
            if(source is not long id)return false;
            SourceType type=Original(SourceTypeById(id));
            while(type.Kind=="array")type=Original(SourceTypeById(type.UnderlyingTypeId!.Value));
            return type.Kind=="member-pointer" && SourceMemberPointers.TryGetValue(type.Id,out var member) &&
                Original(SourceTypeById(member.MemberTypeId)).Kind!="routine";
        }
        void Collect(string type,long offset,bool minusOne,int depth)
        {
            if(depth>128)throw new InvalidDataException("Generated zero model layout exceeds nesting limit");
            if(ScalarTypes.IsArray(type))
            {
                long count=ScalarTypes.ElementCount(type);string element=ScalarTypes.Element(type);
                for(long i=0;i<count;++i)Collect(element,checked(offset+i*StorageSize(element)),minusOne,depth+1);
            }
            else if(ScalarTypes.IsRecord(type))
            {
                var fields=RecordFields(RecordsByKey[type].TypeId);
                if(RecordsByKey[type].LayoutKind=="union")fields=fields.Take(1).ToArray();
                foreach(var field in fields)Collect(Type(field.TypeId),checked(offset+field.ByteOffset),IsDataMemberPointer(field.SourceTypeId),depth+1);
            }
            else if(minusOne)minusOneOffsets.Add(offset);
        }
        Collect(Type(value.ElementTypeId),0,false,0);
        long Offset(Node target,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("Generated zero target exceeds nesting limit");
            if(target.Kind=="binding" && target.BindingId==value.ModelBindingId)return 0;
            if(target.Kind=="member" && target.FieldId is long field)return checked(Offset(body.Child(target.Id,"operand"),depth+1)+Fields[field].ByteOffset);
            if(target.Kind=="dereference")
            {
                Node add=body.Child(target.Id,"operand");
                if(add.Kind=="binary" && add.Operator=="pointer-add")
                {
                    Node array=body.Child(add.Id,"left"),index=body.Child(add.Id,"right");
                    if(array.Kind=="array-decay" && index.Kind=="constant" && long.TryParse(index.ConstantText,out long number) && number>=0)
                        return checked(Offset(body.Child(array.Id,"operand"),depth+1)+number*StorageSize(Type(target.TypeId!.Value)));
                }
            }
            throw new InvalidDataException("Generated zero initializer writes outside its model");
        }
        var assignedMinusOne=new HashSet<long>();
        void Literal(Node literal,long offset)
        {
            if(literal.Kind=="convert") { Literal(body.Child(literal.Id,"operand"),offset);return; }
            bool minusOne=minusOneOffsets.Contains(offset);
            if(literal.Kind!="constant" || (minusOne?literal.ConstantText!="-1":
                literal.ConstantText is null || literal.ConstantText.Any(ch=>ch!='0')))
                throw new InvalidDataException("Generated model differs from typed semantic zero initialization");
            if(minusOne)assignedMinusOne.Add(offset);
        }
        void Stores(Node node)
        {
            if(node.Kind=="block") { foreach(var edge in body.Children(node.Id,"statement"))Stores(body.Nodes[edge.ChildId]);return; }
            if(node.Kind=="expr-stmt") { Stores(body.Child(node.Id,"value"));return; }
            if(node.Kind=="assign") { Literal(body.Child(node.Id,"value"),Offset(body.Child(node.Id,"target")));return; }
            if(node.Kind=="array-init")
            {
                Node target=body.Child(node.Id,"target");long offset=Offset(target);
                string leaf=ScalarTypes.Leaf(Type(target.TypeId!.Value));long size=StorageSize(leaf);
                foreach(var edge in body.Children(node.Id,"element"))Literal(body.Nodes[edge.ChildId],checked(offset+edge.Ordinal*size));
                return;
            }
            throw new InvalidDataException("Generated semantic zero model has an unowned initializer operation");
        }
        Stores(body.Child(initialization.Id,"body"));
        if(!assignedMinusOne.SetEquals(minusOneOffsets))
            throw new InvalidDataException("Generated semantic zero model omitted null data-member-pointer initialization");
    }
}
