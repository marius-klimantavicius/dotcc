using System.Globalization;
namespace DotObj;

internal sealed record SourceExpressionAllocation(long ObjectType,long? Routine,bool New,bool Dependent,bool Placement,bool Aligned,bool ArrayDelete,bool Global,
    bool HasInitializer,bool Braced,bool ParenthesizedAggregate,bool Deducible,bool ParenthesizedType,bool LookupDeferred,
    string InitializationDisposition,string RollbackDisposition);
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
                long id=q.Integer(0);var value=new SourceExpressionAllocation(q.Integer(1),q.OptionalInteger(2),q.Integer(3)==1,q.Integer(4)==1,q.Integer(5)==1,q.Integer(6)==1,q.Integer(7)==1,q.Integer(8)==1,q.Integer(9)==1,q.Integer(10)==1,q.Integer(11)==1,q.Integer(12)==1,q.Integer(13)==1,q.Integer(17)==1,q.Text(14),q.Text(15));
                if(!SourceExpressions.TryGetValue(id,out var expression) || expression.Kind!="allocation-expression" || !SourceTypes.ContainsKey(value.ObjectType) ||
                    value.Routine is long routine && (!SourceExpressionDeclarations.TryGetValue(routine,out var selected) || selected.Kind!="routine") ||
                    Enumerable.Range(3,11).Any(column=>q.Integer(column) is not(0 or 1)) || q.Integer(17) is not(0 or 1) ||
                    value.InitializationDisposition is not("absent-in-original" or "retained-in-edg-graph") || value.RollbackDisposition is not("absent-in-original" or "retained-in-edg-graph") || q.Text(16)!="original-new-delete-expression" ||
                    !SourceExpressionAllocations.TryAdd(id,value))
                    throw new InvalidDataException("Invalid original source allocation expression");
                if(value.InitializationDisposition=="retained-in-edg-graph" || value.RollbackDisposition=="retained-in-edg-graph")
                {
                    using var root=database.Prepare("SELECT node_id FROM edg_graph_roots WHERE role='source-expression' AND ordinal=?");root.Bind(id-1);
                    if(!root.Read())throw new InvalidDataException("Source allocation metadata lacks its retained EDG graph root");
                }
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
        if(!SourceExpressionAllocations.ContainsKey(expression.Id) || expression.SourceTypeId is null ||
            edges.Any(edge=>edge.Role is not("allocation-argument" or "delete-operand" or "array-bound")))
            throw new InvalidDataException("Source allocation projection has invalid typed fields or operand roles");
        // EDG selected the operator, result type and initialization/rollback
        // policy. The raw graph retains those facts without selecting again.
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
