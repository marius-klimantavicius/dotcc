using RoslynObj;
using System.Diagnostics;
using System.Text.Json;

internal static class ProjectInputAcceptance
{
    private const int NameLimit=64,KeyLimit=8,SymbolsPerKey=4,RootLimit=4,OperationLimit=256;
    private sealed record Key(string Kind,string Name);
    private sealed record NameSample(string Kind,string Name,int Matches,int Selected,bool Truncated);
    private sealed record Selection(IReadOnlyList<SemanticEntity> Entities,IReadOnlyList<FunctionSymbol> Roots,IReadOnlyList<NameSample> Names);

    internal static void Run(string path,IReadOnlyList<string> names)
    {
        if(names.Count is 0 or >KeyLimit)throw new ArgumentException($"Supply between one and {KeyLimit} explicit retained-name keys.",nameof(names));
        var keys=names.Select(ParseKey).Distinct().ToArray();
        var checks=0;
        void Check(bool value,string message) { if(!value)throw new Exception(message);checks++; }
        using var process=Process.GetCurrentProcess();process.Refresh();var beforeOpenRss=process.WorkingSet64;
        var watch=Stopwatch.StartNew();
        using var snapshot=RoslynObject.Open(path,new(){PageSize=128,CachedRowCount=4096,CachedPageCount=128});
        watch.Stop();process.Refresh();
        Console.WriteLine(JsonSerializer.Serialize(new{phase="open",path,wall_ms=watch.Elapsed.TotalMilliseconds,rss_before=beforeOpenRss,rss_bytes=process.WorkingSet64,peak_rss_bytes=process.PeakWorkingSet64,commands=snapshot.Statistics.Commands,rows=snapshot.Statistics.RowsRead}));
        IReadOnlyList<SemanticEntity>? firstEntities=null;
        for(var pass=0;pass<2;pass++)
        {
            var before=snapshot.Statistics;var beforeChecks=checks;watch.Restart();
            Check(!string.IsNullOrEmpty(snapshot.Manifest.ArtifactIdentity),"The project object must retain its artifact identity.");
            var selection=Select(snapshot.TranslationUnit,keys,Check);
            if(firstEntities is null)firstEntities=selection.Entities;
            else Check(firstEntities.SequenceEqual(selection.Entities),"Warm project reads must preserve artifact-scoped selected identities.");
            var (operations,calls,truncated)=Inspect(snapshot.TranslationUnit,selection.Roots,Check);
            Check(selection.Roots.Count==0||operations>0,"At least one selected function must retain a nonempty executable operation sample.");
            var after=snapshot.Statistics;watch.Stop();process.Refresh();
            var commands=after.Commands-before.Commands;
            Check(after.CachedRows<=4096&&after.CachedPages<=128&&after.MaximumBatchSize<=900,"Project navigation exceeded its cache or prefetch bounds.");
            Check(commands<=512+12*(operations+selection.Entities.Count),"Project navigation exceeded its finite query budget.");
            Console.WriteLine(JsonSerializer.Serialize(new{phase=pass==0?"cold":"warm",path,names=selection.Names,selected_entities=selection.Entities.Count,routine_roots=selection.Roots.Count,operations,calls,truncated_roots=truncated,operation_limit_per_root=OperationLimit,commands,rows=after.RowsRead-before.RowsRead,wall_ms=watch.Elapsed.TotalMilliseconds,rss_bytes=process.WorkingSet64,peak_rss_bytes=process.PeakWorkingSet64,cached_rows=after.CachedRows,cached_pages=after.CachedPages,max_batch=after.MaximumBatchSize,assertions=checks-beforeChecks}));
        }
        Console.WriteLine($"PASS:project-input, {checks} assertions");
    }

    private static Key ParseKey(string text)
    {
        var separator=text.IndexOf(':');
        if(separator<=0||separator==text.Length-1)throw new ArgumentException("Use function:<name>, type:<name> or symbol:<name> keys.",nameof(text));
        var key=new Key(text[..separator],text[(separator+1)..]);
        if(key.Kind is not ("function" or "type" or "symbol"))throw new ArgumentException("Unknown retained-name key kind.",nameof(text));
        return key;
    }

    private static Selection Select(TranslationUnit unit,IReadOnlyList<Key> keys,Action<bool,string> check)
    {
        var selected=new List<SemanticEntity>();var roots=new List<FunctionSymbol>();var samples=new List<NameSample>();
        check(unit.GlobalScope==unit.Details.GlobalScope,"The project TU must preserve its canonical global-scope descriptor.");
        foreach(var key in keys)
        {
            // FindSymbols is paged. Take bounds materialization; it does not bound the underlying SQL search work.
            var matches=unit.SemanticModel.FindSymbols(key.Name).Take(NameLimit+1).ToArray();
            check(matches.Length<=NameLimit,"An explicit source name exceeds the project consumer's result limit.");
            check(matches.All(s=>s.Name==key.Name),"The name query must preserve exact retained source names.");
            IEnumerable<Symbol> candidates=key.Kind switch
            {
                "function"=>matches.OfType<FunctionSymbol>().Where(f=>f.Body is not null),
                "type"=>matches.OfType<TypeSymbol>(),
                _=>matches
            };
            var sample=candidates.Take(SymbolsPerKey+1).ToArray();
            check(sample.Length!=0,"An explicit project name has no retained symbol of the requested kind.");
            var chosen=sample.Take(SymbolsPerKey).ToArray();
            samples.Add(new(key.Kind,key.Name,matches.Length,chosen.Length,sample.Length>SymbolsPerKey));
            foreach(var symbol in chosen)
            {
                selected.Add(symbol);
                if(symbol is TypeSymbol typeSymbol)
                {
                    var type=typeSymbol.Type;selected.Add(type);
                    check(type==typeSymbol.Details.Type,"The selected type symbol must preserve its canonical recorded type.");
                    if(type is RecordType record)
                        foreach(var field in record.Fields.Take(4))
                        {
                            selected.Add(field);
                            check(field.Details.RecordType==record,"A sampled field must preserve its exact containing record.");
                        }
                }
                else if(symbol.DeclaredType is {} type)selected.Add(type);
                if(key.Kind=="function"&&symbol is FunctionSymbol function&&roots.Count<RootLimit&&!roots.Contains(function))roots.Add(function);
            }
        }
        check(roots.Count<=RootLimit,"The project consumer exceeded its routine-root budget.");
        return new(selected.Distinct().ToArray(),roots,samples);
    }

    private static (int Operations,int Calls,int Truncated) Inspect(TranslationUnit unit,IReadOnlyList<FunctionSymbol> roots,Action<bool,string> check)
    {
        var operations=0;var calls=0;var truncated=0;var model=unit.SemanticModel;
        foreach(var function in roots)
        {
            var body=function.Body!;var sample=body.DescendantOperations().Take(OperationLimit+1).ToArray();
            if(sample.Length>OperationLimit)truncated++;
            var prefix=sample.Take(OperationLimit).ToArray();operations+=prefix.Length;
            check(prefix.Distinct().Count()==prefix.Length&&prefix.All(o=>o.RepresentationRole.Name!="metadata"),"The project execution sample must preserve distinct identities and actual executable roles.");
            var visitor=new DispatchVisitor();
            var nodes=new[]{body.Root}.Concat<SemanticNode>(prefix).Distinct().ToArray();
            foreach(var node in nodes)
            {
                node.Accept(visitor);
                check(model.GetOperation(node)==node as Operation,"The project node must preserve its actual operation facade.");
                if(node is Operation operation)check(model.GetType(operation)==operation.Type,"The sampled operation must preserve its canonical retained type.");
                if(node is CallOperation call)
                {
                    calls++;check(model.GetSymbol(call)==call.TargetMethod,"The sampled call must preserve its recorded callee.");
                    foreach(var argument in call.Arguments.Take(8))check(model.GetType(argument.Value)==argument.Value.Type,"A sampled argument must preserve its canonical expression type.");
                }
                if(node is ReferenceOperation {Target:Symbol target} reference)check(model.GetSymbol(reference)==target,"The sampled reference must preserve its exact symbol target.");
            }
            check(visitor.Nodes==nodes.Length&&visitor.Calls==nodes.OfType<CallOperation>().Count(),"The project named visitor must dispatch every sampled node and call exactly once.");
            check(function.Signature==function.Details.RoutineType&&function.ReturnType==function.Signature.ReturnType,"The project routine must preserve its actual callable and return types.");
            foreach(var parameter in function.Parameters.Take(8))check(parameter.Type==parameter.Descriptor.Type,"A sampled parameter must preserve its exact recorded type.");
        }
        return (operations,calls,truncated);
    }

    private sealed class DispatchVisitor : SemanticVisitor
    {
        internal int Nodes,Calls;
        public override void VisitNode(SemanticNode node)=>Nodes++;
        public override void VisitCall(CallOperation operation) { Calls++;base.VisitCall(operation); }
    }
}
