// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored pinned IEEE/strict-policy and operation-vs-literal range controls.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static void FloatingExceptionPolicies()
    {
        AnalysisResult ieee;
        using (var session = new CompilationSession())
        {
            Equal(FloatingExceptionPolicy.Ieee, session.Options.FloatingExceptions);
            ieee = session.Analyze(Fixture("m4-floating-frontend-ieee.cpp"));
        }
        Check(ieee.IsSuccess, string.Join("; ", ieee.Diagnostics.Select(d => d.Message))); FloatAssertions(ieee);
        FloatingConstantValue infinity = FloatInitializer(ieee, "infinity"), nan = FloatInitializer(ieee, "not_number");
        Equal(FloatingClassification.Infinity, infinity.Classification); Equal(0x7ff0000000000000UL, infinity.Bits);
        Equal(FloatingClassification.NaN, nan.Classification); Equal(0x7ff8000000000000UL, nan.Bits);
        Equal(0x7f800000UL, FloatInitializer(ieee, "narrow_infinity").Bits);
        Check(Variable(ieee,"infinity").ConstantEvaluation!.FloatingConditions.HasFlag(FloatingEvaluationCondition.DivideByZero));
        Check(Variable(ieee,"not_number").ConstantEvaluation!.FloatingConditions.HasFlag(FloatingEvaluationCondition.InvalidOperation));
        Equal(BigInteger.One, Variable(ieee,"truth").ConstantEvaluation!.Value!.Value);
        ConstantEvaluationResult carried = Variable(ieee,"carried").ConstantEvaluation!;
        Equal(FloatingClassification.NaN, carried.FloatingValue!.Classification); Check(carried.Value == null);
        ConstantArrayObject array = carried.Arrays.Single(item => item.Symbol?.Name == "cells");
        Check(array.Elements.All(cell => cell.Value == null && cell.IsAlive == false && cell.FloatingValue != null));
        Equal(FloatingClassification.Infinity, array.Elements[0].FloatingValue!.Classification);
        Equal(FloatingClassification.NaN, array.Elements[1].FloatingValue!.Classification);
        Same(array.Elements[0], carried.ReferenceBindings.Single(binding => binding.Expression.Context == ReferenceBindingContext.Variable).Object);
        Check(carried.Assignments.All(store => store.IntegerValue == null && store.FloatingValue != null));

        foreach (FloatingExceptionPolicy policy in new[] { FloatingExceptionPolicy.Ieee, FloatingExceptionPolicy.Strict })
        {
            using var session = new CompilationSession(new AnalysisOptions { FloatingExceptions = policy });
            foreach (string fixture in new[] { "underflow", "conversion-underflow", "range" })
            {
                AnalysisResult failure = session.Analyze(Fixture("m4-floating-frontend-" + fixture + ".cpp"));
                Check(!failure.IsSuccess && !failure.ContainsUnsupportedFeatures, fixture + " " + policy);
                Equal(ConstantEvaluationStatus.UndefinedBehavior, Variable(failure,fixture=="range"?"value":"lost").ConstantEvaluation!.Status);
            }
            AnalysisResult literal = session.Analyze("constexpr float zero=0x1p-150f;constexpr float tiny=0x3p-149f/2.0f;static_assert(zero==0.0f);static_assert(tiny==0x1p-148f);");
            Check(literal.IsSuccess); FloatAssertions(literal); Equal(0UL,FloatInitializer(literal,"zero").Bits); Equal(2UL,FloatInitializer(literal,"tiny").Bits);
            Check(Variable(literal,"tiny").ConstantEvaluation!.FloatingConditions.HasFlag(FloatingEvaluationCondition.Underflow));
            foreach (string source in new[] { "constexpr int bad=1.0/0.0;", "constexpr int bad=0.0/0.0;", "constexpr float bad=0x1p127f*2.0f;" })
            { AnalysisResult failure = session.Analyze(source); Check(!failure.IsSuccess && !failure.ContainsUnsupportedFeatures); Equal(ConstantEvaluationStatus.UndefinedBehavior,Variable(failure,"bad").ConstantEvaluation!.Status); }
            Check(!session.Analyze("float value=1e1000f;").IsSuccess);
            foreach (string spelling in new[] { "1e-46f", "1e-1000f" })
            {
                AnalysisResult decimalZero = session.Analyze("constexpr float lost="+spelling+";");
                Check(!decimalZero.IsSuccess&&!decimalZero.ContainsUnsupportedFeatures);
                Check(decimalZero.Diagnostics.Any(d=>d.Code=="FLT002"&&d.Class==DiagnosticClass.InvalidSource));
                Check(!session.Analyze("static_assert(sizeof("+spelling+")==4);").IsSuccess);
                AnalysisResult macroRange = session.Analyze("#define LOST "+spelling+"\nfloat value=LOST;");
                Check(macroRange.Diagnostics.Any(d=>d.Code=="FLT002"&&d.Origin.ExpansionHistory.Any(frame=>frame.Name=="LOST")));
                Check(session.Analyze("#define DROP(x) 1\n#define UNUSED "+spelling+"\n#if 0\n"+spelling+"\n#endif\nint value=DROP("+spelling+");").IsSuccess);
            }
            AnalysisResult decimalTiny = session.Analyze("constexpr float tiny=1e-45f;static_assert(tiny>0.0f);");
            Check(decimalTiny.IsSuccess); Equal(1UL,FloatInitializer(decimalTiny,"tiny").Bits);
            Check(session.Analyze("static_assert(sizeof(0x1p-150f)==4);").IsSuccess);
            AnalysisResult unevaluatedTiny = session.Analyze("static_assert(sizeof(0x1p-149f/2.0f)==4);");
            Check(unevaluatedTiny.IsSuccess&&unevaluatedTiny.Diagnostics.All(d=>d.Code!="FLT006"));
        }

        using var strict = new CompilationSession(new AnalysisOptions { FloatingExceptions = FloatingExceptionPolicy.Strict });
        AnalysisResult strictRequired = strict.Analyze(Fixture("m4-floating-frontend-divide.cpp"));
        Check(!strictRequired.IsSuccess && !strictRequired.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.UndefinedBehavior, strictRequired.StaticAssertions.Single().Evaluation.Status);
        AnalysisResult strictNan = strict.Analyze("constexpr double nan=0.0/0.0;");
        Equal(ConstantEvaluationStatus.UndefinedBehavior,Variable(strictNan,"nan").ConstantEvaluation!.Status);
        AnalysisResult directRuntime = Success("double infinity=1.0/0.0;");
        Equal(0,directRuntime.Diagnostics.Count);
        AnalysisResult runtime = Success(Fixture("m4-floating-frontend-runtime.cpp"));
        Check(runtime.Diagnostics.Any(d => d.Code == "FLT005" && d.Severity == DiagnosticSeverity.Warning));
        Equal(2,runtime.Diagnostics.Count(d => d.Code == "FLT006" && d.Severity == DiagnosticSeverity.Warning));
        Check(runtime.Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error));
        Check(Variable(runtime,"infinity").ConstantEvaluation == null);
        Check(strict.Analyze("double divide(double value){return value/0.0;}").IsSuccess);
        AnalysisResult unevaluated = Success("static_assert(sizeof(1.0/0.0)==8);");
        Check(unevaluated.Diagnostics.Count == 0);
        AnalysisResult compound = Success("double divide(double value){value/=0.0;return value;}");
        Check(compound.Diagnostics.Any(d=>d.Code=="FLT005"&&d.Severity==DiagnosticSeverity.Warning));

        var reused = new ConstantInterpreter(new AnalysisOptions());
        ConstantEvaluationResult exceptional = reused.Evaluate(Variable(ieee,"not_number").Initializer!);
        Equal(ConstantEvaluationStatus.Success,exceptional.Status); Equal(FloatingClassification.NaN,exceptional.FloatingValue!.Classification);
        AnalysisResult integerFailure = Analyze("constexpr int invalid=0.0/0.0;");
        Equal(ConstantEvaluationStatus.UndefinedBehavior,reused.Evaluate(Variable(integerFailure,"invalid").Initializer!).Status);
        AnalysisResult finite = Success("constexpr double finite=1.5;");
        ConstantEvaluationResult recovered = reused.Evaluate(Variable(finite,"finite").Initializer!);
        Equal(ConstantEvaluationStatus.Success,recovered.Status); Equal(FloatingEvaluationCondition.None,recovered.FloatingConditions);
        Equal(0,recovered.StorageObjects.Count); Equal(0x3ff8000000000000UL,recovered.FloatingValue!.Bits);
        Throws<ArgumentOutOfRangeException>(()=>new CompilationSession(new AnalysisOptions{FloatingExceptions=(FloatingExceptionPolicy)99}));
        Task<bool>[] modes = new[] { FloatingExceptionPolicy.Ieee, FloatingExceptionPolicy.Strict }.Select(policy=>Task.Run(()=>
        {
            using var session = new CompilationSession(new AnalysisOptions{FloatingExceptions=policy});
            return session.Analyze("constexpr double infinity=1.0/0.0;static_assert(infinity>1.0);").IsSuccess;
        })).ToArray();
        Task.WaitAll(modes); Check(modes[0].Result&&!modes[1].Result);
    }
}
