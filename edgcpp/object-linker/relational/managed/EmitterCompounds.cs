namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit Unit,long Node),string> compoundNames=[];
    private readonly Dictionary<(Unit Unit,string Operator,long Stored,long Right,long Operation),string> compoundHelpers=[];

    private static bool IsCompound(string? operation)=>operation is "add-assign" or "subtract-assign" or "multiply-assign" or
        "divide-assign" or "remainder-assign" or "bit-and-assign" or "bit-or-assign" or "bit-xor-assign" or
        "shift-left-assign" or "shift-right-assign";

    private string CompoundConversion(Unit unit,long from,long to,string value)
    {
        string source=Type(unit,from),target=Type(unit,to);
        if(source==target)return value;
        if(target=="bool")return "("+value+" != 0)";
        if(source=="bool")value="("+value+" ? 1 : 0)";
        return "unchecked(("+target+")"+value+")";
    }

    private void WriteCompoundHelpers(Unit unit,long? function)
    {
        foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function && IsCompound(node.Operator)))
        {
            if(!unit.HasOperationType)continue;
            long operation=node.OperationType??throw unit.Missing(node,"exporter defect: compound operation type missing");
            var operands=unit.Children(node,"operand").ToArray();
            if(operands.Length!=2)throw unit.Missing(node,"compound assignment operand relation missing");
            long stored=operands[0].ExecutionType??operands[0].Type??throw unit.Missing(node,"compound destination type missing");
            long right=operands[1].ExecutionType??operands[1].Type??throw unit.Missing(node,"compound source type missing");
            if(unit.Original(stored).Kind is "pointer" or "lvalue-reference" or "rvalue-reference" or "record" ||
                unit.Original(right).Kind is "pointer" or "lvalue-reference" or "rvalue-reference" or "record")
                throw unit.Missing(node,"pointer/record compound assignment emission is not yet implemented");
            var key=(unit,node.Operator!,stored,right,operation);
            if(!compoundHelpers.TryGetValue(key,out string? name))
            {
                name=names.Allocate("__"+node.Operator!.Replace('-','_')+"_"+Type(unit,stored),"compound_assignment");
                compoundHelpers.Add(key,name);
                string symbol=node.Operator switch
                {
                    "add-assign"=>"+","subtract-assign"=>"-","multiply-assign"=>"*","divide-assign"=>"/","remainder-assign"=>"%",
                    "bit-and-assign"=>"&","bit-or-assign"=>"|","bit-xor-assign"=>"^","shift-left-assign"=>"<<","shift-right-assign"=>">>",
                    _=>throw unit.Missing(node,"compound assignment operator missing")
                };
                string leftValue=CompoundConversion(unit,stored,operation,"(*target)");
                string rightValue=node.Operator is "shift-left-assign" or "shift-right-assign"
                    ?Type(unit,right)=="bool"?"(right ? 1 : 0)":"unchecked((int)right)":CompoundConversion(unit,right,operation,"right");
                string computation="unchecked("+leftValue+" "+symbol+" "+rightValue+")";
                string result=CompoundConversion(unit,operation,stored,computation);
                string storedType=Type(unit,stored),rightType=Type(unit,right);
                Line(1,"private readonly struct "+name);Line(1,"{");
                Line(2,"private readonly "+rightType+" right;");
                Line(2,"public "+name+"("+rightType+" right) { this.right = right; }");
                Line(2,"public "+storedType+"* Apply("+storedType+"* target) => Apply(target, right);");
                Line(2,"public static "+storedType+"* Apply("+storedType+"* target, "+rightType+" right)");Line(2,"{");
                Line(3,"*target = "+result+";");Line(3,"return target;");Line(2,"}");Line(1,"}");
            }
            compoundNames.Add((unit,node.Id),name);
        }
    }

    private void ReleaseCompoundNames(Unit unit,long function)
    {
        foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function && IsCompound(node.Operator)))compoundNames.Remove((unit,node.Id));
    }

    private sealed partial class FunctionWriter
    {
        private bool UsesCompoundHelper(Node node)=>unit.HasOperationType && IsCompound(node.Operator);

        private bool HasCompoundAddress(Node node)
        {
            if(node.Kind=="variable")return node.Symbol is long symbol && (unit.Symbols[symbol].Parent is not null || emitter.HasPinnedScalarStorage(unit,symbol));
            if(node.Kind!="operation")return false;
            if(UsesCompoundHelper(node))return true;
            return node.Operator switch
            {
                "indirect" or "ref-indirect" or "points-to-field" or "subscript"=>true,
                "dot-field" or "lvalue-adjust"=>HasCompoundAddress(unit.Children(node,"operand").First()),
                _=>false
            };
        }

        private string CompoundExpression(Node node,Node left,Node right)
        {
            if(left.Kind=="variable" && left.Symbol is long symbol && unit.Symbols[symbol].Parent is null && !emitter.HasPinnedScalarStorage(unit,symbol))
                throw unit.Missing(node,"compound assignment to address-taken global requires pinned storage support");
            if(!HasCompoundAddress(left))throw unit.Missing(node,"compound assignment destination requires unsupported lvalue address emission");
            if(!emitter.compoundNames.TryGetValue((unit,node.Id),out string? name))
                throw unit.Missing(node,"exporter defect: compound operation type/helper missing");
            string address="(&"+Expression(left)+")",value=Expression(right);
            return node.EvaluationOrder=="right-to-left"
                ?"(*new "+name+"("+value+").Apply("+address+"))"
                :"(*"+name+".Apply("+address+", "+value+"))";
        }
    }
}
