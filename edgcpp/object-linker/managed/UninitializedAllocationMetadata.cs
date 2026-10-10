namespace DotObj;

internal sealed record SourceUninitializedAllocation(long SourceAllocationObligationId,
    long AllocatorEntityId, bool IsPlacement, bool HasInitializer, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceUninitializedAllocation> SourceUninitializedAllocations { get; } = [];

    // A frontend-selected trivial default constructor can leave a registered
    // rollback DIP without an associated new initializer. This source relation
    // is captured before lowering; absence of executable cleanup alone is never
    // treated as evidence that initialization cannot throw.
    private void ReadSourceUninitializedAllocations()
    {
        using var query = database.Prepare("SELECT source_allocation_obligation_id,allocator_entity_id,is_placement,has_initializer,capture_proof FROM source_uninitialized_allocations ORDER BY source_allocation_obligation_id");
        while (query.Read())
        {
            var value = new SourceUninitializedAllocation(query.Integer(0), query.Integer(1),
                query.Integer(2) != 0, query.Integer(3) != 0, query.Text(4));
            if (!SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId, out var obligation) ||
                Original(SourceTypeById(obligation.AllocatedSourceTypeId)).Kind != (obligation.IsArray ? "array" : "record") ||
                !EntitySourceRoutines.TryGetValue(value.AllocatorEntityId, out var allocator) ||
                SourceFunctionKinds[value.AllocatorEntityId] is not ("ordinary" or "runtime") ||
                allocator.ObjectTypeId is not null || allocator.Variadic || allocator.Parameters.Length == 0 ||
                SourceTypeSignature(allocator.ReturnTypeId) != "ptr(void)" ||
                Original(SourceTypeById(allocator.Parameters[0])).Kind is not ("unsigned-long" or "unsigned-int" or "unsigned-long-long") ||
                query.Integer(2) is not (0 or 1) || query.Integer(3) is not (0 or 1) ||
                value.CaptureProof != "original-new-without-initialization" ||
                !SourceUninitializedAllocations.TryAdd(value.SourceAllocationObligationId, value))
                throw new InvalidDataException("Uninitialized new lacks its original allocation type/array shape and selected allocator");
            if (value.IsPlacement)
            {
                var deallocator = EntitySourceRoutines[obligation.DeallocatorEntityId];
                if (allocator.Parameters.Length < 2 || allocator.Parameters.Length != deallocator.Parameters.Length ||
                    !allocator.Parameters.Skip(1).Select(id => SourceParameterSignature(id, 0))
                        .SequenceEqual(deallocator.Parameters.Skip(1).Select(id => SourceParameterSignature(id, 0))))
                    throw new InvalidDataException("Uninitialized placement new differs from its selected matching delete parameters");
            }
        }
        RejectRows("""
            SELECT u.source_allocation_obligation_id FROM source_uninitialized_allocations u
            JOIN array_allocation_operations a ON a.source_allocation_obligation_id=u.source_allocation_obligation_id
            """, "Uninitialized new also has an initialized array allocation protocol");
    }
}
