namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,string> expressionTemporaryStorage=[];
        private readonly HashSet<long> protectedExpressionLifetimes=[];

        private void PrepareExpressionTemporaryStorage(long functionId)
        {
            foreach(long id in cleanupFlags)
            {
                var action=unit.Initializations[id];
                if(action.Function!=functionId || action.Variable is not null || !action.TemporaryLifetime || action.Destructor is null)continue;
                var nodes=unit.Nodes.Values.Where(node=>node.Function==functionId && node.Initialization==id && node.Kind is "temporary" or "lambda").ToArray();
                if(nodes.Length!=1)
                    throw new InvalidDataException("Expression temporary cleanup needs one recorded temporary node for initialization "+id);
                if(action.DestinationInitialization is not null)
                    throw unit.Missing(nodes[0],"shared destination temporary cannot own independent expression cleanup");
                string name=allocator.Allocate("__temporary_"+nodes[0].Id,"expression_temporary");
                expressionTemporaryStorage.Add(id,name);
                cleanupTargets.Add(id,name);
            }
            PrepareUnorderedTemporaryGroups(functionId);
        }

        private void WriteExpressionTemporaryStorage(int indent)
        {
            foreach(var item in expressionTemporaryStorage)
            {
                var action=unit.Initializations[item.Key];
                long type=action.Type??throw new InvalidDataException("Expression temporary type missing for initialization "+action.Id);
                Line(indent,Type(type)+" "+item.Value+" = default;");
            }
        }

        private bool WriteInitializationExpressionLifetime(Initialization initialization,string destination,int indent,string? impliedSource)
        {
            if(initialization.ExpressionLifetime is not long lifetime || protectedExpressionLifetimes.Contains(lifetime))return false;
            var actions=unit.Destructions.GetValueOrDefault(lifetime,[]).Where(id=>!allocationOwnedCleanups.Contains(id)).ToArray();
            if(actions.Length==0)return false;
            foreach(long id in actions)
            {
                var action=unit.Initializations[id];
                if(action.Destructor is not null && (!cleanupFlags.Contains(id) || !cleanupTargets.ContainsKey(id)))
                    throw new InvalidDataException("Initializer expression cleanup destination missing for initialization "+id);
            }
            protectedExpressionLifetimes.Add(lifetime);
            try
            {
                WriteCleanupRegion("initializer_"+initialization.Id+"_lifetime_"+lifetime,actions,indent,
                    level=>WriteInitialization(initialization,destination,level,impliedSource));
            }
            finally {protectedExpressionLifetimes.Remove(lifetime);}
            return true;
        }

        private long[] ExpressionScopeCleanupLifetimes(Node node)
        {
            var lifetimes=unit.Lifetimes.Values.Where(lifetime=>lifetime.OwnerNode==node.Id).Select(lifetime=>lifetime.Id);
            if(node.Lifetime is long direct)lifetimes=lifetimes.Append(direct);
            return lifetimes.Distinct().Where(id=>unit.Destructions.GetValueOrDefault(id,[])
                .Any(action=>!allocationOwnedCleanups.Contains(action))).ToArray();
        }

        private bool WriteVoidLifetimeStatement(Node node,int indent)
        {
            if(node.Kind!="lifetime-scope" || node.Type is not long type || unit.Original(type).Kind!="void")return false;
            var active=ExpressionScopeCleanupLifetimes(node);
            if(active.Length>1)throw unit.Missing(node,"multiple expression cleanup scopes require explicit nesting");
            void Content(int level)
            {
                Node operand=Child(node,"operand");
                if(operand.Kind is "throw" or "rethrow")WriteThrow(operand,level);
                else if(!WriteVoidLifetimeStatement(operand,level) && !WriteVoidDiscardStatement(operand,level))
                    Line(level,StatementExpression(operand,ExpandedExpression(operand,level))+";");
            }
            if(active.Length==0 || protectedExpressionLifetimes.Contains(active[0]))Content(indent);
            else
            {
                protectedExpressionLifetimes.Add(active[0]);
                try {WriteLifetimeRegion(node,indent,Content);}
                finally {protectedExpressionLifetimes.Remove(active[0]);}
            }
            return true;
        }

        private bool IsRecordLvalueScope(Node node)=>node.Kind=="lifetime-scope" && node.ValueCategory=="lvalue" &&
            node.Type is long type && unit.Original(type).Kind=="record";

        private string ExpandedExpressionLifetimeScope(Node node,int indent)
        {
            var active=ExpressionScopeCleanupLifetimes(node);
            if(active.Length==0)return NormalizeValue(node,ExpandedExpression(Child(node,"operand"),indent));
            if(active.Length!=1)throw unit.Missing(node,"multiple expression cleanup scopes require explicit nesting");
            long selectedLifetime=active[0];
            if(protectedExpressionLifetimes.Contains(selectedLifetime))
            {
                string value=ExpandedExpression(Child(node,"operand"),indent);
                expandedValues.Add(node.Id,value);
                return NormalizeValue(node,value);
            }
            var result=unit.Original(node.ExecutionType??node.Type??throw unit.Missing(node,"expression cleanup result type missing"));
            bool location=result.Kind=="record" && node.ValueCategory=="lvalue";
            if(result.Kind is "array" or "function" or "void" || result.Kind=="record" && !location)
                throw unit.Missing(node,"expression cleanup result requires a scalar or recorded reference/address value");
            string name=allocator.Allocate((location?"__scope_address_":"__scope_value_")+node.Id,"expression_scope_result");
            Line(indent,NodeType(node)+(location?"*":"")+" "+name+" = default;");
            protectedExpressionLifetimes.Add(selectedLifetime);
            try
            {
                WriteLifetimeRegion(node,indent,level=>
                {
                    Node operand=Child(node,"operand");
                    if(location)
                    {
                        ExpandAddressLocation(operand,level);
                        Line(level,name+" = "+AddressExpression(operand)+";");
                    }
                    else Line(level,name+" = "+ExpandedExpression(operand,level)+";");
                });
            }
            finally {protectedExpressionLifetimes.Remove(selectedLifetime);}
            string rendered=location?"(*"+name+")":name;
            expandedValues.Add(node.Id,rendered);
            return NormalizeValue(node,rendered);
        }
    }
}
