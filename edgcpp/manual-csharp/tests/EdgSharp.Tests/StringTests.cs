// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored real-source literal/type/storage controls; execution is supervisor-owned.
using System.Numerics;
using System.Runtime.CompilerServices;
using EdgSharp;

internal static partial class Tests
{
    private static StringLiteralExpression StringAt(AnalysisResult result, string function, int index = 0)
        => As<StringLiteralExpression>(As<ExpressionStatement>(Function(result, function).Body!.Statements[index]).Expression);
    internal static void StringGraph()
    {
        AnalysisResult result = Success("void inspect(){\"\"; \"a\\0b\"; u8\"é\"; u\"𐐀\"; U\"𐐀\"; L\"é\"; R\"(a\\nb)\";}");
        uint[][] expected = [ [0], [97,0,98,0], [0xc3,0xa9,0], [0xd801,0xdc00,0], [0x10400,0], [233,0], [97,92,110,98,0] ];
        IntegerKind[] kinds = [IntegerKind.Char,IntegerKind.Char,IntegerKind.Char8,IntegerKind.Char16,IntegerKind.Char32,IntegerKind.WChar,IntegerKind.Char];
        for (int i = 0; i < expected.Length; ++i)
        {
            StringLiteralExpression literal = StringAt(result,"inspect",i);
            Check(literal.CodeUnits.SequenceEqual(expected[i])); Equal(expected[i].Length,literal.Type.Length);
            Equal(kinds[i],Kind(literal.Type.ElementType)); Equal(TypeQualifiers.Const,literal.Type.ElementType.Qualifiers);
            Equal(ValueCategory.LValue,literal.ValueCategory); Check(literal.ConstantValue == null); Equal(1,literal.Parts.Count);
        }
        AnalysisResult raw = Success("void inspect(){R\"(a\r\nb)\";}");
        StringLiteralExpression rawLiteral = StringAt(raw,"inspect");
        Check(rawLiteral.CodeUnits.SequenceEqual(new uint[]{97,10,98,0})); Check(rawLiteral.Parts[0].Token.Spelling.Contains("\r\n",StringComparison.Ordinal));
        Check(rawLiteral.Parts[0].Token.Origin.SpellingRanges.Any(range=>range.Spelling.Contains("\r\n",StringComparison.Ordinal)));
    }
    internal static void StringConcatenation()
    {
        AnalysisResult result = Success("void inspect(){\"a\" \"\\0b\"; u\"\" \"\\xff\"; \"\\xff\" U\"\"; L\"\" \"\\xff\"; u\"\" \"\\x100\"; \"\\x100\" u\"\"; u\"\\x100\"; \"\\xc3\\xa9\" u\"\"; \"\\\\u0041\" u\"\"; \"é\" u\"\"; u\"\" \"é\"; \"é\" u8\"\"; u8\"\" \"é\"; R\"(é)\";}");
        uint[][] expected = [ [97,0,98,0], [255,0], [255,0], [255,0], [0,0], [0,0], [256,0], [233,0], [92,117,48,48,52,49,0], [233,0], [233,0], [195,169,0], [195,169,0], [195,169,0] ];
        for(int i=0;i<expected.Length;++i) Check(StringAt(result,"inspect",i).CodeUnits.SequenceEqual(expected[i]),"String part "+i);
        Equal(2,StringAt(result,"inspect").Parts.Count);
        foreach(string source in new[]{"void f(){\"é\";}","void f(){\"\\x100\";}","void f(){L\"a\" U\"b\";}","void f(){u\"a\" u8\"b\";}"})
        { AnalysisResult invalid=Analyze(source); Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures,source); }
        Check(Analyze("void f(){\"a\"_custom;}").ContainsUnsupportedFeatures);
        Check(Analyze("void f(){u8\"\" \"\\xff\";}").ContainsUnsupportedFeatures);
        AnalysisResult warning=Success("void f(){\"\\q\";}");
        Equal((uint)113,StringAt(warning,"f").CodeUnits[0]); Check(warning.Diagnostics.Any(d=>d.Severity==DiagnosticSeverity.Warning));
        Check(!Analyze("void f(){U\"\\uD800\";}").IsSuccess);
        Check(!Analyze("void f(){\"\\x\";}").IsSuccess);
    }
    internal static void StringSizeOf()
    {
        AnalysisResult result=Success("using R=int&; constexpr int boom(){return 1/0;} static_assert(sizeof(R)==4); static_assert(sizeof(boom())==4); static_assert(sizeof(\"a\"[99])==1); static_assert(sizeof(u\"𐐀\")==6); static_assert(sizeof(U\"𐐀\")==8); static_assert(sizeof(\"a\\0b\")==4); static_assert(sizeof(u8\"é\")==3);");
        Check(result.StaticAssertions.All(a=>a.Evaluation.Status==ConstantEvaluationStatus.Success&&a.Evaluation.StorageObjects.Count==0&&a.Evaluation.MaximumCallDepth==0));
        AnalysisResult template=Success("void sink(int); template<class T> T unusable(T x){return sink(x);} static_assert(sizeof(unusable(1))==4); template<class T> constexpr unsigned long size(T x){return sizeof(T);} static_assert(size(u'A')==2); static_assert(size(u\"A\")==8);");
        FunctionTemplateInstance unused=Template(template,"unusable").Instances.Single(); Equal(TemplateInstanceState.NotInstantiated,unused.State); Equal(0,unused.BodyReplayCount);
        Equal(2,Template(template,"size").Instances.Count); Check(Template(template,"size").Pattern.Body!.Statements.OfType<ReturnStatement>().Single().Expression is DependentSizeOfExpression);
        Check(!Analyze("static_assert(sizeof(void)==0);").IsSuccess);
        Check(!Analyze("static_assert(sizeof(missing)==0);").IsSuccess);
        Check(Success("static_assert(sizeof(char[1000001])==1000001);").IsSuccess);
    }
    internal static void StringArrayVariables()
    {
        AnalysisResult result=Success("constexpr char data[]=\"ab\"; constexpr char padded[5]=\"x\"; static_assert(sizeof(data)==3); static_assert(data[1]=='b'); static_assert(data[2]==0); static_assert(padded[0]=='x'&&padded[1]==0&&padded[4]==0); constexpr int read(){char local[]=\"ab\"; char& alias=local[1]; return alias;} static_assert(read()=='b');");
        Equal(3,As<ArrayType>(Variable(result,"data").Type).Length); Equal(5,As<ArrayType>(Variable(result,"padded").Type).Length);
        ConstantArrayObject initializerValue=As<ConstantArrayObject>(Variable(result,"data").ConstantEvaluation!.ArrayValue);
        Check(initializerValue.Symbol==null&&initializerValue.Elements.All(element=>element.IsConstantReadable));
        Check(initializerValue.Elements.Select(element=>element.Value!.Value).SequenceEqual(new BigInteger[]{97,98,0}));
        ConstantEvaluationResult execution=result.StaticAssertions[^1].Evaluation;
        ConstantArrayObject local=execution.Arrays.Single(array=>array.Symbol?.Name=="local");
        Check(local.Elements.All(cell=>cell.IsAlive==false&&cell.IsReadOnly==false));
        Same(local.Elements[1],execution.ReferenceBindings.Single(binding=>binding.Expression.Context==ReferenceBindingContext.Variable).Object);
        foreach(string source in new[]{"char a[0]=\"\";","char a[2]=\"ab\";","char a[];","char16_t a[]=\"a\";"})
        { AnalysisResult invalid=Analyze(source); Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures,source); }
        foreach(string source in new[]{"char a[-1];","constexpr int negative(){return -1;} char a[negative()];"})
        {
            AnalysisResult negative=Analyze(source);
            Check(!negative.IsSuccess&&!negative.ContainsUnsupportedFeatures);
            Check(negative.Diagnostics.Any(d=>d.Code=="ARR001"&&d.Class==DiagnosticClass.InvalidSource));
            Check(negative.Diagnostics.All(d=>d.Class!=DiagnosticClass.ResourceLimit));
        }
        Check(Analyze("int a[2]={1,2};").ContainsUnsupportedFeatures);
        foreach(string source in new[]{"const char a[]=\"x\"; static_assert(a[0]==120);", "const char* const p=\"x\"; static_assert(p[0]==120);"})
        { AnalysisResult ordinary=Analyze(source); Check(!ordinary.IsSuccess&&!ordinary.ContainsUnsupportedFeatures); Equal(ConstantEvaluationStatus.NotConstant,ordinary.StaticAssertions.Single().Evaluation.Status); }
        Check(Success("constexpr char a[]=\"x\"; static_assert(a[0]==120);").IsSuccess);
        Check(Success("constexpr const char* p=\"x\"; static_assert(p[0]==120);").IsSuccess);
        Check(Success("constexpr int f(){const char a[]=\"x\"; return a[0];} static_assert(f()==120);").IsSuccess);
        AnalysisResult address=Success("const char a[]=\"x\"; constexpr const char* p=a;");
        ConstantArrayObject readonlyGlobal=As<ConstantArrayObject>(Variable(address,"p").ConstantEvaluation!.PointerValue!.Array);
        Check(readonlyGlobal.Elements.All(cell=>cell.IsReadOnly==true&&!cell.IsConstantReadable&&cell.Value==null));
    }
    internal static void StringPointerStorage()
    {
        AnalysisResult result=Success("constexpr const char* text=\"ab\"; constexpr const char* pass(const char* value){return value;} constexpr const char* end(){return text+3;} static_assert(text[1]=='b'); static_assert(pass(text)[2]==0); static_assert(1[text]=='b'); static_assert(end()[-2]=='b'); static_assert(!false && text); constexpr const char* zero=0; static_assert(!zero);");
        ConstantEvaluationResult pointer=Variable(result,"text").ConstantEvaluation!;
        Check(pointer.Value==null);
        ConstantPointerValue pointerValue=As<ConstantPointerValue>(pointer.PointerValue);
        ConstantArrayObject literalArray=As<ConstantArrayObject>(pointerValue.Array);
        Check(!pointerValue.IsNull); Equal(3,literalArray.Type.Length);
        Check(literalArray.Elements.All(cell=>cell.IsReadOnly==true&&cell.IsAlive==true));
        AnalysisResult reversed=Success("constexpr int read(){return 1[\"ab\"];} static_assert(read()=='b');");
        Expression reverseValue=ReturnValue(Function(reversed,"read"));
        while(reverseValue is ConversionExpression conversion) reverseValue=conversion.Operand;
        SubscriptExpression subscript=As<SubscriptExpression>(reverseValue);
        Equal("1[\"ab\"]",subscript.Range.Spelling); Check(subscript.PointerOperandIsSecond);
        Check(subscript.SourceLeft.Type is IntegerType && subscript.SourceRight.Type is PointerType);
        AnalysisResult unpromoted=Success("constexpr int f(char16_t i){return i[\"ab\"];} static_assert(f(1)=='b');");
        Expression indexValue=ReturnValue(Function(unpromoted,"f"));
        while(indexValue is ConversionExpression indexConversion) indexValue=indexConversion.Operand;
        SubscriptExpression retainedIndex=As<SubscriptExpression>(indexValue);
        Equal(IntegerKind.Char16,Kind(retainedIndex.Index.Type)); Equal(ConversionKind.LValueToRValue,As<ConversionExpression>(retainedIndex.Index).Conversion);
        AnalysisResult onePast=Success("constexpr const char* end=\"ab\"+3;"); Check(Variable(onePast,"end").ConstantEvaluation!.PointerValue!.IsOnePastEnd);
        AnalysisResult discarded=Success("constexpr int f(){\"ab\"[3];return 1;} static_assert(f()==1);");
        ConstantEvaluationResult discardedEvaluation=discarded.StaticAssertions.Single().Evaluation;
        Equal(3,discardedEvaluation.StorageObjects.Count); Equal(3,discardedEvaluation.Arrays.Single().Elements.Count);
        Check(discardedEvaluation.StorageObjects.All(cell=>cell.IsKnownObject&&cell.IsConstantReadable));
        AnalysisResult directReference=Success("constexpr int ignore(const char&){return 1;} static_assert(ignore(\"ab\"[3])==1); constexpr const char& r=\"ab\"[3];");
        ConstantEvaluationResult ignored=directReference.StaticAssertions.Single().Evaluation;
        Equal(3,ignored.StorageObjects.Count); Equal(0,ignored.ReferenceBindings.Count);
        Check(ignored.NonObjectReferenceBindings.Single().Address.IsOnePastEnd);
        ConstantEvaluationResult referenceValue=Variable(directReference,"r").ConstantEvaluation!;
        Equal(ConstantEvaluationStatus.Success,referenceValue.Status);
        Check(referenceValue.Value==null&&referenceValue.ReferenceValue==null&&referenceValue.NonObjectReferenceValue!.IsOnePastEnd);
        AnalysisResult indirectReference=Analyze("constexpr int ignore(const char&){return 1;} constexpr const char& r=\"ab\"[3]; static_assert(ignore(r)==1);");
        Equal(ConstantEvaluationStatus.UndefinedBehavior,indirectReference.StaticAssertions.Single().Evaluation.Status);
        AnalysisResult returnedReference=Analyze("constexpr const char& past(){return\"ab\"[3];} constexpr int ignore(const char&){return 1;} static_assert(ignore(past())==1);");
        Check(!returnedReference.IsSuccess&&!returnedReference.ContainsUnsupportedFeatures);
        ConstantEvaluationResult returned=returnedReference.StaticAssertions.Single().Evaluation;
        Equal(ConstantEvaluationStatus.UndefinedBehavior,returned.Status);
        Equal(ReferenceBindingContext.Return,returned.NonObjectReferenceBindings.Single().Expression.Context);
        Equal(3,returned.StorageObjects.Count);
        foreach(string source in new[]{"static_assert(\"ab\"[3]==0);","static_assert(\"ab\"[-1]==0);","constexpr const char* end=\"ab\"+4;"})
        { AnalysisResult invalid=Analyze(source); Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures,source); }
        AnalysisResult expired=Analyze("constexpr const char* bad(){char local[2]=\"a\"; return local;} static_assert(bad()[0]=='a');");
        Equal(ConstantEvaluationStatus.ExpiredObject,expired.StaticAssertions.Single().Evaluation.Status);
        Check(Success("constexpr const char* bad(){char a[2]=\"x\";return a;} static_assert(bad());").IsSuccess);
        AnalysisResult persistentDead=Analyze("constexpr const char* bad(){char a[2]=\"x\";return a;} constexpr const char* p=bad();");
        Equal(ConstantEvaluationStatus.ExpiredObject,Variable(persistentDead,"p").ConstantEvaluation!.Status);
        AnalysisResult mutable=Analyze("char data[]=\"ab\"; constexpr const char* address=data; static_assert(address[0]=='a');");
        Equal(ConstantEvaluationStatus.Success,Variable(mutable,"address").ConstantEvaluation!.Status); Equal(ConstantEvaluationStatus.NotConstant,mutable.StaticAssertions.Single().Evaluation.Status);
        foreach(string source in new[]{"const char* p=\"a\"; bool f(){return p==p;}","const char* p=\"a\"; long f(){return p-p;}","const char* p=\"a\"; const char* f(){return +p;}","const char* p=\"a\"; bool f(){return p==0;}"})
        { AnalysisResult bounded=Analyze(source); Check(!bounded.IsSuccess&&bounded.ContainsUnsupportedFeatures,source); }
        foreach(string source in new[]{"const char* p=\"a\"; int f(){return -p;}","const char* p=\"a\"; bool f(){return p==1;}","const char* p=\"a\"; int f(){return p*p;}"})
        { AnalysisResult invalid=Analyze(source); Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures,source); }
        AnalysisResult boolean=Success("constexpr bool truth(bool value){return value;} static_assert(truth(\"x\")); constexpr int pick(bool){return 1;} constexpr int pick(const char*){return 2;} static_assert(pick(\"x\")==2);");
        CallExpression truth=As<CallExpression>(boolean.StaticAssertions[0].Expression);
        Check(truth.SelectedCandidate.ArgumentConversions.Single().RequiresPointerToBoolean&&truth.SelectedCandidate.ArgumentConversions.Single().RequiresArrayToPointer);
        Equal(StandardConversionRank.Conversion,truth.SelectedCandidate.ArgumentConversions.Single().Rank);
        CallExpression picked=As<CallExpression>(As<BinaryExpression>(boolean.StaticAssertions[1].Expression).Left);
        Check(picked.Candidates.All(candidate=>candidate.IsViable)); Equal(TypeKind.Pointer,picked.SelectedCandidate.FunctionType.ParameterTypes[0].Kind);
    }
    internal static void StringElementReferences()
    {
        AnalysisResult result=Success("constexpr const char& first(){return \"ab\"[0];} constexpr int read(const char& value){return value;} static_assert(first()=='a'); static_assert(read(\"ab\"[1])=='b'); constexpr const char& reference=\"ab\"[1]; static_assert(reference=='b');");
        Check(result.StaticAssertions.All(a=>a.Evaluation.Status==ConstantEvaluationStatus.Success));
        foreach(StaticAssertion assertion in result.StaticAssertions)
            Check(assertion.Evaluation.ReferenceBindings.All(binding=>binding.Object.IsReadOnly==true&&binding.Object.Lifetime==ConstantStorageLifetime.Static));
        Check(!Analyze("char& wrong=\"ab\"[0];").IsSuccess);
        AnalysisResult overload=Success("constexpr int pick(const char* p){return p[0];} constexpr int pick(const char16_t* p){return p[0]+1;} static_assert(pick(\"a\")=='a'); static_assert(pick(u\"a\")==98);");
        CallExpression call=As<CallExpression>(As<BinaryExpression>(overload.StaticAssertions[0].Expression).Left);
        Check(call.SelectedCandidate.ArgumentConversions[0].RequiresArrayToPointer); Equal(StandardConversionRank.ExactMatch,call.SelectedCandidate.ArgumentConversions[0].Rank);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardStringAnalysis(CompilationSession session)
    {
        AnalysisResult result=session.Analyze("constexpr const char16_t* text=u\"𐐀\"; static_assert(text[1]==0xdc00);");
        Check(result.IsSuccess); return new(result.GlobalScope);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardCompoundTypes(TypeTable table)
    {
        var scope=new Scope(ScopeKind.Template,null);
        var parameter=new TemplateParameterType("T",0,scope);
        SemanticType compound=table.Pointer(table.Array(table.Reference(parameter,ReferenceKind.LValue),3));
        _=table.Function(compound,[compound]);
        return new(scope);
    }
    internal static void StringOwnershipAndRollback()
    {
        using var session=new CompilationSession();
        TokenBuffer tokens=session.Lex("array").Tokens;
        var state=new ParseState(tokens.CreateCursor(),new DiagnosticBag());
        var table=new TypeTable(TargetInfo.Lp64);
        ArrayType incomplete=table.Array(table.Integer(IntegerKind.Char),0), complete=table.Array(table.Integer(IntegerKind.Char),4);
        var symbol=new Symbol("array",SymbolKind.Variable,incomplete,state.GlobalScope);
        state.Enter(symbol);
        var declaration=new VariableDeclaration(symbol,incomplete,tokens.Tokens[0].Range,tokens.Tokens[0].Range);
        state.AddDeclaration(declaration);
        ParseCheckpoint outer=state.Checkpoint();
        state.SetVariableType(declaration,complete); Same(complete,symbol.Type); Same(complete,declaration.Type);
        ParseCheckpoint inner=state.Checkpoint();
        var child=new ParseState(tokens.CreateCursor(),state.Diagnostics,state.GlobalScope,state);
        ArrayType alternative=table.Array(table.Integer(IntegerKind.Char),7);
        child.SetVariableType(declaration,alternative);
        state.Restore(inner); Same(complete,symbol.Type); Same(complete,declaration.Type);
        state.Restore(outer); Same(incomplete,symbol.Type); Same(incomplete,declaration.Type);
        AnalysisResult alias=Success("using A=const char[81]; using A=const char[81]; void f(A); void f(const char*); constexpr int read(A p){return p[0];} static_assert(read(\"a\")=='a');");
        Equal(2,alias.GlobalScope.FindLocal("A")!.Declarations.Count); Equal(2,alias.GlobalScope.FindLocal("f")!.Declarations.Count);
        FunctionDeclaration read=Function(alias,"read"); Check(read.Parameters[0].ParameterDeclaredType is ArrayType {Length:81}); Equal(TypeQualifiers.Const,As<PointerType>(read.Parameters[0].Type).PointedToType.Qualifiers);
        Check(!Analyze("using A=char[3]; A f();").IsSuccess);
        WeakReference<Scope> discarded=DiscardStringAnalysis(session), scoped=DiscardCompoundTypes(table);
        for(int i=0;i<3;++i){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
        Check(!discarded.TryGetTarget(out _)); Check(!scoped.TryGetTarget(out _)); GC.KeepAlive(session); GC.KeepAlive(table);
        using var limited=new CompilationSession(new AnalysisOptions{StringLiteralCodeUnitLimit=4});
        Check(limited.Analyze("void f(){\"abcd\";}").Diagnostics.Any(d=>d.Class==DiagnosticClass.ResourceLimit));
        using var work=new CompilationSession(new AnalysisOptions{ConstantEvaluationStepLimit=10});
        AnalysisResult resource=work.Analyze("static_assert(\"abcdefghijk\"[0]=='a');"); Equal(ConstantEvaluationStatus.ResourceLimit,resource.StaticAssertions.Single().Evaluation.Status);
        using var maximum=new CompilationSession(new AnalysisOptions{StringLiteralCodeUnitLimit=int.MaxValue,ConstantEvaluationStepLimit=int.MaxValue});
        AnalysisResult enormous=maximum.Analyze("constexpr int f(){char a[2147483647]=\"x\"; return a[0];} static_assert(f()==120);");
        Equal(ConstantEvaluationStatus.ResourceLimit,enormous.StaticAssertions.Single().Evaluation.Status);
        Check(enormous.StaticAssertions.Single().Evaluation.Arrays.Count==0);
    }
    internal static void StringStorageCombined()
    {
        var provider=new InMemoryIncludeProvider(new Dictionary<string,string>{["m1-string-storage-gate.h"]=Fixture("m1-string-storage-gate.h")});
        AnalysisResult result,invalid;
        using(var session=new CompilationSession(new AnalysisOptions{Preprocessing=new(includeProvider:provider)}))
        {
            result=session.Analyze(Fixture("m1-string-storage-gate.cpp"),"m1-string-storage-gate.cpp");
            invalid=session.Analyze(Fixture("m1-string-storage-invalid.cpp"),"m1-string-storage-invalid.cpp");
        }
        Check(result.IsSuccess,string.Join("; ",result.Diagnostics.Select(d=>d.Code+": "+d.Message)));
        Equal(6,result.StaticAssertions.Count); Check(result.StaticAssertions.All(a=>a.Evaluation.Status==ConstantEvaluationStatus.Success&&a.Evaluation.Value==1));
        FunctionTemplateDeclaration first=Template(result,"πρώτο"); Equal(3,first.Instances.Count);
        foreach(FunctionTemplateInstance instance in first.Instances)
        {
            Equal(1,instance.BodyReplayCount); Check(instance.TypeArguments.Single() is PointerType);
            CallExpression selected=As<CallExpression>(ReturnValue(instance.Declaration!)); Equal(2,selected.Candidates.Count);
            Equal(IntegerKind.Int,Kind(((FunctionType)selected.SelectedFunction.Type).ReturnType));
        }
        Equal(3,result.GlobalScope.FindLocalBinding("βοήθησε")!.Symbols.Count);
        ConstantEvaluationResult execution=result.StaticAssertions[0].Evaluation;
        ConstantArrayObject local=execution.Arrays.Single(a=>a.Symbol?.Name=="text"); Equal(3,local.Type.Length);
        Check(local.Elements.Select(e=>e.Value!.Value).SequenceEqual(new BigInteger[]{233,120,0}));
        Check(local.Elements.All(e=>e.IsAlive==false&&e.IsReadOnly==false));
        Same(local.Elements[0],execution.ReferenceBindings.Single(binding=>binding.Expression.Context==ReferenceBindingContext.Variable).Object);
        ArrayInitializationExpression initializer=As<ArrayInitializationExpression>(Local(Function(result,"τρέξε"),"text").Initializer);
        Equal(2,initializer.Literal.Parts.Count);
        Check(initializer.Literal.Parts.Any(part=>part.Token.Origin.ExpansionHistory.Any(frame=>frame.Name=="ΚΕΙΜΕΝΟ")));
        Check(initializer.Literal.Parts.All(part=>part.Token.Origin.ExpansionNotes.Any(note=>note.Contains("included from",StringComparison.Ordinal))));
        Check(first.BodyTokens.Tokens.Any(token=>token.Origin.ExpansionHistory.Any(frame=>frame.Name=="ΒΟΗΘΟΣ")));
        Check(result.StaticAssertions[^1].Evaluation.Arrays.Count==0);
        Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures); Equal(ConstantEvaluationStatus.NotConstant,invalid.StaticAssertions.Single().Evaluation.Status);
        ConstantPointerValue pointer=Variable(invalid,"address").ConstantEvaluation!.PointerValue!;
        ConstantArrayObject globalArray=As<ConstantArrayObject>(pointer.Array);
        Check(globalArray.Elements.All(element=>!element.IsConstantReadable&&element.Value==null&&element.IsReadOnly==false));
        Same(Variable(invalid,"text").Symbol,globalArray.Symbol!);
    }
}
