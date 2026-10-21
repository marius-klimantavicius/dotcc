// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored macro/replay/frozen lookup/reference/array/control floating gate.
using EdgSharp;

internal static partial class Tests
{
    internal static void FloatingFrontendCombined()
    {
        AnalysisResult result;
        var provider = new InMemoryIncludeProvider(new Dictionary<string,string>
        { ["m4-floating-frontend-gate.h"] = Fixture("m4-floating-frontend-gate.h") });
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: provider) }))
            result = session.Analyze(Fixture("m4-floating-frontend-gate.cpp"), "m4-floating-frontend-gate.cpp");
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(diagnostic => diagnostic.Code + ": " + diagnostic.Message)));
        Equal(6, result.StaticAssertions.Count); FloatAssertions(result);
        FunctionTemplateDeclaration template = Template(result, "accumulate");
        Equal(2, template.Instances.Count);
        Check(template.Instances.Select(instance => As<FloatingType>(instance.TypeArguments.Single()).FloatingKind)
            .SequenceEqual(new[] { FloatingKind.Float, FloatingKind.Double }));
        Equal(3, result.GlobalScope.FindLocalBinding("echo")!.Symbols.Count);
        foreach (FunctionTemplateInstance instance in template.Instances)
        {
            Equal(1, instance.BodyReplayCount); Equal(TemplateInstanceState.Complete, instance.State);
            WhileStatement loop = ControlStatements(instance.Declaration!.Body!).OfType<WhileStatement>().Single();
            AssignmentExpression assign = As<AssignmentExpression>(As<ExpressionStatement>(loop.Body.Statements[0]).Expression);
            BinaryExpression arithmetic = As<BinaryExpression>(assign.Value);
            CallExpression selected = As<CallExpression>(As<ConversionExpression>(arithmetic.Left).Operand);
            Equal(2, selected.Candidates.Count);
            ReferenceType parameter = As<ReferenceType>(selected.SelectedCandidate.FunctionType.ParameterTypes.Single());
            Equal(As<FloatingType>(instance.TypeArguments.Single()).FloatingKind, As<FloatingType>(parameter.ReferredType).FloatingKind);
            Equal(TypeQualifiers.Const, parameter.ReferredType.Qualifiers);
            Equal(StandardConversionRank.ExactMatch, selected.SelectedCandidate.ArgumentConversions.Single().Rank);
            Check(template.DefinitionContext.Bindings["echo"].Contains(selected.SelectedFunction));
            Same(selected.SelectedFunction, selected.SelectedCandidate.Function);
        }
        Check(template.BodyTokens.Tokens.Any(token => token.Origin.ExpansionHistory.Any(frame => frame.Name == "STEP")));
        Check(template.BodyTokens.Tokens.Any(token => token.Origin.ExpansionHistory.Any(frame => frame.Name == "RET")));
        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        ConstantArrayObject array = execution.Arrays.Single(item => item.Symbol?.Name == "cells");
        Check(array.Elements.All(cell => cell.Value == null && cell.FloatingValue != null && cell.IsReadOnly == false && cell.IsAlive == false && cell.LifetimeBeganInEvaluation == true));
        Equal(0x40600000UL, array.Elements[0].FloatingValue!.Bits);
        Equal(0x40400000UL, array.Elements[1].FloatingValue!.Bits);
        ConstantReferenceBinding alias = execution.ReferenceBindings.Single(binding => binding.Expression.Context == ReferenceBindingContext.Variable);
        Same(array.Elements[0], alias.Object);
        ConstantAssignmentObservation[] aliasStores = execution.Assignments.Where(store => store.Expression.Range.File.OriginForRange(store.Expression.Range).ExpansionHistory.Any(frame => frame.Name == "STORE")).ToArray();
        Equal(2, aliasStores.Length); Check(aliasStores.All(store => ReferenceEquals(array.Elements[0], store.Object) && store.IntegerValue == null));
        Check(aliasStores.Select(store => store.FloatingValue!.Bits).SequenceEqual(new ulong[] { 0x40400000, 0x40600000 }));
        Check(execution.ControlFlow.Any(observation => observation.Kind == ConstantControlFlowKind.LoopBody));
        foreach (string name in new[] { "global", "range" })
        {
            AnalysisResult invalid = Analyze(Fixture("m4-floating-frontend-" + name + ".cpp"));
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, name);
        }
        Check(Analyze(Fixture("m4-floating-frontend-overload.cpp")).IsSuccess);
        Check(Analyze(Fixture("m4-floating-frontend-divide.cpp")).IsSuccess);

        using var reused = new CompilationSession();
        const string source = "template<class T> constexpr T id(T x){return x;}static_assert(id(1.5f)==1.5f);";
        AnalysisResult first = reused.Analyze(source), second = reused.Analyze(source);
        Check(first.IsSuccess && second.IsSuccess);
        FunctionTemplateInstance one = Template(first, "id").Instances.Single(), two = Template(second, "id").Instances.Single();
        Check(!ReferenceEquals(one, two) && !ReferenceEquals(one.Symbol, two.Symbol)); Same(one.TypeArguments[0], two.TypeArguments[0]);
        Task<AnalysisResult>[] concurrent = Enumerable.Range(0, 6).Select(_ => Task.Run(() =>
        {
            using var independent = new CompilationSession(); return independent.Analyze(source);
        })).ToArray();
        Task.WaitAll(concurrent);
        Check(concurrent.All(task => task.Result.IsSuccess));
        Check(!ReferenceEquals(Template(concurrent[0].Result, "id").Instances[0].TypeArguments[0], Template(concurrent[1].Result, "id").Instances[0].TypeArguments[0]));
    }
}
