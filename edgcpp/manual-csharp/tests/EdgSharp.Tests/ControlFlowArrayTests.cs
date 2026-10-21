// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored additive array/reference/control integration over exact source bytes.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static void ControlArrayCombined()
    {
        var provider = new InMemoryIncludeProvider(new Dictionary<string,string>
        {
            ["m4-controlflow-array-gate.h"] = Fixture("m4-controlflow-array-gate.h")
        });
        AnalysisResult result;
        using(var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider:provider) }))
            result = session.Analyze(Fixture("m4-controlflow-array-gate.cpp"),"m4-controlflow-array-gate.cpp");
        Check(result.IsSuccess,string.Join("; ",result.Diagnostics.Select(d=>d.Code+": "+d.Message)));
        Equal(5,result.StaticAssertions.Count);
        Check(result.StaticAssertions.All(assertion=>assertion.Evaluation.Status==ConstantEvaluationStatus.Success&&assertion.Evaluation.Value==1));
        FunctionTemplateDeclaration collect = Template(result,"collect");
        Check(collect.Instances.Select(instance=>Kind(instance.TypeArguments.Single())).SequenceEqual(new[]{IntegerKind.Int,IntegerKind.Long}));
        foreach(FunctionTemplateInstance instance in collect.Instances)
        {
            Equal(1,instance.BodyReplayCount);
            WhileStatement loop = ControlStatements(instance.Declaration!.Body!).OfType<WhileStatement>().Single();
            AssignmentExpression accumulating = loop.Body.Statements.OfType<ExpressionStatement>().Select(statement=>statement.Expression).OfType<AssignmentExpression>().Single(expression=>expression.Target is ReferenceExpression {ReferencedSymbol.Name:"total"});
            Expression operand = accumulating.Value;
            while(operand is ConversionExpression conversion) operand = conversion.Operand;
            CallExpression selected = As<CallExpression>(operand);
            Equal(2,selected.Candidates.Count);
            Equal(IntegerKind.Char16,Kind(ReferenceParameter(selected).ReferredType));
            Equal(TypeQualifiers.Const,ReferenceParameter(selected).ReferredType.Qualifiers);
            Check(selected.SelectedCandidate.IsViable);
        }
        Equal(3,result.GlobalScope.FindLocalBinding("helper")!.Symbols.Count);
        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        ConstantArrayObject array = execution.Arrays.Single(item=>item.Symbol?.Name=="cells");
        Check(array.Elements.Select(element=>element.Value!.Value).SequenceEqual(new BigInteger[]{97,98,101,0}));
        Check(array.Elements.All(element=>element.IsReadOnly==false&&element.LifetimeBeganInEvaluation==true&&element.IsAlive==false));
        ConstantAssignmentObservation[] elementStores = execution.Assignments.Where(assignment=>array.Elements.Any(element=>ReferenceEquals(element,assignment.Object))).ToArray();
        Equal(2,elementStores.Length); Same(array.Elements[0],elementStores[0].Object); Same(array.Elements[2],elementStores[1].Object);
        Check(elementStores.Select(store=>store.Value).SequenceEqual(new BigInteger[]{97,101}));
        Check(execution.ReferenceBindings.Any(binding=>ReferenceEquals(array.Elements[2],binding.Object)));
        Check(execution.ControlFlow.Any(observation=>observation.Kind==ConstantControlFlowKind.Continue));
        Check(execution.ControlFlow.Any(observation=>observation.Kind==ConstantControlFlowKind.Break));
        Check(elementStores.All(store=>store.Expression.Range.File.OriginForRange(store.Expression.Range).ExpansionHistory.Any(frame=>frame.Name=="STORE")));
        ArrayInitializationExpression initializer = As<ArrayInitializationExpression>(Local(collect.Instances[0].Declaration!,"cells").Initializer);
        Equal(2,initializer.Literal.Parts.Count);
        Check(initializer.Literal.Parts.All(part=>part.Token.Origin.ExpansionHistory.Any(frame=>frame.Name=="DATA")));
        Check(collect.BodyTokens.Tokens.Any(token=>token.Origin.ExpansionHistory.Any(frame=>frame.Name=="RET")));

        ConstantEvaluationResult reversed = result.StaticAssertions[4].Evaluation;
        ConstantArrayObject reversedArray = reversed.Arrays.Single(item=>item.Symbol?.Name=="cells");
        ConstantAssignmentObservation arrayStore = reversed.Assignments.Single(assignment=>ReferenceEquals(reversedArray.Elements[2],assignment.Object));
        SubscriptExpression destination = As<SubscriptExpression>(arrayStore.Expression.Target);
        Check(destination.PointerOperandIsSecond&&destination.SourceLeft.HasSideEffects&&destination.SourceRight.HasSideEffects);
        Check(!destination.IsValueDependent&&destination.HasSideEffects);
        Check(reversed.Assignments.Where(assignment=>assignment.Object.Symbol?.Name=="counter").Select(assignment=>assignment.Value).SequenceEqual(new BigInteger[]{1,2}));
        Equal(new BigInteger(2),reversed.StorageObjects.Single(cell=>cell.Symbol?.Name=="counter").Value!.Value);
        Equal(new BigInteger(90),reversedArray.Elements[2].Value!.Value);
        Check(reversedArray.Elements.All(element=>element.IsAlive==false&&element.LifetimeBeganInEvaluation==true));

        foreach(var (name,status) in new[]{("expired",ConstantEvaluationStatus.ExpiredObject),("nonobject",ConstantEvaluationStatus.UndefinedBehavior)})
        {
            AnalysisResult invalid = Analyze(Fixture("m4-controlflow-array-"+name+".cpp"));
            Check(!invalid.IsSuccess&&!invalid.ContainsUnsupportedFeatures);
            ConstantEvaluationResult failure = invalid.StaticAssertions.Single().Evaluation;
            Equal(status,failure.Status); Equal(0,failure.Assignments.Count);
            Check(failure.Arrays.All(item=>item.Elements.All(element=>element.IsAlive==false)));
        }
        AnalysisResult readonlyLiteral = Analyze(Fixture("m4-controlflow-array-readonly.cpp"));
        Check(!readonlyLiteral.IsSuccess&&!readonlyLiteral.ContainsUnsupportedFeatures&&readonlyLiteral.Diagnostics.Any(d=>d.Code=="ASG001"));
        Check(Analyze("template<class T> int dependent(T x){int values[x];return 1;}").ContainsUnsupportedFeatures);
        Check(Analyze("template<class T> int dependent(T x){T values[2];return 1;}").ContainsUnsupportedFeatures);

        AnalysisResult noEffects = Success("constexpr int f(){int index=0;char a[2]=\"a\";sizeof(a[index=1]);return index;} static_assert(f()==0);");
        Expression size = As<ExpressionStatement>(Function(noEffects,"f").Body!.Statements[2]).Expression;
        Check(!size.HasSideEffects&&!size.IsValueDependent);
        Equal(0,noEffects.StaticAssertions.Single().Evaluation.Assignments.Count);
        Check(Success("constexpr int f(){char a[2]=\"a\";return a[0];} static_assert(f()=='a');").StaticAssertions.Single().Evaluation.Arrays.Single().Elements.All(element=>element.LifetimeBeganInEvaluation==true));

        using var limited = new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 12 });
        string sum = string.Join("+",Enumerable.Repeat("1",2048));
        AnalysisResult bounded = limited.Analyze("constexpr int f(){char a[2]=\"a\";a["+sum+"]='Z';return 1;} static_assert(f()==1);");
        Equal(ConstantEvaluationStatus.ResourceLimit,bounded.StaticAssertions.Single().Evaluation.Status);
        AssignmentExpression assignmentGraph = As<AssignmentExpression>(As<ExpressionStatement>(Function(bounded,"f").Body!.Statements[1]).Expression);
        Check(!assignmentGraph.Target.HasSideEffects&&!assignmentGraph.Target.IsValueDependent);
        var reused = new ConstantInterpreter(new AnalysisOptions { ConstantEvaluationStepLimit = 12 });
        ConstantEvaluationResult failed = reused.Evaluate(bounded.StaticAssertions.Single().Expression);
        AnalysisResult simple = Success("static_assert(sizeof(\"abc\")==4);");
        Equal(ConstantEvaluationStatus.Success,reused.Evaluate(simple.StaticAssertions.Single().Expression).Status);
        ConstantEvaluationResult again = reused.Evaluate(bounded.StaticAssertions.Single().Expression);
        Equal(failed.Status,again.Status); Equal(failed.Steps,again.Steps);
        Check(again.StorageObjects.All(cell=>cell.IsAlive==false));
    }
}
