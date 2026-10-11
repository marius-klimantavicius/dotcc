namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit Unit,long Node),string> deallocationNames=[];

    private void WriteDeallocationHelpers(Unit unit,long? function)
    {
        foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function && node.Kind=="delete" && node.Operator=="destroy"))
        {
            string name=names.Allocate("__delete_object_"+node.Id,"delete_object");
            deallocationNames.Add((unit,node.Id),name);
            new FunctionWriter(this,unit,null).WriteDeallocationHelper(node,name);
        }
    }

    private void ReleaseDeallocationNames(Unit unit,long function)
    {
        foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function && node.Kind=="delete" && node.Operator=="destroy"))
            deallocationNames.Remove((unit,node.Id));
    }

    private sealed partial class FunctionWriter
    {
        public void WriteDeallocationHelper(Node node,string name)
        {
            Node pointer=Child(node,"operand"),destroy=Child(node,"destroy"),policy=Child(node,"null-check");
            var initialization=unit.Initializations[destroy.Initialization
                ??throw unit.Missing(destroy,"exporter defect: selected destruction action missing")];
            CheckInitialization(initialization);
            if(initialization.Kind!="none" || initialization.Destructor is not long destructor)
                throw unit.Missing(destroy,"object deletion requires its selected destructor-only action");
            bool nullCheck=policy.Constant switch
            {
                "0" or "false"=>false,"1" or "true"=>true,
                _=>throw unit.Missing(policy,"exporter defect: selected deletion null policy missing")
            };
            string parameter=allocator.Allocate("__deleted_pointer_"+node.Id,"deleted_pointer");
            Line(1,"private static void "+name+"("+NodeType(pointer)+" "+parameter+")");Line(1,"{");
            expandedValues.Add(pointer.Id,parameter);
            try
            {
                if(nullCheck){Line(2,"if ("+parameter+" != null)");Line(2,"{");}
                int indent=nullCheck?3:2;
                var selected=emitter.program.Resolve(unit,destructor);
                if(!selected.Unit.FunctionDetails.TryGetValue(selected.Id,out var details) || details.Receiver is not long receiver ||
                   selected.Unit.Parameters.GetValueOrDefault(selected.Id,[]).Count!=1)
                    throw unit.Missing(destroy,"selected source destructor requires its recorded receiver definition");
                string receiverType=emitter.Type(selected.Unit,selected.Unit.Symbols[receiver].Type);
                // The existing source destructor body includes its recorded
                // complete-object member/base epilogue. The complete ABI entry
                // is retained separately as navigation, without a fake body.
                Line(indent,Name(destructor)+"(("+receiverType+")"+parameter+");");
                Node execution=Child(node,"execution");
                Line(indent,StatementExpression(execution)+";");
                if(nullCheck)Line(2,"}");
            }
            finally {expandedValues.Remove(pointer.Id);}
            Line(1,"}");
        }

        private string DeallocationExpression(Node node)=>emitter.deallocationNames[(unit,node.Id)]+"("+Expression(Child(node,"operand"))+")";
    }
}
