namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private const string VaListType="global::Edg.Runtime.VaList";
    private const string VaArgType="global::Edg.Runtime.VaArg";
    private const string VaSpanType="global::System.ReadOnlySpan<"+VaArgType+">";
    private bool needsVarargsRuntime;

    private string VaListStorageType()
    {
        needsVarargsRuntime=true;return VaListType;
    }

    private void WriteVarargsRuntime(string directory)
    {
        if(!needsVarargsRuntime)return;
        CopyRuntimeSource(directory,"VaArg.cs");CopyRuntimeSource(directory,"VaPack.cs");CopyRuntimeSource(directory,"VaList.cs");
    }

    private sealed partial class FunctionWriter
    {
        private sealed record VarargsLocals(string Span,string Capacity,string Workspace,string Pack,string Cursor);
        private readonly VarargsLocals? varargs;

        private void WriteVarargsPrologue(int indent)
        {
            if(varargs is null)return;
            emitter.needsVarargsRuntime=true;
            Line(indent,"int "+varargs.Capacity+" = global::Edg.Runtime.VaPack.GetStackBytes("+varargs.Span+");");
            Line(indent,"byte* "+varargs.Workspace+" = stackalloc byte["+varargs.Capacity+"];");
            Line(indent,"using var "+varargs.Pack+" = new global::Edg.Runtime.VaPack("+varargs.Span+", "+varargs.Workspace+", "+varargs.Capacity+");");
            Line(indent,VaListType+" "+varargs.Cursor+" = "+varargs.Pack+".Value;");
        }

        private string VaCursor(Node operand)
        {
            long id=operand.ExecutionType??operand.Type??throw unit.Missing(operand,"va_list operand type missing");
            var type=unit.Original(id);
            if(unit.VaListRole(id) is not null)return "(&"+Expression(operand)+")";
            if(type.Kind is "pointer" or "lvalue-reference" or "rvalue-reference" && type.Pointee is long pointee && unit.VaListRole(pointee) is not null)
                return Expression(operand);
            throw unit.Missing(operand,"exporter defect: va_list cursor role missing");
        }

        private string VarargsOperation(Node node)
        {
            emitter.needsVarargsRuntime=true;
            var operands=unit.Children(node,"operand").ToArray();
            Node Operand(int index)=>index<operands.Length?operands[index]:throw unit.Missing(node,"variadic operation operand missing");
            switch(node.Operator)
            {
                case "va-start":case "va-start-single":
                    if(varargs is null)throw unit.Missing(node,"va_start requires a variadic function activation");
                    return VaListType+".Start("+VaCursor(Operand(0))+", &"+varargs.Cursor+")";
                case "va-copy":return VaListType+".Copy("+VaCursor(Operand(0))+", "+VaCursor(Operand(1))+")";
                case "va-end":return VaListType+".EndList("+VaCursor(Operand(0))+")";
                case "va-arg":
                    long id=node.Type??throw unit.Missing(node,"va_arg requested type missing");
                    var type=unit.Original(id);
                    if(type.Kind is "record" or "array" || unit.VaListRole(id) is not null)
                        throw unit.Missing(node,"aggregate va_arg emission is not yet implemented");
                    if(type.Size is not >0 || type.Alignment is not >0)throw unit.Missing(node,"va_arg recorded layout missing");
                    return "(*("+Type(id)+"*)"+VaListType+".Take("+VaCursor(Operand(0))+", "+type.Size+"UL, "+type.Alignment+"UL))";
                default:throw unit.Missing(node,"variadic operation emission is not implemented");
            }
        }

        private string VaArgument(Node argument,Func<Node,string> render)
        {
            long id=argument.ExecutionType??argument.Type??throw unit.Missing(argument,"variadic argument type missing");
            var type=unit.Original(id);
            if(type.Kind is "record" or "array" || unit.VaListRole(id) is not null)
                throw unit.Missing(argument,"aggregate variadic argument emission is not yet implemented");
            if(type.Size is not >0 || type.Alignment is not >0)throw unit.Missing(argument,"variadic argument recorded layout missing");
            string value=render(argument);
            if(type.Kind is "i32" or "u32" && type.Size==4 && type.Alignment==4 ||
                type.Kind is "i64" or "u64" or "f64" && type.Size==8 && type.Alignment==8)return value;
            if(type.Kind is "pointer" or "nullptr")
                return VaArgType+".From<nuint>((nuint)"+value+", "+type.Size+", "+type.Alignment+")";
            return VaArgType+".From<"+Type(id)+">("+value+", "+type.Size+", "+type.Alignment+")";
        }

        private List<string> CallArguments(Node call,IReadOnlyList<Node> operands,Func<Node,string> render)
        {
            var signature=CallSignature(operands[0]);
            int receiver=call.Operator is "dot-member-call" or "points-to-member-call"?1:0;
            int fixedCount=unit.TypeParameters.GetValueOrDefault(signature.Id,[]).Count+receiver;
            var arguments=new List<string>();
            for(int index=1;index<operands.Count;index++)
                arguments.Add(signature.Variadic && index>fixedCount?VaArgument(operands[index],render):render(operands[index]));
            if(call.Operator=="dot-member-call")arguments[0]="(&"+arguments[0]+")";
            if(signature.Variadic)
            {
                emitter.needsVarargsRuntime=true;
                // Function pointers have an explicit span parameter; only
                // source routine declarations can use C# params expansion.
                if(operands[0].Kind!="function")
                {
                    string values="["+string.Join(", ",arguments.Skip(fixedCount))+"]";
                    arguments=arguments.Take(fixedCount).ToList();arguments.Add(values);
                }
            }
            return arguments;
        }
    }
}
