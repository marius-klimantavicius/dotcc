namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,string> labelNames=[];
        private readonly Dictionary<long,(long Block,int Ordinal)> labelPositions=[];
        private readonly Dictionary<long,int> activeStatementBlocks=[];

        private void PrepareNamedLabels(long functionId)
        {
            foreach(var label in unit.Nodes.Values.Where(node=>node.Function==functionId && node.Kind=="label"))
                labelNames.Add(label.Id,allocator.Allocate(label.Constant??throw unit.Missing(label,"exporter defect: selected label spelling missing"),"label_"+label.Id));
            foreach(var block in unit.Nodes.Values.Where(node=>node.Function==functionId && node.Kind=="block"))
            {
                var statements=unit.Children(block,"statement").ToArray();
                for(int index=0;index<statements.Length;++index)
                    if(statements[index].Kind=="label")labelPositions.Add(statements[index].Id,(block.Id,index));
            }
        }

        private void WriteBlockStatements(Node block,int indent)
        {
            var statements=unit.Children(block,"statement").ToArray();
            try
            {
                for(int index=0;index<statements.Length;++index)
                {
                    activeStatementBlocks[block.Id]=index;
                    Statement(statements[index],indent);
                }
            }
            finally {activeStatementBlocks.Remove(block.Id);}
        }

        private void WriteNamedGoto(Node node,int indent)
        {
            Node label=Child(node,"target");
            if(label.Kind!="label" || label.Function!=function!.Value.Id || !labelNames.TryGetValue(label.Id,out string? name))
                throw unit.Missing(node,"named goto emission requires its selected function-local label");
            if(node.CommonLifetime!=label.ContainingLifetime)
                throw unit.Missing(node,"named goto emission requires a target in its recorded common lifetime");
            // This bound follows lexical source placement, without analyzing
            // control flow or inventing cleanup actions. Existing finally
            // regions handle exits on the C# goto to this enclosing scope.
            if(!labelPositions.TryGetValue(label.Id,out var target) ||
               !activeStatementBlocks.TryGetValue(target.Block,out int position) || target.Ordinal<=position)
                throw unit.Missing(node,"named goto emission supports forward labels in an enclosing source block");
            Line(indent,"goto "+name+";");
        }

        private void WriteNamedLabel(Node node,int indent)
        {
            if(!labelPositions.ContainsKey(node.Id))
                throw unit.Missing(node,"named label emission requires lexical source block placement");
            Line(indent,labelNames[node.Id]+": ;");
        }
    }
}
