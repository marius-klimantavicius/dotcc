namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private static bool IsIncompleteArray(Unit unit,long id)
    {
        var type=unit.Original(id);
        return type.Kind=="array" && type.Incomplete && type.Count is null && type.ArrayBoundKind is null;
    }

    private sealed partial class FunctionWriter
    {
        private string? IncompleteArrayAddress(Node node)
        {
            if(node.Type is not long id || !IsIncompleteArray(unit,id))return null;
            var type=unit.Original(id);
            if(node.Kind=="variable" && node.Symbol is long symbol)
                return "(("+emitter.Pointer(unit,type.Element??throw unit.Missing(node,"incomplete array element type missing"))+")"+Name(symbol)+")";
            if(node.Kind=="operation" && node.Operator is "indirect" or "ref-indirect")
                return Expression(Child(node,"operand"));
            if(node.Kind=="operation" && node.Operator=="lvalue-adjust")
                return ArrayAddress(Child(node,"operand"));
            throw unit.Missing(node,"incomplete array expression requires an address view without value/stride storage: type "+id);
        }
    }
}
