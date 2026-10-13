using Microsoft.Data.Sqlite;
using RoslynObj;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

if(args.FirstOrDefault()=="--benchmark") { Benchmark.Run(args.Skip(1));return; }
if(args.FirstOrDefault()=="--reject-incompatible")
{
    try { using var incompatible=RoslynObject.Open(args[1]);throw new Exception("An archived incompatible development contract was admitted."); }
    catch(IncompatibleObjectException error) { Console.WriteLine("PASS:archived contract rejected: "+error.Message);return; }
}
var withoutRowidFixtures=args.FirstOrDefault()=="--without-rowid-fixtures";
if(withoutRowidFixtures)args=args.Skip(1).ToArray();

var root=Path.Combine(Path.GetTempPath(),"roslynobj-reader-tests-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var passed=0;
void Check(bool condition,string message) { if(!condition) throw new Exception(message); Interlocked.Increment(ref passed); }
string Resource(string name)
{
    using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("RoslynObj.Tests."+name+".sql")!;
    using var reader=new StreamReader(stream);
    return reader.ReadToEnd();
}
string Fixture(string name,int extraVariables=0,int extraOperations=0,string? additionalRows=null)
{
    var path=Path.Combine(root,name+".roslynobj");
    using var db=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=path}.ToString());
    db.Open();
    using var command=db.CreateCommand();
    command.CommandText="PRAGMA foreign_keys=ON";command.ExecuteNonQuery();
    using var transaction=db.BeginTransaction();command.Transaction=transaction;
    var schema=Resource("schema");
    if(withoutRowidFixtures)
        schema=System.Text.RegularExpressions.Regex.Replace(schema,@"CREATE TABLE \w+ \(\n.*?\n\) STRICT;",match =>
        {
            var key=System.Text.RegularExpressions.Regex.Match(match.Value,@"PRIMARY KEY \(([^)]+)\)");
            return key.Success&&key.Groups[1].Value.Contains(',') ? match.Value.Replace(") STRICT;",") WITHOUT ROWID, STRICT;") : match.Value;
        },System.Text.RegularExpressions.RegexOptions.Singleline);
    command.CommandText=schema;command.ExecuteNonQuery();
    foreach(var id in new long[]{1037050,1037057,5015,5016})
    {
        var kind=RoslynObject.CatalogKind(id);
        command.CommandText="INSERT OR IGNORE INTO kind(id,family,name) VALUES($id,$family,$kind)";
        command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$family",kind.Family);command.Parameters.AddWithValue("$kind",kind.Name);
        command.ExecuteNonQuery();command.Parameters.Clear();
    }
    command.CommandText=Resource("basic").Replace("fixture-artifact",Guid.NewGuid().ToString());command.ExecuteNonQuery();
    command.CommandText="INSERT INTO text_atom(value) VALUES($name),($digest)";
    command.Parameters.AddWithValue("$name","normalized_semantic_schema_v1");
    command.Parameters.AddWithValue("$digest",RoslynObject.SchemaContractFingerprint);command.ExecuteNonQuery();
    command.CommandText="INSERT INTO build_capability(name_id,state_id,value_id,required,enabled) SELECT n.id,k.id,v.id,1,1 FROM text_atom n,text_atom v,kind k WHERE n.value=$name AND v.value=$digest AND k.family='availability' AND k.name='available'";command.ExecuteNonQuery();command.Parameters.Clear();
    for(var i=0;i<extraVariables;i++)
    {
        var id=1000+i;
        command.CommandText=$"INSERT INTO entity VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable')); INSERT INTO symbol(id,kind_id,type_id) VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable'),3); INSERT INTO entity_source_correspondence VALUES({id},707); INSERT INTO variable_symbol(id,type_id,initialization_kind_id) VALUES({id},3,(SELECT id FROM kind WHERE family='initializer' AND name='initk_none'));";
        command.ExecuteNonQuery();
    }
    for(var i=0;i<extraOperations;i++)
    {
        var statement=10000+i;var expression=20000+i;
        command.CommandText=$"INSERT INTO entity VALUES({statement},(SELECT id FROM kind WHERE family='entity' AND name='statement')); INSERT INTO entity VALUES({expression},(SELECT id FROM kind WHERE family='entity' AND name='expression')); INSERT INTO semantic_node(id,kind_id,representation_role_id) VALUES({statement},(SELECT id FROM kind WHERE family='statement' AND name='stmk_expr'),(SELECT id FROM kind WHERE family='representation_role' AND name='execution')); INSERT INTO semantic_node(id,kind_id,representation_role_id) VALUES({expression},(SELECT id FROM kind WHERE family='expression' AND name='enk_constant'),(SELECT id FROM kind WHERE family='representation_role' AND name='execution')); INSERT INTO statement(id,kind_id,expression_id,parent_id) VALUES({statement},(SELECT id FROM kind WHERE family='statement' AND name='stmk_expr'),{expression},10); INSERT INTO expression(id,type_id,value_category_id) VALUES({expression},3,(SELECT id FROM kind WHERE family='value_category' AND name='prvalue')); INSERT INTO constant_expression(id,constant_id) VALUES({expression},30); INSERT INTO structural_child VALUES(10,(SELECT id FROM kind WHERE family='role' AND name='statement'),{i+3},{statement}); INSERT INTO structural_child VALUES({statement},(SELECT id FROM kind WHERE family='role' AND name='operand'),0,{expression}); INSERT INTO block_item VALUES(10,{i+3},{statement});";
        command.ExecuteNonQuery();
    }
    if(additionalRows is not null) { command.CommandText=additionalRows;command.ExecuteNonQuery(); }
    transaction.Commit();
    command.Transaction=null;
    command.CommandText="PRAGMA foreign_key_check";
    using var checks=command.ExecuteReader();Check(!checks.Read(),"Fixture must satisfy the shared foreign keys.");
    return path;
}
try
{
    var path=Fixture("valid");
    using(var obj=RoslynObject.Open(path,new(){PageSize=2,CachedPageCount=2,CachedRowCount=32}))
    {
        var unit=obj.TranslationUnit;
        var function=unit.Functions.Single(f=>f.Name=="compute");
        Check(function.Parameters.Count==1&&function.Parameters[0].Name=="x","Retained parameter variables must navigate through the current signature.");
        var external=unit.Functions.Single(f=>f.Name=="helper");
        Check(external.Parameters.Count==1&&external.Parameters[0].Symbol is null,"Declaration-only signatures must expose anonymous parameter types.");
        var body=function.Body!;
        var operations=body.DescendantOperations().ToArray();
        var visitor=new CountingVisitor();visitor.Visit(body.Root);
        Check(visitor.Blocks==1&&visitor.Calls==1&&visitor.Jumps==1&&visitor.Operations>=operations.Length,"Named visitor hooks must dispatch through the common operation hierarchy and retain captured child traversal.");
        Check(operations.OfType<BlockOperation>().Count()==1&&operations.OfType<JumpOperation>().Count()==1,"Execution traversal must include statements.");
        Check(operations.OfType<ConstantOperation>().Count()==1,"Shared executable children must be visited once and backing metadata excluded.");
        var call=operations.OfType<CallOperation>().Single();
        Check(call.As<CallOperation>()==call&&call.As<ConstantOperation>() is null,"As helpers must expose the captured runtime façade without changing identities.");
        Check(call.TargetMethod==external&&call.SourceTargetMethod is null&&call.Receiver is null,"Call targets must follow retained relationships.");
        Check(call.Arguments.Count==1&&call.Arguments[0].Value.Type.Kind.Name=="tk_integer","Call arguments must navigate to execution types.");
        var constant=operations.OfType<ConstantOperation>().Single().Constant;
        Check(((IntegerValue)constant.Value).Value==BigInteger.Parse("340282366920938463463374607431768211455"),"Wide integers must remain exact.");
        Check(constant.BackingExpression!.RepresentationRole.Name=="metadata","Backing expression role must be preserved separately.");
        var floating=(FloatingValue)unit.Constants.Single(c=>c.Type?.Kind.Name=="tk_float").Value;
        Check(floating.Value.IsZero&&floating.Value.IsNegative,"Floating signed zero must retain its exact components.");
        var targetString=(StringValue)unit.Constants.Single(c=>c.Value is StringValue).Value;
        Check(targetString.Length==3&&targetString.CodeUnits.SequenceEqual(new uint[]{65,0,65535}),"Target string units must retain embedded NUL and non-Unicode units.");
        var type=unit.Types.OfType<ConstructedType>().Single();
        Check(type.Members.Single().Name=="value"&&type.Fields.Single().Name=="value","Members must follow retained type ownership.");
        Check(type.Layout!.Size==16&&type.Layout.Fields.Single().Offset==0,"Layouts must project target quantities.");
        Check(type.TypeArguments.Count==1&&type.OriginalDefinition!.Name=="Widget","Template original and typed arguments must navigate through objects.");
        var template=type.OriginalDefinition!;
        Check(unit.SemanticModel.FindSymbols("Widget").Contains(template.DeclarationSymbol!)&&template.DeclarationSymbol!.Kind.Name=="template_symbol"&&template.TemplateKind.Name=="templk_class","Template descriptors must navigate to their separate real symbol while retaining their template subtype.");
        Check(template.Parameters.Single() is {Kind.Name:"template_parameter",ParameterKind.Name:"tpk_type",Name:"T"},"Template parameters must use their named symbol identity alongside the exact parameter kind.");
        var region=body.ExceptionHandling.CleanupRegions.Single();
        Check(region.Ordinal==0&&region.NextRegion is null,"Actual EH region zero must remain distinct from null sentinel.");
        var state=operations.OfType<CleanupStateOperation>().Single();
        Check(state.Region==region&&state.StateNumber==0,"Cleanup states must resolve normalized regions.");
        Check(body.ExceptionHandling.Details!.NullState==BigInteger.Parse("18446744073709551615"),"Unsigned EH sentinel must remain exact beyond long.");
        var action=region.Actions.Single();
        Check(action.TargetMethod==external&&action.Array!.ElementCount==-1&&action.Storage!.Variable!.Name=="x"&&action.IsUnordered is null,"Exceptional cleanup must preserve storage, routine, dynamic counts and explicitly unknown table ordering.");
        Check(action.AbiDetail!.IsDelegating==true&&action.AbiDetail.DelegationArgumentLocation==action.Storage&&region.Condition!.Storage==action.Storage,"Typed EH supplements must navigate the recorded delegation argument location without decoding flags.");
        Check(body.Lifetimes.Count==1&&unit.Declarations.Count==0&&external.Body is null,"Optional retained collections and body absence must be ordinary values.");
        var invocation=unit.Macros.Invocations.Single();
        Check(invocation.ArgumentText=="( 4 )"&&call.Source!.MacroExpansion==invocation,"Canonical macro argument text and operation origins must be retained.");
        Check(invocation.Children.Count==0&&invocation.Position!.OriginalColumn==7,"Macro optional children and original coordinates must remain accessible.");
        Check(unit.SemanticModel.FindSymbols("helper").Single()==external,"Name search must follow exact atoms.");
        Check(unit.SemanticModel.GetReferences(external).Count==2&&unit.SemanticModel.GetReferences(external).Any(r=>r.Owner==call&&r.ReferencedSymbol==external&&r.Role==SymbolReferenceRole.CallTarget),"References must project selected calls and typed uses without duplicate generic rows.");
        Parallel.For(0,16,_=>Check(unit.Functions.Count==2,"Concurrent list reads must share the same snapshot."));
        Check(obj.Statistics.CachedRows<=32&&obj.Statistics.CachedPages<=2,"Caches must remain bounded.");
    }
    using(var references=RoslynObject.Open(path,new(){PageSize=1,CachedPageCount=2,CachedRowCount=32}))
    {
        var target=references.TranslationUnit.Functions.Single(f=>f.Name=="helper");
        var uses=references.TranslationUnit.SemanticModel.GetReferences(target).ToArray();
        Check(uses.Length==2&&uses.Select(r=>r.Role).SequenceEqual(new[]{SymbolReferenceRole.CallTarget,SymbolReferenceRole.Reference}),"Reference UNION keyset pages must preserve every use across a null role key.");
    }
    using(var batched=RoslynObject.Open(path,new(){PageSize=32,CachedPageCount=32,CachedRowCount=1024}))
    {
        var operations=batched.TranslationUnit.Functions.Single(f=>f.Name=="compute").Body!.DescendantOperations().ToArray();
        var call=operations.OfType<CallOperation>().Single();var before=batched.Statistics.Commands;
        _=call.Arguments.ToArray();
        Check(batched.Statistics.Commands-before<=2,"Typed argument navigation must reuse already hydrated positive and negative companion rows.");
    }
    using(var regional=RoslynObject.Open(Fixture("regional",additionalRows:"""
        INSERT INTO entity VALUES(160,(SELECT id FROM kind WHERE family='entity' AND name='variable'));
        INSERT INTO symbol(id,kind_id,type_id) VALUES(160,(SELECT id FROM kind WHERE family='entity' AND name='variable'),3);
        INSERT INTO entity_source_correspondence VALUES(160,707);
        INSERT INTO variable_symbol(id,type_id,initialization_kind_id) VALUES(160,3,(SELECT id FROM kind WHERE family='initializer' AND name='initk_function_local'));
        INSERT INTO entity VALUES(161,(SELECT id FROM kind WHERE family='entity' AND name='local_static_initialization'));
        INSERT INTO local_static_initialization(id,variable_id,initialization_kind_id,constant_id) VALUES(161,160,(SELECT id FROM kind WHERE family='initializer' AND name='initk_static'),30);
        INSERT INTO scope_local_static_initialization(owner_id,ordinal,initialization_id) VALUES(9,0,161);
        INSERT INTO entity VALUES(162,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol')),(163,(SELECT id FROM kind WHERE family='entity' AND name='declaration'));
        INSERT INTO symbol(id,kind_id,type_id) VALUES(162,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol'),40);
        INSERT INTO entity_source_correspondence VALUES(162,740),(163,740);
        UPDATE source_correspondence SET source_origin_id=73 WHERE id=740;
        INSERT INTO type_symbol(id,type_id) VALUES(162,40);
        UPDATE type SET symbol_id=162 WHERE id=40;
        INSERT INTO declaration(id,kind_id,symbol_id) VALUES(163,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol'),162);
        """)))
    {
        var variable=regional.TranslationUnit.Variables.Single(v=>v.InitializationKind.Name=="initk_function_local");
        Check(variable.Details.ConstantInitializer is null&&variable.ConstantInitializer==regional.TranslationUnit.Functions.Single(f=>f.Name=="compute").Body!.Scope!.LocalStaticInitializations.Single().Constant,"Local-static values must follow their exact scoped descriptor payload when the direct variable field is absent.");
        var declaration=regional.TranslationUnit.Declarations.Single();var declared=regional.TranslationUnit.Types.Single(t=>t.Name=="Widget");
        Check(regional.TranslationUnit.SemanticModel.GetSymbol(declaration)==declared.DeclarationSymbol&&regional.TranslationUnit.SemanticModel.GetType(declaration)==declared,"Declaration nodes must project the separate named symbol and actual type descriptor through ordinary semantic queries.");
        Check(declaration.NodeDetails is null&&declaration.Source?.MacroExpansion is not null,"A declaration must retain its own source record without duplicating a semantic-node row.");
        var visitor=new CountingVisitor();visitor.Visit(declaration);
        Check(visitor.Declarations==1&&declared.DeclarationSymbol!.As<TypeSymbol>() is not null,"Declaration visitor dispatch and symbol As helpers must remain ordinary navigation.");
    }
    using(var structured=RoslynObject.Open(Fixture("structured",additionalRows:"""
        INSERT INTO entity(id,kind_id) SELECT id,(SELECT id FROM kind WHERE family='entity' AND name='constant') FROM (SELECT 230 id UNION ALL SELECT 231 UNION ALL SELECT 260 UNION ALL SELECT 261);
        INSERT INTO entity VALUES(270,(SELECT id FROM kind WHERE family='entity' AND name='constant_value'));
        INSERT INTO constant_value(id,kind_id) VALUES(270,(SELECT id FROM kind WHERE family='constant' AND name='ck_aggregate'));
        INSERT INTO aggregate_value(id,type_id) VALUES(270,40);
        INSERT INTO aggregate_element(owner_id,ordinal,value_id,type_id) VALUES(270,0,31,3);
        INSERT INTO constant(id,type_id,value_id) VALUES(230,3,31),(231,3,31),(260,40,270),(261,40,270);
        INSERT INTO constant_component(owner_id,role_id,ordinal,constant_id) VALUES(260,(SELECT id FROM kind WHERE family='constant_component_role' AND name='element'),0,230),(261,(SELECT id FROM kind WHERE family='constant_component_role' AND name='element'),0,231);
        """)))
    {
        var aggregates=structured.TranslationUnit.Constants.Where(c=>c.Value is AggregateValue).ToArray();
        Check(aggregates.Length==2&&aggregates[0]!=aggregates[1]&&aggregates[0].Value==aggregates[1].Value,"Equal structured values must be shared while constant occurrences retain distinct identities.");
        Check(aggregates[0].Elements.Single()!=aggregates[1].Elements.Single()&&aggregates[0].Elements.Single().Value==aggregates[1].Elements.Single().Value,"Structured occurrence components must retain each original nested constant identity.");
        var value=(AggregateValue)aggregates[0].Value;
        Check(value.Elements.Single().Value==aggregates[0].Elements.Single().Value&&value.Elements.Single().Type==aggregates[0].Elements.Single().Type,"Shared structural elements must navigate only value contents and exact occurrence types.");
    }
    using(var obj=RoslynObject.Open(path))
    using(var other=RoslynObject.Open(Fixture("independent")))
        Check(obj.TranslationUnit.Functions[0]!=other.TranslationUnit.Functions[0],"Identical-looking translation units must retain independent identities.");
    var disposed=RoslynObject.Open(path);var lazy=disposed.TranslationUnit.Functions;disposed.Dispose();
    try { _=lazy.Count;throw new Exception("Disposed reads must fail."); } catch(ObjectDisposedException) { Interlocked.Increment(ref passed); }
    using(var large=RoslynObject.Open(Fixture("scale",1000),new(){PageSize=31,CachedPageCount=3,CachedRowCount=256}))
    {
        var sw=Stopwatch.StartNew();
        Check(large.TranslationUnit.Variables.Count==1001,"Paged count must include every variable.");
        Check(large.TranslationUnit.Variables.Count(v=>v.Name=="x")==1001,"Keyset enumeration and cache eviction must omit no rows.");
        Console.WriteLine($"scale:1001variables,{large.Statistics.Commands}commands,{sw.ElapsedMilliseconds}ms,{large.Statistics.CachedRows}cached rows,{large.Statistics.CachedPages}cached pages");
    }
    using(var entities=RoslynObject.Open(Fixture("entities",1000),new(){PageSize=31,CachedPageCount=3,CachedRowCount=256}))
    {
        var before=entities.Statistics.Commands;
        var matches=entities.TranslationUnit.Entities.OfType<VariableSymbol>().Count(v=>v.Name=="x");
        Check(matches==1001,"Full entity enumeration must hydrate the captured named variable façades without losing identities across pages.");
        Check(entities.Statistics.Commands-before<500,"Principal entity rows must be hydrated in bounded per-kind batches during full enumeration.");
    }
    var graphPath=Fixture("graph",extraOperations:1000);
    using(var graph=RoslynObject.Open(graphPath,new(){PageSize=32,CachedPageCount=256,CachedRowCount=4096}))
    {
        var before=graph.Statistics.Commands;var sw=Stopwatch.StartNew();
        var count=graph.TranslationUnit.Functions.Single(f=>f.Name=="compute").Body!.DescendantOperations().Count();
        var commands=graph.Statistics.Commands-before;
        Check(count==2008,"Batched operation traversal must omit no leaf statements or expressions.");
        Check(commands<2000,"Ordinary noncall traversal must batch missing subtype and child reads.");
        Console.WriteLine($"graph:2008operations,{commands}commands,{sw.ElapsedMilliseconds}ms");
    }
    using(var maximal=RoslynObject.Open(graphPath,new(){PageSize=4096,CachedPageCount=128,CachedRowCount=4096}))
    {
        Check(maximal.TranslationUnit.Functions.Single(f=>f.Name=="compute").Body!.DescendantOperations().Count()==2008,"Maximal page size must preserve wide and nested executable traversal.");
        Check(maximal.Statistics.CachedRows<=4096&&maximal.Statistics.CachedPages<=128,"Maximal paging must preserve explicit cache bounds.");
    }
    using(var widePrefetch=RoslynObject.Open(graphPath,new(){PageSize=1024,CachedPageCount=2048,CachedRowCount=1048576}))
    {
        Check(widePrefetch.TranslationUnit.Functions.Single(f=>f.Name=="compute").Body!.DescendantOperations().Count()==2008,"Large valid cache budgets must preserve all children while bounded prefetch splits a wide page.");
        Check(widePrefetch.Statistics.CachedRows<=1048576&&widePrefetch.Statistics.CachedPages<=2048,"Wide prefetch must preserve the explicitly selected row and page bounds.");
        Check(widePrefetch.Statistics.MaximumBatchSize==900,"A wide page with generous valid cache budgets must exercise the child prefetch limit of 900 keys per query.");
    }
    if(args.FirstOrDefault()=="--workspace")
    {
        if(args.Length!=3)throw new ArgumentException("Workspace acceptance requires two genuine basic artifacts.");
        WorkspaceProjection.Verify(args[1],args[2],root,Check);args=[];
    }
    foreach(var input in args)
    {
        var sw=Stopwatch.StartNew();
        using var obj=RoslynObject.Open(input);
        var unit=obj.TranslationUnit;
        ProfileProjection.Verify(unit,Path.GetFileName(input),Check);
        TablelessEhProjection.Verify(unit,Path.GetFileName(input),Check);
        Numeric.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        ConstantReferences.Verify(unit,input,obj.Manifest.CaptureProfile=="source_semantic",Check);
        CommonProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        SemanticProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        SemanticActivationProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        ScopeRelationsProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        RoutineProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        LayoutProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        ExecutionProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        ExecutionMetadataProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        ScalarFamilies.Verify(unit,Path.GetFileName(input),Check);
        RetainedTypeProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        ClassRelationsProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        ConstantRelationsProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",obj.Manifest.CaptureProfile,Check);
        StatementRelationsProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",obj.Manifest.CaptureProfile,Check);
        PragmaScopeProjection.Verify(unit,Path.GetFileName(input),Check);
        DeclarationTemplateProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        RetainedBranchesProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        RoutineEhInitializationProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        DependentConstantProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        AvailabilityOrphanDerivationProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        PrototypeUuidProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile=="source_semantic",Check);
        ExpressionIdentityProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        ListControlIdentityProjection.Verify(unit,Path.GetFileName(input),obj.Manifest.CaptureProfile,Check);
        UnevaluatedProjection.Verify(unit,Path.GetFileName(input),Check);
        NavigationFamilies.Verify(unit,Path.GetFileName(input),Check);
        var functionCount=0;var operationCount=0;var executableBodyCount=0;var callCount=0;var cleanupActions=0;var targets=new List<string?>();
        foreach(var function in unit.Functions)
        {
            _=function.Name;_=function.ReturnType.Kind;
            foreach(var parameter in function.Parameters) _=parameter.Type.Kind;
            functionCount++;
            if(function.Body is not {} body)continue;
            if(body.Root.RepresentationRole.Name!="metadata")executableBodyCount++;
            foreach(var operation in body.DescendantOperations())
            {
                operationCount++;
                if(operation is ExpressionOperation expression) _=expression.Type.Kind;
                if(operation is CallOperation call) { callCount++;targets.Add(call.TargetMethod?.Name);foreach(var argument in call.Arguments) _=argument.Value.Type.Kind; }
            }
            foreach(var region in body.ExceptionHandling.CleanupRegions)
                foreach(var action in region.Actions) { cleanupActions++;_=action.Kind;_=action.TargetMethod?.Name;_=action.Storage?.Offset; }
        }
        Check(unit.Entities.Count>0&&(executableBodyCount==0||operationCount>0),"A real EDG object must expose captured entities and present executable bodies; metadata-only prototypes do not supply executable operations.");
        if(Path.GetFileName(input)=="basic.roslynobj")
        {
            Check(targets.Contains("add"),"The real basic initializer must expose its EDG-selected add call in executable containment.");
            var add=unit.Functions.Single(f=>f.Name=="add");
            Check(unit.SemanticModel.GetReferences(add).Any(r=>r.Role==SymbolReferenceRole.CallTarget&&r.Owner is CallOperation),"The real selected add call must appear in ordinary symbol references.");
            var main=unit.Functions.Single(f=>f.Name=="main");
            Check(unit.EntryPoint==main,"The real entry-point edge must navigate EDG's retained routine identity.");
            var local=main.Body!.Locals.Single(v=>v.Name=="x");
            Check(unit.SemanticModel.GetReferences(local).Count>0,"Real variable references must project typed reference-expression facts.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="SUM");
            Check(macro.Text=="#define SUM(x,y) ((x)+(y))","The real macro definition must retain EDG's text.");
            if(unit.SemanticModel.GetAvailability(unit).Any(a=>a.Property=="macro_arguments"&&a.State==AvailabilityState.Available))
                Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="a, b"),"The real SUM invocation must retain canonical arguments and definition identity when the build retains them.");
            else
                Check(unit.Macros.Invocations.All(m=>m.ArgumentText is null),"A build without retained macro arguments must expose their explicit absence without fabricating text.");
        }
        if(Path.GetFileName(input)=="m0.roslynobj")
        {
            var declaredBase=(RecordType)unit.Types.Single(t=>t.Name=="Base");
            Check(declaredBase.DeclaredVirtualMethods.Any(f=>f.Name=="get")&&declaredBase.DeclaredVirtualMethods.All(f=>f.ContainingType==declaredBase&&f.IsVirtual),"Declared virtual method navigation must expose retained routines owned by the actual class without inventing table slots or inherited declarations.");
            Check(cleanupActions==(unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==true)?6:0),"The real C++ object must preserve its profile's actual exceptional cleanup tables.");
            if(obj.Manifest.CaptureProfile!="source_semantic")
                Check(unit.Functions.Where(f=>f.ContainingType is not null).SelectMany(f=>f.Parameters).Any(p=>p.Role.Name=="this_parameter"),"Lowered retained method signatures must expose their explicit receiver roles.");
            else
                Check(unit.Functions.Where(f=>f.ContainingType is not null).SelectMany(f=>f.Parameters).All(p=>p.Role.Name!="this_parameter"),"Source method signatures must preserve their actual formal list before implicit receiver parameters are added by lowering.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="INC");
            Check(macro.Text=="#define INC(v) ((v)+1)","The real INC macro must retain definition text.");
            if(unit.SemanticModel.GetAvailability(unit).Any(a=>a.Property=="macro_arguments"&&a.State==AvailabilityState.Available))
                Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="y"),"The real INC invocation must retain its argument text and definition identity when the profile retains arguments.");
            else
                Check(unit.Macros.Invocations.All(m=>m.ArgumentText is null),"Profiles without retained macro arguments must preserve their explicit absence.");
            using var paged=RoslynObject.Open(input,new(){PageSize=1,CachedPageCount=2,CachedRowCount=32});
            var parameter=paged.TranslationUnit.Functions.Single(f=>f.Name=="use").SourceParameters.Single(p=>p.Name=="n").Symbol!;
            var uses=paged.TranslationUnit.SemanticModel.GetReferences(parameter);
            Check(uses.Count>=3&&uses.ToArray().Length==uses.Count&&uses.All(r=>r.ReferencedSymbol==parameter),"Real symbol references must enumerate every captured use across multiple UNION pages.");
            var field=unit.Types.Where(t=>t.Name=="Base").SelectMany(t=>t.Fields).First(f=>f.Name=="x");
            Check(unit.SemanticModel.GetReferences(field).Any(r=>r.Owner is ReferenceOperation),"Real field uses must follow the typed reference-expression target.");
            if(obj.Manifest.CaptureProfile=="cfront_lowered")
            {
                var parameters=unit.Functions.SelectMany(f=>f.Parameters).ToArray();
                Check(parameters.Count(p=>p.Role.Name=="destructor_control_parameter")==2&&parameters.Count(p=>p.Role.Name=="virtual_base_parameter")==2,"Cfront current signatures must retain explicit destructor-control and virtual-base roles.");
                Check(parameters.Where(p=>p.Role.Name=="virtual_base_parameter").All(p=>p.BaseSubobject is not null)&&parameters.Where(p=>p.Role.Name=="this_parameter").All(p=>p.Type is PointerType),"Cfront parameter façades must retain their exact base-subobject relationships and receiver types.");
            }
        }
        if(Path.GetFileName(input)=="exact.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.ConstantInitializer is not null).ToDictionary(v=>v.Name!,StringComparer.Ordinal);
            Check(((IntegerValue)variables["wide"].ConstantInitializer!.Value).Value==(BigInteger.One<<127)+17,"The real unsigned128-bit global must preserve every bit.");
            var zero=((FloatingValue)variables["negative_zero"].ConstantInitializer!.Value).Value;
            Check(zero.IsZero&&zero.IsNegative,"The real long-double negative zero must retain its sign.");
            var finite=((FloatingValue)variables["finite"].ConstantInitializer!.Value).Value;
            var binary64=((FloatingValue)variables["finite"].ConstantInitializer!.Value).Details.RepresentationKind?.Name=="binary64";
            Check(finite.Radix==2&&(binary64 ? finite.Significand==(BigInteger.One<<52)&&finite.Exponent==-38 : finite.Significand==(BigInteger.One<<63)+8&&finite.Exponent==-49),"The real long-double literal must preserve its configured target significand/exponent and rounding.");
            Check(((FloatingValue)variables["infinity"].ConstantInitializer!.Value).Value.Class==FloatingPointClass.Infinity,"The real infinity must retain its classification.");
            var nan=((FloatingValue)variables["nan_value"].ConstantInitializer!.Value).Value;
            Check(nan.Class==FloatingPointClass.NaN&&nan.NaNPayload==801&&nan.IsQuietNaN==true,"The real NaN must retain its payload and quiet state.");
            var first=variables["a"].ConstantInitializer!;var second=variables["b"].ConstantInitializer!;
            Check(first!=second&&first.Value==second.Value,"Identical target strings must share value without merging occurrence identities.");
            Check(((StringValue)first.Value).TargetBytes.SequenceEqual(new byte[]{97,98,0,99,100,0}),"The real narrow string must retain internal and trailing NUL bytes.");
            Check(((StringValue)variables["wide_text"].ConstantInitializer!.Value).TargetBytes.SequenceEqual(new byte[]{97,0,0,0,187,3,0,0}),"The real char16 string must preserve target byte order and embedded NUL.");
        }
        if(Path.GetFileName(input)=="float_kinds.roslynobj")
        {
            var values=unit.Variables.Where(v=>v.ConstantInitializer is not null).ToDictionary(v=>v.Name!,v=>(FloatingValue)v.ConstantInitializer!.Value);
            Check(new[]{"negative_zero_long","negative_zero_float","negative_zero_double","narrow_negative_zero"}.All(n=>values[n].Value is {IsZero:true,IsNegative:true}),"Signed zeros must preserve their sign across every retained target float kind.");
            var extended=values["extended_finite"].Value;var wide=values["wide_finite"].Value;var narrow=values["narrow_finite"].Value;
            Check(extended.Significand==(BigInteger.One<<63)+1&&extended.Exponent==-63&&wide.Significand==(BigInteger.One<<112)+1&&wide.Exponent==-112&&narrow.Significand==1025&&narrow.Exponent==-10,"Extended, quad and half target floats must preserve exact significands and exponents without host rounding.");
        }
        if(Path.GetFileName(input)=="bit_precise.roslynobj")
        {
            var nine=(ArithmeticType)unit.Types.OfType<TypeReference>().Single(t=>t.Name=="SignedNine").UnderlyingType;
            var wide=(ArithmeticType)unit.Types.OfType<TypeReference>().Single(t=>t.Name=="UnsignedWide").UnderlyingType;
            Check(nine.Arithmetic is {Precision:9,Bits:16,Signed:true}&&wide.Arithmetic is {Precision:129,Bits:136,Signed:false},"Real bit-precise types must preserve semantic precision separately from allocated target storage bits.");
            Check(((IntegerValue)unit.Variables.Single(v=>v.Name=="nine_value").ConstantInitializer!.Value).Value==-7&&((IntegerValue)unit.Variables.Single(v=>v.Name=="wide_value").ConstantInitializer!.Value).Value==4660,"Real signed and unsigned bit-precise constants must preserve exact values.");
        }
        if(Path.GetFileName(input)=="exported.roslynobj")
        {
            var template=unit.Templates.Single(t=>t.Name=="exported_add");
            Check(template.Details.IsExported&&!template.Details.IgnoreExport&&unit.SemanticModel.FindSymbols("exported_add").Contains(template),"An actual legacy exported-template profile must retain export flags through the common template symbol identity.");
            Check(unit.Functions.Any(f=>f.Name=="exported_add"&&f.Instantiation is {IsPrototype:false}&&f.Body is not null)&&targets.Contains("exported_add"),"Actual exported template instances must retain selected calls and executable definitions.");
        }
        if(Path.GetFileName(input)=="macros.roslynobj")
        {
            var definitions=unit.Macros.Definitions.Where(d=>d.Name=="INC").ToArray();
            Check(definitions.Length==3&&definitions.Count(d=>d.Details.IsUndef)==1,"Define/undef/redefine must retain separate definition identities.");
            var invocations=unit.Macros.Invocations.ToArray();
            var nested=invocations.Single(i=>i.Definition.Name=="INNER");
            Check(nested.Parent?.Definition.Name=="OUTER"&&nested.Parent.Parent?.Definition.Name=="INC"&&nested.Parent.Parent.Parent is null,"The real macro expansion parent chain must retain every nested invocation.");
            Check(invocations.All(i=>i.ArgumentText=="x"),"The real nested macro invocations must retain canonical argument text.");
            var source=unit.Functions.Single(f=>f.Name=="second").Source!.Position!;
            Check(source.PresumedName=="logical.cpp"&&source.LogicalLine==200,"The real #line mapping must preserve presumed filename and logical line separately from the physical file.");
        }
        if(Path.GetFileName(input)=="templates.roslynobj")
        {
            var holder=unit.Templates.Single(t=>t.Name=="Holder");
            var header=holder.Headers.Single();
            Check(header.Template==holder.Descriptor&&header.Parameters.Select(p=>p.Name).SequenceEqual(new[]{"T","N"}),"Real template headers must preserve owning descriptors and parameter order.");
            var typeParameter=header.Parameters[0];var valueParameter=header.Parameters[1];
            Check(typeParameter.PlaceholderType!.DeclaredParameters.Contains(typeParameter)&&typeParameter.PlaceholderType.DeclarationSymbol==typeParameter,"A type placeholder must resolve to its actual parameter symbol without a second type declaration identity.");
            Check(valueParameter.PlaceholderConstant!.DeclaredParameters.Contains(valueParameter)&&valueParameter.PlaceholderConstant.DeclarationSymbol==valueParameter,"A non-type placeholder must resolve through the exact constant occurrence rather than its value or coordinates.");
            Check(typeParameter.PlaceholderType is DependentType&&valueParameter.PlaceholderConstant?.Value is ConstantValue<DependentValueDetails>,"Real template parameters must retain their type and value placeholders.");
            Check(((IntegerValue)valueParameter.DefaultArgument!.Constant!.Value).Value==3,"Real non-type template defaults must preserve the exact selected constant.");
            Check(holder.OriginalDefinition==holder.Descriptor&&holder.Definition==holder.Descriptor&&holder.Pattern is RecordType,"Real canonical and definition links must retain EDG descriptor identity.");
            var alias=unit.Types.OfType<TypeReference>().Single(t=>t.Name=="Alias"&&t.Instantiation?.IsDependent==false);
            Check(alias.Template?.Name=="Alias"&&alias.TypeArguments.Single().Kind.Name=="tk_integer"&&alias.UnderlyingType.Template?.Name=="Holder","Real alias instances must expose their own arguments and exact underlying class template.");
            var value=unit.Variables.Single(v=>v.Name=="shared_value"&&v.Instantiation?.IsPrototype==false);
            Check(value.Template?.Name=="shared_value"&&((IntegerValue)value.ConstantInitializer!.Value).Value==42,"Real variable-template instances must retain the template and exact selected value.");
            var specialization=unit.Functions.Single(f=>f.Name=="identity"&&f.Instantiation?.IsSpecialized==true);
            Check(specialization.TemplateArguments.Single().Type?.Kind.Name=="tk_integer"&&specialization.Template?.Name=="identity","Real specialized functions must preserve retained primary arguments and template relationships.");
            Check(specialization.Instantiation!.SelectedSpecialization?.Entity==specialization&&specialization.Instantiation.SelectedSpecialization.Kind.Name=="retained_specialization","Real retained specialization rows must preserve the exact specialized entity without assigning unrecorded source syntax.");
            var local=unit.Types.Single(t=>t.Name=="Local");var method=local.Methods.Single(f=>f.Name=="read");
            Check(local.Members.Contains(method)&&local.Members.OfType<FieldSymbol>().Any(f=>f.Name=="value"),"Real type members must include authoritative retained method and field owners.");
            Check(unit.Functions.Single(f=>f.Name=="local_class_owner").Body!.Locals.All(v=>!method.Parameters.Any(p=>p.Symbol==v)),"Body locals must stop at nested local-class method scopes.");
            var localStatic=unit.Functions.Single(f=>f.Name=="scalar_static").Body!.Locals.Single(v=>v.Name=="retained_scalar");
            Check(((IntegerValue)localStatic.ConstantInitializer!.Value).Value==42,"Real local-static scalar initialization must remain available through ordinary properties.");
        }
        if(Path.GetFileName(input)=="attributes.roslynobj")
        {
            var type=unit.Types.Single(t=>t.Name=="Aligned");Symbol symbol=type.DeclarationSymbol!;
            Check(symbol.Attributes.Count>0&&symbol.Attributes.All(a=>a.Owners.Contains(type)&&a.Owners.Contains(symbol))&&symbol.Attributes.Select(a=>a.Kind.Name).SequenceEqual(type.Attributes.Select(a=>a.Kind.Name)),"Named class attributes must navigate the authoritative descriptor rows through the common Symbol façade.");
        }
        if(Path.GetFileName(input)=="types.roslynobj")
        {
            var colour=(EnumSymbol)unit.Types.Single(t=>t.Name=="Colour").DeclarationSymbol!;
            Check(colour.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"red","blue"}),"Real enum members must expose separate named symbols in retained order.");
            var red=(EnumeratorSymbol)unit.SemanticModel.FindSymbols("red").Single();
            Check(((IntegerValue)red.Value).Value==2&&red.Constant==colour.Enumerators[0].Constant,"Real enum symbols must retain their exact constant relationship and value.");
            var declaration=red.Declarations.First();
            Check(unit.SemanticModel.GetSymbol(declaration)==red&&unit.SemanticModel.GetType(declaration)==red.DeclaredType,"Real enumerator declarations must resolve their named symbol and captured type.");
            var uses=unit.SemanticModel.GetReferences(red).ToArray();
            Check(uses.Length>0&&uses.All(r=>r.Owner is ConstantOperation constant&&unit.SemanticModel.GetSymbol(constant)==red),"Real folded enum uses must resolve symbols through actual constant identities.");
            var status=(EnumSymbol)unit.Types.Single(t=>t.Name=="Status").DeclarationSymbol!;
            Check(status.UnderlyingType?.Size==2&&status.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"ready","done"}),"A real scoped enum must retain its explicit underlying type and named members.");
            var integer=(AliasSymbol)unit.Types.Single(t=>t.Name=="Integer").DeclarationSymbol!;
            var count=(AliasSymbol)unit.Types.Single(t=>t.Name=="Count").DeclarationSymbol!;
            Check(integer.Type.Name=="Integer"&&integer.UnderlyingType?.Kind.Name=="tk_integer"&&count.UnderlyingType?.Kind.Name=="tk_integer","Real typedef and using aliases must keep descriptor identities and explicit underlying-type links.");
            BigInteger Quantity(string name) => ((IntegerValue)unit.Variables.Single(v=>v.Name==name).ConstantInitializer!.Value).Value;
            Check(integer.Type.Size==Quantity("integer_size")&&integer.Type.Alignment==Quantity("integer_alignment"),"Ordinary alias size/alignment must match EDG's own sizeof/alignof constants.");
            var aligned=unit.Types.Single(t=>t.Name=="AlignedInteger");
            Check(aligned.Size==Quantity("aligned_integer_size")&&aligned.Alignment==Quantity("aligned_integer_alignment"),"Explicit aligned typedefs must preserve effective target alignment without blind underlying-type projection.");
            var qualified=unit.Types.Single(t=>t.Name=="QualifiedInteger");
            Check(qualified.Size==Quantity("qualified_integer_size")&&qualified.DescriptorSize!=qualified.Size,"Qualified aliases must expose effective size alongside the retained descriptor quantity.");
            Check(colour.Attributes.Any(a=>a.Kind.Name=="ak_deprecated"&&a.Owners.Contains(colour.Type)&&a.Owners.Contains(colour))&&integer.Attributes.Any(a=>a.Kind.Name=="ak_deprecated"&&a.Owners.Contains(integer.Type)&&a.Owners.Contains(integer)),"Named enum and typedef attributes must retain their descriptor authority through symbol navigation.");
            var operands=unit.Entities.OfType<TypeOperandOperation>().ToArray();
            Check(operands.Length>0&&operands.All(o=>unit.SemanticModel.GetSymbol(o)==o.OperandType?.DeclarationSymbol),"Retained type operand symbol queries must follow only the actual descriptor-symbol relationship.");
            Check(operands.Where(o=>o.OperandType?.DeclarationSymbol is not null).All(o=>unit.SemanticModel.GetReferences(o.OperandType!.DeclarationSymbol!).Any(r=>r.Owner==o)),"Named type descriptor uses must appear in ordinary symbol references.");
        }
        if(Path.GetFileName(input)=="template_edges.roslynobj")
        {
            var populated=unit.Variables.Single(v=>v.Name=="populated_pack").Type.Instantiation!;
            var pack=populated.Arguments.Single();
            Check(pack.IsPack&&pack.Elements.Select(a=>a.Type!.Size).SequenceEqual(new BigInteger?[]{4,8,1}),"A real type argument pack must expose ordered retained element types.");
            Check(pack.Kind.Name=="tak_start_of_pack_expansion"&&!pack.Details.IsPack,"The actual expansion marker must preserve its false retained is_pack bit independently from the public pack convenience.");
            Check(unit.Variables.Single(v=>v.Name=="empty_pack").Type.Instantiation!.Arguments.Single().Elements.Count==0,"A real empty argument pack must remain an empty pack rather than an absent argument.");
            var selected=unit.Variables.Single(v=>v.Name=="selected_pointer").Type.Instantiation!;
            Check(selected.ArgumentMappings.Select(a=>a.Role.Name).SequenceEqual(new[]{"primary","selected_partial"})&&selected.ArgumentMappings[0].Argument.Type is PointerType&&selected.ArgumentMappings[1].Argument.Type?.Kind.Name=="tk_integer","Selected partial arguments must preserve their own map alongside the primary arguments.");
            Check(unit.Variables.Single(v=>v.Name=="rebound").Type.Instantiation!.Arguments[0].Template?.Name=="ParameterBase","A real template-template argument must retain its definition identity.");
            var templateParameter=unit.Templates.Single(t=>t.Name=="Rebound").Parameters.Single(p=>p.Name=="C");
            var templatePlaceholder=templateParameter.PlaceholderTemplate!;
            Check(templatePlaceholder.TemplateKind.Name=="templk_template_template_param"&&templatePlaceholder.DeclaredParameter==templateParameter&&templatePlaceholder.DeclarationSymbols.Single()==templateParameter,"A template-template placeholder must remain a descriptor of its real parameter without a phantom template symbol.");
            Check(unit.TemplateDescriptors.Where(d=>d.TemplateKind.Name=="templk_template_template_param").All(d=>d.DeclarationSymbols.All(s=>s is TemplateParameter)),"Every retained template-template placeholder association must name actual parameter declarations.");
            var folded=unit.Variables.Single(v=>v.Name=="folded_sum"&&v.Instantiation?.IsPrototype==false);
            Check(folded.Instantiation!.Arguments.Single().Elements.Select(a=>((IntegerValue)a.Constant!.Value).Value).SequenceEqual(new BigInteger[]{1,2,3}),"A real non-type pack must preserve each exact argument in source order.");
            var descriptors=unit.Entities.OfType<DependentOperation>().ToArray();
            var requires=descriptors.Single(o=>o.Kind.Name=="enk_requires"&&o.Parameters.Count==2);
            Check(requires.Parameters.Select(p=>p.Name).SequenceEqual(new[]{"left","right"})&&requires.Parameters.All(p=>p.Descriptor.Details.IsRequiresExpressionParameter),"Requires parameters must use their retained canonical parameter descriptors.");
            var compound=requires.Requirements[0].Expression.As<DependentOperation>()!;
            Check(compound.Kind.Name=="enk_compound_req"&&compound.NoexceptRequired==true&&compound.WrappedExpression is not null&&requires.Requirements[1].Expression.Kind.Name=="enk_nested_req","Real requires expressions must preserve ordered compound and nested requirements with noexcept facts.");
            Check(descriptors.Any(o=>o.Kind.Name=="enk_concept_id"&&o.Template?.Name=="Addable"&&o.TemplateArguments.Count==1)&&descriptors.Any(o=>o.Kind.Name=="enk_sizeof_pack"&&o.PackType is not null),"Concept arguments and sizeof-pack operands must remain available as typed retained metadata.");
            var concept=unit.Templates.Single(t=>t.Name=="Addable");
            Check(unit.SemanticModel.FindSymbols("Addable").Contains(concept)&&descriptors.Where(o=>o.Template==concept.Descriptor).All(o=>unit.SemanticModel.GetSymbol(o)==concept)&&unit.SemanticModel.GetReferences(concept).Any(r=>r.Owner is DependentOperation),"Retained concept expressions must resolve and reference the separate actual template symbol identity.");
            Check(descriptors.Where(o=>o.Kind.Name=="enk_fold").All(o=>o.FoldOperator?.Name=="tok_plus"&&o.Dependency.FoldLeftAssociative==false),"Fold operators must retain the exact token and associativity.");
            var lambda=unit.Entities.OfType<LambdaOperation>().Single();
            Check(lambda.CallOperator is not null&&lambda.ClosureType is RecordType&&lambda.Captures.Single() is {IsPack:true,IsInitCapture:true,Initialization:not null},"A real retained pack lambda must navigate its closure, callable and initialization capture.");
            var prototype=unit.Functions.Single(f=>f.Name=="local_copy"&&f.Instantiation?.IsPrototype==true);
            var captured=Captured(prototype.Body!.Root).ToArray();
            Check(captured.OfType<InitializationOperation>().Any()&&captured.OfType<ReferenceOperation>().Any()&&captured.All(n=>n.RepresentationRole.Name=="metadata"),"A retained prototype initializer and its actual children must preserve metadata roles throughout the captured graph.");
            Check(!prototype.Body.DescendantOperations().Any(),"Executable traversal must omit a metadata-only template prototype.");
            var packFunction=unit.Functions.Single(f=>f.Name=="captured_pack"&&f.Instantiation?.IsPrototype==true);
            var expanded=unit.Functions.Single(f=>f.Name=="captured_pack"&&f.Instantiation?.IsPrototype==false);
            Check(packFunction.Parameters.Single().Descriptor.IsPack&&expanded.Parameters.Count==3&&expanded.Parameters.All(p=>p.Descriptor.IsPackElement&&p.Descriptor.ParameterNumber==1)&&expanded.Parameters.Select(p=>p.Ordinal).SequenceEqual(new long[]{0,1,2}),"Actual parameter packs must preserve one source parameter number across three distinct expanded signature ordinals.");
        }
        if(Path.GetFileName(input)=="source_order.roslynobj")
        {
            var entries=unit.GlobalScope.SourceEntries.ToArray();
            Check(entries.Length>0&&entries.All(e=>e.SourceSequenceOwner==unit.GlobalScope),"A file source sequence must retain its actual owner through ordinary logical enumeration.");
            var repeated=unit.Functions.Single(f=>f.Name=="repeated");
            var occurrences=entries.Where(e=>e.DeclaredSymbol==repeated).ToArray();
            Check(occurrences.Length>=2&&occurrences.Any(e=>e.Declaration?.Secondary is not null),"Repeated source declarations must preserve separate actual secondary occurrences of the same symbol.");
            var secondary=occurrences.Select(e=>e.Declaration).First(d=>d?.Secondary is not null)!;
            Check(secondary.Source?.Position is not null&&secondary.Source!=repeated.Source&&secondary.LexicalScope is null,"Secondary source and list ownership must not be substituted with the primary declaration's source or lexical scope.");
            Check(secondary.Positions?.IdentifierRange is not null&&secondary.Positions.DeclaratorRange is not null,"Real secondary declarations must retain their own identifier and declarator ranges.");
            var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="Sequence");
            var members=record.SourceDeclarations.ToArray();
            Check(members.Select(m=>m.DeclaredSymbol?.Name).SequenceEqual(new string?[]{"Nested","first","static_value","source_friend","second","Nested","Alias",null,"method"}),"Class occurrence order must include friends and repeated declarations while excluding implicit members.");
            Check(members[0]!=members[5]&&members[0].Target==members[5].Target&&members.Where(m=>m.Tag.Name!="iek_src_seq_secondary_decl").All(m=>m.Declaration is null),"Positionless member-list nodes must retain distinct identities without assigning the target's primary declaration to each occurrence.");
            var local=unit.Functions.Single(f=>f.Name=="local_entries").Body!.Scope!;
            var localEntries=local.SourceEntries.ToArray();
            string LocalTarget(SourceEntry entry) => entry.DeclaredSymbol?.Name ?? entry.Target switch
            {
                StatementOperation statement=>statement.Kind.Name,
                SourceEndConstruct end=>"end:"+(end.Target switch {CppType type=>type.Name,StatementOperation statement=>statement.Kind.Name,_=>null}),
                _=>entry.Tag.Name
            };
            Check(local.SourceSublists.Count>0&&local.SourceSequence.Any(e=>e.Target is SourceSublist)&&localEntries.Select(LocalTarget).SequenceEqual(new[]{"stmk_block","stmk_decl","external_local","stmk_decl","Local","member","end:Local","stmk_decl","local","stmk_return","end:stmk_block"})&&localEntries.All(e=>e.SourceSequenceOwners.Contains(local)),"Logical source enumeration must preserve exact local target order and owning function scope across real file-region sublist splices.");
            Check(entries.Select(e=>e.Target).OfType<InstantiationDirective>().Any(d=>d.DoNotInstantiate)&&entries.Select(e=>e.Target).OfType<InstantiationDirective>().Any(d=>!d.DoNotInstantiate),"Explicit and extern template instantiation directives must retain separate typed occurrences.");
            Check(entries.Select(e=>e.Target).OfType<LinkageSpecification>().Single().NameLinkage.Name=="nlk_external","A real braced linkage specification must preserve its exact linkage kind and string constant.");
            Check(unit.Pragmas.Any(p=>p.Standard is {Kind.Name:"stdc_pk_fp_contract",Value.Name:"stdc_pv_on"}),"Resident STDC pragma union fields must navigate as exact typed kind and value metadata.");
        }
        if(Path.GetFileName(input)=="provenance.roslynobj")
        {
            var files=unit.Sources.ToArray();var includes=files.SelectMany(f=>f.IncludeOccurrences).ToArray();
            var mappings=unit.SourceMappings.ToArray();var remaps=unit.Entities.OfType<LineDirective>().ToArray();
            Check(files.Length==8&&includes.Length==9&&remaps.Length==2&&mappings.Length==17,"The real source descriptor graph must preserve canonical physical files, repeated occurrences and distinct remap segments.");
            var repeated=files.Single(f=>Path.GetFileName(f.Path)=="provenance_repeat.hpp");
            Check(repeated.IncludeOccurrences.Count==2&&repeated.IncludeOccurrences[0]!=repeated.IncludeOccurrences[1]&&repeated.IncludeOccurrences.All(i=>i.File==repeated)&&repeated.IncludeOccurrences.Select(i=>i.Ordinal).SequenceEqual(new long[]{5,6}),"Repeated includes must share one physical source file while retaining distinct occurrences and source ordinals.");
            var nested=unit.Variables.Single(v=>v.Name=="nested_presumed_line").Source!.Position!;
            var translation=unit.Variables.Single(v=>v.Name=="remapped_value").Source!.Position!;
            Check(nested.PresumedName=="presumed_nested.hpp"&&nested.LogicalLine==400&&nested.PhysicalLine==3&&Path.GetFileName(nested.File!.Path)=="provenance_nested.hpp"&&nested.Mapping!.LineDirective!.PresumedName==nested.PresumedName,"A nested remap must preserve its presumed line and name independently from the actual physical source file and line.");
            Check(translation.PresumedName=="presumed_translation.cpp"&&translation.LogicalLine==900&&translation.PhysicalLine==11&&translation.File==unit.PrimaryInput&&translation.Mapping!.Include==translation.Include,"Translation-unit remaps must retain the actual include and mapping relationship at each source position.");
            var angled=includes.Single(i=>i.NameAsWritten=="angled.hpp");var quoted=includes.Single(i=>i.NameAsWritten=="quoted.hpp");
            Check(angled.UsesAngleBrackets&&!angled.IsSystem&&!quoted.UsesAngleBrackets&&quoted.IsSystem,"Include token spelling and system-directory provenance must remain separate captured facts.");
            Check(includes.Single(i=>Path.GetFileName(i.File.Path)=="provenance_macros.hpp") is {IsPreinclude:true,IsMacrosOnly:true}&&includes.Single(i=>Path.GetFileName(i.File.Path)=="provenance_preinclude.hpp") is {IsPreinclude:true,IsMacrosOnly:false}&&!unit.Variables.Any(v=>v.Name=="macros_only_declaration_must_be_ignored"),"Preinclude and macros-only descriptors must preserve their distinct flags and the actual retained declarations.");
            var zero=unit.Macros.Definitions.Where(m=>m.Position?.Sequence==0).ToArray();
            Check(zero.Length>0&&zero.All(m=>m.Position!.File is null&&m.Position.Mapping is null&&m.Position.LogicalLine is null&&m.Position.PhysicalLine is null),"Actual sequence-zero macro origins must remain unknown rather than acquiring an arbitrary physical source association.");
        }
        if(Path.GetFileName(input)=="type_edges.roslynobj")
        {
            var alias=unit.Types.OfType<TypeReference>().Single(t=>t.Name=="ForwardAlias");
            Check(alias.IsIncomplete&&alias.DescriptorIsIncomplete==false&&alias.UnderlyingType.IsIncomplete,"Effective incompleteness must follow retained EDG type semantics while preserving a differing raw descriptor flag.");
            var pointer=(PointerType)unit.Types.OfType<TypeReference>().Single(t=>t.Name=="ForwardPointer").UnderlyingType;
            Check(!pointer.IsIncomplete&&pointer.PointeeType.IsIncomplete,"A complete pointer descriptor must retain its incomplete pointed-to record separately.");
            Check(unit.Types.Single(t=>t.Name=="AliasingInteger").Details.MayAlias,"A real GNU may_alias typedef must retain its actual descriptor flag.");
            var packed=unit.Types.OfType<RecordType>().Single(t=>t.Name=="PackedRecord");
            Check(packed.Record.IsPacked&&packed.Record.MaximumMemberAlignment==1&&packed.Layout!.Fields.Select(f=>f.Offset).SequenceEqual(new BigInteger?[]{0,1}),"Packed-record flags and actual target member offsets must remain typed captured quantities.");
            var small=(EnumSymbol)unit.Types.Single(t=>t.Name=="SmallEnum").DeclarationSymbol!;
            Check(small.Details.IsPacked&&small.UnderlyingType is ArithmeticType {Arithmetic.Bits:var bits}&&bits==8,"A packed enum must retain both its packed fact and EDG's effective underlying integer descriptor.");
            var unsigned=(EnumSymbol)unit.Types.Single(t=>t.Name=="UnsignedUnderlying").DeclarationSymbol!;
            Check(!unsigned.HasExplicitBase&&unsigned.ExplicitUnderlyingType is null&&unsigned.Details.UnderlyingTypeShouldUseUnsigned&&unsigned.UnderlyingType is ArithmeticType {Arithmetic.Signed:false},"Implicit unsigned enum underlying types must remain distinct from an explicitly written base type.");
            Check(unit.Types.Single(t=>t.Name=="TaggedRecord").Details.InGnuAbiTagNamespace,"The actual ABI-tag namespace association must remain available on its record descriptor.");
            var exceptionPointer=unit.Types.OfType<PointerType>().Single(t=>t.Details.UsedInExceptionOrRtti&&t.PointeeType.Name=="PolymorphicType");
            Check(exceptionPointer.PointeeType is RecordType,"The retained exception pointer must navigate its actual pointed-to record in each profile.");
            if(obj.Manifest.Stage=="lowered")Check(exceptionPointer.PointeeType.Details.TypeinfoVariable is not null,"The lowered profile must retain its actual synthesized record typeinfo variable.");
            Check(unit.Types.OfType<ArrayType>().Any(t=>t.Array.IsTemplateDependent&&t.Array.DependentBoundConstant is not null),"A dependent array must preserve its actual bound constant in addition to the bound classification.");
        }
        if(Path.GetFileName(input)=="retained.roslynobj")
        {
            Concurrency.Verify(input,Check);
            var function=unit.Functions.Single(f=>f.Name=="coroutine_protocol");
            var operations=function.Body!.DescendantOperations().ToArray();
            var coroutine=operations.OfType<CoroutineOperation>().Single();
            Check(coroutine.Function==function&&coroutine.Promise is not null&&coroutine.Handle is not null&&coroutine.TraitsType is RecordType&&coroutine.Coroutine.HasReturnVoid,"A real coroutine must retain its routine, promise, handle, traits and return protocol facts.");
            Check(coroutine.InitialSuspend is AwaitOperation&&coroutine.FinalSuspend is AwaitOperation&&coroutine.ReturnObject is not null&&coroutine.Exception is not null,"Initial/final suspension, return-object and exception protocol expressions must navigate from the coroutine descriptor.");
            var awaits=unit.Entities.OfType<AwaitOperation>().ToArray();
            bool Protocol(ExpressionOperation? expression,string name) => expression is not null&&Captured(expression).OfType<CallOperation>().Any(c=>c.TargetMethod?.Name==name);
            Check(awaits.Length==12&&awaits.Count(a=>a.Kind.Name=="enk_yield")==3&&awaits.All(a=>Protocol(a.Ready,"await_ready")&&Protocol(a.Suspend,"await_suspend")&&Protocol(a.Resume,"await_resume")),"Real await and yield descriptors must preserve EDG's selected ready/suspend/resume protocol calls, including wrapped result assignments.");
            var parameterFunction=unit.Functions.Single(f=>f.Name=="coroutine_parameter");
            var parameterCoroutine=parameterFunction.Body!.DescendantOperations().OfType<CoroutineOperation>().Single();
            Check(parameterCoroutine.ParameterCopies.Single().Copy is {Type.Name:"CopyParameter",Details.IsParameter:true},"A by-value coroutine must expose its actual parameter-copy variable without trailing scope locals or inferred original-parameter relationships.");
            Check(unit.Functions.Single(f=>f.Name=="run"&&f.ContainingType?.Name=="MemberCoroutine").Body!.DescendantOperations().OfType<CoroutineOperation>().Single().ThisParameterCopy is not null,"A member coroutine must expose the actual retained this-parameter copy.");
            if(obj.Manifest.Stage=="lowered")
                Check(parameterFunction.Body.DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod is {ContainingType.Name:"CopyParameter",Details.SpecialFunction.Name:"sfk_constructor"})==1,"A lowered coroutine parameter copy must expose its actual constructor replacement exactly once through the body.");
            if(obj.Manifest.Stage=="lowered")
                Check(new[]{"operator new","operator new[]","operator delete","operator delete[]","default_value","read"}.All(targets.Contains),"Lowered retained allocation, default-argument and explicit-object uses must expose their actual selected calls.");
            else
            {
                var allocations=unit.Entities.OfType<AllocationOperation>().ToArray();
                Check(allocations.Length==4&&allocations.Single(a=>a.IsDelete) is {IsArray:true,Deallocator.Name:"operator delete[]",Operand:not null},"Source array deletion must navigate its actual deallocator and evaluated operand.");
                Check(allocations.Single(a=>!a.IsDelete&&a.IsArray) is {Allocator.Name:"operator new[]",ArrayCount:not null,Initialization:not null,ExceptionFreeing:not null},"Source array allocation must preserve count, initialization and exceptional freeing.");
                Check(allocations.Single(a=>a.Allocation.IsAligned).AllocatedType?.Name=="Allocated"&&allocations.Single(a=>a.IsPlacement).Arguments.Count==1,"Aligned and placement allocation must preserve retained flags, types and ordered placement arguments.");
                var typeids=unit.Entities.OfType<TypeQueryOperation>().Where(o=>o.QueryKind.Name=="enk_typeid").ToArray();
                Check(typeids.Length==2&&typeids.Single(t=>t.IsDynamic).Operand is not null&&typeids.Single(t=>!t.IsDynamic).OperandType?.Kind.Name=="tk_integer","Source typeid must retain distinct dynamic-expression and static-type operands.");
                var lambdas=unit.Entities.OfType<LambdaDefinition>().ToArray();
                Check(lambdas.Length==3&&lambdas.Single(l=>l.IsMutable).Captures.Count==2&&lambdas.Single(l=>l.IsGeneric).Captures.Single().CaptureByReference&&lambdas.Single(l=>l.HasCaptureDefault).Captures.Single().IsImplicit,"Source lambda definitions must expose capture lists and retained mutable/generic/default flags.");
                Check(unit.Entities.OfType<Initialization>().Count(i=>i.Lambda is not null)==3&&lambdas.All(l=>l.Captures.All(c=>c.Owner==l)),"Source lambda initialization must navigate the canonical definition and capture owner identity.");
                var parameter=unit.Functions.Single(f=>f.Name=="default_parameter").Parameters.Single();
                Check(parameter.DefaultExpression is CallOperation {TargetMethod.Name:"default_value",RepresentationRole.Name:"metadata"},"A source parameter default must retain its selected call through the canonical descriptor as metadata.");
                Check(parameter.Descriptor.Details.HasDefaultArgument&&parameterFunction.Parameters.Single().Descriptor.Details.PassedViaCopyConstructor&&unit.Functions.Where(f=>f.ContainingType?.Name=="CopyParameter").SelectMany(f=>f.Parameters).Any(p=>p.Descriptor.Details.CopyOrMoveConstructorParameter),"Actual source parameters must retain default, by-value copy and copy-constructor parameter flags.");
                Check(unit.Functions.Single(f=>f.Name=="read"&&f.ContainingType?.Name=="ExplicitObject").Parameters.Single().Descriptor.IsExplicitThis,"An actual explicit-object method must retain its explicit-this signature descriptor flag.");
                var initializations=unit.Functions.Single(f=>f.Name=="source_initializations").Body!.DescendantOperations().ToArray();
                var condition=initializations.OfType<ConditionOperation>().Single();var throwing=initializations.OfType<ThrowOperation>().Single();
                var handler=initializations.OfType<TryOperation>().Single().Handlers.Single();
                Check(condition.Initialization is StatementOperation&&condition.DynamicInitialization?.Constructor?.ContainingType?.Name=="InitializationValue"&&throwing.Initialization?.Constructor?.ContainingType?.Name=="InitializationValue"&&handler.ParameterInitialization?.Constructor?.ContainingType?.Name=="InitializationValue","Source class conditions, throws and by-value catches must preserve separate typed initialization relationships.");
                Check(initializations.OfType<CallOperation>().Count(c=>c.TargetMethod?.Name=="initialization_effect")==3&&initializations.OfType<InitializationOperation>().Count(i=>i.Initialization.Constructor?.ContainingType?.Name=="InitializationValue")==4,"Actual source initializer arguments and handler construction must appear exactly once through executable containment.");
                Check(initializations.All(n=>n.RepresentationRole.Name=="source_semantic"),"Actual source condition/throw/handler children must preserve source-semantic roles throughout their evaluated graph.");
            }
            var visitor=new ProtocolVisitor();foreach(var operation in awaits)visitor.Visit(operation);visitor.Visit(coroutine);
            Check(visitor.Awaits>=12&&visitor.Coroutines==1,"Named coroutine and await visitors must dispatch to their corresponding hooks.");
        }
        if(Path.GetFileName(input)=="source_statements.roslynobj"&&obj.Manifest.CaptureProfile=="source_semantic")
        {
            var nodes=unit.Entities.OfType<SemanticNode>().ToArray();
            var range=nodes.OfType<RangeForOperation>().Single();
            Check(range.RangeVariable is not null&&range.BeginVariable is not null&&range.EndVariable is not null&&range.IteratorVariable?.Name=="value"&&range.Initialization is not null&&range.Comparison is not null&&range.Increment is not null&&range.Body is not null,"Source range-for must expose its actual variables, initialization, test, increment and body.");
            var conditionals=nodes.OfType<ConditionalOperation>().ToArray();
            Check(conditionals.Length==2&&conditionals.Any(i=>i.Conditional.SelectionKnown==false)&&conditionals.Any(i=>i.Conditional is {SelectionKnown:true,SelectedThen:true}),"Source constexpr-if must retain unknown prototype and selected instantiated branch states.");
            var assigned=nodes.OfType<JumpOperation>().Single(o=>o.Kind.Name=="stmk_assigned_goto");
            Check(assigned.Value is not null,"Source assigned-goto must expose its retained destination expression.");
            var statementExpression=nodes.OfType<StatementExpressionOperation>().Single();
            Check(Captured(statementExpression).OfType<StatementOperation>().Any(o=>o.Kind.Name=="stmk_stmt_expr_result"&&o.Expression is not null),"Source GNU statement expressions must preserve the result statement and its expression.");
            var assembly=nodes.OfType<AssemblyOperation>().ToArray();
            Check(assembly.Length==2&&assembly.All(a=>a.AsmString?.Value is StringValue&&a.Clobbers.Single().RegisterKind is not null&&a.OperandConstraints.Count==a.Operands.Count)&&assembly.Single(a=>a.IsGoto==true).Labels.Count==1,"GNU assembly must retain target-string templates, structured operand constraints, register clobbers and goto labels.");
            var read=nodes.OfType<CallOperation>().Single(c=>c.SourceTargetMethod?.Name=="read");
            Check(read.SourceDispatchKind?.Name=="non_virtual"&&read.Receiver is not null,"Source member calls must retain their exact dispatch selector and receiver.");
            var visitor=new FamilyVisitor();foreach(var node in nodes)node.Accept(visitor);
            Check(visitor.Ranges>0&&visitor.Conditionals>0&&visitor.Assemblies>0,"Named source-statement visitors must dispatch range-for, conditional and assembly hooks.");
        }
        if(Path.GetFileName(input)=="source_vla.roslynobj")
        {
            var nodes=unit.Entities.OfType<SemanticNode>().ToArray();var vlas=nodes.OfType<VlaStatementOperation>().ToArray();
            Check(vlas.Length==4&&vlas.Count(v=>v.Dimension is not null)==2&&vlas.Single(v=>v.Vla.IsTypedefDeclaration).Type is TypeReference {UnderlyingType:ArrayType},"Source VLA statements must preserve typed dimensions and the actual typedef descriptor linked to its array type.");
            Check(vlas.Where(v=>v.Dimension is not null).All(v=>v.Dimension is {HasSizeStatement:true,Expression:not null}),"Source VLA dimensions must preserve their size expressions and retained evaluation flag.");
            Check(nodes.OfType<VlaExpressionOperation>().Single().Variable?.Name=="rows","Source VLA deallocation must retain its actual variable relationship.");
            var visitor=new FamilyVisitor();foreach(var node in nodes)node.Accept(visitor);Check(visitor.Vlas>=4,"Named VLA visitors must dispatch retained size/declaration operations.");
        }
        if(Path.GetFileName(input)=="native_microsoft.roslynobj")
        {
            var nodes=unit.Entities.OfType<SemanticNode>().ToArray();var loops=nodes.OfType<ForEachOperation>().ToArray();
            Check(loops.Length==2&&loops.All(l=>l.CollectionReference is not null&&l.Iterator is not null&&l.Body is not null&&l.Test is not null&&l.Increment is not null&&l.Scope is not null)&&loops.Single(l=>l.UsesPreviousDeclarationIterator).Assignment is not null,"Native Microsoft foreach must expose retained collection variables, iterator scopes and previous-iterator assignment.");
            var tries=nodes.OfType<MicrosoftTryOperation>().ToArray();
            Check(tries.Length==2&&tries.Single(t=>t.IsFinally).Finally is not null&&tries.Single(t=>!t.IsFinally) is {Filter:not null,Handler:not null}&&tries.All(t=>t.Try.ExceptOrFinallyPosition is not null),"Native Microsoft try must preserve separate finally/except arms and keyword source positions.");
            Check(unit.Types.Single(t=>t.Name=="NativeProperty").Fields.Single(f=>f.Name=="property").Property is {GetName:"get",PutName:"set"},"Native Microsoft field properties must expose retained accessor spellings without reconstructing lookup.");
            var visitor=new FamilyVisitor();foreach(var node in nodes)node.Accept(visitor);Check(visitor.ForEach>0&&visitor.MicrosoftTry>0,"Native Microsoft operations must dispatch their named visitor hooks.");
        }
        if(Path.GetFileName(input)=="native_attributes.roslynobj")
        {
            var attributes=unit.Entities.OfType<MicrosoftAttribute>().ToArray();
            var module=attributes.Single(a=>a.Name=="module");
            var arguments=module.Arguments.ToDictionary(a=>a.ParameterName!);
            Check(module.SourceEntry is not null&&module.Arguments.Select(a=>a.ParameterName).SequenceEqual(new[]{"type","name","uuid","version","lcid","control"}),"A native Microsoft module attribute must preserve its retained source entry and ordered named arguments.");
            Check(arguments["type"].EnumerationOrdinal==0&&arguments["name"].String?.Value is StringValue text&&text.StorageUnits.SequenceEqual("NativeModule".Select(c=>(uint)c).Append(0U)),"Native enumeration and string arguments must retain their exact descriptor ordinal and target storage units.");
            Check(arguments["uuid"].Uuid=="12345678-1234-1234-1234-123456789abc"&&arguments["version"].OtherText=="(1.2)"&&arguments["lcid"].Integer==1033&&arguments["control"].Boolean==true,"Native UUID, unclassified text, integer and boolean arguments must retain all six actual union payloads.");
            Check(attributes.Single(a=>a.Name=="uuid").Arguments.Single().Uuid=="23456789-1234-1234-1234-123456789abc"&&unit.Entities.OfType<Scope>().Any(s=>s.MicrosoftAttributes.SequenceEqual(attributes)),"The native attribute owner list must navigate canonical descriptors and preserve the second UUID argument independently.");
        }
        if(Path.GetFileName(input)=="module_interface.roslynobj")
            Check(unit.EntryPoint is null&&unit.ModuleImports.Count==0,"A real module interface may have no entry point or imported module occurrences.");
        if(Path.GetFileName(input)=="module_import.roslynobj")
        {
            var import=unit.ModuleImports.Single();var module=import.Module;
            Check(module.Name=="RoslynNative"&&module.Kind.Name=="mk_unit"&&module.FileKind.Name=="mfk_edg_ifc"&&module.ResolvedFile is not null&&module.UsesAngleBrackets==false,"A real named module import must preserve its exact module kind, resolved IFC and import syntax flag.");
            Check(unit.Modules.Single()==module&&import.SourcePosition is not null&&import.ModuleNamePosition is not null&&!import.ImplementationUnitImportingSelf&&unit.EntryPoint is null,"A real import occurrence must navigate its canonical module and separate retained keyword/name positions.");
        }
        Console.WriteLine($"real:{input},{functionCount}functions,{operationCount}operations,{callCount}calls,{obj.Statistics.Commands}commands,{sw.ElapsedMilliseconds}ms");
    }
    Console.WriteLine($"PASS:{passed}reader assertions");
}
finally { Directory.Delete(root,true); }

static IEnumerable<SemanticNode> Captured(SemanticNode root)
{
    var seen=new HashSet<SemanticNode>();var pending=new Stack<SemanticNode>();pending.Push(root);
    while(pending.TryPop(out var node))
    {
        if(!seen.Add(node))continue;yield return node;
        foreach(var child in node.Children.Reverse())pending.Push(child);
    }
}

sealed class ProtocolVisitor : SemanticVisitor
{
    public int Awaits,Coroutines;
    public override void VisitAwait(AwaitOperation operation) { Awaits++;base.VisitAwait(operation); }
    public override void VisitCoroutine(CoroutineOperation operation) { Coroutines++;base.VisitCoroutine(operation); }
}

sealed class FamilyVisitor : SemanticVisitor
{
    public int Ranges,Conditionals,Assemblies,Vlas,ForEach,MicrosoftTry;
    public override void VisitRangeFor(RangeForOperation operation) { Ranges++;base.VisitRangeFor(operation); }
    public override void VisitConditional(ConditionalOperation operation) { Conditionals++;base.VisitConditional(operation); }
    public override void VisitAssembly(AssemblyOperation operation) { Assemblies++;base.VisitAssembly(operation); }
    public override void VisitVlaStatement(VlaStatementOperation operation) { Vlas++;base.VisitVlaStatement(operation); }
    public override void VisitForEach(ForEachOperation operation) { ForEach++;base.VisitForEach(operation); }
    public override void VisitMicrosoftTry(MicrosoftTryOperation operation) { MicrosoftTry++;base.VisitMicrosoftTry(operation); }
}

sealed class CountingVisitor : SemanticVisitor
{
    public int Blocks,Calls,Jumps,Declarations,Operations;
    public override void VisitBlock(BlockOperation operation) { Blocks++;base.VisitBlock(operation); }
    public override void VisitCall(CallOperation operation) { Calls++;base.VisitCall(operation); }
    public override void VisitJump(JumpOperation operation) { Jumps++;base.VisitJump(operation); }
    public override void VisitDeclaration(Declaration declaration) { Declarations++;base.VisitDeclaration(declaration); }
    public override void VisitOperation(Operation operation) { Operations++;base.VisitOperation(operation); }
}
