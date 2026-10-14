using RoslynObj;
using System.Diagnostics;
using System.Text.Json;

internal static class RealInputAcceptance
{
    private const int NameLimit=64,RootLimit=4,OperationLimit=256;
    private sealed record Selection(IReadOnlyList<SemanticEntity> Entities,IReadOnlyList<FunctionSymbol> Roots);

    internal static void Run(string path,string inputCase)
    {
        var checks=0;
        void Check(bool value,string message) { if(!value)throw new Exception(message);checks++; }
        using var process=Process.GetCurrentProcess();process.Refresh();var beforeOpenRss=process.WorkingSet64;
        var watch=Stopwatch.StartNew();
        using var snapshot=RoslynObject.Open(path,new(){PageSize=128,CachedRowCount=4096,CachedPageCount=128});
        watch.Stop();process.Refresh();
        Console.WriteLine(JsonSerializer.Serialize(new{phase="open",path,input_case=inputCase,wall_ms=watch.Elapsed.TotalMilliseconds,rss_before=beforeOpenRss,rss_bytes=process.WorkingSet64,peak_rss_bytes=process.PeakWorkingSet64,commands=snapshot.Statistics.Commands,rows=snapshot.Statistics.RowsRead}));
        IReadOnlyList<SemanticEntity>? firstEntities=null;
        for(var pass=0;pass<2;pass++)
        {
            var before=snapshot.Statistics;var beforeChecks=checks;watch.Restart();
            var selection=Select(snapshot.TranslationUnit,inputCase,Check);
            Check(selection.Roots.Count<=RootLimit,"The real-input consumer exceeded its selected routine-root budget.");
            if(firstEntities is null)firstEntities=selection.Entities;
            else Check(firstEntities.SequenceEqual(selection.Entities),"Warm reads must preserve the same artifact-scoped canonical selected entities.");
            var (operations,shallowNodes,calls,truncated)=Inspect(snapshot.TranslationUnit,selection.Roots,Check);
            var after=snapshot.Statistics;watch.Stop();process.Refresh();
            var commands=after.Commands-before.Commands;
            Check(after.CachedRows<=4096&&after.CachedPages<=128&&after.MaximumBatchSize<=900,"Selective real-input navigation exceeded the configured cache or prefetch bounds.");
            Check(commands<=512+12*(operations+shallowNodes+selection.Entities.Count),"Selective real-input navigation exceeded its finite query budget.");
            Console.WriteLine(JsonSerializer.Serialize(new{phase=pass==0?"cold":"warm",path,input_case=inputCase,selected_entities=selection.Entities.Count,routine_roots=selection.Roots.Count,operations,shallow_nodes=shallowNodes,calls,truncated_roots=truncated,operation_limit_per_root=OperationLimit,commands,rows=after.RowsRead-before.RowsRead,wall_ms=watch.Elapsed.TotalMilliseconds,rss_bytes=process.WorkingSet64,peak_rss_bytes=process.PeakWorkingSet64,cached_rows=after.CachedRows,cached_pages=after.CachedPages,max_batch=after.MaximumBatchSize,assertions=checks-beforeChecks}));
        }
        Console.WriteLine($"PASS:real-input {inputCase}, {checks} assertions");
    }

    private static Selection Select(TranslationUnit unit,string inputCase,Action<bool,string> check)
    {
        var model=unit.SemanticModel;var selected=new List<SemanticEntity>();var roots=new List<FunctionSymbol>();
        IReadOnlyList<Symbol> Find(string name)
        {
            var values=model.FindSymbols(name).Take(NameLimit+1).ToArray();
            check(values.Length<=NameLimit,"A selected source name exceeds the consumer's explicit result limit.");
            return values;
        }
        T Symbol<T>(string name) where T:Symbol
        {
            var symbol=Find(name).OfType<T>().Single();selected.Add(symbol);return symbol;
        }
        RecordType Record(string name)
        {
            var symbol=Find(name).OfType<TypeSymbol>().Single(s=>s is not AliasSymbol&&s.Type is RecordType);selected.Add(symbol);var type=(RecordType)symbol.Type;
            check(type.DeclarationSymbols.Contains(symbol),"The selected real class must retain its canonical declaration association.");
            selected.Add(type);return type;
        }
        void Root(FunctionSymbol function)
        {
            check(function.Body is not null,"A selected implemented routine has no retained body.");
            selected.Add(function);roots.Add(function);
        }
        switch(inputCase)
        {
            case "templates":
            {
                var enumeration=Symbol<EnumSymbol>("CLS");
                check(enumeration.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"PRIMARY_TEMPLATE","PARTIAL_SPEC","EXPLICIT_SPEC"})&&enumeration.Enumerators.All(e=>e.Owner==enumeration),"The pristine template test must retain its exact ordered CLS enumerators.");
                var space=Symbol<NamespaceSymbol>("class_tmpl");
                var outer=Find("Outer").OfType<TemplateSymbol>().Single(t=>t.TemplateKind.Name=="templk_class"&&t.SourceCorrespondence?.ParentScope?.AssociatedEntity==space);
                selected.Add(outer);
                check(outer.Descriptor.DeclarationSymbol==outer&&outer.Pattern is RecordType&&outer.Instances.Take(8).Any(i=>!i.IsPrototype&&i.Result is RecordType),"The pristine class template must retain its actual symbol, pattern and concrete instance.");
                var inner=Find("Inner").OfType<TemplateSymbol>().Where(t=>t.TemplateKind.Name=="templk_class").Take(4).ToArray();
                check(inner.Length!=0&&inner.Any(t=>t.Descriptor.Specializations.Take(8).Any()),"The pristine nested template must retain actual specialization relationships.");
                selected.AddRange(inner);
                foreach(var function in Find("inner_dpdt_fn").OfType<FunctionSymbol>().Where(f=>f.Body is not null).Take(RootLimit))Root(function);
                check(roots.Count!=0,"The pristine template test must retain implemented dependent member-function roots.");
                break;
            }
            case "lifetimes":
            {
                var yType=Record("Y");var field=yType.Fields.Single(f=>f.Name=="z");var x=(RecordType)field.DeclaredType!;selected.Add(field);selected.Add(x);
                check(x.Instantiation?.OriginalDefinition?.Name=="X"&&x.TypeArguments.Single() is ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"},"The pristine destructor test must retain Y.z's exact X<int> class instance.");
                check(x.Methods.Any(f=>f.Details.SpecialFunction?.Name=="sfk_destructor"&&f.IsDefined),"The pristine X<int> instance must retain its user-defined destructor instantiation.");
                var y=Symbol<VariableSymbol>("y");
                check(y.Type is PointerType pointer&&pointer.PointeeType==yType,"The pristine global y must retain its exact Y pointer type.");
                Root(Symbol<FunctionSymbol>("main"));
                break;
            }
            case "inheritance":
            {
                var d2=Record("D2");var p=Record("P");var variable=Symbol<VariableSymbol>("d2");var pVariable=Symbol<VariableSymbol>("p");
                check(variable.Type==d2&&pVariable.Type==p,"The pristine globals must retain their exact D2 and P class descriptors.");
                check(d2.Bases.Select(b=>b.BaseType.Name).SequenceEqual(new[]{"V1","V2"})&&p.Bases.Select(b=>b.BaseType.Name).SequenceEqual(new[]{"N","O"}),"The pristine classes must retain the actual declared direct-base order.");
                var shared=d2.BaseSubobjects.Single(b=>b.BaseType.Name=="B");
                check(shared.IsVirtual&&shared.MostDerivedType==d2&&shared.Derivations.Count==2,"The pristine D2 descriptor must retain its exact shared virtual B and two contextual derivations.");
                Root(Symbol<FunctionSymbol>("main"));
                break;
            }
            case "types-tu":
            {
                var type=Record("a_type");var kind=type.Fields.Single(f=>f.Name=="kind");var next=type.Fields.Single(f=>f.Name=="next");selected.Add(kind);selected.Add(next);
                check(kind.Details.RecordType==type&&next.Details.RecordType==type,"The real compiler a_type fields must retain their exact containing class.");
                foreach(var name in new[]{"skip_typerefs","is_incomplete_type","is_function_type","is_integral_or_enum_type"})Root(Symbol<FunctionSymbol>(name));
                break;
            }
            default:throw new ArgumentException("Unknown real-input acceptance case.",nameof(inputCase));
        }
        return new(selected.Distinct().ToArray(),roots);
    }

    private static (int Operations,int ShallowNodes,int Calls,int Truncated) Inspect(TranslationUnit unit,IReadOnlyList<FunctionSymbol> roots,Action<bool,string> check)
    {
        var operations=0;var shallowNodes=0;var calls=0;var truncated=0;var model=unit.SemanticModel;
        foreach(var function in roots)
        {
            var body=function.Body!;var sampled=body.DescendantOperations().Take(OperationLimit+1).ToArray();
            if(sampled.Length>OperationLimit)truncated++;
            var prefix=sampled.Take(OperationLimit).ToArray();operations+=prefix.Length;
            check(prefix.Distinct().Count()==prefix.Length&&prefix.All(o=>o.RepresentationRole.Name!="metadata"),"The selected execution prefix must preserve identity deduplication and actual executable roles.");
            var visitor=new DispatchVisitor();
            var nodes=new[]{body.Root}.Concat(body.Root.Children.Take(8)).Concat(prefix).Distinct().ToArray();shallowNodes+=nodes.Length-prefix.Length;
            foreach(var node in nodes)
            {
                node.Accept(visitor);check(model.GetOperation(node)==node as Operation,"The selected node must retain its actual operation facade.");
                if(node is Operation operation)check(model.GetType(operation)==operation.Type,"The selected operation's semantic type must be its canonical retained descriptor.");
                if(node is CallOperation call)
                {
                    calls++;check(model.GetSymbol(call)==call.TargetMethod,"The selected call query must preserve the actual recorded callee.");
                    foreach(var argument in call.Arguments.Take(8))check(argument.Value.Type==model.GetType(argument.Value),"Typed call arguments must preserve their exact canonical expression types.");
                }
                if(node is ReferenceOperation {Target:Symbol target} reference)check(model.GetSymbol(reference)==target,"The selected reference query must preserve the exact symbol target.");
            }
            check(visitor.Nodes==nodes.Length&&visitor.Calls==nodes.OfType<CallOperation>().Count(),"The bounded named visitor must dispatch each sampled node through its common hook exactly once.");
            if(prefix.OfType<CallOperation>().FirstOrDefault(c=>c.TargetMethod is not null) is {} selectedCall)
            {
                var references=model.GetReferences(selectedCall.TargetMethod!).Take(8).ToArray();
                check(references.Length!=0&&references.All(r=>r.ReferencedSymbol==selectedCall.TargetMethod),"The selected callee reference query must retain nonempty captured uses of its artifact-scoped canonical target.");
            }
            check(function.Signature==function.Details.RoutineType&&function.ReturnType==function.Signature.ReturnType,"The selected routine signature must retain its exact callable type and return descriptor.");
            foreach(var parameter in function.Parameters.Take(8))check(parameter.Type==parameter.Descriptor.Type,"The selected signature parameter must preserve its canonical type descriptor.");
        }
        return (operations,shallowNodes,calls,truncated);
    }

    private sealed class DispatchVisitor : SemanticVisitor
    {
        internal int Nodes,Calls;
        public override void VisitNode(SemanticNode node)=>Nodes++;
        public override void VisitCall(CallOperation operation) { Calls++;base.VisitCall(operation); }
    }
}
