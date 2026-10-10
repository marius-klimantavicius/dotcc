using System.Globalization;

namespace DotObj;

internal sealed record ArrayAllocationOperation(long NodeId, long SourceAllocationObligationId, long? SourceDestructionObligationId,
    long AllocatorEntityId, long DeallocatorEntityId, long? ConstructorEntityId, long? DestructorEntityId,
    long ElementTypeId, long PaddingSize, string HelperKind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, ArrayAllocationOperation> ArrayAllocationOperations { get; } = [];

    public Dictionary<long, long> SourceArrayAllocationElements { get; } = [];

    private void ReadSourceArrayAllocationElements()
    {
        using var query = database.Prepare("SELECT source_allocation_obligation_id,source_destruction_obligation_id,capture_proof FROM source_array_allocation_elements ORDER BY source_allocation_obligation_id");
        while (query.Read())
        {
            long allocationId = query.Integer(0), destructionId = query.Integer(1);
            if (query.Text(2) != "before-lowering" ||
                !SourceAllocationObligations.TryGetValue(allocationId, out var allocation) || !allocation.IsArray ||
                !SourceDestructionObligations.TryGetValue(destructionId, out var destruction) || !destruction.IsPartialAggregate ||
                allocation.FunctionEntityId != destruction.FunctionEntityId || allocation.SourceInitializerId != destruction.SourceInitializerId ||
                DestructionElementType(allocation.AllocatedSourceTypeId).ExecutionTypeId != DestructionElementType(destruction.SourceTypeId).ExecutionTypeId ||
                !SourceArrayAllocationElements.TryAdd(allocationId, destructionId))
                throw new InvalidDataException("Original array allocation element lacks its exact original source association");
        }
    }

    private void ReadArrayAllocationOperations()
    {
        ReadSourceArrayAllocationElements();
        using var query = database.Prepare("SELECT a.node_id,a.source_allocation_obligation_id,a.allocator_entity_id,a.deallocator_entity_id,a.constructor_entity_id,a.destructor_entity_id,a.element_type_id,a.padding_size,a.helper_kind,a.capture_proof,n.function_id,a.source_destruction_obligation_id FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id ORDER BY a.node_id");
        while (query.Read())
        {
            var value = new ArrayAllocationOperation(query.Integer(0), query.Integer(1), query.OptionalInteger(11), query.Integer(2), query.Integer(3),
                query.OptionalInteger(4), query.OptionalInteger(5), query.Integer(6), query.Integer(7), query.Text(8), query.Text(9));
            long functionId = query.Integer(10);
            if (!SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId, out var original) ||
                !original.IsArray || !Functions.ContainsKey(functionId) ||
                !IsSourceLifetimeExecution(SourceLifetimes[original.LifetimeId], functionId) ||
                OriginalFunction(value.DeallocatorEntityId) != original.DeallocatorEntityId ||
                DestructionElementType(original.AllocatedSourceTypeId).ExecutionTypeId != value.ElementTypeId ||
                value.PaddingSize < 0 || value.HelperKind is not ("ia64-vector-new2" or "ia64-vector-new3") ||
                value.CaptureProof != "observed-array-new-runtime" || !ArrayAllocationOperations.TryAdd(value.NodeId, value))
                throw new InvalidDataException("Vector allocation lacks its exact original array allocation, type or execution owner");
            if (value.SourceDestructionObligationId is long destructionId)
            {
                if (SourceArrayAllocationElements.GetValueOrDefault(value.SourceAllocationObligationId) != destructionId ||
                    !SourceDestructionObligations.TryGetValue(destructionId, out var destruction) ||
                    !IsSourceLifetimeExecution(SourceLifetimes[destruction.LifetimeId], functionId) ||
                    value.DestructorEntityId is not long destructorId || OriginalFunction(destructorId) != destruction.DestructorEntityId ||
                    DestructionElementType(destruction.SourceTypeId).ExecutionTypeId != value.ElementTypeId)
                    throw new InvalidDataException("Vector allocation prefix rollback differs from original element destruction");
            }
            else if (value.DestructorEntityId is not null)
                throw new InvalidDataException("Vector allocation destructor has no original source obligation");
            else if (SourceArrayAllocationElements.TryGetValue(value.SourceAllocationObligationId, out long eliminatedElement))
                RequireScalar($"SELECT count(*) FROM destruction_elisions WHERE source_obligation_id={eliminatedElement} AND function_entity_id={functionId} AND kind='stock-no-effect' AND capture_proof='observed-no-effect-destruction-removal'", 1);
            if (AllocationCleanupActions.Values.Any(action => action.SourceAllocationObligationId == original.Id && action.FunctionEntityId == functionId))
                throw new InvalidDataException("Vector helper allocation also has an explicit allocation cleanup region");
        }
        RejectRows("""
            SELECT a.node_id FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id
            JOIN internal_try_expressions x ON x.source_allocation_obligation_id=a.source_allocation_obligation_id
            JOIN nodes b ON b.id=x.node_id WHERE b.function_id=n.function_id
            """, "Vector helper allocation also has an internal rollback boundary");
        RejectRows("""
            SELECT a.node_id FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id
            JOIN allocation_elisions e ON e.source_allocation_obligation_id=a.source_allocation_obligation_id AND e.function_entity_id=n.function_id
            """, "Vector helper allocation also claims empty initialization");
    }
    public void VerifyArrayAllocationBody(long functionId, FunctionBody body)
    {
        foreach (var value in ArrayAllocationOperations.Values.Where(value => body.Nodes.ContainsKey(value.NodeId)))
        {
            Node call = body.Nodes[value.NodeId];
            string helperName = value.HelperKind == "ia64-vector-new2" ? "__cxa_vec_new2" : "__cxa_vec_new3";
            if (call.Kind != "call" || call.EntityId is not long helperId || !Entities.TryGetValue(helperId, out var helper) ||
                helper.Definition != "declaration" || helper.SourceName != helperName || helper.ExternalKey != helperName ||
                SourceFunctionKinds.GetValueOrDefault(helperId) != "runtime" || Type(helper.ReturnTypeId) != "ptr(void)" ||
                Routines[helper.RoutineTypeId].Variadic)
                throw new InvalidDataException("Vector allocation is not its observed direct runtime helper call");
            bool sized = value.HelperKind == "ia64-vector-new3";
            var parameters = Parameters.GetValueOrDefault(helperId, []);
            bool Signature(string type, string result, params string[] arguments) => RoutineTypes.IsFunctionPointer(type) &&
                RoutineTypes.Signature(ScalarTypes.Pointee(type)) is var signature && !signature.Variadic &&
                signature.Result == result && signature.Parameters.SequenceEqual(arguments);
            if (parameters.Count != 7 || !parameters.Take(3).SequenceEqual(["u64", "u64", "u64"]) ||
                !Signature(parameters[3], "void", "ptr(void)") || !Signature(parameters[4], "void", "ptr(void)") ||
                !Signature(parameters[5], "ptr(void)", "u64") ||
                !Signature(parameters[6], "void", sized ? ["ptr(void)", "u64"] : ["ptr(void)"]))
                throw new InvalidDataException("Vector allocation runtime signature differs from its observed Itanium protocol");
            var edges = body.Children(call.Id, "argument").ToArray();
            if (edges.Length != 7 || !edges.Select(edge => edge.Ordinal).SequenceEqual(Enumerable.Range(0, 7).Select(i => (long)i)))
                throw new InvalidDataException("Vector allocation runtime arguments are incomplete");
            Node[] args = edges.Select(edge => body.Nodes[edge.ChildId]).ToArray();
            for (int index = 0; index < args.Length; index++)
                if (args[index].TypeId is not long argumentType || Type(argumentType) != parameters[index])
                    throw new InvalidDataException("Vector allocation argument type differs from its exact helper ABI");
            Node Leaf(Node node)
            {
                var visited = new HashSet<long>();
                while (node.Kind == "convert")
                {
                    if (!visited.Add(node.Id)) throw new InvalidDataException("Cyclic vector callback conversion");
                    node = body.Child(node.Id, "operand");
                }
                return node;
            }
            long Constant(Node node)
            {
                node = Leaf(node);
                if (node.Kind != "constant" || !long.TryParse(node.ConstantText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                    throw new InvalidDataException("Vector allocation extent or padding is not its observed constant");
                return result;
            }
            if (Constant(args[1]) != StorageSize(Type(value.ElementTypeId)) || Constant(args[2]) != value.PaddingSize)
                throw new InvalidDataException("Vector allocation extent or padding differs from observed element layout");
            void Callback(Node node, long? entityId, string result, string[] expectedParameters, bool receiver)
            {
                node = Leaf(node);
                if (entityId is null)
                {
                    if (node.Kind != "constant" || node.ConstantText != "0")
                        throw new InvalidDataException("Absent vector callback is not a null pointer");
                    return;
                }
                if (node.Kind != "function-address" || node.EntityId != entityId || !Entities.TryGetValue(entityId.Value, out var entity) ||
                    Type(entity.ReturnTypeId) != result || Routines[entity.RoutineTypeId].Variadic)
                    throw new InvalidDataException("Vector callback differs from its observed function");
                var actual = Parameters.GetValueOrDefault(entity.Id, []);
                if (receiver ? actual.Count != 1 || actual[0] != "ptr(" + Type(value.ElementTypeId) + ")" && actual[0] != "ptr(void)"
                    : !actual.SequenceEqual(expectedParameters))
                    throw new InvalidDataException("Vector callback parameters differ from element or allocation ABI");
            }
            Callback(args[3], value.ConstructorEntityId, "void", [], true);
            Callback(args[4], value.DestructorEntityId, "void", [], true);
            Callback(args[5], value.AllocatorEntityId, "ptr(void)", ["u64"], false);
            Callback(args[6], value.DeallocatorEntityId, "void", sized ? ["ptr(void)", "u64"] : ["ptr(void)"], false);
        }
    }

}
