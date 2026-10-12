namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly HashSet<long> allocationOwnedCleanups=[];
        private int placementWrapperDepth;

        private bool IsPlacementWrapper(Node node)
        {
            if(node.Kind!="lifetime-scope" && (node.Kind!="operation" || node.Operator is not ("cast" or "ref-cast" or "lvalue-cast" or "lvalue-adjust" or "class-rvalue-adjust")))return false;
            var operands=unit.Children(node,"operand").ToArray();
            return operands.Length==1 && (operands[0].Kind=="new" && operands[0].Operator=="placement" || IsPlacementWrapper(operands[0]));
        }

        private string ExpandedPlacementWrapper(Node node,int indent)
        {
            if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Any(id=>!allocationOwnedCleanups.Contains(id)))
                throw unit.Missing(node,"placement construction wrapper has unsupported additional expression cleanup");
            ++placementWrapperDepth;
            string value;
            try {value=ExpandedExpression(Child(node,"operand"),indent);}
            finally {--placementWrapperDepth;}
            // A source void cast discards the returned pointer. C# expresses
            // that discard at the statement, after construction has run.
            return NodeType(node)=="void"?value:Expression(node);
        }

        private void PreparePlacementCleanups(long functionId)
        {
            foreach(var node in unit.Nodes.Values.Where(node=>node.Function==functionId && node.Kind=="new" && node.Operator=="placement"))
            {
                Node cleanup=Child(node,"allocation-cleanup");
                allocationOwnedCleanups.Add(cleanup.Initialization
                    ??throw unit.Missing(cleanup,"exporter defect: allocation cleanup action missing"));
            }
        }

        private string ExpandedPlacementAllocation(Node node,int indent)
        {
            if(expressionExpansionDepth-placementWrapperDepth!=1)
                throw unit.Missing(node,"nested placement construction requires selected sequencing emission");
            if(!unit.Allocations.TryGetValue(node.Id,out var allocation))
                throw unit.Missing(node,"exporter defect: selected allocation facts missing");
            bool nullCheck=allocation.ConstructionNullCheck
                ??throw unit.Missing(node,"exporter defect: selected construction null guard missing");
            var allocated=unit.Original(allocation.Type);
            if(allocated.Kind is "array" or "function" or "void" || allocated.Incomplete || allocated.Size is not >0 ||
               allocation.CountValue is not null || allocation.CountSymbol is not null ||
               allocation.Check is not null || allocation.Failure is not null || allocation.ResultOffset!=0)
                throw unit.Missing(node,"placement construction supports a complete object without array/cookie handling");
            if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Any(id=>!allocationOwnedCleanups.Contains(id)))
                throw unit.Missing(node,"placement construction has unsupported additional expression cleanup");
            var arguments=unit.Children(node,"placement-argument").ToArray();
            var aliases=unit.Children(node,"exception-argument").ToArray();
            if(arguments.Length!=1 || aliases.Length!=arguments.Length ||
               arguments.Where((argument,index)=>argument.Id!=aliases[index].Id).Any())
                throw unit.Missing(node,"placement construction requires one recorded reusable scalar argument");
            var argumentType=unit.Original(arguments[0].ExecutionType??arguments[0].Type
                ??throw unit.Missing(arguments[0],"placement argument type missing"));
            if(argumentType.Kind is "record" or "array" or "function" or "void")
                throw unit.Missing(arguments[0],"placement argument storage emission is not implemented for "+argumentType.Kind);
            Node initialize=Child(node,"exception-protected"),cleanupNode=Child(node,"allocation-cleanup");
            if(initialize.Id!=Child(node,"initialize").Id)
                throw unit.Missing(node,"placement protected action emission requires its recorded initialization alias");
            var initialization=unit.Initializations[initialize.Initialization
                ??throw unit.Missing(initialize,"exporter defect: placement initialization action missing")];
            if(initialization.Kind is not ("constructor" or "expression" or "constant" or "zero") || initialization.Destructor is not null || initialization.DestinationInitialization is not null)
                throw unit.Missing(initialize,"placement initialization requires its selected complete-object action without end-lifetime cleanup");
            var cleanup=unit.Initializations[cleanupNode.Initialization
                ??throw unit.Missing(cleanupNode,"exporter defect: allocation cleanup action missing")];
            if(cleanup.Kind!="none" || !cleanup.FreeOnException || cleanup.Destructor is not long deallocator)
                throw unit.Missing(cleanupNode,"placement allocation cleanup requires its selected exception-only deallocator");
            CheckInitialization(initialization);
            string argument=allocator.Allocate("__placement_argument_"+node.Id,"placement_argument");
            Line(indent,NodeType(arguments[0])+" "+argument+" = "+ExpandedExpression(arguments[0],indent)+";");
            string result=allocator.Allocate("__placement_result_"+node.Id,"placement_result");
            string call=Name(allocation.Allocator)+"("+Expression(unit.Nodes[allocation.Size])+", "+argument+")";
            Line(indent,NodeType(node)+" "+result+" = ("+NodeType(node)+")"+call+";");
            if(nullCheck){Line(indent,"if ("+result+" != null)");Line(indent,"{");}
            int level=indent+(nullCheck?1:0);
            Line(level,"try");Line(level,"{");
            WriteInitialization(initialization,"(*"+result+")",level+1);Line(level,"}");
            string error=allocator.Allocate("__placement_error_"+node.Id,"placement_error");
            Line(level,"catch (global::System.Exception "+error+") when (!global::Edg.Runtime.ObjectProgram.IsRuntimeTransfer("+error+"))");
            Line(level,"{");
            var signature=unit.Original(unit.Symbols[deallocator].Type);
            var parameters=unit.TypeParameters.GetValueOrDefault(signature.Id,[]);
            if(parameters.Count!=2)throw unit.Missing(cleanupNode,"placement deallocator argument emission requires its recorded two-parameter signature");
            Line(level+1,Name(deallocator)+"(("+Type(parameters[0])+")"+result+", "+argument+");");
            Line(level+1,"throw;");Line(level,"}");
            if(nullCheck)Line(indent,"}");
            expandedValues.Add(node.Id,result);
            return NormalizeValue(node,result);
        }
    }
}
