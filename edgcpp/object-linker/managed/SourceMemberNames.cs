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
        // An unknown-owner name token is retained as EDG emitted it. Its
        // graph can have multiple use contexts; do not invent a unique receiver.
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
