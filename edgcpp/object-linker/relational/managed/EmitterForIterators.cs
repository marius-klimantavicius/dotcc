namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private string ForIterator(Node node)
        {
            // A for iterator discards each selected comma operand's value.
            // C# permits the same ordered statement expressions in its header.
            if(node.Kind=="operation" && node.Operator=="comma")
            {
                if(node.EvaluationOrder=="right-to-left")
                    throw unit.Missing(node,"right-to-left for iterator comma emission is not implemented");
                var lifetimes=unit.Lifetimes.Values.Where(lifetime=>lifetime.OwnerNode==node.Id).Select(lifetime=>lifetime.Id);
                if(node.Lifetime is long direct)lifetimes=lifetimes.Append(direct);
                if(lifetimes.Any(id=>unit.Destructions.GetValueOrDefault(id,[]).Any(action=>!allocationOwnedCleanups.Contains(action))))
                    throw unit.Missing(node,"for iterator comma cleanup requires statement expansion");
                var operands=unit.Children(node,"operand").ToArray();
                if(operands.Length==0)throw unit.Missing(node,"for iterator comma operands missing");
                return string.Join(", ",operands.Select(ForIterator));
            }
            return Header(node);
        }
    }
}
