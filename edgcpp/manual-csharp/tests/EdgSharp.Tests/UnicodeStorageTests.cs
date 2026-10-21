// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored integration controls over the unchanged Unicode/storage fixtures.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static void UnicodeReferenceStorageGate()
    {
        var provider = new InMemoryIncludeProvider(new Dictionary<string, string>
        {
            ["m1-unicode-storage-gate.h"] = Fixture("m1-unicode-storage-gate.h")
        });
        AnalysisResult result, invalid;
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: provider) }))
        {
            result = session.Analyze(Fixture("m1-unicode-storage-gate.cpp"), "m1-unicode-storage-gate.cpp");
            invalid = session.Analyze(Fixture("m1-unicode-storage-invalid-read.cpp"), "m1-unicode-storage-invalid-read.cpp");
        }
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        Equal(5, result.StaticAssertions.Count);
        Check(result.StaticAssertions.All(assertion => assertion.Evaluation.Status == ConstantEvaluationStatus.Success && assertion.Evaluation.Value == BigInteger.One));
        Equal(2, result.PhysicalSources.Count); Equal(1, result.Preprocessing.Includes.Count);

        FunctionTemplateDeclaration copy = Template(result, "αντίγραφο");
        Same(copy.Symbol, result.GlobalScope.FindLocal(@"\u03b1ντίγραφο")!);
        Check(copy.Instances.Select(instance => Kind(instance.TypeArguments.Single())).SequenceEqual(new[] { IntegerKind.Char16, IntegerKind.Long }));
        foreach (FunctionTemplateInstance instance in copy.Instances)
        {
            Equal(1, instance.BodyReplayCount);
            CallExpression selected = As<CallExpression>(As<ConversionExpression>(ReturnValue(instance.Declaration!)).Operand);
            Equal(2, selected.Candidates.Count);
            Equal("ηχώ", selected.SelectedFunction.Name);
            Same(selected.SelectedFunction, result.GlobalScope.FindLocalBinding(@"\u03b7χώ")!.Symbols.Single(symbol => ReferenceEquals(symbol, selected.SelectedFunction)));
            ReferenceType parameter = ReferenceParameter(selected);
            Equal(ReferenceKind.LValue, parameter.ReferenceKind);
            Equal(TypeQualifiers.Const, parameter.ReferredType.Qualifiers);
            Equal(Kind(instance.TypeArguments.Single()), Kind(parameter.ReferredType));
            Equal(StandardConversionRank.ExactMatch, selected.SelectedCandidate.ArgumentConversions.Single().Rank);
        }
        Equal(3, result.GlobalScope.FindLocalBinding("ηχώ")!.Symbols.Count);
        Check(copy.BodyTokens.Tokens.Any(token => token.IdentifierText == "ηχώ" && token.Spelling == @"\u03b7\u03c7\u03ce"));
        Token retained = copy.BodyTokens.Tokens.First(token => token.IdentifierText == "ηχώ");
        Check(retained.Origin.SpellingRanges.Any(range => range.Spelling == "ηχώ"));
        Check(retained.Origin.ExpansionHistory.Any(frame => frame.Name == "ΕΠΙΣΤΡΟΦΗ"));
        Check(retained.Origin.ExpansionNotes.Any(note => note.Contains("included from", StringComparison.Ordinal)));
        Equal(OriginalSourceEncoding.ManagedUtf16, retained.Origin.PrimaryPhysicalRange.File.Encoding.OriginalEncoding);

        ConstantEvaluationResult execution = result.StaticAssertions[0].Evaluation;
        ConstantStorageObject local = execution.StorageObjects.Single(storage => storage.Symbol is { Name: "τιμή" } symbol && symbol.Scope.Kind == ScopeKind.Function && storage.Invocation == 1);
        Equal(IntegerKind.Char16, local.Type.IntegerKind); Equal(new BigInteger(233), local.Value!.Value);
        Check(local.IsAlive == false);
        ConstantReferenceBinding alias = execution.ReferenceBindings.Single(binding => binding.Expression.Context == ReferenceBindingContext.Variable);
        Same(local, alias.Object);
        Check(execution.ReferenceBindings.Any(binding => binding.Expression.Context == ReferenceBindingContext.Return && binding.Expression.Range.File.OriginForRange(binding.Expression.Range).ExpansionHistory.Any(frame => frame.Name == "ΕΠΙΣΤΡΟΦΗ")));
        Check(execution.StorageObjects.All(storage => storage.IsKnownObject && storage.IsAlive == false));
        Check(result.StaticAssertions[3].Evaluation.StorageObjects.Count == 0);
        ConstantStorageObject global = result.StaticAssertions[4].Evaluation.StorageObjects.Single();
        Same(Variable(result, "κόσμος").Symbol, global.Symbol!);
        Equal(ConstantStorageLifetime.Static, global.Lifetime);
        Check(global.IsAlive == true && !global.IsConstantReadable && global.Value == null);
        Check(result.StaticAssertions[4].Evaluation.ReferenceBindings.All(binding => ReferenceEquals(global, binding.Object)));

        Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.NotConstant, invalid.StaticAssertions.Single().Evaluation.Status);
        Check(invalid.Diagnostics.Any(diagnostic => diagnostic.Code == "CEX001"));
        Check(!ReferenceEquals(copy.Symbol, Template(invalid, "αντίγραφο").Symbol));
        Check(Template(invalid, "αντίγραφο").Instances.Count == 0);
    }
}
