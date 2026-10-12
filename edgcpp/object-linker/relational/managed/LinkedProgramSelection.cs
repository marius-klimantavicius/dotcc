namespace DotObj.Relational;

internal sealed partial class LinkedProgram
{
    public int CapturedFunctionDefinitions {get;private set;}
    public int CapturedGlobalDefinitions {get;private set;}
    public int ConservativeEmissionRoots {get;private set;}
    public string EmissionSelection {get;private set;}="all-bodies";

    public void SelectEmission(string mode,string? entry)
    {
        CapturedFunctionDefinitions=Functions.Count;
        CapturedGlobalDefinitions=Globals.Count;
        if(mode=="all-bodies")return;
        if(Units.All(unit=>!unit.HasEmissionRoots))
        {
            EmissionSelection="all-bodies (legacy objects without root facts)";
            ConservativeEmissionRoots=Functions.Count+Globals.Count;
            return;
        }
        EmissionSelection="native-roots";
        var availableFunctions=Functions.ToHashSet();
        var availableGlobals=Globals.ToHashSet();
        var selected=new HashSet<SymbolRef>();
        var pending=new Queue<SymbolRef>();
        var visitedGlobals=new HashSet<(Unit,long)>();
        var visitedGlobalInitializations=new HashSet<(Unit,long)>();
        var visitedSymbolReferences=new HashSet<(Unit,long)>();
        var virtualDefinitions=new Dictionary<string,(Unit Unit,VirtualTable Table)>(StringComparer.Ordinal);
        var visitedTables=new HashSet<string>(StringComparer.Ordinal);
        var aliasTargets=new Dictionary<(Unit,long),List<long>>();
        foreach(var unit in Units)
        {
            foreach(var table in unit.VirtualTables.Values.Where(table=>table.Definition))
                virtualDefinitions.TryAdd(TableKey(unit,table),(unit,table));
            foreach(var alias in unit.FunctionAbiEntries.Values.SelectMany(entries=>entries).Where(alias=>alias.AliasTarget is not null))
            {
                var key=(unit,alias.Entry);
                if(!aliasTargets.TryGetValue(key,out var targets))aliasTargets.Add(key,targets=[]);
                targets.Add(alias.AliasTarget!.Value);
            }
        }
        // A root in any input declaration belongs to the resolved definition,
        // even when that definition came from another weak-provider object.
        foreach(var unit in Units)
            foreach(var symbol in unit.Symbols.Values)
            {
                if(symbol.EmissionRoot==true)AddSymbol(unit,symbol.Id);
                else if(symbol.EmissionRoot is null && symbol.Definition)
                {
                    var definition=Resolve(unit,symbol.Id);
                    if(availableFunctions.Contains(definition) || availableGlobals.Contains(definition))
                    {
                        ++ConservativeEmissionRoots;
                        AddSymbol(unit,symbol.Id);
                    }
                }
            }
        foreach(var unit in Units)
            foreach(var startup in unit.StartupGroups)AddSymbol(unit,startup.Function);
        if(entry is not null)
        {
            var matches=Functions.Where(function=>function.Symbol.Name==entry || function.Symbol.LinkName==entry).ToArray();
            if(matches.Length!=1)throw new InvalidDataException("Entry must identify exactly one selected function: "+entry);
            AddSymbol(matches[0].Unit,matches[0].Id);
        }
        while(pending.TryDequeue(out var definition))
        {
            var unit=definition.Unit;
            if(availableFunctions.Contains(definition))
            {
                using var lease=LoadFunction(definition);
                VisitBody(unit,definition.Id);
            }
            else if(unit.Initializers.TryGetValue(definition.Id,out long initializer))
                VisitNode(unit,unit.Nodes[initializer],visitedGlobals,visitedGlobalInitializations,[],[]);
        }
        Functions.RemoveAll(function=>!selected.Contains(function));
        Globals.RemoveAll(global=>!selected.Contains(global));

        void AddSymbol(Unit unit,long id)
        {
            if(!visitedSymbolReferences.Add((unit,id)))return;
            // ABI alias targets are explicit references, without variant-name
            // parsing or substituting a source destructor for an ABI routine.
            if(aliasTargets.TryGetValue((unit,id),out var aliases))
                foreach(long target in aliases)AddSymbol(unit,target);
            var definition=Resolve(unit,id);
            AddDefinition(definition);
            if(aliasTargets.TryGetValue((definition.Unit,definition.Id),out var resolvedAliases))
                foreach(long target in resolvedAliases)AddSymbol(definition.Unit,target);
            foreach(var vtt in definition.Unit.VttEntries.GetValueOrDefault(definition.Id,[]))
                VisitTable(definition.Unit,vtt.Table);
        }
        void AddDefinition(SymbolRef definition)
        {
            if((availableFunctions.Contains(definition) || availableGlobals.Contains(definition)) && selected.Add(definition))
                pending.Enqueue(definition);
        }
        void VisitTable(Unit unit,long id)
        {
            var table=unit.VirtualTables[id];
            string key=TableKey(unit,table);
            if(!visitedTables.Add(key))return;
            if(virtualDefinitions.TryGetValue(key,out var provided)){unit=provided.Unit;table=provided.Table;}
            if(!table.Definition)return;
            foreach(var slot in unit.VirtualEntries.GetValueOrDefault(table.Id,[]).Where(slot=>slot.Kind=="ordinary"))
            {
                long? target=slot.AbiEntrySymbol??(slot.Variant=="ordinary"?slot.Target:null);
                if(target is long symbol)AddSymbol(unit,symbol);
            }
        }
        void VisitBody(Unit unit,long function)
        {
            var nodes=new HashSet<(Unit,long)>();
            var initializations=new HashSet<(Unit,long)>();
            var virtualCallees=new HashSet<long>();
            foreach(var call in unit.VirtualCalls.Values)
                foreach(var edge in unit.Edges.GetValueOrDefault(call.Node,[]).Where(edge=>edge.Role=="operand" && edge.Ordinal==0))
                    if(unit.Nodes[edge.Child].Kind=="function")virtualCallees.Add(edge.Child);
            foreach(var parent in unit.Edges)
                foreach(var edge in parent.Value)
                    if(virtualCallees.Contains(edge.Child) &&
                       !(unit.VirtualCalls.ContainsKey(parent.Key) && edge.Role=="operand" && edge.Ordinal==0))
                        virtualCallees.Remove(edge.Child);
            var virtualDestructions=new HashSet<long>();
            var destroyNodes=new HashSet<long>();
            foreach(var deletion in unit.Nodes.Values.Where(node=>node.Kind=="delete" && node.Operator=="virtual-destroy"))
                foreach(var child in unit.Children(deletion,"destroy"))
                    if(child.Initialization is long action){virtualDestructions.Add(action);destroyNodes.Add(child.Id);}
            foreach(var node in unit.Nodes.Values)
                if(node.Initialization is long action && !destroyNodes.Contains(node.Id))virtualDestructions.Remove(action);
            foreach(long action in unit.Destructions.Values.SelectMany(actions=>actions))virtualDestructions.Remove(action);
            foreach(var member in unit.ConstructorInitializers.Values.SelectMany(members=>members))virtualDestructions.Remove(member.Initialization);
            // These are recorded program nodes in the selected body, including
            // source statements the host C# compiler may later simplify.
            foreach(var node in unit.Nodes.Values.Where(node=>node.Function==function))
                VisitNode(unit,node,nodes,initializations,virtualCallees,virtualDestructions);
            foreach(var action in unit.Initializations.Values.Where(action=>action.Function==function))
                VisitInitialization(unit,action.Id,nodes,initializations,virtualCallees,virtualDestructions);
            foreach(var member in unit.ConstructorInitializers.GetValueOrDefault(function,[]))
                VisitInitialization(unit,member.Initialization,nodes,initializations,virtualCallees,virtualDestructions);
            foreach(var write in unit.VptrWrites.GetValueOrDefault(function,[]))
                if(write.Table is long table)VisitTable(unit,table);
            foreach(var handler in unit.Handlers.Values.SelectMany(handlers=>handlers))
                if(handler.Initialization is long action)VisitInitialization(unit,action,nodes,initializations,virtualCallees,virtualDestructions);
            foreach(var site in unit.Throws.Values)
            {
                if(site.Initialization is long action)VisitInitialization(unit,action,nodes,initializations,virtualCallees,virtualDestructions);
                if(site.Destructor is long destructor)AddSymbol(unit,destructor);
            }
        }
        void VisitInitialization(Unit unit,long id,HashSet<(Unit,long)> nodes,HashSet<(Unit,long)> actions,
            HashSet<long> virtualCallees,HashSet<long> virtualDestructions)
        {
            if(!actions.Add((unit,id)))return;
            var action=unit.Initializations[id];
            if(action.Variable is long variable)AddSymbol(unit,variable);
            if(action.Constructor is long constructor)AddSymbol(unit,constructor);
            if(action.SelectedCleanupEntry is long cleanup)AddSymbol(unit,cleanup);
            else if(action.Destructor is long destructor && !virtualDestructions.Contains(id))AddSymbol(unit,destructor);
            if(action.Value is long value)VisitNode(unit,unit.Nodes[value],nodes,actions,virtualCallees,virtualDestructions);
            foreach(long argument in unit.InitializationArguments.GetValueOrDefault(id,[]).Concat(unit.CleanupArguments.GetValueOrDefault(id,[])))
                VisitNode(unit,unit.Nodes[argument],nodes,actions,virtualCallees,virtualDestructions);
            if(action.DestructionCondition is long condition)VisitNode(unit,unit.Nodes[condition],nodes,actions,virtualCallees,virtualDestructions);
            if(action.DestinationInitialization is long destination)VisitInitialization(unit,destination,nodes,actions,virtualCallees,virtualDestructions);
            // source_initialization_id is provenance. It does not select a
            // second source destructor in addition to the selected ABI action.
        }
        void VisitNode(Unit unit,Node node,HashSet<(Unit,long)> nodes,HashSet<(Unit,long)> actions,
            HashSet<long> virtualCallees,HashSet<long> virtualDestructions)
        {
            if(!nodes.Add((unit,node.Id)) || node.Kind is "static-assertion" or "type-reference")return;
            if(node.Symbol is long symbol && node.Kind is "variable" or "function" or "routine-address" or "type-info-address" or "vtt-address" or "initialize" or "static-declaration")
                if(!(node.Kind=="function" && virtualCallees.Contains(node.Id)))AddSymbol(unit,symbol);
            if(node.Initialization is long action)VisitInitialization(unit,action,nodes,actions,virtualCallees,virtualDestructions);
            if(unit.Allocations.TryGetValue(node.Id,out var allocation))
            {
                AddSymbol(unit,allocation.Allocator);
                if(allocation.Failure is long failure)AddSymbol(unit,failure);
                VisitNode(unit,unit.Nodes[allocation.Size],nodes,actions,virtualCallees,virtualDestructions);
                if(allocation.CountValue is long count)VisitNode(unit,unit.Nodes[count],nodes,actions,virtualCallees,virtualDestructions);
                if(allocation.Check is long check)VisitNode(unit,unit.Nodes[check],nodes,actions,virtualCallees,virtualDestructions);
            }
            foreach(var element in unit.ArrayElements.GetValueOrDefault(node.Id,[]))
                VisitNode(unit,unit.Nodes[element.Value],nodes,actions,virtualCallees,virtualDestructions);
            foreach(var element in unit.RecordElements.GetValueOrDefault(node.Id,[]))
                VisitNode(unit,unit.Nodes[element.Value],nodes,actions,virtualCallees,virtualDestructions);
            foreach(var edge in unit.Edges.GetValueOrDefault(node.Id,[]))
                if(edge.Role is not ("selected-type" or "source-condition" or "source-name" or "selected-abi-entry"))
                    VisitNode(unit,unit.Nodes[edge.Child],nodes,actions,virtualCallees,virtualDestructions);
        }
    }

    private static string TableKey(Unit unit,VirtualTable table)=>
        (table.Linkage=="external"?"external:"+table.LinkName:unit.Path+":"+table.Id)+":"+table.AddressPoint;
}
