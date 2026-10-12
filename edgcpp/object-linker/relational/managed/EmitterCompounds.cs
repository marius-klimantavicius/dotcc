namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit Unit,long Node),string> compoundNames=[];
    private readonly Dictionary<(Unit Unit,string Operator,long Stored,long Right,long Operation),string> compoundHelpers=[];

    private static bool IsCompound(string? operation)=>operation is "add-assign" or "subtract-assign" or "multiply-assign" or
        "divide-assign" or "remainder-assign" or "bit-and-assign" or "bit-or-assign" or "bit-xor-assign" or
        "shift-left-assign" or "shift-right-assign" or "pointer-add-assign" or "pointer-subtract-assign";

    private static bool IsPointerCompound(string? operation)=>operation is "pointer-add-assign" or "pointer-subtract-assign";

    private string CompoundConversion(Unit unit,long from,long to,string value)
    {
        string source=Type(unit,from),target=Type(unit,to);
        if(source==target)return value;
        if(target=="bool")return "("+value+" != 0)";
        if(source=="bool")value="("+value+" ? 1 : 0)";
        return "unchecked(("+target+")"+value+")";
    }

    private static bool IsSourceCompoundCandidate(Unit unit,Node node,long? function)
    {
        if(function is null || node.Kind!="operation" || node.Operator is not ("bit-and-assign" or "bit-or-assign" or "bit-xor-assign" or "multiply-assign") ||
           node.OperationType is not long calculation)return false;
        var operands=unit.Children(node,"operand").ToArray();
        if(operands.Length!=2 || operands[0].Kind!="variable" || operands[0].Symbol is not long symbol ||
           unit.Symbols[symbol].Parent!=function || unit.Symbols[symbol].Kind is not ("local" or "parameter"))return false;
        string stored=unit.Original(unit.Symbols[symbol].Type).Kind;
        if(stored is not ("i32" or "u32" or "i64" or "u64" or "i128" or "u128") ||
           operands[1].Type is not long right || unit.Original(right).Kind!=stored ||
           unit.Original(calculation).Kind!=stored)return false;
        if(operands[0].ExecutionType is long leftExecution && unit.Original(leftExecution).Kind!=stored ||
           operands[1].ExecutionType is long rightExecution && unit.Original(rightExecution).Kind!=stored)return false;
        if(unit.IsVolatile(unit.Symbols[symbol].Type) ||
           node.OriginalLvalueType is long original && unit.IsVolatile(original) ||
           node.Type is long type && unit.IsVolatile(type))return false;
        if(node.EvaluationOrder!="right-to-left" && !IsPureCompoundRight(unit,operands[1],function))return false;
        return node.Lifetime is not long lifetime || unit.Destructions.GetValueOrDefault(lifetime,[]).Count==0;
    }

    private static bool IsPureCompoundRight(Unit unit,Node node,long? function)
    {
        if(node.Type is long type && unit.IsVolatile(type))return false;
        if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Count!=0)return false;
        if(node.Kind=="constant")return true;
        if(node.Kind=="variable")
            return node.Symbol is long symbol && unit.Symbols[symbol].Parent==function &&
                unit.Symbols[symbol].Kind is "local" or "parameter" && !unit.IsVolatile(unit.Symbols[symbol].Type);
        if(node.Kind!="operation" || node.Operator is not ("cast" or "bool-cast" or "lvalue-adjust" or
            "plus" or "negate" or "complement" or "not" or "add" or "subtract" or "multiply" or "divide" or
            "remainder" or "bit-and" or "bit-or" or "bit-xor" or "shift-left" or "shift-right"))return false;
        return unit.Children(node,"operand").All(operand=>IsPureCompoundRight(unit,operand,function));
    }

    private void WriteCompoundHelpers(Unit unit,long? function)
    {
        var discarded=new HashSet<long>();var otherUses=new HashSet<long>();
        foreach(var parent in unit.Edges)
            foreach(var edge in parent.Value)
            {
                if(unit.Nodes[parent.Key].Kind=="expr-stmt" && edge.Role=="value")discarded.Add(edge.Child);
                else otherUses.Add(edge.Child);
            }
        foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function && IsCompound(node.Operator)))
        {
            // Only this exact discarded site is emitted directly. Helpers for
            // other expression sites of the same kind/types remain available.
            if(discarded.Contains(node.Id) && !otherUses.Contains(node.Id) && IsSourceCompoundCandidate(unit,node,function))continue;
            if(!unit.HasOperationType)continue;
            long operation=node.OperationType??throw unit.Missing(node,"exporter defect: compound operation type missing");
            var operands=unit.Children(node,"operand").ToArray();
            if(operands.Length!=2)throw unit.Missing(node,"compound assignment operand relation missing");
            if(SelectedBitfield(unit,operands[0]) is not null)
                throw unit.Missing(node,"Bitfield compound/increment result emission is not yet implemented");
            long stored=operands[0].ExecutionType??operands[0].Type??throw unit.Missing(node,"compound destination type missing");
            long right=operands[1].ExecutionType??operands[1].Type??throw unit.Missing(node,"compound source type missing");
            bool pointer=IsPointerCompound(node.Operator);
            if(pointer)
            {
                RequireFixedPointerLayout(unit,node,stored,"pointer compound assignment");
                RequireFixedPointerLayout(unit,node,operation,"pointer compound assignment");
                if(unit.Original(right).Kind is not ("i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64"))
                    throw unit.Missing(node,"pointer compound assignment offset emission requires a recorded native integer type");
            }
            else if(unit.Original(stored).Kind is "pointer" or "lvalue-reference" or "rvalue-reference" or "record" ||
                unit.Original(right).Kind is "pointer" or "lvalue-reference" or "rvalue-reference" or "record")
                throw unit.Missing(node,"pointer/record compound assignment emission is not yet implemented");
            var key=(unit,node.Operator!,stored,right,operation);
            if(!compoundHelpers.TryGetValue(key,out string? name))
            {
                name=names.Allocate("__"+node.Operator!.Replace('-','_')+"_"+Type(unit,stored),"compound_assignment");
                compoundHelpers.Add(key,name);
                string symbol=node.Operator switch
                {
                    "add-assign" or "pointer-add-assign"=>"+","subtract-assign" or "pointer-subtract-assign"=>"-",
                    "multiply-assign"=>"*","divide-assign"=>"/","remainder-assign"=>"%",
                    "bit-and-assign"=>"&","bit-or-assign"=>"|","bit-xor-assign"=>"^","shift-left-assign"=>"<<","shift-right-assign"=>">>",
                    _=>throw unit.Missing(node,"compound assignment operator missing")
                };
                string leftValue=CompoundConversion(unit,stored,operation,"(*target)");
                // Pointer offsets remain integers. Typed pointer arithmetic
                // scales them by the emitted pointee layout, without converting
                // the offset to the pointer operation type.
                string rightValue=pointer?"right":node.Operator is "shift-left-assign" or "shift-right-assign"
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

        private bool WriteSourceCompoundStatement(Node node,int indent)
        {
            if(!IsSourceCompoundCandidate(unit,node,function?.Id))return false;
            var operands=unit.Children(node,"operand").ToArray();
            // A direct local designation has no effects. For an effectful
            // RHS, save the selected value before C# reads the old local:
            // that RHS can change the same local through an escaped address.
            string right;
            if(IsPureCompoundRight(unit,operands[1],function?.Id))right=Expression(operands[1]);
            else
            {
                right=allocator.Allocate("__compound_right_"+node.Id,"compound_right");
                string value=ExpandedExpression(operands[1],indent);
                Line(indent,NodeType(operands[1])+" "+right+" = "+value+";");
            }
            string operation=node.Operator switch {"bit-and-assign"=>"&=","bit-or-assign"=>"|=","multiply-assign"=>"*=",_=>"^="};
            string statement=Expression(operands[0])+" "+operation+" "+right+";";
            Line(indent,node.Operator=="multiply-assign"?"unchecked { "+statement+" }":statement);
            return true;
        }

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
