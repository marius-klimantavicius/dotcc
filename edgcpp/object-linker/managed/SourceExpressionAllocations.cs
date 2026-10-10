using System.Globalization;
namespace DotObj;

internal sealed record SourceExpressionAllocation(long ObjectType,long? Routine,bool New,bool Dependent,bool Placement,bool Aligned,bool ArrayDelete,bool Global,
    bool HasInitializer,bool Braced,bool ParenthesizedAggregate,bool Deducible,bool ParenthesizedType,bool LookupDeferred);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceExpressionAllocation> SourceExpressionAllocations { get; }=[];
    public Dictionary<long,long> SourceDeclarationOperators { get; }=[];
    private void ReadSourceExpressionAllocations()
    {
        using(var q=database.Prepare("SELECT declaration_id,operator_kind,capture_proof FROM source_declaration_operators ORDER BY declaration_id"))
            while(q.Read())
                if(!SourceExpressionDeclarations.TryGetValue(q.Integer(0),out var declaration) || declaration.Kind!="routine" || declaration.RoutineKind!="operator" ||
                    q.Integer(1) is <1 or >47 || q.Text(2)!="original-operator-declaration" || !SourceDeclarationOperators.TryAdd(q.Integer(0),q.Integer(1)))
                    throw new InvalidDataException("Source operator declaration lost its exact original operator kind");
        using(var q=database.Prepare("SELECT expression_id,object_source_type_id,operator_declaration_id,is_new,is_dependent_selection,is_placement,is_aligned,is_array_delete,is_global,has_initializer,is_braced_initializer,is_parenthesized_aggregate,has_deducible_type,has_parenthesized_type_id,initialization_disposition,rollback_disposition,capture_proof,is_lookup_deferred FROM source_expression_allocations ORDER BY expression_id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceExpressionAllocation(q.Integer(1),q.OptionalInteger(2),q.Integer(3)==1,q.Integer(4)==1,q.Integer(5)==1,q.Integer(6)==1,q.Integer(7)==1,q.Integer(8)==1,q.Integer(9)==1,q.Integer(10)==1,q.Integer(11)==1,q.Integer(12)==1,q.Integer(13)==1,q.Integer(17)==1);
                if(!SourceExpressions.TryGetValue(id,out var expression) || expression.Kind!="allocation-expression" || !SourceTypes.ContainsKey(value.ObjectType) ||
                    value.Routine is long routine && !SourceExpressionDeclarations.ContainsKey(routine) ||
                    Enumerable.Range(3,11).Any(column=>q.Integer(column) is not(0 or 1)) || q.Integer(17) is not(0 or 1) ||
                    q.Text(14)!="absent-in-original" || q.Text(15)!="absent-in-original" || q.Text(16)!="original-new-delete-expression" ||
                    !SourceExpressionAllocations.TryAdd(id,value))
                    throw new InvalidDataException("Invalid original source allocation expression");
            }
    }
    private void VerifySourceAllocationPresence()
    {
        foreach(var expression in SourceExpressions.Values)
            if((expression.Kind=="allocation-expression")!=SourceExpressionAllocations.ContainsKey(expression.Id))
                throw new InvalidDataException("Original allocation expression is missing its original new/delete provenance");
        foreach(var declaration in SourceExpressionDeclarations.Values)
            if((declaration.RoutineKind=="operator")!=SourceDeclarationOperators.ContainsKey(declaration.Id))
                throw new InvalidDataException("Original operator declaration is missing its exact operator kind");
    }
    private void VerifySourceExpressionAllocation(SourceExpression expression,List<SourceExpressionEdge> edges)
    {
        var value=SourceExpressionAllocations[expression.Id];
        if(expression.SourceTypeId is not long result || expression.Category!="prvalue" ||
            (value.Routine is null)!=value.Dependent || value.LookupDeferred && (!value.New || !value.Dependent) ||
            value.Dependent && !value.LookupDeferred && !SourceTypeContainsParameter(value.ObjectType,0) ||
            !value.New && (value.Placement || value.HasInitializer || value.Braced || value.ParenthesizedAggregate || value.Deducible || value.ParenthesizedType) ||
            value.New && value.ArrayDelete || (value.Braced || value.ParenthesizedAggregate) && !value.HasInitializer || value.Braced && value.ParenthesizedAggregate)
            throw new InvalidDataException("Original source allocation lost its exact selection or new/delete flags");
        var objectType=Original(SourceTypeById(value.ObjectType));var resultType=Original(SourceTypeById(result));
        if(value.New)
        {
            long pointee=objectType.Kind=="array"?objectType.UnderlyingTypeId!.Value:value.ObjectType;
            if(resultType.Kind!="pointer" || resultType.UnderlyingTypeId is not long target || SourceTypeSignature(target)!=SourceTypeSignature(pointee))
                throw new InvalidDataException("Source new expression result does not point to its original allocated type");
        }
        else if(resultType.Kind!="void")throw new InvalidDataException("Original source delete expression must have void result");
        if(edges.Any(edge=>edge.Role is not("allocation-argument" or "delete-operand" or "array-bound")) ||
            edges.Count(edge=>edge.Role=="delete-operand")!=(value.New?0:1) ||
            !value.New && edges.Any(edge=>edge.Role is "allocation-argument" or "array-bound") ||
            edges.Count(edge=>edge.Role=="array-bound")>1 || edges.Any(edge=>edge.Role=="array-bound") && objectType.Kind!="array")
            throw new InvalidDataException("Original source allocation lost its exact ordered operands and array bound");
        if(!value.New)
        {
            var operand=SourceExpressions[edges.Single(edge=>edge.Role=="delete-operand").Child];
            if(operand.SourceTypeId is not long operandId)throw new InvalidDataException("Original delete operand lacks its exact pointer/object correspondence");
            var operandType=Original(SourceTypeById(operandId));
            bool typed=operandType.Kind=="pointer" && operandType.UnderlyingTypeId is long pointed && SourceTypeSignature(pointed)==SourceTypeSignature(value.ObjectType);
            // Before substitution EDG can retain both the deleted type and
            // operand result as the very same original unknown-dependent type.
            // The owned operand graph retains the unresolved cast/member/call.
            // scan_delete_operator first recognizes an original dependent
            // operand (or pointer to one), then deliberately records the
            // unknown deleted type until substitution, even for a typed T*.
            bool DependentObject(SourceType type)=>type.Kind=="dependent" ||
                type.Kind=="record" && SourceRecordsByType.TryGetValue(type.Id,out var record) && record.IsDependent;
            bool unresolved=value.Dependent && objectType.Kind=="dependent" &&
                SourceDependentTypes.TryGetValue(objectType.Id,out var dependent) && dependent.Kind=="unknown" &&
                (DependentObject(operandType) || operandType.Kind=="pointer" && operandType.UnderlyingTypeId is long pointee && DependentObject(Original(SourceTypeById(pointee))));
            if(!typed && !unresolved)throw new InvalidDataException("Original delete operand lacks its exact pointer/object correspondence");
        }
        if(value.Routine is long routine)
        {
            long expected=value.New?(objectType.Kind=="array"?3:1):(value.ArrayDelete?4:2);
            if(!SourceDeclarationOperators.TryGetValue(routine,out long kind) || kind!=expected || value.Global && SourceExpressionDeclarations[routine].OwnerType is not null)
                throw new InvalidDataException("Original allocation selected a different operator family");
        }
    }
    private string SourceAllocationIdentity(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Source allocation identity depth");
        var v=SourceExpressionAllocations[id];
        return SourceIdentityFrame(SourceExpressionTypeKey(v.ObjectType,depth+1))+SourceIdentityFrame(v.Routine is long routine?SourceStructuralDeclarationKey(routine,depth+1):"")+
            (v.New?"1":"0")+(v.Dependent?"1":"0")+(v.Placement?"1":"0")+(v.Aligned?"1":"0")+(v.ArrayDelete?"1":"0")+(v.Global?"1":"0")+
            (v.HasInitializer?"1":"0")+(v.Braced?"1":"0")+(v.ParenthesizedAggregate?"1":"0")+(v.Deducible?"1":"0")+(v.ParenthesizedType?"1":"0")+(v.LookupDeferred?"1":"0");
    }
}
