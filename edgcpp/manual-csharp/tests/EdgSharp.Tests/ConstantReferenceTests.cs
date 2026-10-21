// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored storage, lifetime and source integration tests; native execution is
// supervisor-owned. Public observations are immutable snapshots, never cells.
using System.Numerics;
using System.Runtime.CompilerServices;
using EdgSharp;

internal static partial class Tests
{
    private static ConstantEvaluationResult SingleEvaluation(AnalysisResult result)
        => result.StaticAssertions.Single().Evaluation;

    internal static void ConstantReferenceStorage()
    {
        AnalysisResult address = Success(Fixture("m4-reference-constexpr-address-discard.cpp"));
        ConstantEvaluationResult transported = Variable(address, "alias").ConstantEvaluation!;
        Equal(ConstantEvaluationStatus.Success, transported.Status); Check(transported.Value == null);
        ConstantStorageObject global = transported.ReferenceValue!.Object;
        Same(Variable(address, "global").Symbol, global.Symbol!);
        Check(global.IsAlive == true && !global.IsConstantReadable && global.Value == null);
        ConstantEvaluationResult discarded = SingleEvaluation(address);
        Equal(BigInteger.One, discarded.Value!.Value); Equal(2, discarded.MaximumCallDepth);
        Check(discarded.ReferenceBindings.Count >= 2);
        Check(discarded.ReferenceBindings.All(binding => ReferenceEquals(binding.Object, discarded.StorageObjects.Single())));
        Check(discarded.StorageObjects.All(storage => !storage.IsConstantReadable && storage.Value == null));
        AnalysisResult extended = Success(Fixture("m4-reference-constexpr-extended-global-temporary.cpp"));
        ConstantEvaluationResult aliases = SingleEvaluation(extended);
        Equal(BigInteger.One, aliases.Value!.Value);
        ConstantStorageObject temporary = aliases.StorageObjects.Single();
        Equal(ConstantStorageLifetime.Static, temporary.Lifetime); Check(temporary.IsAlive == true);
        Same(Variable(extended, "stable").Symbol, temporary.LifetimeOwner!);
        Check(aliases.ReferenceBindings.All(binding => ReferenceEquals(temporary, binding.Object)));
        Check(Variable(extended, "stable").ConstantEvaluation!.ReferenceValue != null);
        AnalysisResult optional = Success(Fixture("m4-reference-constexpr-local-optional.cpp"));
        VariableDeclaration local = Local(Function(optional, "parameter"), "alias");
        Equal(ConstantEvaluationStatus.NotConstant, local.ConstantEvaluation!.Status);
        Check(optional.StaticAssertions.All(assertion => assertion.Evaluation.Status == ConstantEvaluationStatus.Success));
        Check(optional.StaticAssertions[0].Evaluation.StorageObjects.Any(storage => storage.Symbol?.Name == "value" && storage.Value == 4));
        AnalysisResult uninitializedAddress = Success("constexpr int f(){int value; const int& alias=value; alias; return 1;} static_assert(f()==1);");
        Check(SingleEvaluation(uninitializedAddress).StorageObjects.Any(storage => storage.IsInitialized == false));
        AnalysisResult unknown = Success(Fixture("m4-reference-constexpr-unknown-reference-transport.cpp"));
        Equal(2, unknown.StaticAssertions.Count);
        ConstantStorageObject symbolic = unknown.StaticAssertions[0].Evaluation.StorageObjects.Single();
        Check(!symbolic.IsKnownObject && symbolic.Identity == null && symbolic.Value == null && !symbolic.IsConstantReadable);
        Equal(ConstantStorageLifetime.Unknown, symbolic.Lifetime);
        Check(symbolic.IsAlive == null && symbolic.IsInitialized == null && symbolic.Symbol == null && symbolic.LifetimeOwner == null);
        Same(Function(unknown, "outer").Parameters.Single().Symbol, symbolic.UnknownReference!);
        Check(unknown.StaticAssertions[0].Evaluation.ReferenceBindings.All(binding => ReferenceEquals(symbolic, binding.Object)));
        Check(unknown.StaticAssertions[1].Evaluation.StorageObjects.All(storage => storage.IsKnownObject));
        AnalysisResult unknownRead = Analyze(Fixture("m4-reference-constexpr-unknown-reference-read.cpp"));
        Check(!unknownRead.IsSuccess && !unknownRead.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.NotConstant, unknownRead.StaticAssertions[0].Evaluation.Status);
        AnalysisResult unknownConstant = Analyze(Fixture("m4-reference-constexpr-unknown-reference-constant.cpp"));
        Check(!unknownConstant.IsSuccess && !unknownConstant.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.NotConstant, Local(Function(unknownConstant, "outer"), "alias").ConstantEvaluation!.Status);
    }

    internal static void ConstantReferenceArguments()
    {
        AnalysisResult result = Success(Fixture("m4-reference-constexpr-argument-temporary.cpp"));
        foreach (StaticAssertion assertion in result.StaticAssertions)
        {
            ConstantStorageObject argument = assertion.Evaluation.StorageObjects.Single();
            Equal(ConstantStorageLifetime.FullExpressionTemporary, argument.Lifetime);
            Check(argument.IsAlive == false && argument.IsInitialized == true);
            Check(assertion.Evaluation.ReferenceBindings.All(binding => ReferenceEquals(argument, binding.Object)));
        }
        Equal(2, result.StaticAssertions[1].Evaluation.ReferenceBindings.Count(binding => binding.Expression.Context == ReferenceBindingContext.Return));
        AnalysisResult recursive = Success("constexpr bool descend(const int& value,int total){return value==0 || descend(value-1,value+total);} static_assert(descend(3,7));");
        ConstantEvaluationResult execution = SingleEvaluation(recursive);
        Equal(4, execution.MaximumCallDepth);
        ConstantStorageObject[] totals = execution.StorageObjects.Where(storage => storage.Symbol?.Name == "total").ToArray();
        Check(totals.Select(storage => storage.Value!.Value).SequenceEqual(new BigInteger[] { 7, 10, 12, 13 }));
        Equal(4, totals.Select(storage => storage.Invocation).Distinct().Count());
        Check(totals.All(storage => storage.IsAlive == false));
        Equal(4, execution.StorageObjects.Count(storage => storage.Lifetime == ConstantStorageLifetime.FullExpressionTemporary));
        AnalysisResult values = Success("constexpr int add(int x,int y){return x+y;} static_assert(add(1,add(2,3))==6);");
        ConstantStorageObject[] parameters = SingleEvaluation(values).StorageObjects.Where(storage => storage.Symbol?.Kind == SymbolKind.Parameter).ToArray();
        Equal(4, parameters.Length); Equal(4, parameters.Select(storage => storage.Identity).Distinct().Count());
        Equal(2, parameters.Where(storage => storage.Symbol!.Name == "x").Select(storage => storage.Invocation).Distinct().Count());
        Check(Success("int global=3; constexpr int ignore(const int& x){return 1;} static_assert(ignore(global)==1);").IsSuccess);
        Check(!Analyze("int global=3; constexpr int read(int x){return 1;} static_assert(read(global)==1);").IsSuccess);
    }

    internal static void ConstantReferenceLifetimes()
    {
        foreach (var (name, status) in new[]
        {
            ("mutable-read", ConstantEvaluationStatus.NotConstant),
            ("expired-local", ConstantEvaluationStatus.ExpiredObject),
            ("expired-return", ConstantEvaluationStatus.ExpiredObject)
        })
        {
            AnalysisResult invalid = Analyze(Fixture("m4-reference-constexpr-" + name + ".cpp"));
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures);
            Equal(status, SingleEvaluation(invalid).Status);
            Check(invalid.Diagnostics.Any(diagnostic => diagnostic.Code == "CEX001"));
        }
        AnalysisResult persistent = Analyze(Fixture("m4-reference-constexpr-persistent-temporary.cpp"));
        Check(!persistent.IsSuccess && !persistent.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.ExpiredObject, Variable(persistent, "alias").ConstantEvaluation!.Status);
        AnalysisResult argument = Success("constexpr const int& pass(const int& x){return x;} constexpr int take(){const int& ref=4; return pass(ref);} static_assert(take()==4);");
        Check(SingleEvaluation(argument).StorageObjects.All(storage => storage.IsAlive == false));
        foreach (string source in new[]
        {
            "constexpr int f(){int value; const int& alias=value; return alias;} static_assert(f()==1);",
            "constexpr int f(){int& alias=alias; return alias;} static_assert(f()==1);"
        }) Equal(ConstantEvaluationStatus.UninitializedRead, SingleEvaluation(Analyze(source)).Status);
        AnalysisResult runtime = Analyze("int&& get(); constexpr int value=get();");
        Check(!runtime.IsSuccess && !runtime.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.NotConstant, Variable(runtime, "value").ConstantEvaluation!.Status);
        AnalysisResult parameter = Analyze("constexpr const int& escape(int value){return value;} static_assert(escape(3)==3);");
        Equal(ConstantEvaluationStatus.ExpiredObject, SingleEvaluation(parameter).Status);
        AnalysisResult runtimeAlias = Analyze("int global=3; int& alias=global; static_assert(alias==3);");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(runtimeAlias).Status);
    }

    internal static void ConstantReferenceOptionalReplay()
    {
        foreach (string name in new[] { "declaration-time-late-body", "value-late-body", "declaration-time-available-body" })
        {
            AnalysisResult result = Success(Fixture("m4-reference-constexpr-" + name + ".cpp"));
            Equal(ConstantEvaluationStatus.Success, SingleEvaluation(result).Status);
            if (name != "declaration-time-available-body")
            {
                string variable = name == "value-late-body" ? "value" : "reference";
                Equal(ConstantEvaluationStatus.NotConstant, Variable(result, variable).ConstantEvaluation!.Status);
            }
        }
        AnalysisResult required = Analyze(Fixture("m4-reference-constexpr-required-late-body.cpp"));
        Check(!required.IsSuccess && required.Diagnostics.Any(diagnostic => diagnostic.Code == "CEX001"));
        Equal(ConstantEvaluationStatus.NotConstant, Variable(required, "reference").ConstantEvaluation!.Status);
        AnalysisResult frozen = Success("constexpr int choose(long); const int value=choose(1); constexpr int choose(int x){return 99;} constexpr int choose(long x){return 3;} static_assert(value==3);");
        CallExpression selected = As<CallExpression>(Variable(frozen, "value").Initializer);
        Equal(IntegerKind.Long, SelectedParameter(selected)); Equal(1, selected.Candidates.Count);
        Check(SingleEvaluation(frozen).StorageObjects.Any(storage => storage.Symbol?.Name == "value" && storage.Value == 3));
        AnalysisResult cycle = Analyze("const int value=value; static_assert(value==1);");
        Check(!cycle.IsSuccess); Equal(ConstantEvaluationStatus.UninitializedRead, SingleEvaluation(cycle).Status);
        AnalysisResult mutable = Analyze("int global=3; const int value=global; static_assert(value==3);");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(mutable).Status);
        foreach (string name in new[] { "initializing-address-transport", "initializing-required-address" })
        {
            AnalysisResult transport = Success(Fixture("m4-reference-constexpr-" + name + ".cpp"));
            Equal(new BigInteger(1), Variable(transport, "value").ConstantEvaluation!.Value!.Value);
            Check(Variable(transport, "value").ConstantEvaluation!.StorageObjects.Any(storage => storage.Symbol?.Name == "value" && storage.IsInitialized == false && storage.Value == null));
            Equal(ConstantEvaluationStatus.Success, SingleEvaluation(transport).Status);
        }
        foreach (string name in new[] { "initializing-scalar-read", "initializing-required-read" })
        {
            AnalysisResult read = Analyze(Fixture("m4-reference-constexpr-" + name + ".cpp"));
            Check(!read.IsSuccess && !read.ContainsUnsupportedFeatures);
            Equal(ConstantEvaluationStatus.UninitializedRead, Variable(read, "value").ConstantEvaluation!.Status);
        }
    }

    internal static void ConstantReferenceLogical()
    {
        AnalysisResult result = Success("int global=3; constexpr int& bad(){int local=1; return local;} static_assert(true || global==1); static_assert(false || (true && 3)); static_assert(true || bad()==1); static_assert(true || 1/0); static_assert(!false && true || false);");
        Check(result.StaticAssertions.All(assertion => assertion.Evaluation.Status == ConstantEvaluationStatus.Success));
        Check(result.StaticAssertions[0].Evaluation.StorageObjects.Count == 0);
        Check(result.StaticAssertions[2].Evaluation.MaximumCallDepth == 0);
        Check(result.StaticAssertions[3].Evaluation.Status == ConstantEvaluationStatus.Success);
        AnalysisResult falseCondition = Analyze("static_assert(false && 1/0);");
        Equal(ConstantEvaluationStatus.Success, SingleEvaluation(falseCondition).Status);
        Check(falseCondition.Diagnostics.Any(diagnostic => diagnostic.Code == "CEX002"));
        foreach (string source in new[] { "static_assert(true || missing);", "void nothing(); static_assert(true || nothing());" }) Check(!Analyze(source).IsSuccess);
        Check(Analyze("static_assert(true || sizeof(int));").ContainsUnsupportedFeatures);
        AnalysisResult live = Analyze("int global=3; static_assert(false || global==1);");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(live).Status);
        AnalysisResult typed = Success("bool f(int x,int y){return x && y || 0;}");
        BinaryExpression logical = As<BinaryExpression>(ReturnValue(Function(typed, "f")));
        Equal(IntegerKind.Bool, Kind(logical.Type)); Equal(TokenKind.OrOr, logical.Operator);
        Equal(ConversionKind.BooleanConversion, As<ConversionExpression>(As<BinaryExpression>(logical.Left).Left).Conversion);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardConstantReferenceResult(ConstantInterpreter interpreter)
    {
        AnalysisResult source = Success("constexpr const int& identity(const int& value){return value;} static_assert(identity(7)==7);");
        Equal(ConstantEvaluationStatus.Success, interpreter.Evaluate(source.StaticAssertions.Single().Expression).Status);
        return new WeakReference<Scope>(source.GlobalScope);
    }

    internal static void ConstantReferenceCleanup()
    {
        AnalysisResult success = Success(Fixture("m4-reference-constexpr-argument-temporary.cpp"));
        Expression expression = success.StaticAssertions[1].Expression;
        var interpreter = new ConstantInterpreter(new AnalysisOptions());
        ConstantEvaluationResult first = interpreter.Evaluate(expression);
        AnalysisResult early = Success("constexpr int f(){int value=8; const int& alias=value; return alias; int never=1/0;} static_assert(f()==8);");
        Equal(ConstantEvaluationStatus.Success, interpreter.Evaluate(early.StaticAssertions.Single().Expression).Status);
        AnalysisResult failed = Analyze(Fixture("m4-reference-constexpr-expired-return.cpp"));
        ConstantEvaluationResult failure = interpreter.Evaluate(failed.StaticAssertions.Single().Expression);
        Equal(ConstantEvaluationStatus.ExpiredObject, failure.Status);
        Check(failure.StorageObjects.All(storage => storage.Lifetime == ConstantStorageLifetime.Static || storage.IsAlive == false));
        ConstantEvaluationResult again = interpreter.Evaluate(expression);
        Equal(first.Steps, again.Steps); Equal(first.MaximumCallDepth, again.MaximumCallDepth);
        Check(!ReferenceEquals(first.StorageObjects.Single(), again.StorageObjects.Single()));
        WeakReference<Scope> dropped = DiscardConstantReferenceResult(interpreter);
        for (int i = 0; i < 3; ++i) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(!dropped.TryGetTarget(out _)); GC.KeepAlive(interpreter);
        using var limited = new CompilationSession(new AnalysisOptions { ConstantEvaluationCallLimit = 5 });
        AnalysisResult recursion = limited.Analyze("constexpr int recur(const int& x){return recur(x);} static_assert(recur(1)==1);");
        Equal(ConstantEvaluationStatus.ResourceLimit, SingleEvaluation(recursion).Status);
        Equal(5, SingleEvaluation(recursion).MaximumCallDepth);
        Check(SingleEvaluation(recursion).StorageObjects.All(storage => storage.IsAlive == false));
    }

    internal static void ConstantReferenceCombined()
    {
        AnalysisResult result = Success(Fixture("m4-reference-constexpr-combined.cpp"));
        Equal(3, result.StaticAssertions.Count);
        Check(result.StaticAssertions.All(assertion => assertion.Evaluation.Status == ConstantEvaluationStatus.Success && assertion.Evaluation.Value == 1));
        FunctionTemplateDeclaration copy = Template(result, "copy");
        Check(copy.Instances.Select(instance => Kind(instance.TypeArguments.Single())).SequenceEqual(new[] { IntegerKind.Char16, IntegerKind.Long }));
        foreach (FunctionTemplateInstance instance in copy.Instances)
        {
            Equal(1, instance.BodyReplayCount);
            CallExpression call = As<CallExpression>(As<ConversionExpression>(ReturnValue(instance.Declaration!)).Operand);
            Equal(2, call.Candidates.Count); Equal(ReferenceKind.LValue, ReferenceParameter(call).ReferenceKind);
            Equal(TypeQualifiers.Const, ReferenceParameter(call).ReferredType.Qualifiers);
        }
        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        Check(execution.StorageObjects.Any(storage => storage.Symbol?.Name == "value" && storage.Type.IntegerKind == IntegerKind.Char16 && storage.Value == 65));
        Check(execution.ReferenceBindings.Any(binding => binding.Expression.Range.File.OriginForRange(binding.Expression.Range).ExpansionHistory.Any(frame => frame.Name == "RET")));
        Check(execution.StorageObjects.All(storage => storage.IsAlive == false));
        Check(copy.BodyTokens.Tokens.Any(token => token.Origin.ExpansionHistory.Any(frame => frame.Name == "RET")));
    }
}
