namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit Unit,long Node),string> allocationNames=[];

    private void WriteAllocationHelpers(Unit unit,long? function)
    {
        foreach(var allocation in unit.Allocations.Values.Where(value=>unit.Nodes[value.Node].Function==function &&
            (function is not null || executableGlobalNodes.Contains((unit,value.Node)))))
        {
            if(allocation.CountSymbol is not long count)
            {
                if(unit.Nodes[allocation.Node].Operator=="zero-initialized")
                    throw unit.Missing(unit.Nodes[allocation.Node],"runtime zero-initialized allocation requires its selected count binding");
                if(allocation.CountValue is not null || allocation.Check is not null || allocation.Failure is not null)
                    throw unit.Missing(unit.Nodes[allocation.Node],"exporter defect: allocation check requires its selected count binding");
                continue;
            }
            string name=names.Allocate("__new_array_"+allocation.Node,"allocate_array");
            allocationNames.Add((unit,allocation.Node),name);
            string parameter=names.Allocate("__element_count_"+allocation.Node,"allocation_count");
            new FunctionWriter(this,unit,null).WriteAllocationHelper(allocation,name,count,parameter);
        }
    }

    private void ReleaseAllocationNames(Unit unit,long function)
    {
        foreach(var allocation in unit.Allocations.Values.Where(value=>unit.Nodes[value.Node].Function==function))
            allocationNames.Remove((unit,allocation.Node));
    }

    private sealed partial class FunctionWriter
    {
        public void WriteAllocationHelper(Allocation allocation,string name,long count,string parameter)
        {
            locals.Add(count,parameter);
            var node=unit.Nodes[allocation.Node];
            Line(1,"private static "+NodeType(node)+" "+name+"("+Type(unit.Symbols[count].Type)+" "+parameter+")");Line(1,"{");
            if(allocation.Check is long check)
            {
                long failure=allocation.Failure??throw unit.Missing(node,"exporter defect: allocation length failure target missing");
                Line(2,"if ("+Truth(unit.Nodes[check])+") "+Name(failure)+"();");
            }
            else if(allocation.Failure is not null)throw unit.Missing(node,"exporter defect: allocation length predicate missing");
            if(node.Operator=="zero-initialized")WriteZeroInitializedAllocation(node,allocation,2);
            else Line(2,"return "+AllocationCall(node,allocation)+";");
            Line(1,"}");
        }

        private void WriteZeroInitializedAllocation(Node node,Allocation allocation,int indent)
        {
            Node initialize=Child(node,"initialize");
            if(!IsAllocationByteZero(unit,initialize))
                throw unit.Missing(initialize,"runtime byte-zero allocation requires its selected zero initialization action");
            var initialization=unit.Initializations[initialize.Initialization!.Value];
            CheckInitialization(initialization);
            if(initialization.Destructor is not null || initialization.DestinationInitialization is not null ||
               Child(initialize,"byte-count").Id!=allocation.Size || allocation.ResultOffset!=0)
                throw unit.Missing(initialize,"runtime byte-zero allocation requires its recorded size without object cleanup/cookie handling");
            bool nullCheck=allocation.ConstructionNullCheck
                ??throw unit.Missing(node,"exporter defect: selected construction null guard missing");
            Node size=unit.Nodes[allocation.Size];
            string bytes=allocator.Allocate("__allocation_bytes_"+node.Id,"allocation_bytes");
            Line(indent,NodeType(size)+" "+bytes+" = "+Expression(size)+";");
            string result=allocator.Allocate("__allocation_result_"+node.Id,"allocation_result");
            Line(indent,NodeType(node)+" "+result+" = "+AllocationCall(node,allocation,bytes)+";");
            string clear="global::System.Runtime.InteropServices.NativeMemory.Clear("+result+", unchecked((nuint)"+bytes+"));";
            if(nullCheck)Line(indent,"if ("+result+" != null) "+clear);
            else Line(indent,clear);
            Line(indent,"return "+result+";");
        }

        private string AllocationCall(Node node,Allocation allocation,string? size=null)
        {
            string call=Name(allocation.Allocator)+"("+(size??Expression(unit.Nodes[allocation.Size]))+")";
            if(allocation.ResultOffset!=0)call="((byte*)"+call+" + ("+allocation.ResultOffset+"))";
            return "(("+NodeType(node)+")"+call+")";
        }

        private string AllocationExpression(Node node)
        {
            if(node.Operator=="placement")throw unit.Missing(node,"placement construction requires statement expansion");
            if(!unit.Allocations.TryGetValue(node.Id,out var allocation))
                throw unit.Missing(node,"exporter defect: selected allocation facts missing");
            if(allocation.CountSymbol is not null)
            {
                long value=allocation.CountValue??throw unit.Missing(node,"exporter defect: allocation count value missing");
                return emitter.allocationNames[(unit,node.Id)]+"("+Expression(unit.Nodes[value])+")";
            }
            return AllocationCall(node,allocation);
        }
    }
}
