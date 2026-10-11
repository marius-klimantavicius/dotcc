namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private string PointerDifference(Node node,Node left,Node right)
        {
            foreach(var operand in new[]{left,right})
            {
                var pointer=unit.Original(operand.ExecutionType??operand.Type??throw unit.Missing(operand,"pointer difference operand type missing"));
                if(pointer.Kind!="pointer" || pointer.Pointee is not long target)
                    throw unit.Missing(node,"exporter defect: pointer difference requires recorded pointer operands");
                var pointee=unit.Original(target);
                if(pointee.Dependent || pointee.Incomplete || pointee.Size is not >0 ||
                    pointee.Kind is "void" or "function" || pointee.Kind=="array" && pointee.Count is not >0)
                    throw unit.Missing(node,"pointer difference requires fixed positive pointee layout; void, function, incomplete and runtime-sized pointees are not supported");
            }
            // Typed C# subtraction uses the emitted pointee layout and produces
            // an element count. Preserve EDG's recorded result type explicitly.
            return "unchecked(("+NodeType(node)+")("+Expression(left)+" - "+Expression(right)+"))";
        }
    }
}
