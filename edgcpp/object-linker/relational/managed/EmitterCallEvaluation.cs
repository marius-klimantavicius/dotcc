namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Stack<Node> expressionExpansionPath=[];
        private readonly HashSet<long> orderedCallRenderings=[];

        private string ExpandedRightToLeftCall(Node call,int indent)
        {
            // Keep selected branches and larger ordered expressions bounded.
            // The actual operator call is under reference/designation wrappers.
            if(expressionExpansionPath.Skip(1).Any(parent=>
                !(parent.Kind=="lifetime-scope" && parent.Lifetime is long lifetime && protectedExpressionLifetimes.Contains(lifetime)) &&
                (parent.Kind!="operation" || parent.Operator is not ("ref-indirect" or "indirect" or "reference-to" or "address-of" or
                    "lvalue-adjust" or "class-rvalue-adjust" or "cast" or "ref-cast" or "lvalue-cast"))))
                throw unit.Missing(call,"right-to-left call expansion in this enclosing expression is not implemented");
            if(unit.VirtualCalls.ContainsKey(call.Id))
                throw unit.Missing(call,"right-to-left virtual call expansion is not implemented");
            var operands=unit.Children(call,"operand").ToArray();
            if(operands.Length==0 || operands[0].Kind!="function")
                throw unit.Missing(call,"right-to-left call requires a recorded direct function target");
            var signature=CallSignature(operands[0]);
            if(signature.Variadic || signature.ReturnStorage=="caller-provided")
                throw unit.Missing(call,"right-to-left variadic/class-result call expansion is not implemented");
            var arguments=operands.Skip(1).ToArray();
            if(arguments.Select(argument=>argument.Id).Distinct().Count()!=arguments.Length)
                throw unit.Missing(call,"right-to-left call with aliased operand slots is not implemented");
            var previous=new List<(long Id,bool Present,string? Value)>();
            try
            {
                for(int index=operands.Length-1;index>=1;--index)
                {
                    Node argument=operands[index];
                    string name=allocator.Allocate("__call_argument_"+call.Id+"_"+index,"call_argument");
                    string value,type;
                    bool receiver=index==1 && call.Operator=="dot-member-call";
                    if(receiver)
                    {
                        ExpandAddressLocation(argument,indent);
                        value=AddressExpression(argument);
                        type=NodeType(argument)+"*";
                    }
                    else {value=ExpandedExpression(argument,indent);type=NodeType(argument);}
                    Line(indent,type+" "+name+" = "+value+";");
                    bool present=expandedValues.TryGetValue(argument.Id,out string? old);
                    previous.Add((argument.Id,present,old));
                    expandedValues[argument.Id]=receiver?"(*"+name+")":name;
                }
                orderedCallRenderings.Add(call.Id);
                string rendered=RawExpression(call);
                expandedValues.Add(call.Id,rendered);
                return NormalizeValue(call,rendered);
            }
            finally
            {
                orderedCallRenderings.Remove(call.Id);
                foreach(var item in previous)
                    if(item.Present)expandedValues[item.Id]=item.Value!;
                    else expandedValues.Remove(item.Id);
            }
        }
    }
}
