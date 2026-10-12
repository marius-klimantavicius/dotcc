namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private Node? VoidDiscardOperand(Node node)
        {
            if(node.Kind!="operation" || node.Operator!="cast" || node.Type is not long type || unit.Original(type).Kind!="void")return null;
            if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Count!=0)
                throw unit.Missing(node,"discarded void conversion requires its selected cleanup region");
            return Child(node,"operand");
        }

        private bool WriteVoidDiscardStatement(Node node,int indent)
        {
            if(VoidDiscardOperand(node) is not Node operand)return false;
            if(WriteVoidDiscardStatement(operand,indent))return true;
            // A folded constant has no evaluation effects. Retain the source
            // spelling without inventing a C# value of type void.
            if(operand.Kind=="constant" && operand.Constant is string constant)
                Line(indent,"// (void)"+constant+";");
            else
                Line(indent,StatementExpression(operand,ExpandedExpression(operand,indent))+";");
            return true;
        }
    }
}
