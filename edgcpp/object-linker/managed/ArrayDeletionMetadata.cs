using System.Globalization;

namespace DotObj;

internal sealed record ArrayDeletionOperation(long NodeId, long SourceObligationId, long DestructorEntityId,
    long DeallocatorEntityId, long ElementTypeId, long PaddingSize, string HelperKind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, long> SourceArrayDeletions { get; } = [];
    public Dictionary<long, ArrayDeletionOperation> ArrayDeletionOperations { get; } = [];

    private void ReadArrayDeletionOperations()
    {
        using (var sources = database.Prepare("SELECT source_obligation_id,deallocator_entity_id,capture_proof FROM source_array_deletions ORDER BY source_obligation_id"))
            while (sources.Read())
            {
                long obligationId = sources.Integer(0), deallocatorId = sources.Integer(1);
                if (!SourceDestructionObligations.TryGetValue(obligationId, out var original) || !original.IsPartialAggregate ||
                    !EntitySourceRoutines.TryGetValue(deallocatorId, out var signature) || signature.ObjectTypeId is not null || signature.Variadic ||
                    SourceFunctionKinds[deallocatorId] is not ("ordinary" or "runtime") ||
                    Original(SourceTypeById(signature.ReturnTypeId)).Kind != "void" || signature.Parameters.Length is not (1 or 2) ||
                    Original(SourceTypeById(signature.Parameters[0])).Kind != "pointer" ||
                    sources.Text(2) != "before-array-delete-lowering" || !SourceArrayDeletions.TryAdd(obligationId, deallocatorId))
                    throw new InvalidDataException("Array delete lacks its original partial destruction and selected deallocation routine");
            }
        using var query = database.Prepare("SELECT a.node_id,a.source_obligation_id,a.destructor_entity_id,a.deallocator_entity_id,a.element_type_id,a.padding_size,a.helper_kind,a.capture_proof,n.function_id FROM array_deletion_operations a JOIN nodes n ON n.id=a.node_id ORDER BY a.node_id");
        while (query.Read())
        {
            var value = new ArrayDeletionOperation(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3),
                query.Integer(4), query.Integer(5), query.Text(6), query.Text(7));
            long functionId = query.Integer(8);
            if (!SourceArrayDeletions.TryGetValue(value.SourceObligationId, out long deallocator) ||
                !SourceDestructionObligations.TryGetValue(value.SourceObligationId, out var original) ||
                !IsSourceLifetimeExecution(SourceLifetimes[original.LifetimeId], functionId) || !Functions.ContainsKey(functionId) ||
                OriginalFunction(value.DeallocatorEntityId) != deallocator || OriginalFunction(value.DestructorEntityId) != original.DestructorEntityId ||
                DestructionElementType(original.SourceTypeId).ExecutionTypeId != value.ElementTypeId || value.PaddingSize < 0 ||
                value.HelperKind is not ("ia64-vector-delete2" or "ia64-vector-delete3") || value.CaptureProof != "observed-array-delete-runtime" ||
                !ArrayDeletionOperations.TryAdd(value.NodeId, value))
                throw new InvalidDataException("Array delete protocol differs from original destruction, deallocation or execution ownership");
        }
        if (SourceArrayDeletions.Keys.Except(ArrayDeletionOperations.Values.Select(operation => operation.SourceObligationId)).Any())
            throw new InvalidDataException("Original array delete lacks its observed runtime protocol");
    }

    public void VerifyArrayDeletionBody(long functionId, FunctionBody body)
    {
        foreach (var value in ArrayDeletionOperations.Values.Where(value => body.Nodes.ContainsKey(value.NodeId)))
        {
            Node call = body.Nodes[value.NodeId];
            bool sized = value.HelperKind == "ia64-vector-delete3";
            string helperName = sized ? "__cxa_vec_delete3" : "__cxa_vec_delete2";
            if (call.FunctionId != functionId || call.Kind != "call" || call.EntityId is not long helperId ||
                !Entities.TryGetValue(helperId, out var helper) || helper.Definition != "declaration" ||
                helper.SourceName != helperName || helper.ExternalKey != helperName ||
                SourceFunctionKinds.GetValueOrDefault(helperId) != "runtime" || Type(helper.ReturnTypeId) != "void" || Routines[helper.RoutineTypeId].Variadic)
                throw new InvalidDataException("Array delete is not its observed direct runtime helper call");
            bool Signature(string type, params string[] arguments) => RoutineTypes.IsFunctionPointer(type) &&
                RoutineTypes.Signature(ScalarTypes.Pointee(type)) is var signature && !signature.Variadic &&
                signature.Result == "void" && signature.Parameters.SequenceEqual(arguments);
            var parameters = Parameters.GetValueOrDefault(helperId, []);
            if (parameters.Count != 5 || !parameters.Take(3).SequenceEqual(["ptr(void)", "u64", "u64"]) ||
                !Signature(parameters[3], "ptr(void)") || !Signature(parameters[4], sized ? ["ptr(void)", "u64"] : ["ptr(void)"]))
                throw new InvalidDataException("Array delete helper has a different runtime ABI");
            var edges = body.Children(call.Id, "argument").ToArray();
            if (edges.Length != 5 || !edges.Select(edge => edge.Ordinal).SequenceEqual(Enumerable.Range(0, 5).Select(i => (long)i)))
                throw new InvalidDataException("Array delete helper arguments are incomplete");
            Node[] args = edges.Select(edge => body.Nodes[edge.ChildId]).ToArray();
            for (int index = 0; index < args.Length; ++index)
                if (args[index].TypeId is not long type || Type(type) != parameters[index])
                    throw new InvalidDataException("Array delete argument type differs from exact runtime ABI");
            Node Leaf(Node node)
            {
                var visited = new HashSet<long>();
                while (node.Kind == "convert")
                {
                    if (!visited.Add(node.Id)) throw new InvalidDataException("Cyclic array delete callback conversion");
                    node = body.Child(node.Id, "operand");
                }
                return node;
            }
            long Constant(Node node)
            {
                node = Leaf(node);
                if (node.Kind != "constant" || !long.TryParse(node.ConstantText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                    throw new InvalidDataException("Array delete extent or padding lacks its observed constant");
                return result;
            }
            if (Constant(args[1]) != StorageSize(Type(value.ElementTypeId)) || Constant(args[2]) != value.PaddingSize)
                throw new InvalidDataException("Array delete extent or padding differs from observed element layout");
            void Callback(Node node, long entityId, bool destructor)
            {
                node = Leaf(node);
                if (node.Kind != "function-address" || node.EntityId != entityId || !Entities.TryGetValue(entityId, out var entity) ||
                    Type(entity.ReturnTypeId) != "void" || Routines[entity.RoutineTypeId].Variadic)
                    throw new InvalidDataException("Array delete callback differs from its observed routine");
                var actual = Parameters.GetValueOrDefault(entityId, []);
                if (destructor ? actual.Count != 1 || actual[0] != "ptr(" + Type(value.ElementTypeId) + ")" && actual[0] != "ptr(void)"
                    : !actual.SequenceEqual(sized ? ["ptr(void)", "u64"] : ["ptr(void)"]))
                    throw new InvalidDataException("Array delete callback differs from element or sized-delete ABI");
            }
            Callback(args[3], value.DestructorEntityId, true);
            Callback(args[4], value.DeallocatorEntityId, false);
        }
    }
}
