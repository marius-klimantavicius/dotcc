// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored source-based mutation/control-flow tests. Supervisor executes them;
// native witnesses are separate fixtures and never frontend implementation input.
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using EdgSharp;

internal static partial class Tests
{
    private static IEnumerable<Statement> ControlStatements(Statement statement)
    {
        yield return statement;
        IEnumerable<Statement> children = statement switch
        {
            BlockStatement block => block.Statements,
            IfStatement conditional => conditional.Else == null ? [conditional.Then] : [conditional.Then, conditional.Else!],
            ForStatement loop => [loop.Initializer, loop.Body],
            LoopStatement loop => [loop.Body],
            _ => []
        };
        foreach (Statement child in children)
            foreach (Statement descendant in ControlStatements(child)) yield return descendant;
    }
    private static void AllControlAssertions(AnalysisResult result)
    {
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        Check(result.StaticAssertions.Count != 0);
        Check(result.StaticAssertions.All(a => a.Evaluation.Status == ConstantEvaluationStatus.Success && a.Evaluation.Value == BigInteger.One));
    }

    internal static void ScalarAssignmentGraph()
    {
        AnalysisResult result = Success("int f(int x,long y){x=y; return x=3;} int& ref(int& x){return x=2;}");
        FunctionDeclaration function = Function(result, "f");
        AssignmentExpression store = As<AssignmentExpression>(As<ExpressionStatement>(function.Body!.Statements[0]).Expression);
        Equal(TokenKind.Equal, store.Operator); Equal(ValueCategory.LValue, store.ValueCategory);
        Same(function.Parameters[0].Type, store.Type); Same(function.Parameters[0].Symbol, As<ReferenceExpression>(store.Target).ReferencedSymbol);
        Check(store.ConstantValue == null && store.HasSideEffects && store.OperationType == null);
        ConversionExpression conversion = As<ConversionExpression>(store.Value);
        Equal(ConversionKind.IntegralConversion, conversion.Conversion); Equal(IntegerKind.Int, Kind(conversion.Type));
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(conversion.Operand).Conversion);
        ConversionExpression returned = As<ConversionExpression>(ReturnValue(function));
        Equal(ConversionKind.LValueToRValue, returned.Conversion);
        Check(As<AssignmentExpression>(returned.Operand).HasSideEffects && returned.ConstantValue == null);
        ReferenceBindingExpression binding = As<ReferenceBindingExpression>(ReturnValue(Function(result, "ref")));
        Check(binding.Source is AssignmentExpression && binding.Referent is AssignmentExpression && binding.HasSideEffects);
        Equal(ReferenceBindingKind.Direct, binding.Binding.Kind);
        AnalysisResult nested = Success("constexpr int f(){int x; int y; x=y=4; (x=1)=2; return x+y;} static_assert(f()==6);");
        AllControlAssertions(nested);
        Check(SingleEvaluation(nested).Assignments.Select(a => a.Value).SequenceEqual(new BigInteger[] { 4, 4, 1, 2 }));
        Throws<NotSupportedException>(() => ((IList)SingleEvaluation(nested).Assignments).Clear());
        Throws<NotSupportedException>(() => ((IList)SingleEvaluation(nested).ControlFlow).Clear());
    }

    internal static void ScalarAssignmentConversions()
    {
        AnalysisResult result = Success("constexpr int narrow(){unsigned char value=250; value+=10; return value;} " +
            "constexpr int signed_narrow(){short value=32767; value+=1; return value;} " +
            "constexpr unsigned int bits(){unsigned int value=4294967295u; value+=1; value|=8; value^=3; value&=14; value*=3; value/=2; value%=7; value-=2; return value;} " +
            "constexpr bool boolean(){bool value=false; value+=2; value*=0; return value;} " +
            "static_assert(narrow()==4); static_assert(signed_narrow()==-32768); static_assert(bits()==4294967295u); static_assert(!boolean());");
        AllControlAssertions(result);
        AssignmentExpression narrowing = As<AssignmentExpression>(As<ExpressionStatement>(Function(result, "narrow").Body!.Statements[1]).Expression);
        Equal(IntegerKind.Int, narrowing.OperationType!.IntegerKind); Equal(IntegerKind.UnsignedChar, Kind(narrowing.Type));
        Check(narrowing.Target is ReferenceExpression); // destination is not replaced by a copied value
        AnalysisResult mixed = Success("constexpr int f(){short value=1; value+=2L; return value;} static_assert(f()==3);");
        AssignmentExpression wide = As<AssignmentExpression>(As<ExpressionStatement>(Function(mixed, "f").Body!.Statements[1]).Expression);
        Equal(IntegerKind.Long, wide.OperationType!.IntegerKind); Equal(IntegerKind.Long, Kind(wide.Value.Type));
        foreach (string operation in new[] { "value+=1", "value*=2", "value/=-1", "value%=-1" })
        {
            string initial = operation == "value+=1" || operation == "value*=2" ? "2147483647" : "(-2147483647-1)";
            AnalysisResult failure = Analyze($"constexpr int f(){{int value={initial}; {operation}; return value;}} static_assert(f()==0);");
            Equal(ConstantEvaluationStatus.UndefinedBehavior, SingleEvaluation(failure).Status);
        }
        AnalysisResult zero = Analyze("constexpr int f(){int value=3; value/=0; return value;} static_assert(f()==0);");
        Equal(ConstantEvaluationStatus.UndefinedBehavior, SingleEvaluation(zero).Status);
    }

    internal static void ScalarAssignmentCells()
    {
        AnalysisResult result = Success(Fixture("m4-controlflow-positive.cpp"));
        AllControlAssertions(result);
        ConstantEvaluationResult mutation = result.StaticAssertions[1].Evaluation;
        ConstantStorageObject value = mutation.StorageObjects.Single();
        Same(Function(result, "mutate").Parameters.Single().Symbol, value.Symbol!);
        Equal(new BigInteger(7), value.Value!.Value); Check(value.IsAlive == false && value.LifetimeBeganInEvaluation == true && value.IsReadOnly == false);
        Check(mutation.Assignments.All(a => ReferenceEquals(a.Object, value)));
        Check(mutation.ReferenceBindings.All(b => ReferenceEquals(b.Object, value)));
        Check(mutation.Assignments.Select(a => a.Value).SequenceEqual(new BigInteger[] { 2, 3, 7 }));
        AnalysisResult copy = Success("constexpr int change(int value){value=9; return value;} constexpr int f(){int original=2; int result=change(original); return original+result;} static_assert(f()==11);");
        AllControlAssertions(copy);
        ConstantStorageObject original = SingleEvaluation(copy).StorageObjects.Single(s => s.Symbol?.Name == "original");
        Equal(new BigInteger(2), original.Value!.Value);
        Check(SingleEvaluation(copy).Assignments.All(a => !ReferenceEquals(a.Object, original)));
        AnalysisResult init = Success("constexpr int f(){int value; int& alias=value; alias=4; return value;} static_assert(f()==4);");
        Check(SingleEvaluation(init).StorageObjects.Single().IsInitialized == true);
        AnalysisResult constAlias = Success("constexpr int f(){int value=1; const int& view=value; int& alias=value; alias=4; return view;} static_assert(f()==4);");
        Check(SingleEvaluation(constAlias).StorageObjects.Single().IsReadOnly == false);
        // Actual allocated const qualification defaults independently of the
        // read-access expression or whether the object is evaluation-local.
        using var session = new CompilationSession();
        IntegerType constType = As<IntegerType>(Variable(session.Analyze("const int stable=1;"), "stable").Type);
        var protectedCell = new ConstantCell(1, 0, constType, null, null, ConstantStorageLifetime.Static, true);
        Check(protectedCell.ReadOnly && new ConstantStorageObject(protectedCell).IsReadOnly == true);
    }

    internal static void ScalarAssignmentFailures()
    {
        foreach (var (fixture, code) in new[]
        {
            ("const-target", "ASG001"), ("non-lvalue", "ASG001"), ("invalid-conversion", "SEM024"), ("unselected-invalid", "ASG001")
        })
        {
            AnalysisResult result = Analyze(Fixture("m4-controlflow-" + fixture + ".cpp"));
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, fixture);
            Check(result.Diagnostics.Any(d => d.Code == code), fixture);
        }
        foreach (var (fixture, status) in new[]
        {
            ("global-mutation", ConstantEvaluationStatus.NotConstant), ("unknown-mutation", ConstantEvaluationStatus.NotConstant),
            ("expired-mutation", ConstantEvaluationStatus.ExpiredObject), ("uninitialized-read", ConstantEvaluationStatus.UninitializedRead)
        })
        {
            AnalysisResult result = Analyze(Fixture("m4-controlflow-" + fixture + ".cpp"));
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, fixture);
            Equal(status, SingleEvaluation(result).Status); Check(SingleEvaluation(result).Assignments.Count == 0);
        }
        AnalysisResult unknown = Analyze(Fixture("m4-controlflow-unknown-mutation.cpp"));
        ConstantStorageObject symbolic = SingleEvaluation(unknown).StorageObjects.Single();
        Check(!symbolic.IsKnownObject && symbolic.IsReadOnly == null && symbolic.LifetimeBeganInEvaluation == null && symbolic.Value == null);
        AnalysisResult oldLocal = Analyze("int outer(int value){static_assert((value=4)==4); return value;}");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(oldLocal).Status);
        Check(SingleEvaluation(oldLocal).StorageObjects.Single().LifetimeBeganInEvaluation == false);
        AnalysisResult staticTemporary = Analyze("int&& global=3; constexpr int f(){global=4; return 1;} static_assert(f()==1);");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(staticTemporary).Status);
        Check(SingleEvaluation(staticTemporary).StorageObjects.Single().LifetimeBeganInEvaluation == false);
        Check(SingleEvaluation(staticTemporary).Assignments.Count == 0);
        AnalysisResult localTemporary = Success("constexpr int f(){int&& local=1; local=2; return local;} static_assert(f()==2);");
        ConstantStorageObject temporary = SingleEvaluation(localTemporary).StorageObjects.Single();
        Check(temporary.LifetimeBeganInEvaluation == true && temporary.IsReadOnly == false && temporary.IsAlive == false);
        Same(temporary, SingleEvaluation(localTemporary).Assignments.Single().Object);
        Check(Success("int&& global=1; constexpr int f(){global=2; return 1;}").IsSuccess);
        AllControlAssertions(Success("int global=1; constexpr int f(){if(false) global=2; return 1;} static_assert(f()==1);"));
        AnalysisResult runtime = Success(Fixture("m4-controlflow-runtime-hazards.cpp"));
        Check(runtime.StaticAssertions.Count == 0); // hazards are not invented language errors
        Check(!Analyze("int&& result(); int f(){result()=2; return 1;}").IsSuccess);
    }

    internal static void ScalarAssignmentOrder()
    {
        AnalysisResult result = Success(Fixture("m4-controlflow-positive.cpp"));
        ConstantEvaluationResult order = result.StaticAssertions[0].Evaluation;
        Check(order.Assignments.Select(a => a.Value).SequenceEqual(new BigInteger[] { 11, 1, 11, 12 }));
        ConstantStorageObject target = order.StorageObjects.Single(s => s.Symbol?.Name == "value");
        Check(order.Assignments.All(a => ReferenceEquals(a.Object, target)));
        Equal(2, order.ReferenceBindings.Count(b => b.Expression.Context == ReferenceBindingContext.Return));
        AnalysisResult nesting = Success("constexpr int f(){int value=1; value=(value=3)+1; value+=(value=5); return value;} static_assert(f()==10);");
        AllControlAssertions(nesting);
        Check(SingleEvaluation(nesting).Assignments.Select(a => a.Value).SequenceEqual(new BigInteger[] { 3, 4, 5, 10 }));
    }

    internal static void ScalarAssignmentSequencing()
    {
        foreach (string expression in new[] { "value+(value=2)", "(value=2)+value", "(value=2)+(value=3)", "value+(true && (value=2))" })
        {
            AnalysisResult failed = Analyze($"constexpr int f(){{int value=1; return {expression};}} static_assert(f()==0);");
            Equal(ConstantEvaluationStatus.UndefinedBehavior, SingleEvaluation(failed).Status);
        }
        // Assignment operands and logical operands have C++20 sequencing; call
        // argument evaluations have a permitted deterministic order. Function
        // executions are separate from direct unsequenced caller accesses.
        AnalysisResult sequenced = Success("constexpr int write(int& value){value=2; return value;} constexpr int sum(int x,int y){return x+y;} " +
            "constexpr int f(){int value=1; value=value+1; if(value && (value=3)) return sum((value=4),(value=5)); return 0;} " +
            "constexpr int g(){int value=1; return write(value)+value;} static_assert(f()==9); static_assert(g()==4);");
        AllControlAssertions(sequenced);
        AnalysisResult skipped = Success("constexpr int f(){int value=1; false && (value=2); true || (value=3); return value;} static_assert(f()==1);");
        Check(SingleEvaluation(skipped).Assignments.Count == 0);
        Expression logical = As<ExpressionStatement>(Function(skipped, "f").Body!.Statements[1]).Expression;
        Check(logical.HasSideEffects && logical.ConstantValue == 0);
    }

    internal static void ControlIfSelection()
    {
        AnalysisResult result = Success("constexpr int choose(bool take){int value=0; if(take){value=3;} else {value=8;} return value;} " +
            "constexpr int nested(bool a,bool b){if(a) if(b) return 1; else return 2; return 3;} " +
            "constexpr int safe(bool choose){if(choose) return 4; else return 1/0;} " +
            "static_assert(choose(true)==3); static_assert(choose(false)==8); static_assert(nested(true,false)==2); static_assert(nested(false,true)==3); static_assert(safe(true)==4);");
        AllControlAssertions(result);
        IfStatement conditional = As<IfStatement>(Function(result, "choose").Body!.Statements[1]);
        Equal(IntegerKind.Bool, Kind(conditional.Condition.Type)); Check(conditional.Then != conditional.Else);
        ConstantControlFlowObservation selected = result.StaticAssertions[0].Evaluation.ControlFlow.Single(o => ReferenceEquals(o.Statement, conditional));
        Equal(ConstantControlFlowKind.Then, selected.Kind);
        Equal(ConstantControlFlowKind.Else, result.StaticAssertions[1].Evaluation.ControlFlow.Single(o => ReferenceEquals(o.Statement, conditional)).Kind);
        Check(result.StaticAssertions[0].Evaluation.Assignments.Single().Expression.Range.Spelling.Contains("value=3", StringComparison.Ordinal));
        foreach (string body in new[] { "if(false) missing=1;", "if(true) return 1; else {const int value=1; value=2;}", "if(false) {void f();}", "while(false){int value=0; value=nothing();}" })
        {
            AnalysisResult invalid = Analyze("void nothing(); constexpr int f(){" + body + " return 1;}");
            Check(!invalid.IsSuccess, body);
        }
    }

    internal static void ControlIfScopes()
    {
        AnalysisResult result = Success("constexpr int f(bool choose){int value=1; if(choose){int value=2; const int& temporary=3; value+=temporary;} else {int value=8;} return value;} " +
            "int local_scope(bool choose){if(choose) int hidden=3; else int hidden=4; int hidden=5; return hidden;} static_assert(f(true)==1); static_assert(f(false)==1);");
        AllControlAssertions(result);
        IfStatement conditional = As<IfStatement>(Function(result, "f").Body!.Statements[1]);
        Same(Function(result, "f").ParameterScope, conditional.Then.Scope.Parent!);
        Check(!ReferenceEquals(conditional.Then.Scope, conditional.Else!.Scope));
        ConstantEvaluationResult first = result.StaticAssertions[0].Evaluation;
        Check(first.StorageObjects.All(s => s.IsAlive == false));
        Check(first.StorageObjects.Any(s => s.Lifetime == ConstantStorageLifetime.ReferenceVariableTemporary && s.Value == 3));
        Check(first.StorageObjects.Where(s => s.Symbol?.Name == "value").Select(s => s.Value!.Value).SequenceEqual(new BigInteger[] { 1, 5 }));
        Check(!Analyze("int f(){if(true) int hidden=1; return hidden;}").IsSuccess);
        Check(!Analyze("int f(){if(true){int hidden=1;} else {return hidden;} return 1;}").IsSuccess);
        IfStatement implicitScopes = As<IfStatement>(Function(result, "local_scope").Body!.Statements[0]);
        Check(implicitScopes.Then.Scope.FindLocal("hidden") != null && implicitScopes.Else!.Scope.FindLocal("hidden") != null);
        Check(Function(result, "local_scope").ParameterScope.FindLocal("hidden") != null);
    }

    internal static void ControlWhile()
    {
        AnalysisResult result = Success("constexpr int total(int limit){int i=0; int value=0; while(i<limit){const int& item=i+1; value+=item; i+=1;} return value;} " +
            "constexpr int condition(){int i=0; while((i=i+1)<4){} return i;} " +
            "static_assert(total(4)==10); static_assert(total(0)==0); static_assert(condition()==4);");
        AllControlAssertions(result);
        WhileStatement loop = As<WhileStatement>(Function(result, "total").Body!.Statements[2]);
        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        Equal(4, execution.ControlFlow.Count(o => ReferenceEquals(o.Statement, loop) && o.Kind == ConstantControlFlowKind.LoopBody));
        Equal(1, execution.ControlFlow.Count(o => ReferenceEquals(o.Statement, loop) && o.Kind == ConstantControlFlowKind.LoopExit));
        ConstantStorageObject[] temporaries = execution.StorageObjects.Where(s => s.Lifetime == ConstantStorageLifetime.ReferenceVariableTemporary).ToArray();
        Equal(4, temporaries.Length); Equal(4, temporaries.Select(s => s.Identity).Distinct().Count());
        Check(temporaries.Select(s => s.Value!.Value).SequenceEqual(new BigInteger[] { 1, 2, 3, 4 })); Check(temporaries.All(s => s.IsAlive == false));
        Check(result.StaticAssertions[1].Evaluation.StorageObjects.All(s => s.Lifetime != ConstantStorageLifetime.ReferenceVariableTemporary));
        Equal(4, result.StaticAssertions[2].Evaluation.Assignments.Count);
    }

    internal static void ControlTransfers()
    {
        AnalysisResult result = Success("constexpr int nested(){int outer=0; int total=0; while(outer<3){outer+=1; const int& keep=outer; " +
            "int inner=0; while(inner<4){inner+=1; {const int& temporary=7; if(inner==2) continue; if(inner==3) break; total+=keep+temporary;}} total+=keep;} return total;} static_assert(nested()==33);");
        AllControlAssertions(result);
        FunctionDeclaration function = Function(result, "nested");
        WhileStatement[] loops = ControlStatements(function.Body!).OfType<WhileStatement>().ToArray(); Equal(2, loops.Length);
        BreakStatement breaking = ControlStatements(function.Body!).OfType<BreakStatement>().Single();
        ContinueStatement continuing = ControlStatements(function.Body!).OfType<ContinueStatement>().Single();
        Same(loops[1].Target, breaking.Target); Same(loops[1].Target, continuing.Target); Check(!ReferenceEquals(loops[0].Target, breaking.Target));
        ConstantEvaluationResult execution = SingleEvaluation(result);
        Equal(3, execution.ControlFlow.Count(o => o.Kind == ConstantControlFlowKind.Break));
        Equal(3, execution.ControlFlow.Count(o => o.Kind == ConstantControlFlowKind.Continue));
        Equal(3, execution.StorageObjects.Count(s => s.Symbol?.Name == "inner"));
        Equal(9, execution.StorageObjects.Count(s => s.LifetimeOwner?.Name == "temporary"));
        Check(execution.StorageObjects.All(s => s.IsAlive == false));
        Check(execution.Assignments.Where(a => a.Expression.Range.Spelling == "total+=keep").Select(a => a.Value).SequenceEqual(new BigInteger[] { 9, 20, 33 }));
    }

    internal static void ControlReturnCleanup()
    {
        AnalysisResult result = Success("constexpr int f(int limit){const int& root=9; while(limit){const int& per=3; {int inner=4; if(inner) return root+per+inner;} limit-=1;} return 0;} " +
            "constexpr bool probe(const int& value){return value!=0;} constexpr int condition(){if(probe(1)){const int& local=2; return local;} return 0;} " +
            "static_assert(f(1)==16); static_assert(condition()==2);");
        AllControlAssertions(result);
        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        Check(execution.StorageObjects.All(s => s.IsAlive == false));
        Equal(2, execution.StorageObjects.Count(s => s.Lifetime == ConstantStorageLifetime.ReferenceVariableTemporary));
        Equal(1, execution.ControlFlow.Count(o => o.Kind == ConstantControlFlowKind.Return));
        Check(!execution.ControlFlow.Any(o => o.Kind == ConstantControlFlowKind.LoopExit));
        Check(result.StaticAssertions[1].Evaluation.StorageObjects.Any(s => s.Lifetime == ConstantStorageLifetime.FullExpressionTemporary && s.IsAlive == false));
        AnalysisResult escaped = Analyze(Fixture("m4-controlflow-expired-mutation.cpp"));
        Equal(ConstantEvaluationStatus.ExpiredObject, SingleEvaluation(escaped).Status);
        Check(SingleEvaluation(escaped).StorageObjects.Where(s => s.Lifetime != ConstantStorageLifetime.Static).All(s => s.IsAlive == false));
        AnalysisResult end = Analyze("constexpr int f(bool branch){if(branch) return 1;} static_assert(f(false)==0);");
        Equal(ConstantEvaluationStatus.NotConstant, SingleEvaluation(end).Status);
    }

    internal static void ControlDoFor()
    {
        AnalysisResult result = Success(Fixture("m4-controlflow-positive.cpp")); AllControlAssertions(result);
        ForStatement counted = As<ForStatement>(Function(result, "counted").Body!.Statements[1]);
        Same(counted.Scope, As<DeclarationStatement>(counted.Initializer).Declaration.Symbol.Scope);
        Same(counted.Scope, counted.Body.Scope.Parent!);
        ConstantEvaluationResult forExecution = result.StaticAssertions[6].Evaluation;
        Equal(1, forExecution.StorageObjects.Count(s => s.Symbol?.Name == "i"));
        Equal(new BigInteger(4), forExecution.StorageObjects.Single(s => s.Symbol?.Name == "i").Value!.Value);
        Check(forExecution.StorageObjects.All(s => s.IsAlive == false));
        DoStatement once = As<DoStatement>(Function(result, "once").Body!.Statements[1]);
        Same(once.Target, ControlStatements(once.Body).OfType<ContinueStatement>().Single().Target);
        Same(once.Target, ControlStatements(once.Body).OfType<BreakStatement>().Single().Target);
        Equal(3, result.StaticAssertions[7].Evaluation.ControlFlow.Count(o => ReferenceEquals(o.Statement, once) && o.Kind == ConstantControlFlowKind.LoopBody));
        AnalysisResult omitted = Success("constexpr int f(){int i=0; for(;;){i+=1; if(i==3) break;} do {i+=1;} while(false); return i;} " +
            "constexpr int cleanup(){for(int i=0;i<3;i+=1){const int& local=i; if(i==1) return local;} return 9;} " +
            "static_assert(f()==4); static_assert(cleanup()==1);");
        AllControlAssertions(omitted); Check(omitted.StaticAssertions[1].Evaluation.StorageObjects.All(s => s.IsAlive == false));
        AnalysisResult scopes = Success("int f(){for(int i=0;i<1;i+=1){{int i=3;}} int i=4; return i;}"); Check(scopes.IsSuccess);
        Check(!Analyze("int f(){for(int i=0;i<1;i+=1){int i=2;} return 1;}").IsSuccess);
        Check(!Analyze("int f(){for(int i=0;i<1;i+=1){} return i;}").IsSuccess);
    }

    internal static void ControlBoundaries()
    {
        AnalysisResult transfer = Analyze(Fixture("m4-controlflow-invalid-transfer.cpp"));
        Check(!transfer.IsSuccess && !transfer.ContainsUnsupportedFeatures); Equal(2, transfer.Diagnostics.Count(d => d.Code == "CTL001"));
        foreach (string body in new[] { "if constexpr(true) return 1;", "if(int x=1) return x;", "if(1;true) return 1;", "if(;true) return 1;", "while(int x=1) break;", "for(;int x=1;) break;", "for(int x:items) break;", "switch(1){case 1: break;}", "int x=1; x>>=1;" })
        {
            AnalysisResult result = Analyze("int f(){" + body + " return 0;}");
            Check(!result.IsSuccess && result.ContainsUnsupportedFeatures, body);
        }
        foreach (string body in new[] { "if(true) return 1; else return nothing();", "while(nothing()) {}", "do {} while(nothing());", "for(;nothing();) {}", "else return 1;" })
            Check(!Analyze("void nothing(); int f(){" + body + " return 0;}").IsSuccess, body);
        AnalysisResult strayElse = Analyze("int f(){else return 1;}");
        Check(!strayElse.IsSuccess && !strayElse.ContainsUnsupportedFeatures && strayElse.Diagnostics.Any(d => d.Code == "CTL003"));
        // A failed loop/branch parse must restore loop and lexical owners.
        AnalysisResult recovered = Analyze("int f(){while(true){int x=; break;} continue; return 0;} int after(){return 3;}");
        Check(recovered.Diagnostics.Any(d => d.Code == "CTL001")); Check(Function(recovered, "after").Body != null);
    }

    internal static void ControlLimits()
    {
        using var limited = new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 160 });
        foreach (string source in new[]
        {
            Fixture("m4-controlflow-infinite.cpp"),
            "constexpr int f(bool run){for(;run;){} return 1;} static_assert(f(true)==1);",
            "constexpr int f(bool run){do {} while(run); return 1;} static_assert(f(true)==1);"
        })
        {
            AnalysisResult result = limited.Analyze(source); Equal(ConstantEvaluationStatus.ResourceLimit, SingleEvaluation(result).Status);
            Check(SingleEvaluation(result).Steps > 160); Check(SingleEvaluation(result).StorageObjects.All(s => s.IsAlive == false));
        }
        AnalysisResult finite = limited.Analyze("constexpr int f(){int i=0; while(i<2) i+=1; return i;} static_assert(f()==2);");
        AllControlAssertions(finite);
        using var depth = new CompilationSession(new AnalysisOptions { ConstantEvaluationCallLimit = 4 });
        AnalysisResult recursive = depth.Analyze("constexpr int f(int value){if(value) return f(value-1); return 0;} static_assert(f(8)==0);");
        Equal(ConstantEvaluationStatus.ResourceLimit, SingleEvaluation(recursive).Status); Equal(4, SingleEvaluation(recursive).MaximumCallDepth);
        Check(SingleEvaluation(recursive).StorageObjects.All(s => s.IsAlive == false));
        AnalysisResult success = depth.Analyze("constexpr int f(int value){if(value) return f(value-1); return 0;} static_assert(f(2)==0);");
        AllControlAssertions(success);
        // Bounded flat-source adversary: a deep graph without deeply nested
        // grammar. The tiny work budget must win, and public fact queries must
        // not recurse over the left spine on the CLR stack.
        const int terms = 2048;
        string sum = string.Join("+", Enumerable.Repeat("1", terms));
        using var small = new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 12 });
        AnalysisResult flat = small.Analyze($"constexpr int f(){{int value; value={sum}; return value;}} static_assert(f()=={terms});");
        Equal(ConstantEvaluationStatus.ResourceLimit, SingleEvaluation(flat).Status);
        AssignmentExpression assignment = As<AssignmentExpression>(As<ExpressionStatement>(Function(flat, "f").Body!.Statements[1]).Expression);
        Check(!assignment.IsValueDependent && assignment.HasSideEffects);
        Check(!assignment.Value.IsValueDependent && !assignment.Value.HasSideEffects);
        var reused = new ConstantInterpreter(new AnalysisOptions { ConstantEvaluationStepLimit = 12 });
        Equal(ConstantEvaluationStatus.ResourceLimit, reused.Evaluate(flat.StaticAssertions.Single().Expression).Status);
        Expression simple = Success("static_assert(1);").StaticAssertions.Single().Expression;
        Equal(ConstantEvaluationStatus.Success, reused.Evaluate(simple).Status);
        Equal(ConstantEvaluationStatus.ResourceLimit, reused.Evaluate(flat.StaticAssertions.Single().Expression).Status);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DropControlResult(CompilationSession session, ConstantInterpreter interpreter, bool fail)
    {
        AnalysisResult result = session.Analyze(fail ? Fixture("m4-controlflow-uninitialized-read.cpp") : Fixture("m4-controlflow-combined.cpp"));
        ConstantEvaluationResult evaluation = interpreter.Evaluate(result.StaticAssertions[0].Expression);
        Equal(fail ? ConstantEvaluationStatus.UninitializedRead : ConstantEvaluationStatus.Success, evaluation.Status);
        return new WeakReference<Scope>(result.GlobalScope);
    }
    internal static void ControlLifetime()
    {
        using var session = new CompilationSession();
        var interpreter = new ConstantInterpreter(new AnalysisOptions());
        AnalysisResult result = session.Analyze(Fixture("m4-controlflow-positive.cpp"));
        Expression expression = result.StaticAssertions[0].Expression;
        ConstantEvaluationResult first = interpreter.Evaluate(expression);
        AnalysisResult failed = session.Analyze(Fixture("m4-controlflow-uninitialized-read.cpp"));
        Equal(ConstantEvaluationStatus.UninitializedRead, interpreter.Evaluate(failed.StaticAssertions.Single().Expression).Status);
        ConstantEvaluationResult again = interpreter.Evaluate(expression);
        Equal(first.Steps, again.Steps); Equal(first.Assignments.Count, again.Assignments.Count);
        Check(!ReferenceEquals(first.Assignments[0].Object, again.Assignments[0].Object));
        Check(first.Assignments.Select(a => a.Value).SequenceEqual(again.Assignments.Select(a => a.Value)));
        WeakReference<Scope>[] dropped = [DropControlResult(session, interpreter, false), DropControlResult(session, interpreter, true)];
        for (int i = 0; i < 3; ++i) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(dropped.All(weak => !weak.TryGetTarget(out _)), "Live session/interpreter retained discarded control-flow source graphs.");
        session.Dispose();
        Check(first.StorageObjects.All(s => s.IsAlive == false)); Equal(new BigInteger(12), first.Assignments.Last().Value);
        Check(first.ControlFlow.Any(o => o.Kind == ConstantControlFlowKind.Return));
        GC.KeepAlive(session); GC.KeepAlive(interpreter);
    }

    internal static void ControlCombined()
    {
        ControlArrayCombined();
        AnalysisResult result = Success(Fixture("m4-controlflow-combined.cpp")); AllControlAssertions(result);
        FunctionTemplateDeclaration template = Template(result, "accumulate");
        Check(template.Instances.Select(i => Kind(i.TypeArguments.Single())).SequenceEqual(new[] { IntegerKind.Int, IntegerKind.Long }));
        foreach (FunctionTemplateInstance instance in template.Instances)
        {
            Equal(1, instance.BodyReplayCount); Equal(TemplateInstanceState.Complete, instance.State);
            WhileStatement loop = ControlStatements(instance.Declaration!.Body!).OfType<WhileStatement>().Single();
            VariableDeclaration hold = ControlStatements(loop.Body).OfType<DeclarationStatement>().Select(s => s.Declaration).OfType<VariableDeclaration>().Single(v => v.Name == "hold");
            CallExpression call = As<CallExpression>(hold.Initializer); Equal(2, call.Candidates.Count);
            ReferenceType selected = As<ReferenceType>(call.SelectedCandidate.FunctionType.ParameterTypes.Single());
            Equal(TypeQualifiers.Const, selected.ReferredType.Qualifiers);
            Check(loop.Body.Statements.OfType<ExpressionStatement>().Any(s => s.Expression is AssignmentExpression));
            Check(ControlStatements(loop.Body).OfType<ContinueStatement>().All(s => ReferenceEquals(s.Target, loop.Target)));
        }
        ConstantEvaluationResult evaluation = result.StaticAssertions[0].Evaluation;
        ConstantStorageObject value = evaluation.StorageObjects.Single(s => s.Symbol?.Name == "value");
        Equal(new BigInteger(11), value.Value!.Value);
        Check(evaluation.ReferenceBindings.Any(b => ReferenceEquals(b.Object, value)));
        Check(evaluation.Assignments.Any(a => ReferenceEquals(a.Object, value) && a.Value == 11));
        Check(evaluation.ControlFlow.Any(o => o.Kind == ConstantControlFlowKind.Break) && evaluation.ControlFlow.Any(o => o.Kind == ConstantControlFlowKind.Continue));
        Check(evaluation.StorageObjects.All(s => s.IsAlive == false));
        Check(evaluation.Assignments.Any(a => a.Expression.Range.File.OriginForRange(a.Expression.Range).ExpansionHistory.Any(frame => frame.Name == "STORE")));
        Check(template.BodyTokens.Tokens.Any(t => t.Origin.ExpansionHistory.Any(frame => frame.Name == "STEP")));
        Throws<NotSupportedException>(() => ((IList)evaluation.ControlFlow).Clear());
    }
}
