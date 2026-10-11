namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit,long),string> exceptionDescriptors=[];
    private readonly Dictionary<(Unit,long),string> catchDescriptors=[];
    private readonly Dictionary<SymbolRef,string> payloadDestructors=[];

    private (ProgramType Type,string Identity) ExceptionType(Unit unit,long id,bool reference=false)
    {
        var type=unit.Original(id);
        if(reference && type.Kind is "lvalue-reference" or "rvalue-reference")type=unit.Original(type.Pointee!.Value);
        if(type.Kind=="record")return(type,type.Linkage=="external"?"record:"+type.LinkName:"record:"+unit.Source+":"+type.Id);
        if(type.BuiltinName is string builtin)return(type,"builtin:"+builtin);
        throw new InvalidDataException("Exception descriptor emission not implemented for "+type.Kind+" "+type.Name);
    }

    private void PrepareExceptionDescriptors()
    {
        foreach(var unit in program.Units)
        {
            foreach(var site in unit.Throws.Values.Where(site=>!site.Rethrow))
            {
                long id=site.Type??throw new InvalidDataException("Thrown type missing");
                var (type,identity)=ExceptionType(unit,id);
                if(!exceptionDescriptors.ContainsKey((unit,id)))
                {
                    if(type.Size is null or <=0 || type.Alignment is null or <=0)throw new InvalidDataException("Thrown payload layout missing");
                    string name=names.Allocate("__exception_type_"+exceptionDescriptors.Count,"exception_type");
                    exceptionDescriptors.Add((unit,id),name);
                    Line(1,"private static readonly global::Edg.Runtime.CppExceptionType "+name+" = new("+StringLiteral(identity)+", "+StringLiteral(identity)+", "+StringLiteral(type.Kind)+", global::System.Array.Empty<int>(), "+type.Size+"U, "+type.Alignment+"U, "+(site.Destructor is null?"false":"true")+", global::System.Array.Empty<global::Edg.Runtime.CppExceptionBase>());");
                }
                if(site.Destructor is long destructor)
                {
                    var selected=program.Resolve(unit,destructor);
                    if(!payloadDestructors.ContainsKey(selected))
                    {
                        string name=names.Allocate("__destroy_payload_"+payloadDestructors.Count,"destroy_payload");
                        payloadDestructors.Add(selected,name);
                        long parameter=selected.Unit.Parameters.GetValueOrDefault(selected.Id,[]).Single();
                        Line(1,"private static void "+name+"(void* payload) => "+SymbolName(unit,destructor)+"(("+Type(selected.Unit,selected.Unit.Symbols[parameter].Type)+")payload);");
                    }
                }
            }
            foreach(var handler in unit.Handlers.Values.SelectMany(handlers=>handlers).Where(handler=>handler.Type is not null))
            {
                long id=handler.Type!.Value;
                if(catchDescriptors.ContainsKey((unit,id)))continue;
                var (type,identity)=ExceptionType(unit,id,reference:true);
                string name=names.Allocate("__catch_type_"+catchDescriptors.Count,"catch_type");
                catchDescriptors.Add((unit,id),name);
                Line(1,"private static readonly global::Edg.Runtime.CppCatchType "+name+" = new("+StringLiteral(identity)+", "+StringLiteral(identity)+", "+StringLiteral(type.Kind)+", global::System.Array.Empty<int>());");
            }
        }
    }

    private sealed partial class FunctionWriter
    {
        private void WriteThrow(Node node,int indent)
        {
            var site=unit.Throws[node.Id];
            if(site.Rethrow){Line(indent,"throw "+ExceptionRuntime+".Rethrow();");return;}
            var initialization=unit.Initializations[site.Initialization??throw unit.Missing(node,"throw payload initialization missing")];
            long type=site.Type??throw unit.Missing(node,"throw payload type missing");
            string payload="__payload_"+node.Id;
            string callback=site.Destructor is long destructor?"&"+emitter.payloadDestructors[emitter.program.Resolve(unit,destructor)]:"null";
            Line(indent,"var "+payload+" = "+ExceptionRuntime+".Allocate("+emitter.exceptionDescriptors[(unit,type)]+", "+callback+");");
            Line(indent,"try");Line(indent,"{");
            WriteInitialization(initialization,"(*("+Type(type)+"*)"+payload+".ObjectAddress)",indent+1);
            Line(indent+1,ExceptionRuntime+".EnsureStarted("+payload+");");
            Line(indent+1,ExceptionRuntime+".MarkComplete("+payload+");");
            Line(indent+1,"throw "+payload+";");Line(indent,"}");
            string error="__throw_error_"+node.Id;
            Line(indent,"catch (global::System.Exception "+error+") when ("+ExceptionRuntime+".FilterInitializationFailure("+payload+", "+error+"))");
            Line(indent,"{");Line(indent+1,ExceptionRuntime+".ReleaseIncomplete("+payload+");");Line(indent+1,"throw;");Line(indent,"}");
        }

        private void WriteTry(Node node,int indent)
        {
            if(node.Operator=="function-try")throw unit.Missing(node,"function-try emission is not yet implemented");
            if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Count!=0)
                throw unit.Missing(node,"try-owned cleanup emission is not yet implemented");
            Line(indent,"try");Body(Child(node,"body"),indent);
            foreach(var handler in unit.Handlers.GetValueOrDefault(node.Id,[]))
            {
                string error="__caught_error_"+handler.CatchNode,frame="__handler_"+handler.CatchNode;
                string descriptor=handler.Type is long type?", "+emitter.catchDescriptors[(unit,type)]:"";
                string filter=handler.Type is null?"FilterCatchAll":"FilterTypedCatch";
                string enter=handler.Type is null?"EnterCatch":"EnterTypedCatch";
                Line(indent,"catch (global::System.Exception "+error+") when ("+ExceptionRuntime+"."+filter+"("+error+descriptor+"))");
                Line(indent,"{");
                Line(indent+1,"var "+frame+" = "+ExceptionRuntime+"."+enter+"("+error+descriptor+");");
                Line(indent+1,"try");Line(indent+1,"{");
                if(handler.Parameter is long parameter)
                {
                    if(handler.Initialization is long sourceInitialization && unit.Initializations[sourceInitialization].Destructor is not null)
                        throw unit.Missing(unit.Nodes[handler.CatchNode],"nontrivial catch parameter cleanup is not yet implemented");
                    long parameterType=unit.Symbols[parameter].Type;
                    var original=unit.Original(parameterType);
                    string value=original.Kind is "lvalue-reference" or "rvalue-reference"
                        ?"(("+Type(parameterType)+")"+frame+".ObjectAddress)"
                        :"(*("+Type(parameterType)+"*)"+frame+".ObjectAddress)";
                    Line(indent+2,Type(parameterType)+" "+Name(parameter)+" = default;");
                    if(handler.Initialization is long initialization)
                        WriteInitialization(unit.Initializations[initialization],Name(parameter),indent+2,value);
                    else throw unit.Missing(unit.Nodes[handler.CatchNode],"catch parameter initialization missing");
                }
                Line(indent+2,ExceptionRuntime+".MarkCaught("+frame+");");
                Statement(Child(unit.Nodes[handler.CatchNode],"body"),indent+2);
                Line(indent+1,"}");
                Line(indent+1,"catch (global::System.Exception __handler_error_"+handler.CatchNode+") when ("+ExceptionRuntime+".FilterHandlerFailure("+frame+", __handler_error_"+handler.CatchNode+"))");
                Line(indent+1,"{");Line(indent+2,"throw;");Line(indent+1,"}");
                Line(indent+1,"finally");Line(indent+1,"{");Line(indent+2,ExceptionRuntime+".LeaveCatch("+frame+");");Line(indent+1,"}");
                Line(indent,"}");
            }
        }
    }
}
