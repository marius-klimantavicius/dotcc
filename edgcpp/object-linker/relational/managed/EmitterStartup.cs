namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private void WriteStartupCalls(int indent)
    {
        foreach(var unit in program.Units)
            foreach(var group in unit.StartupGroups)
            {
                if(group.ThreadLocal)
                    throw new InvalidDataException("Thread-local startup emission is not yet implemented: "+unit.Source);
                if(group.Priority!=0)
                    throw new InvalidDataException("Nondefault startup priority emission is not yet implemented: "+group.Priority+" in "+unit.Source);
                var function=program.Resolve(unit,group.Function);
                if(function.Unit.Parameters.GetValueOrDefault(function.Id,[]).Count!=0)
                    throw new InvalidDataException("Startup group requires its recorded parameterless entry: "+function.Symbol.LinkName);
                NeedsExceptionRuntime=true;
                Line(indent,"// EDG startup group "+group.Ordinal+", priority "+group.Priority+"; TU order follows linker inputs.");
                Line(indent,"try");Line(indent,"{");
                Line(indent+1,SymbolName(unit,group.Function)+"();");
                Line(indent,"}");
                // The ordinary group body performs its selected temporary
                // cleanup first. Escaping nonlocal initialization terminates
                // at this invocation boundary, without changing its signature.
                Line(indent,"catch (global::System.Exception __startup_error) when (!global::Edg.Runtime.ObjectProgram.IsRuntimeTransfer(__startup_error))");
                Line(indent,"{");
                Line(indent+1,"global::System.Environment.FailFast(\"Exception escaped nonlocal initialization\", __startup_error);");
                Line(indent+1,"throw;");Line(indent,"}");
            }
    }

    private sealed partial class FunctionWriter
    {
        private void WriteGlobalInitialization(Node node,long symbol,int indent)
        {
            var action=unit.Initializations[node.Initialization
                ??throw unit.Missing(node,"global initialization requires its recorded action")];
            if(action.Destructor is not null || action.Lifetime is not null || action.ExpressionLifetime is not null)
                throw unit.Missing(node,"persistent startup destruction or initializer lifetime emission is not yet implemented");
            string address=Name(symbol);
            if(unit.IsArray(unit.Symbols[symbol].Type))
            {
                CheckInitialization(action);
                if(action.Kind is not ("constant" or "expression" or "aggregate") || action.Value is not long value)
                    throw unit.Missing(node,"startup array requires its recorded aggregate initialization action");
                WriteArrayInitializer(unit.Nodes[value],address,indent);return;
            }
            WriteInitialization(action,"(*"+address+")",indent);
        }
    }
}
