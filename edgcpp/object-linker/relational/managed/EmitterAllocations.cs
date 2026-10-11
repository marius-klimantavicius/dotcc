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
            Line(2,"return "+AllocationCall(node,allocation)+";");Line(1,"}");
        }

        private string AllocationCall(Node node,Allocation allocation)
        {
            string call=Name(allocation.Allocator)+"("+Expression(unit.Nodes[allocation.Size])+")";
            if(allocation.ResultOffset!=0)call="((byte*)"+call+" + ("+allocation.ResultOffset+"))";
            return "(("+NodeType(node)+")"+call+")";
        }

        private string AllocationExpression(Node node)
        {
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
