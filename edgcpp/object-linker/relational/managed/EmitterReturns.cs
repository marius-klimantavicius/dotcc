namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private ProgramType CallSignature(Node target)
        {
            var type=unit.Original(target.Type??target.ExecutionType??throw unit.Missing(target,"callable type missing"));
            if(type.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
                type=unit.Original(type.Pointee??throw unit.Missing(target,"callable pointee type missing"));
            if(type.Kind!="function")throw unit.Missing(target,"callable function signature missing");
            return type;
        }

        private void CheckReturnCleanup()
        {
            if(returnDestination is null)return;
            foreach(long id in unit.Destructions.Values.SelectMany(actions=>actions))
            {
                var initialization=unit.Initializations[id];
                if(initialization.Function!=function!.Value.Id || initialization.Destructor is not long destructor)continue;
                var selected=emitter.program.Resolve(unit,destructor);
                if(!selected.Unit.FunctionDetails.TryGetValue(selected.Id,out var details))
                    throw new InvalidDataException("No definition for return cleanup destructor "+selected.Symbol.LinkName);
                if(details.ExceptionPolicy!="terminate")
                    throw new InvalidDataException("Caller-provided return with potentially throwing cleanup requires explicit return-result unwinding facts: "+selected.Symbol.Name);
            }
        }

        private void WriteReturn(Node node,int indent)
        {
            if(constructorCompletionRequired)Line(indent,"__constructor_complete = true;");
            if(node.Initialization is long id)
            {
                if(cleanupFlags.Contains(id))
                    throw unit.Missing(node,"return-result lifetime transfer emission is not yet implemented");
                var initialization=unit.Initializations[id];
                if(returnDestination is not null)
                {
                    WriteInitialization(initialization,"(*"+returnDestination+")",indent);
                    Line(indent,"return;");
                }
                else
                {
                    long result=unit.Original(function!.Value.Symbol.Type).Result
                        ??throw unit.Missing(node,"function result type missing");
                    string value="__return_value_"+node.Id;
                    Line(indent,Type(result)+" "+value+" = default;");
                    WriteInitialization(initialization,value,indent);
                    Line(indent,"return "+value+";");
                }
                return;
            }
            if(returnDestination is not null)
                throw unit.Missing(node,"exporter defect: caller-provided return initialization missing");
            Line(indent,unit.Children(node,"value").FirstOrDefault() is Node expression
                ?"return "+ExpandedExpression(expression,indent)+";":"return;");
        }

        private void WriteClassResultCall(Node call,string destination,int indent)
        {
            if(unit.VirtualCalls.TryGetValue(call.Id,out var dispatch))
            {
                if(call.Lifetime is long virtualLifetime && unit.Destructions.GetValueOrDefault(virtualLifetime,[]).Count!=0)
                    throw unit.Missing(call,"virtual class result expression cleanup emission is not yet implemented");
                if(call.EvaluationOrder=="right-to-left")
                    throw unit.Missing(call,"right-to-left virtual class result expansion is not yet implemented");
                Line(indent,VirtualCallExpression(call,dispatch,destination)+";");return;
            }
            if(call.Kind=="operation" && call.Operator=="class-rvalue-adjust")
            {
                if(call.Lifetime is long adjustedLifetime && unit.Destructions.GetValueOrDefault(adjustedLifetime,[]).Count!=0)
                    throw unit.Missing(call,"class result adjustment cleanup emission is not yet implemented");
                WriteClassResultCall(Child(call,"operand"),destination,indent);return;
            }
            if(call.Kind!="operation" || call.Operator is not ("call" or "dot-member-call" or "points-to-member-call"))
                throw unit.Missing(call,"class result destination requires supported direct call emission");
            if(call.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Count!=0)
                throw unit.Missing(call,"class result call expression cleanup emission is not yet implemented");
            var operands=unit.Children(call,"operand").ToArray();
            if(operands.Length==0)throw unit.Missing(call,"class result call target missing");
            if(CallSignature(operands[0]).ReturnStorage!="caller-provided")
                throw unit.Missing(call,"exporter defect: class-result call return storage missing");
            if(call.EvaluationOrder=="right-to-left" && operands.Length>1)
                throw unit.Missing(call,"right-to-left class result argument expansion is not yet implemented");
            string target=ExpandedExpression(operands[0],indent);
            if(operands[0].Kind!="function")
            {
                string saved="__return_call_target_"+call.Id;
                Line(indent,NodeType(operands[0])+" "+saved+" = "+target+";");
                target=saved;
            }
            var arguments=operands.Skip(1).Select(operand=>ExpandedExpression(operand,indent)).ToArray();
            if(call.Operator=="dot-member-call")
            {
                if(arguments.Length==0)throw unit.Missing(call,"class result member-call receiver missing");
                arguments[0]="(&"+arguments[0]+")";
            }
            Line(indent,target+"(&"+destination+(arguments.Length==0?"":", "+string.Join(", ",arguments))+");");
        }
    }
}
