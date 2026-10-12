using Microsoft.Data.Sqlite;
using RoslynObj;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

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
string Fixture(string name,int extraVariables=0,int extraOperations=0)
{
    var path=Path.Combine(root,name+".roslynobj");
    using var db=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=path}.ToString());
    db.Open();
    using var command=db.CreateCommand();
    command.CommandText="PRAGMA foreign_keys=ON";command.ExecuteNonQuery();
    using var transaction=db.BeginTransaction();command.Transaction=transaction;
    command.CommandText=Resource("schema");command.ExecuteNonQuery();
    command.CommandText=Resource("basic").Replace("fixture-artifact",Guid.NewGuid().ToString());command.ExecuteNonQuery();
    for(var i=0;i<extraVariables;i++)
    {
        var id=1000+i;
        command.CommandText=$"INSERT INTO entity VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable')); INSERT INTO symbol(id,kind_id,name_id,containing_entity_id,type_id) VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable'),8,1,3); INSERT INTO variable_symbol(id,type_id) VALUES({id},3);";
        command.ExecuteNonQuery();
    }
    for(var i=0;i<extraOperations;i++)
    {
        var statement=10000+i;var expression=20000+i;
        command.CommandText=$"INSERT INTO entity VALUES({statement},(SELECT id FROM kind WHERE family='entity' AND name='statement')); INSERT INTO entity VALUES({expression},(SELECT id FROM kind WHERE family='entity' AND name='expression')); INSERT INTO semantic_node(id,kind_id,representation_role_id) VALUES({statement},(SELECT id FROM kind WHERE family='statement' AND name='stmk_expr'),(SELECT id FROM kind WHERE family='representation_role' AND name='execution')); INSERT INTO semantic_node(id,kind_id,representation_role_id) VALUES({expression},(SELECT id FROM kind WHERE family='expression' AND name='enk_constant'),(SELECT id FROM kind WHERE family='representation_role' AND name='execution')); INSERT INTO statement(id,kind_id,expression_id,parent_id) VALUES({statement},(SELECT id FROM kind WHERE family='statement' AND name='stmk_expr'),{expression},10); INSERT INTO expression(id,type_id,value_category_id) VALUES({expression},3,(SELECT id FROM kind WHERE family='value_category' AND name='prvalue')); INSERT INTO constant_expression(id,constant_id) VALUES({expression},30); INSERT INTO structural_child VALUES(10,(SELECT id FROM kind WHERE family='role' AND name='statement'),{i+3},{statement}); INSERT INTO structural_child VALUES({statement},(SELECT id FROM kind WHERE family='role' AND name='operand'),0,{expression}); INSERT INTO block_item VALUES(10,{i+3},{statement});";
        command.ExecuteNonQuery();
    }
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
        Check(operations.OfType<BlockOperation>().Count()==1&&operations.OfType<JumpOperation>().Count()==1,"Execution traversal must include statements.");
        Check(operations.OfType<ConstantOperation>().Count()==1,"Shared executable children must be visited once and backing metadata excluded.");
        var call=operations.OfType<CallOperation>().Single();
        Check(call.TargetMethod==external&&call.SourceTargetMethod is null&&call.Receiver is null,"Call targets must follow retained relationships.");
        Check(call.Arguments.Count==1&&call.Arguments[0].Value.Type.Kind.Name=="tk_integer","Call arguments must navigate to execution types.");
        var constant=operations.OfType<ConstantOperation>().Single().Constant;
        Check(((IntegerValue)constant.Value).Value==BigInteger.Parse("340282366920938463463374607431768211455"),"Wide integers must remain exact.");
        Check(constant.BackingExpression!.RepresentationRole.Name=="metadata","Backing expression role must be preserved separately.");
        var floating=(FloatingValue)unit.Constants.Single(c=>c.Type.Kind.Name=="tk_float").Value;
        Check(floating.Value.IsZero&&floating.Value.IsNegative,"Floating signed zero must retain its exact components.");
        var targetString=(StringValue)unit.Constants.Single(c=>c.Value is StringValue).Value;
        Check(targetString.Length==3&&targetString.CodeUnits.SequenceEqual(new uint[]{65,0,65535}),"Target string units must retain embedded NUL and non-Unicode units.");
        var type=unit.Types.OfType<ConstructedType>().Single();
        Check(type.Members.Single().Name=="value"&&type.Fields.Single().Name=="value","Members must follow retained type ownership.");
        Check(type.Layout!.Size==16&&type.Layout.Fields.Single().Offset==0,"Layouts must project target quantities.");
        Check(type.TypeArguments.Count==1&&type.OriginalDefinition!.Name=="Widget","Template original and typed arguments must navigate through objects.");
        var region=body.ExceptionHandling.CleanupRegions.Single();
        Check(region.Ordinal==0&&region.NextRegion is null,"Actual EH region zero must remain distinct from null sentinel.");
        var state=operations.OfType<CleanupStateOperation>().Single();
        Check(state.Region==region&&state.StateNumber.IsZero,"Cleanup states must resolve normalized regions.");
        Check(body.ExceptionHandling.Details!.NullState==BigInteger.Parse("18446744073709551615"),"Unsigned EH sentinel must remain exact beyond long.");
        var action=region.Actions.Single();
        Check(action.TargetMethod==external&&action.Array!.ElementCount==-1&&action.Storage!.Variable!.Name=="x","Exceptional cleanup must preserve storage, routine and dynamic counts.");
        Check(action.AbiDetail!.Details.IsDelegating&&region.Condition!.Storage==action.Storage,"Typed EH supplements must navigate without decoding flags.");
        Check(body.Lifetimes.Count==1&&unit.Declarations.Count==0&&external.Body is null,"Optional retained collections and body absence must be ordinary values.");
        var invocation=unit.Macros.Invocations.Single();
        Check(invocation.ArgumentText=="( 4 )"&&call.Source!.MacroExpansion==invocation,"Canonical macro argument text and operation origins must be retained.");
        Check(invocation.Children.Count==0&&invocation.Position!.OriginalColumn==7,"Macro optional children and original coordinates must remain accessible.");
        Check(unit.SemanticModel.FindSymbols("helper").Single()==external,"Name search must follow exact atoms.");
        Check(unit.SemanticModel.GetReferences(external).Count==1,"References must follow recorded uses.");
        Parallel.For(0,16,_=>Check(unit.Functions.Count==2,"Concurrent list reads must share the same snapshot."));
        Check(obj.Statistics.CachedRows<=32&&obj.Statistics.CachedPages<=2,"Caches must remain bounded.");
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
    foreach(var input in args)
    {
        var sw=Stopwatch.StartNew();
        using var obj=RoslynObject.Open(input);
        var unit=obj.TranslationUnit;
        var functionCount=0;var operationCount=0;var callCount=0;var cleanupActions=0;var targets=new List<string?>();
        foreach(var function in unit.Functions)
        {
            _=function.Name;_=function.ReturnType.Kind;
            foreach(var parameter in function.Parameters) _=parameter.Type.Kind;
            functionCount++;
            if(function.Body is not {} body)continue;
            foreach(var operation in body.DescendantOperations())
            {
                operationCount++;
                if(operation is ExpressionOperation expression) _=expression.Type.Kind;
                if(operation is CallOperation call) { callCount++;targets.Add(call.TargetMethod?.Name);foreach(var argument in call.Arguments) _=argument.Value.Type.Kind; }
            }
            foreach(var region in body.ExceptionHandling.CleanupRegions)
                foreach(var action in region.Actions) { cleanupActions++;_=action.Kind;_=action.TargetMethod?.Name;_=action.Storage?.Offset; }
        }
        Check(functionCount>0&&operationCount>0,"Real EDG output must expose bodies and definitions.");
        if(Path.GetFileName(input)=="basic.roslynobj")
        {
            Check(targets.Contains("add"),"The real basic initializer must expose its EDG-selected add call in executable containment.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="SUM");
            Check(macro.Text=="#define SUM(x,y) ((x)+(y))","The real macro definition must retain EDG's text.");
            Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="a, b"),"The real SUM invocation must retain canonical arguments and definition identity.");
        }
        if(Path.GetFileName(input)=="m0.roslynobj")
        {
            Check(cleanupActions==6,"The real C++ object must expose all six selected exceptional cleanup actions.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="INC");
            Check(macro.Text=="#define INC(v) ((v)+1)","The real INC macro must retain definition text.");
            Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="y"),"The real INC invocation must retain its argument text and definition identity.");
        }
        if(Path.GetFileName(input)=="exact.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.ConstantInitializer is not null).ToDictionary(v=>v.Name!,StringComparer.Ordinal);
            Check(((IntegerValue)variables["wide"].ConstantInitializer!.Value).Value==(BigInteger.One<<127)+17,"The real unsigned128-bit global must preserve every bit.");
            var zero=((FloatingValue)variables["negative_zero"].ConstantInitializer!.Value).Value;
            Check(zero.IsZero&&zero.IsNegative,"The real long-double negative zero must retain its sign.");
            var finite=((FloatingValue)variables["finite"].ConstantInitializer!.Value).Value;
            Check(finite.Radix==2&&finite.Significand==(BigInteger.One<<63)+8&&finite.Exponent==-49,"The real extended float must preserve exact significand/exponent.");
            Check(((FloatingValue)variables["infinity"].ConstantInitializer!.Value).Value.Class==FloatingPointClass.Infinity,"The real infinity must retain its classification.");
            var nan=((FloatingValue)variables["nan_value"].ConstantInitializer!.Value).Value;
            Check(nan.Class==FloatingPointClass.NaN&&nan.NaNPayload==801&&nan.IsQuietNaN==true,"The real NaN must retain its payload and quiet state.");
            var first=variables["a"].ConstantInitializer!;var second=variables["b"].ConstantInitializer!;
            Check(first!=second&&first.Value==second.Value,"Identical target strings must share value without merging occurrence identities.");
            Check(((StringValue)first.Value).TargetBytes.SequenceEqual(new byte[]{97,98,0,99,100,0}),"The real narrow string must retain internal and trailing NUL bytes.");
            Check(((StringValue)variables["wide_text"].ConstantInitializer!.Value).TargetBytes.SequenceEqual(new byte[]{97,0,0,0,187,3,0,0}),"The real char16 string must preserve target byte order and embedded NUL.");
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
            Check(source.File?.Name=="logical.cpp"&&source.LogicalLine==200,"The real #line mapping must preserve logical filename and line.");
        }
        Console.WriteLine($"real:{input},{functionCount}functions,{operationCount}operations,{callCount}calls,{obj.Statistics.Commands}commands,{sw.ElapsedMilliseconds}ms");
    }
    Console.WriteLine($"PASS:{passed}reader assertions");
}
finally { Directory.Delete(root,true); }
