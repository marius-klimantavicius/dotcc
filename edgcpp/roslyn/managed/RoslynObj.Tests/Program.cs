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
string Fixture(string name,int extraVariables=0,int extraOperations=0,string? additionalRows=null)
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
        command.CommandText=$"INSERT INTO entity VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable')); INSERT INTO symbol(id,kind_id,name_id,containing_entity_id,type_id) VALUES({id},(SELECT id FROM kind WHERE family='entity' AND name='variable'),8,1,3); INSERT INTO variable_symbol(id,type_id,initialization_kind_id) VALUES({id},3,(SELECT id FROM kind WHERE family='initializer' AND name='initk_none'));";
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
        Check(operations.OfType<BlockOperation>().Count()==1&&operations.OfType<JumpOperation>().Count()==1,"Execution traversal must include statements.");
        Check(operations.OfType<ConstantOperation>().Count()==1,"Shared executable children must be visited once and backing metadata excluded.");
        var call=operations.OfType<CallOperation>().Single();
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
    using(var regional=RoslynObject.Open(Fixture("regional",additionalRows:"""
        INSERT INTO entity VALUES(160,(SELECT id FROM kind WHERE family='entity' AND name='variable'));
        INSERT INTO symbol(id,kind_id,name_id,containing_entity_id,type_id) VALUES(160,(SELECT id FROM kind WHERE family='entity' AND name='variable'),8,6,3);
        INSERT INTO variable_symbol(id,type_id,initialization_kind_id) VALUES(160,3,(SELECT id FROM kind WHERE family='initializer' AND name='initk_function_local'));
        INSERT INTO static_initialization(owner_id,ordinal,variable_id,initialization_kind_id,constant_id) VALUES(1,0,160,(SELECT id FROM kind WHERE family='initializer' AND name='initk_static'),30);
        INSERT INTO entity VALUES(162,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol')),(163,(SELECT id FROM kind WHERE family='entity' AND name='declaration'));
        INSERT INTO symbol(id,kind_id,name_id,containing_entity_id,type_id) VALUES(162,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol'),41,1,40);
        INSERT INTO type_symbol(id,type_id) VALUES(162,40);
        UPDATE type SET symbol_id=162 WHERE id=40;
        INSERT INTO declaration(id,kind_id,symbol_id,lexical_scope_id,source_origin_id) VALUES(163,(SELECT id FROM kind WHERE family='entity' AND name='type_symbol'),162,2,73);
        """)))
    {
        var variable=regional.TranslationUnit.Variables.Single(v=>v.InitializationKind.Name=="initk_function_local");
        Check(variable.Details.ConstantInitializer is null&&variable.ConstantInitializer==regional.TranslationUnit.StaticInitializations.Single().Constant,"Local-static values must follow the exact region payload when the direct variable field is absent.");
        var declaration=regional.TranslationUnit.Declarations.Single();var declared=regional.TranslationUnit.Types.Single(t=>t.Name=="Widget");
        Check(regional.TranslationUnit.SemanticModel.GetSymbol(declaration)==declared.DeclarationSymbol&&regional.TranslationUnit.SemanticModel.GetType(declaration)==declared,"Declaration nodes must project the separate named symbol and actual type descriptor through ordinary semantic queries.");
        Check(declaration.NodeDetails is null&&declaration.Source?.MacroExpansion is not null,"A declaration must retain its own source record without duplicating a semantic-node row.");
    }
    using(var structured=RoslynObject.Open(Fixture("structured",additionalRows:"""
        INSERT INTO entity(id,kind_id) SELECT id,(SELECT id FROM kind WHERE family='entity' AND name='constant') FROM (SELECT 230 id UNION ALL SELECT 231 UNION ALL SELECT 260 UNION ALL SELECT 261);
        INSERT INTO entity VALUES(270,(SELECT id FROM kind WHERE family='entity' AND name='constant_value'));
        INSERT INTO constant_value(id,kind_id) VALUES(270,(SELECT id FROM kind WHERE family='constant' AND name='ck_aggregate'));
        INSERT INTO aggregate_value(id,type_id) VALUES(270,40);
        INSERT INTO aggregate_element(owner_id,ordinal,value_id,type_id,field_id) VALUES(270,0,31,3,42);
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
            var add=unit.Functions.Single(f=>f.Name=="add");
            Check(unit.SemanticModel.GetReferences(add).Any(r=>r.Role==SymbolReferenceRole.CallTarget&&r.Owner is CallOperation),"The real selected add call must appear in ordinary symbol references.");
            var main=unit.Functions.Single(f=>f.Name=="main");
            var local=main.Body!.Locals.Single(v=>v.Name=="x");
            Check(unit.SemanticModel.GetReferences(local).Count>0,"Real variable references must project typed reference-expression facts.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="SUM");
            Check(macro.Text=="#define SUM(x,y) ((x)+(y))","The real macro definition must retain EDG's text.");
            Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="a, b"),"The real SUM invocation must retain canonical arguments and definition identity.");
        }
        if(Path.GetFileName(input)=="m0.roslynobj")
        {
            Check(cleanupActions==6,"The real C++ object must expose all six selected exceptional cleanup actions.");
            Check(unit.Functions.Where(f=>f.ContainingType is not null).SelectMany(f=>f.Parameters).Any(p=>p.Role.Name=="this_parameter"),"Real retained method parameters must expose explicit receiver roles.");
            var macro=unit.Macros.Definitions.Single(m=>m.Name=="INC");
            Check(macro.Text=="#define INC(v) ((v)+1)","The real INC macro must retain definition text.");
            Check(unit.Macros.Invocations.Any(m=>m.Definition==macro&&m.ArgumentText=="y"),"The real INC invocation must retain its argument text and definition identity.");
            using var paged=RoslynObject.Open(input,new(){PageSize=1,CachedPageCount=2,CachedRowCount=32});
            var parameter=paged.TranslationUnit.Functions.Single(f=>f.Name=="use").SourceParameters.Single(p=>p.Name=="n").Symbol!;
            var uses=paged.TranslationUnit.SemanticModel.GetReferences(parameter);
            Check(uses.Count>=3&&uses.ToArray().Length==uses.Count&&uses.All(r=>r.ReferencedSymbol==parameter),"Real symbol references must enumerate every captured use across multiple UNION pages.");
            var field=unit.Types.Where(t=>t.Name=="Base").SelectMany(t=>t.Fields).First(f=>f.Name=="x");
            Check(unit.SemanticModel.GetReferences(field).Any(r=>r.Owner is ReferenceOperation),"Real field uses must follow the typed reference-expression target.");
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
        if(Path.GetFileName(input)=="templates.roslynobj")
        {
            var holder=unit.Templates.Single(t=>t.Name=="Holder");
            var header=holder.Headers.Single();
            Check(header.Template==holder&&header.Parameters.Select(p=>p.Name).SequenceEqual(new[]{"T","N"}),"Real template headers must preserve owning definitions and parameter order.");
            var typeParameter=header.Parameters[0];var valueParameter=header.Parameters[1];
            Check(typeParameter.PlaceholderType is DependentType&&valueParameter.PlaceholderConstant?.Value is ConstantValue<DependentValueDetails>,"Real template parameters must retain their type and value placeholders.");
            Check(((IntegerValue)valueParameter.DefaultArgument!.Constant!.Value).Value==3,"Real non-type template defaults must preserve the exact selected constant.");
            Check(holder.OriginalDefinition==holder&&holder.Definition==holder&&holder.Pattern is RecordType,"Real canonical and definition links must retain EDG template identity.");
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
        }
        Console.WriteLine($"real:{input},{functionCount}functions,{operationCount}operations,{callCount}calls,{obj.Statistics.Commands}commands,{sw.ElapsedMilliseconds}ms");
    }
    Console.WriteLine($"PASS:{passed}reader assertions");
}
finally { Directory.Delete(root,true); }
