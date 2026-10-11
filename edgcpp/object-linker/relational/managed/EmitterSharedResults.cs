namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,string> sharedDestinations=[];

        private string SharedDestination(Initialization initialization)
        {
            long master=initialization.DestinationInitialization
                ??throw new InvalidDataException("Shared destination reference missing for initialization "+initialization.Id);
            if(!sharedDestinations.TryGetValue(master,out string? destination))
                throw new InvalidDataException("Shared destination "+master+" is not bound for initialization "+initialization.Id);
            if(initialization.Destructor is not null || cleanupFlags.Contains(initialization.Id))
                throw new InvalidDataException("Shared dependent initialization "+initialization.Id+" has unsupported independent cleanup ownership");
            return destination;
        }

        private void WriteSharedResult(Initialization initialization,string destination,int indent)
        {
            Node value=unit.Nodes[initialization.Value
                ??throw new InvalidDataException("Shared result effects missing for initialization "+initialization.Id)];
            bool hadPrevious=sharedDestinations.TryGetValue(initialization.Id,out string? previous);
            sharedDestinations[initialization.Id]=destination;
            try {WriteSharedResultEffects(value,indent,requiresDestination:true);}
            finally
            {
                if(hadPrevious)sharedDestinations[initialization.Id]=previous!;
                else sharedDestinations.Remove(initialization.Id);
            }
        }

        private void WriteSharedResultEffects(Node node,int indent,bool requiresDestination)
        {
            // This edge, rather than the expression's class result type,
            // selects the supplied destination. Nested masters bind their
            // own dependents while using the same outer destination.
            if((node.Kind is "temporary" or "lambda") && node.Initialization is long id &&
               unit.Initializations[id].DestinationInitialization is not null)
            {
                var initialization=unit.Initializations[id];
                WriteInitialization(initialization,SharedDestination(initialization),indent);
                return;
            }
            if(node.Kind=="operation" && node.Operator=="conditional")
            {
                var operands=unit.Children(node,"operand").ToArray();
                if(operands.Length!=3)throw unit.Missing(node,"exporter defect: shared conditional operands missing");
                Line(indent,"if ("+Truth(operands[0],ExpandedExpression(operands[0],indent))+")");Line(indent,"{");
                WriteSharedResultEffects(operands[1],indent+1,requiresDestination);Line(indent,"}");
                Line(indent,"else");Line(indent,"{");
                WriteSharedResultEffects(operands[2],indent+1,requiresDestination);Line(indent,"}");
                return;
            }
            if(node.Kind=="operation" && node.Operator=="comma")
            {
                var operands=unit.Children(node,"operand").ToArray();
                if(operands.Length==0)throw unit.Missing(node,"exporter defect: shared comma operands missing");
                for(int index=0;index<operands.Length;++index)
                    WriteSharedResultEffects(operands[index],indent,requiresDestination && index==operands.Length-1);
                return;
            }
            if(node.Kind=="operation" && node.Operator is "lvalue-adjust" or "class-rvalue-adjust")
            {
                WriteSharedResultEffects(Child(node,"operand"),indent,requiresDestination);
                return;
            }
            if(node.Kind is "throw" or "rethrow"){WriteThrow(node,indent);return;}
            if(requiresDestination)
                throw unit.Missing(node,"shared result effects require a recorded dependent destination action");
            Line(indent,StatementExpression(node,ExpandedExpression(node,indent))+";");
        }
    }
}
