namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private bool NeedsExceptionRuntime {get;set;}

    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,string> expandedValues=[];
        private readonly Dictionary<long,string> cleanupTargets=[];
        private readonly HashSet<long> hoisted=[];
        private readonly HashSet<long> cleanupFlags=[];
        private bool constructorCompletionRequired;
        private string Flag(long initialization)=>"__constructed_"+initialization;
        private string Target(ConstructorInitializer member,long? source=null)
        {
            long receiver=source??unit.FunctionDetails[function!.Value.Id].Receiver
                ??throw new InvalidDataException("Constructor/destructor receiver was not exported");
            if(member.Kind=="field" && member.Field is long field)
                return Name(receiver)+"->"+emitter.fieldNames[(unit,field)];
            if(member.Kind=="direct-base" && !member.Virtual)
            {
                var initialization=unit.Initializations[member.Initialization];
                bool hasRecordActions=initialization.Kind is "constant" or "expression" or "aggregate" &&
                    initialization.Value is long value && unit.RecordInitializers.ContainsKey(value);
                if(unit.FunctionDetails[function!.Value.Id].Kind=="constructor" &&
                    initialization.Kind is not ("none" or "constructor") && !hasRecordActions)
                    throw new InvalidDataException("Base subobject initialization needs explicit copy/zero extent emission: "+member.Initialization);
                long selected=member.Base??throw new InvalidDataException("Exporter defect: constructor/destructor base subobject missing");
                if(!emitter.baseProjections.TryGetValue((unit,selected),out string? projection))
                    throw new InvalidDataException("Constructor/destructor base projection is unavailable: "+selected);
                return "(*"+projection+"("+Name(receiver)+"))";
            }
            throw new InvalidDataException("Virtual base/delegating constructor emission is not yet implemented");
        }

        private void WriteFunctionBody(Node body,int indent)
        {
            long functionId=function!.Value.Id;
            var details=unit.FunctionDetails[functionId];
            if(details.ExceptionPolicy=="dynamic")
                throw unit.Missing(body,"dynamic exception specification runtime boundary emission is not yet implemented");
            PreparePlacementCleanups(functionId);
            PrepareNamedLabels(functionId);
            CheckReturnCleanup();
            foreach(var lifetime in unit.Lifetimes.Values.Where(lifetime=>lifetime.Function==functionId))
                foreach(long initialization in unit.Destructions.GetValueOrDefault(lifetime.Id,[]))
                {
                    var action=unit.Initializations[initialization];
                    if(allocationOwnedCleanups.Contains(initialization))continue;
                    if(action.Destructor is null)continue;
                    if(details.Kind=="destructor" && action.ConstructorInit)continue;
                    cleanupFlags.Add(initialization);
                    if(action.Variable is long variable)
                    {
                        cleanupTargets[initialization]=Name(variable);
                        if(unit.Symbols[variable].Kind=="local")hoisted.Add(variable);
                    }
                }
            foreach(var member in unit.ConstructorInitializers.GetValueOrDefault(functionId,[]))
                cleanupTargets[member.Initialization]=Target(member);
            constructorCompletionRequired=details.Kind=="constructor" && cleanupFlags.Any(id=>unit.Initializations[id].ConstructorInit);
            Line(indent,"{");
            foreach(long variable in hoisted.Order())Line(indent+1,Type(unit.Symbols[variable].Type)+" "+Name(variable)+" = default;");
            foreach(long initialization in cleanupFlags.Order())Line(indent+1,"bool "+Flag(initialization)+" = false;");
            if(constructorCompletionRequired)Line(indent+1,"bool __constructor_complete = false;");
            void Core(int level)
            {
                WriteLocalArrayStorage(level);
                WriteVarargsPrologue(level);
                void Body(int content)
                {
                    if(details.Kind=="constructor")
                    {
                        bool wroteVptrs=false;
                        foreach(var member in unit.ConstructorInitializers.GetValueOrDefault(functionId,[]))
                        {
                            if(member.Kind=="field" && !wroteVptrs)
                            {
                                WriteVptrWrites("after-bases-before-members",content);wroteVptrs=true;
                            }
                            var initialization=unit.Initializations[member.Initialization];
                            string? source=initialization.ImpliedSource
                                ?Target(member,member.CopySource??throw new InvalidDataException("Exporter defect: implied constructor member copy source missing")):null;
                            WriteInitialization(initialization,Target(member),content,source);
                        }
                        if(!wroteVptrs)WriteVptrWrites("after-bases-before-members",content);
                    }
                    WriteVptrWrites("before-body",content);
                    WriteBlockStatements(body,content);
                    if(constructorCompletionRequired && unit.Children(body,"statement").LastOrDefault()?.Kind!="return")Line(content,"__constructor_complete = true;");
                }
                void Root(int content)=>WriteLifetimeRegion(body,content,Body);
                if(details.Kind=="destructor")
                {
                    var members=unit.ConstructorInitializers.GetValueOrDefault(functionId,[])
                        .Select(member=>member.Initialization).Where(id=>unit.Initializations[id].Destructor is not null).ToArray();
                    WriteCleanupRegion("destructor",members,level,Root,always:true);
                }
                else Root(level);
            }
            bool empty=!unit.Children(body,"statement").Any(child=>child.Kind!="empty" && !(child.Kind=="return" && !unit.Children(child,"value").Any())) &&
                unit.ConstructorInitializers.GetValueOrDefault(functionId,[]).Count==0 && cleanupFlags.Count==0 &&
                unit.VptrWrites.GetValueOrDefault(functionId,[]).Count==0 && varargs is null && localArrayOwners.Count==0;
            if(details.ExceptionPolicy=="terminate" && !empty)
            {
                Line(indent+1,"try");Line(indent+1,"{");Core(indent+2);Line(indent+1,"}");
                Line(indent+1,"catch (global::System.Exception __noexcept_error) when (!global::Edg.Runtime.ObjectProgram.IsRuntimeTransfer(__noexcept_error))");
                Line(indent+1,"{");Line(indent+2,"global::System.Environment.FailFast(\"Exception escaped noexcept function\", __noexcept_error);");Line(indent+2,"throw;");Line(indent+1,"}");
            }
            else Core(indent+1);
            Line(indent,"}");
        }

        private void WriteBlock(Node block,int indent)
        {
            Line(indent,"{");
            WriteLifetimeRegion(block,indent+1,content=>WriteBlockStatements(block,content));
            Line(indent,"}");
        }

        private void WriteLifetimeRegion(Node owner,int indent,Action<int> content)
        {
            var lifetimes=unit.Lifetimes.Values.Where(lifetime=>lifetime.OwnerNode==owner.Id)
                .Select(lifetime=>lifetime.Id).ToList();
            if(owner.Lifetime is long direct && !lifetimes.Contains(direct))lifetimes.Add(direct);
            var active=lifetimes.Where(id=>unit.Destructions.GetValueOrDefault(id,[]).Any(ActionNeeded)).ToArray();
            if(active.Length>1)throw unit.Missing(owner,"multiple cleanup lifetimes on one owner need explicit nesting emission");
            if(active.Length==0){content(indent);return;}
            WriteCleanupRegion("lifetime_"+active[0],unit.Destructions[active[0]].Where(ActionNeeded).ToArray(),indent,content);
            bool ActionNeeded(long id)=>!allocationOwnedCleanups.Contains(id) &&
                !(unit.FunctionDetails[function!.Value.Id].Kind=="destructor" && unit.Initializations[id].ConstructorInit);
        }

        private void WriteCleanupRegion(string key,IReadOnlyList<long> actions,int indent,Action<int> content,bool always=false)
        {
            if(actions.Count==0){content(indent);return;}
            string unwinding="__unwinding_"+key;
            Line(indent,"bool "+unwinding+" = false;");
            Line(indent,"try");Line(indent,"{");content(indent+1);Line(indent,"}");
            Line(indent,"catch (global::System.Exception __error_"+key+") when (global::Edg.Runtime.ObjectProgram.BeginUnwind(__error_"+key+", ref "+unwinding+"))");
            Line(indent,"{");Line(indent+1,"throw;");Line(indent,"}");
            Line(indent,"finally");Line(indent,"{");Cleanup(0,indent+1);Line(indent,"}");
            void Cleanup(int index,int level)
            {
                if(index==actions.Count)return;
                var action=unit.Initializations[actions[index]];
                if(action.Destructor is null){Cleanup(index+1,level);return;}
                CheckInitialization(action);
                string target=cleanupTargets.GetValueOrDefault(action.Id)
                    ??throw new InvalidDataException("Cleanup destination missing for initialization "+action.Id);
                Line(level,"try");Line(level,"{");
                string condition=always?"true":Flag(action.Id);
                // A body-local destructor can fail after the source return or
                // fallthrough marked completion. The existing filters mark
                // this region as unwinding before later cleanup actions run.
                if(action.ConstructorInit && unit.FunctionDetails[function!.Value.Id].Kind=="constructor")
                    condition+=" && (!__constructor_complete || "+unwinding+")";
                Line(level+1,"if ("+condition+")");Line(level+1,"{");
                if(!always)Line(level+2,Flag(action.Id)+" = false;");
                Line(level+2,emitter.SymbolName(unit,action.Destructor.Value)+"(&"+target+");");
                Line(level+1,"}");Line(level,"}");
                string error="__cleanup_error_"+key+"_"+index;
                Line(level,"catch (global::System.Exception "+error+") when ("+unwinding+" && global::Edg.Runtime.ObjectProgram.TerminateCleanupFailure("+error+"))");
                Line(level,"{");Line(level+1,"throw;");Line(level,"}");
                Line(level,"catch (global::System.Exception "+error+"_begin) when (global::Edg.Runtime.ObjectProgram.BeginUnwind("+error+"_begin, ref "+unwinding+"))");
                Line(level,"{");Line(level+1,"throw;");Line(level,"}");
                if(index+1<actions.Count){Line(level,"finally");Line(level,"{");Cleanup(index+1,level+1);Line(level,"}");}
            }
        }

        private void CheckInitialization(Initialization initialization)
        {
            if(initialization.Unordered || initialization.OverlapsInner || initialization.PartialAggregate || initialization.FreeOnException || initialization.StaticStorage || initialization.OverlapLifetime is not null)
                throw new InvalidDataException("Initialization "+initialization.Id+" needs unsupported unordered/overlap/array/storage cleanup emission");
            bool hasAggregateActions=initialization.Value is long aggregate &&
                (unit.ArrayInitializers.ContainsKey(aggregate) || unit.RecordInitializers.ContainsKey(aggregate) || unit.LiteralInitializers.ContainsKey(aggregate));
            if(initialization.PartiallyInitialized && !hasAggregateActions || initialization.ValueInitialization ||
                initialization.TemporaryLifetime && initialization.Destructor is not null)
                throw new InvalidDataException("Initialization "+initialization.Id+" needs unsupported partial/value/temporary initialization emission");
            if(initialization.ImpliedSource && initialization.Kind!="bitwise-copy")
                throw new InvalidDataException("Initialization "+initialization.Id+" needs implied-source constructor argument emission");
            if(initialization.ExpressionLifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Any(id=>!allocationOwnedCleanups.Contains(id)))
                throw new InvalidDataException("Initialization "+initialization.Id+" requires initializer expression cleanup");
        }

        private int expressionExpansionDepth;

        private string ExpandedExpression(Node node,int indent)
        {
            ++expressionExpansionDepth;
            try {return ExpandedExpressionCore(node,indent);}
            finally {--expressionExpansionDepth;}
        }

        private string ExpandedExpressionCore(Node node,int indent)
        {
            if(expandedValues.TryGetValue(node.Id,out string? found))return NormalizeValue(node,found);
            if(node.Type is long zeroArray && IsZeroExtentArray(unit,zeroArray) && ZeroExtentFieldAddress(node) is string address)
                return NormalizeValue(node,address);
            if(IsPlacementWrapper(node))return ExpandedPlacementWrapper(node,indent);
            if(node.Kind=="new" && node.Operator=="placement")return ExpandedPlacementAllocation(node,indent);
            if((node.Kind is "temporary" or "lambda") && node.Initialization is long dependent &&
               unit.Initializations[dependent].DestinationInitialization is not null)
            {
                var initialization=unit.Initializations[dependent];
                string destination=SharedDestination(initialization);
                WriteInitialization(initialization,destination,indent);
                return NormalizeValue(node,destination);
            }
            // The source operand is navigation; the selected execution graph
            // owns its evaluation, including any converted call arguments.
            if(node.Kind=="delete")
            {
                return node.Operator is "destroy" or "virtual-destroy"?Expression(node):NormalizeValue(node,ExpandedExpression(Child(node,"execution"),indent));
            }
            // These helpers preserve the recorded operand order inside the
            // expression, including conditional and short-circuit branches.
            if(UsesCompoundHelper(node))return Expression(node);
            // A named local's designation has no evaluation effect. Keeping
            // this recorded value assignment inline preserves short-circuit
            // and conditional selection without moving its RHS outwards.
            if(IsDirectLocalAssignment(node))return Expression(node);
            var operands=unit.Children(node,"operand").ToArray();
            if(node.EvaluationOrder=="right-to-left" && operands.Length>1)
            {
                if(node.Operator is not("assign" or "add-assign" or "subtract-assign" or "pointer-add-assign" or "pointer-subtract-assign" or "multiply-assign" or "divide-assign" or "remainder-assign"))
                    throw unit.Missing(node,"right-to-left operation lowering is not yet implemented");
                Node right=operands[1];
                string name="__rhs_"+node.Id;
                string value=ExpandedExpression(right,indent);
                Line(indent,NodeType(right)+" "+name+" = "+value+";");
                expandedValues.Add(right.Id,name);
                try
                {
                    ExpandedExpression(operands[0],indent);
                    return Expression(node);
                }
                finally {expandedValues.Remove(right.Id);}
            }
            if(node.Kind is "temporary" or "lambda")
            {
                var initialization=unit.Initializations[node.Initialization??throw unit.Missing(node,"temporary initialization missing")];
                if(initialization.Destructor is not null)throw unit.Missing(node,"temporary cleanup expansion is not yet implemented");
                string name="__temporary_"+node.Id;
                Line(indent,Type(initialization.Type??node.Type!.Value)+" "+name+" = default;");
                WriteInitialization(initialization,name,indent);
                expandedValues.Add(node.Id,name);
                return NormalizeValue(node,name);
            }
            if(node.Kind=="operation" && node.Operator is "conditional" or "logical-and" or "logical-or")return Expression(node);
            // A virtual call's selected declaration describes its slot. It
            // need not have a body (for example, a pure virtual declaration).
            // The recorded table target supplies executable dispatch.
            if(unit.VirtualCalls.ContainsKey(node.Id))operands=operands.Skip(1).ToArray();
            if(node.EvaluationOrder=="right-to-left")operands=operands.Reverse().ToArray();
            foreach(var operand in operands.Where(operand=>operand.Kind!="field"))ExpandedExpression(operand,indent);
            return Expression(node);
        }

        private void WriteInitialization(Initialization initialization,string destination,int indent,string? impliedSource=null)
        {
            if(initialization.DestinationInitialization is not null)destination=SharedDestination(initialization);
            CheckInitialization(initialization);
            switch(initialization.Kind)
            {
                case "none":break;
                case "shared-result":WriteSharedResult(initialization,destination,indent);break;
                case "zero":Line(indent,destination+" = default;");break;
                case "expression":case "constant":
                    var value=unit.Nodes[initialization.Value??throw new InvalidDataException("Initialization value missing")];
                    if(unit.RecordInitializers.ContainsKey(value.Id))WriteRecordInitializer(value,destination,indent);
                    else if(unit.ArrayInitializers.ContainsKey(value.Id) || unit.LiteralInitializers.ContainsKey(value.Id))WriteArrayInitializer(value,"(&"+destination+")",indent);
                    else Line(indent,destination+" = "+ExpandedExpression(value,indent)+";");
                    break;
                case "aggregate":
                    var aggregate=unit.Nodes[initialization.Value??throw new InvalidDataException("Exporter defect: aggregate initialization actions missing")];
                    if(unit.RecordInitializers.ContainsKey(aggregate.Id))WriteRecordInitializer(aggregate,destination,indent);
                    else if(unit.ArrayInitializers.ContainsKey(aggregate.Id))WriteArrayInitializer(aggregate,"(&"+destination+")",indent);
                    else throw unit.Missing(aggregate,"exporter defect: aggregate initialization requires selected record or array actions");
                    break;
                case "bitwise-copy":
                    string source=initialization.ImpliedSource?impliedSource??throw new InvalidDataException("Implied initialization source missing")
                        :ExpandedExpression(unit.Nodes[initialization.Value??throw new InvalidDataException("Copy initialization source missing")],indent);
                    Line(indent,destination+" = "+source+";");break;
                case "constructor":
                    long constructor=initialization.Constructor??throw new InvalidDataException("Selected constructor missing");
                    var arguments=unit.InitializationArguments.GetValueOrDefault(initialization.Id,[])
                        .Select(id=>ExpandedExpression(unit.Nodes[id],indent)).ToArray();
                    Line(indent,emitter.SymbolName(unit,constructor)+"(&"+destination+(arguments.Length==0?"":", "+string.Join(", ",arguments))+");");break;
                case "class-result":
                    WriteClassResultCall(unit.Nodes[initialization.Value??throw new InvalidDataException("Class result call missing")],destination,indent);break;
                default:throw new InvalidDataException("Unsupported initialization kind "+initialization.Kind);
            }
            if(cleanupFlags.Contains(initialization.Id))Line(indent,Flag(initialization.Id)+" = true;");
        }

        private void WriteDeclaration(Node node,int indent)
        {
            if(node.Kind=="static-assertion")
            {
                Line(indent,"// static_assert (accepted by EDG).");
                return;
            }
            if(node.Kind=="static-declaration"){WriteStaticDeclaration(node,indent);return;}
            if(node.Symbol is long array && unit.IsArray(unit.Symbols[array].Type)){WriteArrayDeclaration(node,indent);return;}
            if(node.Initialization is not long id){Line(indent,Declaration(node)+";");return;}
            long variable=node.Symbol??unit.Initializations[id].Variable??throw unit.Missing(node,"initialization destination missing");
            if(unit.Initializations[id].Destructor is not null && !cleanupFlags.Contains(id))
                throw unit.Missing(node,"local destructor has no emitted lifetime cleanup obligation");
            if(!hoisted.Contains(variable))Line(indent,Type(unit.Symbols[variable].Type)+" "+Name(variable)+" = default;");
            WriteInitialization(unit.Initializations[id],Name(variable),indent);
        }
    }
}
