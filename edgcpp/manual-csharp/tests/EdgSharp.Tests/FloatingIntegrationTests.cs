// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored source-to-graph floating integration controls. No native dependency.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static FloatingConstantValue FloatInitializer(AnalysisResult result, string name)
        => As<FloatingConstantValue>(Variable(result, name).ConstantEvaluation!.FloatingValue);
    private static void FloatAssertions(AnalysisResult result)
        => Check(result.StaticAssertions.All(assertion => assertion.Evaluation.Status == ConstantEvaluationStatus.Success && assertion.Evaluation.Value == 1));

    internal static void FloatingFrontendTypes()
    {
        AnalysisResult result = Success("using F=float; using D=double; float f(const F); float f(F x){return x;} constexpr float a=1.5f; constexpr double b=1.5; static_assert(sizeof(F)==4); static_assert(sizeof(D)==8); static_assert(sizeof(const double[3])==24);");
        FloatingType f = As<FloatingType>(As<FunctionType>(Function(result, "f").Type).ReturnType);
        Equal(FloatingKind.Float, f.FloatingKind); Equal(24, f.Precision); Equal(32, f.Width);
        Equal(FloatingKind.Double, As<FloatingType>(Variable(result, "b").Type).FloatingKind);
        Equal(TypeQualifiers.Const, Variable(result, "a").Type.Qualifiers);
        Same(f, As<FloatingLiteralExpression>(Variable(result, "a").Initializer).Type);
        Equal(0x3fc00000UL, FloatInitializer(result, "a").Bits);
        Equal(0x3ff8000000000000UL, FloatInitializer(result, "b").Bits);
        Check(Variable(result, "a").Initializer!.ConstantValue == null);
        Check(Variable(result, "a").ConstantEvaluation!.Value == null);
        FloatAssertions(result);
        foreach (string source in new[] { "unsigned float x;", "short double x;", "float double x;", "long float x;", "float float x;", "double double x;" })
        { AnalysisResult invalid = Analyze(source); Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source); }
        Check(Analyze("long double x;").ContainsUnsupportedFeatures);
        Check(Analyze("constexpr long double x=1.0L;").ContainsUnsupportedFeatures);
    }

    internal static void FloatingFrontendLiterals()
    {
        AnalysisResult result = Success("constexpr float tie=0x1.000001p0f; constexpr float odd=0x1.000003p0f; constexpr double large=9007199254740993.0; constexpr float tiny=0x1p-149f; constexpr float zero=0x1p-150f; constexpr double negative=-0.0; static_assert(tie==1.0f); static_assert(odd==0x1.000004p0f); static_assert(large==9007199254740992.0); static_assert(tiny>0.0f); static_assert(!zero); static_assert(!negative);");
        Equal(0x3f800000UL, FloatInitializer(result, "tie").Bits);
        Equal(0x3f800002UL, FloatInitializer(result, "odd").Bits);
        Equal(1UL, FloatInitializer(result, "tiny").Bits);
        Equal(0UL, FloatInitializer(result, "zero").Bits);
        Check(Variable(result,"zero").ConstantEvaluation!.FloatingConditions.HasFlag(FloatingEvaluationCondition.Underflow));
        Equal(0x8000000000000000UL, FloatInitializer(result, "negative").Bits);
        Check(!As<FloatingLiteralExpression>(Variable(result, "tie").Initializer).IsExact);
        Check(result.Diagnostics.Any(diagnostic => diagnostic.Code == "FLT003" && diagnostic.Severity == DiagnosticSeverity.Warning));
        FloatAssertions(result);
        foreach (string source in new[] { "float x=1e1000f;", "double x=1e+;", "double x=0x1p-;", "double x=0x1.p_tag;", "double x=1.0.0;", "int x=1_bad.0;", "int x=1_bad'0;" })
        { AnalysisResult invalid = Analyze(source); Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source); }
        using var session = new CompilationSession();
        foreach (string spelling in new[] { "1e_tag", "1Efoo", "1e", "0x1p_tag", "0x1Pfoo", "0x1p", "1.0e_tag", "1.0e", "1.0_custom", "1.0f32" })
        {
            Check(session.Lex(spelling).IsSuccess, spelling);
            AnalysisResult unsupported = session.Analyze("double value=" + spelling + ";");
            Check(!unsupported.IsSuccess && unsupported.ContainsUnsupportedFeatures, spelling);
            Check(unsupported.Diagnostics.All(d => d.Class != DiagnosticClass.InvalidSource), spelling);
        }
        foreach (string spelling in new[] { "1e_tag", "1Efoo", "1e", "0x1p_tag", "0x1Pfoo", "0x1p" })
            Equal(TokenKind.IntegerLiteral, session.Lex(spelling).Tokens.Tokens[0].Kind);
        foreach (string spelling in new[] { "1e+", "1e-", "1e'1", "0x1p+", "0x1p'1" }) Check(!session.Lex(spelling).IsSuccess, spelling);
        LexicalResult restored = session.Lex("1llL");
        Check(restored.IsSuccess); Equal(TokenKind.IntegerLiteral,restored.Tokens.Tokens[0].Kind);
        Equal(TokenKind.IntegerLiteral,session.Lex("1lL").Tokens.Tokens[0].Kind);
        AnalysisResult restoredUdl = session.Analyze("int value=1llL;");
        Check(!restoredUdl.IsSuccess && restoredUdl.ContainsUnsupportedFeatures);
        Check(restoredUdl.Diagnostics.All(d => d.Class != DiagnosticClass.InvalidSource));
        AnalysisResult mixedBuiltin = session.Analyze("int value=1lL;");
        Check(!mixedBuiltin.IsSuccess && !mixedBuiltin.ContainsUnsupportedFeatures);
        Check(mixedBuiltin.Diagnostics.Any(d => d.Code == "LEX010" && d.Class == DiagnosticClass.InvalidSource));
        Check(session.Analyze("#define DROP(x) 1\n#define UNUSED 1e+\n#if 0\n1e+\n#endif\nint value=DROP(1e+);").IsSuccess);
        Check(session.Analyze("#define DROP(x) 1\nint value=DROP(1e_tag);").IsSuccess);
    }

    internal static void FloatingFrontendConversions()
    {
        AnalysisResult result = Success("constexpr float from=16777217; constexpr double widened=from; constexpr int truncated=-2.75; constexpr unsigned char byte=255.9; constexpr unsigned int zero=-0.5; static_assert(from==16777216.0f); static_assert(widened==16777216.0); static_assert(truncated==-2); static_assert(byte==255); static_assert(zero==0); static_assert(0.5f+0.25==0.75); static_assert(2+0.5f==2.5f); static_assert(!0.0f&&!!0.25); static_assert(-1.25f<0&&1.25>1);");
        Equal(ConversionKind.FloatingIntegralConversion, As<ConversionExpression>(Variable(result, "from").Initializer).Conversion);
        ConversionExpression widening = As<ConversionExpression>(Variable(result, "widened").Initializer);
        Equal(ConversionKind.FloatingPromotion, widening.Conversion);
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(widening.Operand).Conversion);
        Check(Variable(result, "from").ConstantEvaluation!.FloatingConditions.HasFlag(FloatingEvaluationCondition.Inexact));
        FloatAssertions(result);
        foreach (string source in new[] { "double x=1.0%2.0;", "double x=1.0 bitand 2;", "double x=~1.0;" })
        { AnalysisResult invalid = Analyze(source); Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source); }
        Check(Success("constexpr int f(){int x=1;x+=1.75;return x;} static_assert(f()==2);").IsSuccess);
        AnalysisResult compound = Success("constexpr int f(){int x=3;x*=1.5f;return x;}static_assert(f()==4);");
        AssignmentExpression store = As<AssignmentExpression>(As<ExpressionStatement>(Function(compound,"f").Body!.Statements[1]).Expression);
        Equal(FloatingKind.Float,As<FloatingType>(store.OperationType).FloatingKind);
        Equal(new BigInteger(4),compound.StaticAssertions.Single().Evaluation.Assignments.Single().Value);
    }

    internal static void FloatingFrontendOverloads()
    {
        AnalysisResult result = Success("int pick(double); int pick(int); int exact(float); int exact(double); int entry(){return pick(1.0f)+exact(1.0f);} template<class T> constexpr T id(T x){return x;} static_assert(id(1.25f)==1.25f); static_assert(id(1.25)==1.25); static_assert(id(2.0f)==2.0f);");
        BinaryExpression sum = As<BinaryExpression>(ReturnValue(Function(result, "entry")));
        CallExpression pick = As<CallExpression>(sum.Left), exact = As<CallExpression>(sum.Right);
        Equal(FloatingKind.Double, As<FloatingType>(pick.SelectedCandidate.FunctionType.ParameterTypes.Single()).FloatingKind);
        Equal(StandardConversionRank.Promotion, pick.SelectedCandidate.ArgumentConversions.Single().Rank);
        Equal(StandardConversionRank.ExactMatch, exact.SelectedCandidate.ArgumentConversions.Single().Rank);
        Equal(FloatingKind.Float, As<FloatingType>(exact.SelectedCandidate.FunctionType.ParameterTypes.Single()).FloatingKind);
        FunctionTemplateDeclaration id = Template(result, "id"); Equal(2, id.Instances.Count);
        Check(id.Instances.All(instance => instance.BodyReplayCount == 1));
        Check(id.Instances.Select(instance => As<FloatingType>(instance.TypeArguments.Single()).FloatingKind).SequenceEqual(new[] { FloatingKind.Float, FloatingKind.Double }));
        foreach (string source in new[] { "int pick(float);int pick(double);int f(){return pick(1);}", "int pick(double,int);int pick(float,double);int f(){return pick(1.0f,1);}" })
        { AnalysisResult ambiguous = Analyze(source); Check(!ambiguous.IsSuccess && ambiguous.Diagnostics.Any(d => d.Code == "SEM026"), source); }
        AnalysisResult conflict = Analyze("template<class T> T repeated(T x,T y){return x;} int f(){return repeated(1.0f,1.0);}");
        Check(!conflict.IsSuccess && conflict.Diagnostics.Any(d => d.Code == "SEM023"));
    }

    internal static void FloatingFrontendReferences()
    {
        AnalysisResult result = Success("constexpr const double& global=1.25; constexpr float& same(float& value){return value;} constexpr float f(){float value=1.0f;float& alias=same(value);alias+=0.5f;return alias;} static_assert(global==1.25); static_assert(f()==1.5f); constexpr float pass(const float& value){return value;} static_assert(pass(1)==1.0f);");
        ConstantEvaluationResult execution = result.StaticAssertions[1].Evaluation;
        ConstantStorageObject value = execution.StorageObjects.Single(cell => cell.Symbol?.Name == "value");
        Equal(FloatingKind.Float, As<FloatingType>(value.Type).FloatingKind);
        Check(value.Value == null && value.FloatingValue!.Bits == 0x3fc00000UL);
        Check(value.IsAlive == false && value.IsReadOnly == false && value.LifetimeBeganInEvaluation == true);
        Check(execution.ReferenceBindings.All(binding => ReferenceEquals(binding.Object, value)));
        Same(value, execution.Assignments.Single().Object);
        Equal(0x3fc00000UL, execution.Assignments.Single().FloatingValue!.Bits);
        Check(execution.Assignments.Single().IntegerValue == null);
        ConstantReferenceValue global = As<ConstantReferenceValue>(Variable(result, "global").ConstantEvaluation!.ReferenceValue);
        Equal(ConstantStorageLifetime.Static, global.Object.Lifetime); Equal(0x3ff4000000000000UL, global.Object.FloatingValue!.Bits);
        AnalysisResult expired = Analyze("constexpr const float& bad(){const float& temp=1.0f;return temp;} static_assert(bad()==1.0f);");
        Equal(ConstantEvaluationStatus.ExpiredObject, expired.StaticAssertions.Single().Evaluation.Status);
        FloatAssertions(result);
    }

    internal static void FloatingFrontendArrayStores()
    {
        AnalysisResult result = Success("constexpr float f(){float values[2];values[0]=1.0f;values[1]=2.0f;float& alias=values[0];float* p=values;int i=0;while(i<2){p[i]+=0.5f;i+=1;}return alias+values[1];} static_assert(f()==4.0f); constexpr double reversed(){double cells[3];int i=0;(i=1)[cells+(i=2)-1]=0.5;return i+cells[2];} static_assert(reversed()==2.5);");
        ConstantEvaluationResult evaluation = result.StaticAssertions[0].Evaluation;
        ConstantArrayObject array = evaluation.Arrays.Single(item => item.Symbol?.Name == "values");
        Equal(2, array.Elements.Count); Check(array.Elements.All(cell => cell.Value == null && cell.FloatingValue != null && cell.IsAlive == false));
        Equal(0x3fc00000UL, array.Elements[0].FloatingValue!.Bits); Equal(0x40200000UL, array.Elements[1].FloatingValue!.Bits);
        Same(array.Elements[0], evaluation.ReferenceBindings.Single(binding => binding.Expression.Context == ReferenceBindingContext.Variable).Object);
        Check(evaluation.Assignments.Where(write => write.FloatingValue != null).All(write => array.Elements.Any(cell => ReferenceEquals(cell, write.Object))));
        Check(Success("static_assert(sizeof(float[1000001])==4000004);").IsSuccess);
        Check(Analyze("float values[2]={1.0f,2.0f};").ContainsUnsupportedFeatures);
        FloatAssertions(result);
    }

    internal static void FloatingFrontendFailures()
    {
        foreach (string source in new[] { "static_assert(0x1p127f*2.0f>0.0f);", "constexpr int bad=1.0/0.0;", "constexpr unsigned char bad=256.0;", "constexpr unsigned int bad=-1.0;" })
        {
            AnalysisResult invalid = Analyze(source); Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source);
            Check(invalid.Diagnostics.Any(d => d.Code == "CEX001"));
        }
        AnalysisResult discarded = Success("constexpr int ignore(const float&){return 1;} float runtime=1.0f; static_assert(ignore(runtime)==1); static_assert(true || runtime>0.0f); static_assert(false && 1.0/0.0 || true);");
        ConstantStorageObject runtime = discarded.StaticAssertions[0].Evaluation.StorageObjects.Single(cell => cell.Symbol?.Name == "runtime");
        Check(!runtime.IsConstantReadable && runtime.Value == null && runtime.FloatingValue == null && runtime.IsReadOnly == false);
        foreach (string source in new[] { "float runtime=1.0f;static_assert(runtime==1.0f);", "const float runtime=1.0f;static_assert(runtime==1.0f);", "constexpr float bad(){float value;return value;}static_assert(bad()==0.0f);" })
        { AnalysisResult invalid = Analyze(source); Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source); }
        Check(Success("constexpr float runtime=1.0f;static_assert(runtime==1.0f);").IsSuccess);
        Check(Success("constexpr float ignore(const float&){return 1.0f;}constexpr float value=ignore(value);static_assert(value==1.0f);").IsSuccess);
        AnalysisResult unknown = Success("constexpr int ignore(const float&){return 1;}constexpr int outer(const float& input){static_assert(ignore(input)==1);return 1;}static_assert(outer(1.0f)==1);");
        ConstantStorageObject symbolic = unknown.StaticAssertions[0].Evaluation.StorageObjects.Single();
        Check(!symbolic.IsKnownObject && symbolic.Identity == null && symbolic.IsAlive == null && symbolic.IsReadOnly == null && symbolic.FloatingValue == null && symbolic.Value == null);
        Check(!Analyze("constexpr int outer(const float& input){static_assert(input==1.0f);return 1;}").IsSuccess);
        Check(!Analyze("constexpr float f(){const float x=1.0f;x=2.0f;return x;}").IsSuccess);
        Check(!Analyze("constexpr float f(){float x=0.0f;return (x=1.0f)+(x=2.0f);}static_assert(f()==3.0f);").IsSuccess);
        FloatingExceptionPolicies();
    }

    internal static void FloatingFrontendLimits()
    {
        using var session = new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 30 });
        AnalysisResult limited = session.Analyze("constexpr float f(){float x=0.0f;while(true){x+=1.0f;}return x;}static_assert(f()==0.0f);");
        Equal(ConstantEvaluationStatus.ResourceLimit, limited.StaticAssertions.Single().Evaluation.Status);
        Check(limited.StaticAssertions.Single().Evaluation.StorageObjects.All(cell => cell.IsAlive == false));
        AnalysisResult good = Success("static_assert(1.0f+0.5f==1.5f);");
        var interpreter = new ConstantInterpreter(new AnalysisOptions { ConstantEvaluationStepLimit = 30 });
        ConstantEvaluationResult first = interpreter.Evaluate(limited.StaticAssertions.Single().Expression);
        Equal(ConstantEvaluationStatus.Success, interpreter.Evaluate(good.StaticAssertions.Single().Expression).Status);
        ConstantEvaluationResult again = interpreter.Evaluate(limited.StaticAssertions.Single().Expression);
        Equal(first.Status, again.Status); Equal(first.Steps, again.Steps);
        using var numerical = new CompilationSession(new AnalysisOptions { FloatingIntermediateBitLimit = 64 });
        AnalysisResult resource = numerical.Analyze("constexpr double x=1.23456789012345678901234567890123456789;");
        Check(resource.Diagnostics.Any(d => d.Class == DiagnosticClass.ResourceLimit));
        Check(numerical.Analyze("constexpr double x=1.5; static_assert(x==1.5);").IsSuccess);
        Throws<ArgumentOutOfRangeException>(() => new CompilationSession(new AnalysisOptions { FloatingIntermediateBitLimit = 63 }));
        // Closed numeric warning/fold classification is local and capped; a
        // flat prefix must not trigger another full prefix query/evaluation.
        string flat = string.Join("+",Enumerable.Repeat("1.0f",2048));
        using var flatSession = new CompilationSession(new AnalysisOptions{ConstantEvaluationStepLimit=12});
        Check(flatSession.Analyze("float value="+flat+";").IsSuccess);
        AnalysisResult flatRequired = flatSession.Analyze("static_assert(("+flat+")>0.0f);");
        Equal(ConstantEvaluationStatus.ResourceLimit,flatRequired.StaticAssertions.Single().Evaluation.Status);
        Check(flatRequired.Diagnostics.All(d=>d.Class!=DiagnosticClass.UnsupportedFeature));
        Expression graph = flatRequired.StaticAssertions.Single().Expression;
        Check(!graph.HasSideEffects&&!graph.IsValueDependent);
        var flatInterpreter = new ConstantInterpreter(new AnalysisOptions{ConstantEvaluationStepLimit=12});
        ConstantEvaluationResult flatFirst = flatInterpreter.Evaluate(flatRequired.StaticAssertions.Single().Expression);
        ConstantEvaluationResult flatGood = flatInterpreter.Evaluate(Variable(Success("constexpr float value=1.0f;"),"value").Initializer!);
        Equal(ConstantEvaluationStatus.Success,flatGood.Status);
        ConstantEvaluationResult flatAgain = flatInterpreter.Evaluate(flatRequired.StaticAssertions.Single().Expression);
        Equal(flatFirst.Status,flatAgain.Status); Equal(flatFirst.Steps,flatAgain.Steps);

    }
}
