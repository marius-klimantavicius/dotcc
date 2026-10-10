namespace DotObj;
internal sealed record GeneratedEntityInitializer(long FunctionEntityId,long OwnerFunctionEntityId,long SourceAllocationObligationId,
    long SourceElementTypeId,long ElementTypeId,long DestinationBindingId,long CountBindingId,long ModelBindingId,long CopyNodeId);
internal sealed partial class ObjectFile
{
    public Dictionary<long,GeneratedEntityInitializer> GeneratedEntityInitializers {get;}=[];
    public Dictionary<long,long> GeneratedEntityInitializerCalls {get;}=[];
    private void ReadGeneratedEntityInitializers()
    {
        using(var q=database.Prepare("SELECT function_entity_id,owner_function_entity_id,source_allocation_obligation_id,source_element_type_id,element_type_id,destination_binding_id,count_binding_id,model_binding_id,copy_node_id,capture_proof FROM generated_entity_initializers ORDER BY function_entity_id"))
        while(q.Read())
        {
            var value=new GeneratedEntityInitializer(q.Integer(0),q.Integer(1),q.Integer(2),q.Integer(3),q.Integer(4),q.Integer(5),q.Integer(6),q.Integer(7),q.Integer(8));
            string element=Type(value.ElementTypeId),pointer="ptr("+element+")";
            if(SourceLanguage!="c++" || !Entities.TryGetValue(value.FunctionEntityId,out var helper) || helper.Linkage!="internal" || helper.Definition!="strong" ||
               !Functions.ContainsKey(helper.Id) || Type(helper.ReturnTypeId)!="void" || Routines[helper.RoutineTypeId].Variadic ||
               !Parameters.GetValueOrDefault(helper.Id,[]).SequenceEqual([pointer,"u64",pointer]) ||
               !ScalarTypes.IsRecord(element) || RecordsByKey[element].RepresentationKind!="complete" ||
               !SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId,out var allocation) || !allocation.IsArray ||
               !IsSourceLifetimeExecution(SourceLifetimes[allocation.LifetimeId],value.OwnerFunctionEntityId) ||
               DestructionElementType(allocation.AllocatedSourceTypeId).ExecutionTypeId!=value.ElementTypeId ||
               SourceTypeSignature(DestructionElementType(allocation.AllocatedSourceTypeId).Id)!=SourceTypeSignature(value.SourceElementTypeId) ||
               SourceTypeById(value.SourceElementTypeId).ExecutionTypeId!=value.ElementTypeId ||
               q.Text(9)!="observed-counted-model-initializer-factory" || !GeneratedEntityInitializers.TryAdd(helper.Id,value))
                throw new InvalidDataException("Counted model initializer lacks its exact generated factory, element and source allocation");
            long[] bindings=[value.DestinationBindingId,value.CountBindingId,value.ModelBindingId];
            string[] roles=["entity-destination","entity-count","entity-model"];
            if(bindings.Distinct().Count()!=3)throw new InvalidDataException("Counted initializer parameters overlap");
            for(int index=0;index<3;++index)
            {
                if(ParameterRoles[(helper.Id,index)].Role!=roles[index])throw new InvalidDataException("Counted initializer parameter role changed");
                RequireScalar($"SELECT count(*) FROM bindings WHERE id={bindings[index]} AND function_id={helper.Id} AND kind='parameter' AND ordinal={index} AND is_synthetic=1",1);
                RequireScalar($"SELECT count(*) FROM source_declarations WHERE binding_id={bindings[index]}",0);
            }
            RequireScalar($"SELECT count(*) FROM nodes WHERE id={value.CopyNodeId} AND function_id={helper.Id} AND kind='record-transfer' AND type_id={value.ElementTypeId}",1);
        }
        using(var q=database.Prepare("SELECT node_id,helper_entity_id,capture_proof FROM generated_entity_initializer_calls ORDER BY node_id"))
        while(q.Read())
        {
            long node=q.Integer(0),helper=q.Integer(1);
            if(!GeneratedEntityInitializers.TryGetValue(helper,out var value) || q.Text(2)!="observed-entity-initializer-call" ||
               !GeneratedEntityInitializerCalls.TryAdd(node,helper))throw new InvalidDataException("Counted initializer call lacks its exact factory");
            RequireScalar($"SELECT count(*) FROM nodes WHERE id={node} AND kind='call' AND entity_id={helper} AND function_id={value.OwnerFunctionEntityId}",1);
            RequireScalar($"SELECT count(*) FROM nodes WHERE kind='call' AND entity_id={helper}",1);
            RequireScalar($"SELECT count(*) FROM nodes WHERE kind='function-address' AND entity_id={helper}",0);
        }
        foreach(long helper in GeneratedEntityInitializers.Keys)
            if(GeneratedEntityInitializerCalls.Values.Count(value=>value==helper)!=1)throw new InvalidDataException("Counted initializer must have its unique observed direct call");
        RejectRows("SELECT p.entity_id FROM parameters p LEFT JOIN generated_entity_initializers g ON g.function_entity_id=p.entity_id WHERE p.parameter_role IN ('entity-destination','entity-count','entity-model') AND g.function_entity_id IS NULL", "Counted initializer parameter lacks its generated factory");
    }
    private void VerifyGeneratedEntityModelCopies()
    {
        foreach(var value in GeneratedEntityInitializers.Values)
            if(!RecordStorageCopies.TryGetValue(value.CopyNodeId,out var copy) || copy.Kind!="entity-model" ||
               copy.SourceRecordId!=RecordsByKey[Type(value.ElementTypeId)].SourceRecordId)
                throw new InvalidDataException("Counted model initializer lost exact complete storage copy proof");
        foreach(var pair in RecordStorageCopies.Where(pair=>pair.Value.Kind=="entity-model"))
            if(!GeneratedEntityInitializers.Values.Any(value=>value.CopyNodeId==pair.Key))
                throw new InvalidDataException("Entity model copy lacks its generated helper owner");
    }
    public void VerifyGeneratedEntityInitializerBody(long functionId,FunctionBody body)
    {
        foreach(var call in GeneratedEntityInitializerCalls.Where(pair=>GeneratedEntityInitializers[pair.Value].OwnerFunctionEntityId==functionId))
        {
            var value=GeneratedEntityInitializers[call.Value];var args=body.Children(call.Key,"argument").ToArray();
            string pointer="ptr("+Type(value.ElementTypeId)+")";
            if(args.Length!=3 || Type(body.Nodes[args[0].ChildId].TypeId!.Value)!=pointer ||
               Type(body.Nodes[args[1].ChildId].TypeId!.Value)!="u64" || Type(body.Nodes[args[2].ChildId].TypeId!.Value)!=pointer)
                throw new InvalidDataException("Counted initializer call changed destination/count/model arguments");
        }
        if(!GeneratedEntityInitializers.TryGetValue(functionId,out var origin))return;
        bool Parameter(Node node,long binding)=>node.Kind=="binding" && node.BindingId==binding;
        Node copy=body.Nodes[origin.CopyNodeId],target=body.Child(copy.Id,"target"),model=body.Child(copy.Id,"value");
        if(target.Kind!="dereference" || body.Child(target.Id,"operand") is not {Kind:"mutate",Operator:"post-inc"} advance ||
           !Parameter(body.Child(advance.Id,"operand"),origin.DestinationBindingId) || model.Kind!="dereference" ||
           !Parameter(body.Child(model.Id,"operand"),origin.ModelBindingId))
            throw new InvalidDataException("Counted initializer copy lost exact advancing destination and fixed model");
        bool Count(Node value)
        {
            if(value.Kind=="convert" && Type(value.TypeId!.Value)=="bool")return Count(body.Child(value.Id,"operand"));
            if(value.Kind=="binary" && value.Operator=="ne")
            {
                var right=body.Child(value.Id,"right");
                return right.Kind=="constant" && right.ConstantText=="0" && Count(body.Child(value.Id,"left"));
            }
            return value.Kind=="mutate" && value.Operator=="post-dec" && Parameter(body.Child(value.Id,"operand"),origin.CountBindingId);
        }
        int loops=0,copies=0;bool returned=false;
        void Walk(Node node,bool inside=false,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("Counted initializer nesting exceeds bound");
            if(node.Kind=="block") {foreach(var edge in body.Children(node.Id,"statement"))Walk(body.Nodes[edge.ChildId],inside,depth+1);return;}
            if(node.Kind=="expr-stmt") {Walk(body.Child(node.Id,"value"),inside,depth+1);return;}
            if(node.Kind=="return" && !inside && !body.Edges.GetValueOrDefault(node.Id,[]).Any() && !returned) {returned=true;return;}
            if(returned)throw new InvalidDataException("Counted initializer has operations after return");
            if(node.Kind=="while" && !inside && loops++==0 && Count(body.Child(node.Id,"condition"))) {Walk(body.Child(node.Id,"body"),true,depth+1);return;}
            if(inside && node.Id==origin.CopyNodeId && copies++==0)return;
            throw new InvalidDataException("Counted initializer contains an unowned operation or loop");
        }
        Walk(body.Nodes[body.RootId]);
        if(loops!=1 || copies!=1)throw new InvalidDataException("Counted initializer lost its unique loop/copy");
    }
}
