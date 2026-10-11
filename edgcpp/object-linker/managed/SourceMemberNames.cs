namespace DotObj;

internal sealed partial class ObjectFile
{
    // Stock expr.c represents an unresolved selection as dot_static/points_to_static
    // with a tpck_member name on the right. Its receiver remains the left operand;
    // the unknown proxy type is not a standalone template argument identity.
    private readonly Dictionary<long,(long Operation,long Receiver)> sourceMemberNames=[];
    private readonly HashSet<long> sourceMemberNameConstants=[];
    private void ReadSourceMemberNames()
    {
        foreach(var constant in SourceSemanticConstants.Values)
            if(constant.Kind=="member" && constant.OwnerSourceTypeId is long owner &&
                SourceTypes[owner].Kind=="dependent" && SourceDependentTypes[owner].Kind=="unknown")
                sourceMemberNameConstants.Add(constant.Id);
        if(sourceMemberNameConstants.Count==0)return;
        var parents=new Dictionary<long,List<(long Parent,SourceExpressionEdge Edge)>>();
        foreach(var pair in SourceExpressionEdges)
            foreach(var edge in pair.Value)
            {
                if(!parents.TryGetValue(edge.Child,out var list))parents[edge.Child]=list=[];
                list.Add((pair.Key,edge));
            }
        var usedConstants=new HashSet<long>();
        foreach(var pair in SourceExpressionConstants.Where(pair=>sourceMemberNameConstants.Contains(pair.Value)))
        {
            var uses=parents.GetValueOrDefault(pair.Key,[]);
            if(uses.Count!=1 || uses[0].Edge.Role!="operand" || uses[0].Edge.Ordinal!=1 ||
                !SourceExpressionOperations.TryGetValue(uses[0].Parent,out var operation) ||
                operation.Name is not("eok_dot_static" or "eok_points_to_static"))
                throw new InvalidDataException("Unknown-owner member name lacks its exact source receiver operation");
            var edges=SourceExpressionEdges[uses[0].Parent];
            if(edges.Count!=2 || edges[0].Role!="operand" || edges[0].Ordinal!=0 || edges[1].Child!=pair.Key ||
                SourceExpressions[uses[0].Parent].Kind!="operation" || SourceExpressions[pair.Key].Kind!="constant" ||
                SourceExpressions[pair.Key].SourceTypeId!=SourceSemanticConstants[pair.Value].SourceTypeId)
                throw new InvalidDataException("Unknown-owner member name has an invalid receiver or name operand");
            sourceMemberNames.Add(pair.Key,(uses[0].Parent,edges[0].Child));
            usedConstants.Add(pair.Value);
        }
        foreach(long constant in sourceMemberNameConstants)
            if(!usedConstants.Contains(constant))
                throw new InvalidDataException("Unknown-owner member constant has no original member selection");
        string expressions=string.Join(",",sourceMemberNames.Keys.Order().Select(id=>id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using(var roots=database.Prepare("SELECT m.name,p.[from] FROM sqlite_master m JOIN pragma_foreign_key_list(m.name) p WHERE m.type='table' AND p.[table]='source_expressions'"))
            while(roots.Read())
            {
                string table=roots.Text(0),column=roots.Text(1);
                if(table=="source_expression_edges" && column=="child_expression_id" ||
                    column=="expression_id" && table is "source_expression_constants" or "source_expression_flags" or "source_expression_bindings")continue;
                using var q=database.Prepare($"SELECT 1 FROM [{table}] WHERE [{column}] IN ({expressions}) LIMIT 1");
                if(q.Read())throw new InvalidDataException("Unknown-owner member name cannot escape its receiver as a source-expression root");
            }
        // Reject every non-expression use, including nested constant operands,
        // template arguments/defaults and initializers. Consult the verified schema
        // so newly added semantic relations cannot accidentally bypass this rule.
        string names=string.Join(",",sourceMemberNameConstants.Order().Select(id=>id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using var references=database.Prepare("SELECT m.name,p.[from] FROM sqlite_master m JOIN pragma_foreign_key_list(m.name) p WHERE m.type='table' AND p.[table]='source_semantic_constants' AND m.name!='source_expression_constants'");
        while(references.Read())
        {
            string table=references.Text(0),column=references.Text(1);
            using var q=database.Prepare($"SELECT 1 FROM [{table}] WHERE [{column}] IN ({names}) LIMIT 1");
            if(q.Read())
                throw new InvalidDataException("Unknown-owner member name cannot be used as a standalone semantic constant");
        }
    }
    private string SourceExpressionConstantIdentity(long expression,int depth)
    {
        long constant=SourceExpressionConstants[expression];
        if(!sourceMemberNames.TryGetValue(expression,out var context))return SourceConstantIdentity(constant,depth);
        var value=SourceSemanticConstants[constant];var operation=SourceExpressionOperations[context.Operation];
        return "dependent-member-name:"+SourceIdentityFrame(SourceConstantTypeIdentity(value.SourceTypeId,depth+1))+
            SourceIdentityFrame(value.SourceName)+SourceIdentityFrame(operation.Opcode.ToString(System.Globalization.CultureInfo.InvariantCulture))+
            SourceIdentityFrame(SourceStructuralExpressionKey(context.Receiver,depth+1));
    }
}
