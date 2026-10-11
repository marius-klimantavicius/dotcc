namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Stack<string?> breakTargets=[];
        private readonly Dictionary<long,string> caseLabels=[];

        private void WriteBreak(Node node,int indent)
        {
            if(!breakTargets.TryPeek(out string? target))throw unit.Missing(node,"break has no emitted loop or switch target");
            Line(indent,target is null?"break;":"goto "+target+";");
        }

        private void WriteCase(Node node,int indent)
        {
            if(!caseLabels.TryGetValue(node.Id,out string? label))
                throw unit.Missing(node,"exporter defect: case label has no selected enclosing switch");
            Line(indent,label+":;");
        }

        private void WriteSwitch(Node statement,int indent)
        {
            Node condition=Child(statement,"condition");
            Node body=Child(statement,"body");
            Node[] cases=unit.Children(statement,"case").ToArray();
            Node[] statements=body.Kind=="block"?unit.Children(body,"statement").ToArray():[body];
            var directLabels=statements.Where(node=>node.Kind is "case" or "default").Select(node=>node.Id).ToHashSet();
            foreach(var label in cases)
            {
                if(label.Kind is not ("case" or "default"))throw unit.Missing(label,"exporter defect: switch case relation does not select a case/default label");
                if(!directLabels.Contains(label.Id))throw unit.Missing(label,"switch dispatch into a nested source scope or cleanup region is not yet supported");
                if(unit.Children(label,"range-end").Any())throw unit.Missing(label,"GNU switch case range emission is not yet supported");
            }
            string end=allocator.Allocate("__switch_end_"+statement.Id,"switch_end");
            foreach(var label in cases)caseLabels.Add(label.Id,allocator.Allocate("__switch_"+statement.Id+"_"+(label.Kind=="default"?"default":"case_"+label.Id),"switch_case"));
            try
            {
                Line(indent,"{");
                void Selection(int level,Node value)
                {
                    var type=unit.Original(value.ExecutionType??value.Type??throw unit.Missing(value,"switch expression type missing"));
                    if(type.Kind is "i128" or "u128")throw unit.Missing(value,"128-bit switch dispatch is not yet supported by the C# emitter");
                    string selected=ExpandedExpression(value,level);
                    void Contents(int content)
                    {
                        // Dispatch and source labels share the recorded body
                        // lifetime. The following statement sequence retains
                        // C++ fallthrough without C# case reachability analysis.
                        Line(content,"switch ("+selected+")");Line(content,"{");
                        foreach(var label in cases)
                            Line(content+1,(label.Kind=="default"?"default":"case "+Expression(Child(label,"value")))+": goto "+caseLabels[label.Id]+";");
                        if(!cases.Any(label=>label.Kind=="default"))Line(content+1,"default: goto "+end+";");
                        Line(content,"}");
                        breakTargets.Push(end);
                        try {foreach(var child in statements)Statement(child,content);}
                        finally {breakTargets.Pop();}
                    }
                    Line(level,"{");
                    if(body.Kind=="block")WriteLifetimeRegion(body,level+1,Contents);
                    else Contents(level+1);
                    Line(level,"}");
                    Line(level,end+":;");
                }
                if(condition.Kind=="condition")WriteLifetimeRegion(condition,indent+1,level=>
                {
                    if(unit.Children(condition,"initializer").FirstOrDefault() is Node initializer)WriteForInitializer(initializer,level);
                    if(condition.Initialization is not null)WriteDeclaration(condition,level);
                    Selection(level,Child(condition,"value"));
                });
                else Selection(indent+1,condition);
                Line(indent,"}");
            }
            finally {foreach(var label in cases)caseLabels.Remove(label.Id);}
        }
    }
}
