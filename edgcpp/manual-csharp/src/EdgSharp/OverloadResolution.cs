// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten scalar subset of EDG overload.c: add_function_to_candidate_functions_list,
// determine_arg_match_level, compare_standard_conversions, select_best_candidate_functions
// and match_is_better_on_at_least_one_arg. Scalar reference binding is in
// References.cs; class/pointer/user-defined conversion and extension branches remain.
using System.Collections.ObjectModel;

namespace EdgSharp;

public enum StandardConversionRank { ExactMatch, Promotion, Conversion }
public enum CandidateRejectionReason { None, ArityMismatch, ArgumentConversion, DeductionFailure, InvalidTemplateDefinition }

/// <summary>
/// One scalar argument match, including explicit reference binding. Glvalue
/// transformation and by-value top-level cv removal do not worsen its rank.
/// </summary>
public sealed class StandardConversionSequence
{
    public IntegerType SourceType { get; }
    public IntegerType TargetType { get; }
    public bool RequiresLValueToRValue { get; }
    public StandardConversionRank Rank { get; }
    public ReferenceBinding? ReferenceBinding { get; }
    public SemanticType ParameterType => ReferenceBinding is { } binding ? binding.ReferenceType : TargetType;
    internal StandardConversionSequence(IntegerType source, IntegerType target, bool lvalue, StandardConversionRank rank)
        => (SourceType, TargetType, RequiresLValueToRValue, Rank) = (source, target, lvalue, rank);
    internal StandardConversionSequence(ReferenceBinding binding)
        => (SourceType, TargetType, RequiresLValueToRValue, Rank, ReferenceBinding) =
            (binding.SourceType, binding.TargetType, binding.RequiresLValueToRValue, binding.Rank, binding);
}

/// <summary>A source-visible entity and its viability/matches for one specific call.</summary>
public sealed class OverloadCandidate
{
    public Symbol Function { get; }
    public FunctionType FunctionType { get; }
    public FunctionTemplateDeclaration? Template { get; }
    public ReadOnlyCollection<SemanticType> TemplateArguments { get; }
    public CandidateRejectionReason RejectionReason { get; }
    public bool IsViable => RejectionReason == CandidateRejectionReason.None;
    /// <summary>One descriptor per argument for viable candidates; empty for rejected candidates.</summary>
    public ReadOnlyCollection<StandardConversionSequence> ArgumentConversions { get; }
    internal OverloadCandidate(Symbol function, CandidateRejectionReason rejection, StandardConversionSequence[] conversions,
        FunctionType? type = null, SemanticType[]? arguments = null)
        => (Function, FunctionType, Template, TemplateArguments, RejectionReason, ArgumentConversions) =
            (function, type ?? (FunctionType)function.Type, function.TemplateDefinition, Array.AsReadOnly((SemanticType[])(arguments ?? []).Clone()),
                rejection, Array.AsReadOnly((StandardConversionSequence[])conversions.Clone()));
}

internal sealed partial class Parser
{
    private StandardConversionRank IntegerConversionRank(IntegerType source, IntegerType destination)
    {
        if (source.IntegerKind == destination.IntegerKind) return StandardConversionRank.ExactMatch;
        return types.Promote(source).IntegerKind == destination.IntegerKind
            ? StandardConversionRank.Promotion : StandardConversionRank.Conversion;
    }

    private OverloadCandidate MatchCandidate(Symbol function, IReadOnlyList<Expression> arguments)
    {
        var type = (FunctionType)function.Type;
        if (type.ParameterTypes.Count != arguments.Count)
            return new OverloadCandidate(function, CandidateRejectionReason.ArityMismatch, []);
        SemanticType[] templateArguments = [];
        if (function.TemplateDefinition is FunctionTemplateDeclaration template)
        {
            if (!template.IsValid) return new OverloadCandidate(function, CandidateRejectionReason.InvalidTemplateDefinition, []);
            var deduced = new SemanticType?[template.TypeParameters.Count];
            for (int i = 0; i < arguments.Count; ++i)
            {
                if (type.ParameterTypes[i] is not TemplateParameterType parameter) continue;
                if (arguments[i].Type is not IntegerType integer)
                    return new OverloadCandidate(function, CandidateRejectionReason.DeductionFailure, []);
                SemanticType actual = types.Unqualified(integer);
                if (deduced[parameter.Index] is SemanticType previous && !ReferenceEquals(previous, actual))
                    return new OverloadCandidate(function, CandidateRejectionReason.DeductionFailure, []);
                deduced[parameter.Index] = actual;
            }
            if (deduced.Any(argument => argument == null))
                return new OverloadCandidate(function, CandidateRejectionReason.DeductionFailure, []);
            templateArguments = deduced.Select(argument => argument!).ToArray();
            type = types.Function(SubstituteType(type.ReturnType, templateArguments), type.ParameterTypes.Select(parameter => SubstituteType(parameter, templateArguments)));
        }
        var conversions = new StandardConversionSequence[arguments.Count];
        for (int i = 0; i < arguments.Count; ++i)
        {
            Expression argument = arguments[i];
            if (type.ParameterTypes[i] is ReferenceType reference)
            {
                ReferenceBinding? binding = MatchReferenceBinding(argument, reference);
                if (binding == null) return new OverloadCandidate(function, CandidateRejectionReason.ArgumentConversion, []);
                conversions[i] = new StandardConversionSequence(binding);
                continue;
            }
            if (argument.Type is not IntegerType source || type.ParameterTypes[i] is not IntegerType target)
                return new OverloadCandidate(function, CandidateRejectionReason.ArgumentConversion, []);
            conversions[i] = new StandardConversionSequence(source, target,
                argument.ValueCategory != ValueCategory.PRValue, IntegerConversionRank(source, target));
        }
        return new OverloadCandidate(function, CandidateRejectionReason.None, conversions, type, templateArguments);
    }

    private static bool BetterCandidate(OverloadCandidate candidate, OverloadCandidate other)
    {
        bool strictlyBetter = false;
        for (int i = 0; i < candidate.ArgumentConversions.Count; ++i)
        {
            int comparison = CompareStandardConversions(candidate.ArgumentConversions[i], other.ArgumentConversions[i]);
            if (comparison > 0) return false;
            if (comparison < 0) strictlyBetter = true;
        }
        if (strictlyBetter) return true;
        if (candidate.Template == null && other.Template != null) return true;
        if (candidate.Template != null && other.Template != null)
            return PatternDeduces(other.Template, candidate.Template) && !PatternDeduces(candidate.Template, other.Template);
        return false;
    }

    private static int CompareStandardConversions(StandardConversionSequence left, StandardConversionSequence right)
    {
        int rank = left.Rank.CompareTo(right.Rank);
        if (rank != 0) return rank;
        // Immediate reference cv is not a qualification-conversion subsequence.
        if (left.ReferenceBinding is not { } l || right.ReferenceBinding is not { } r) return 0;
        // Strict Core1374 ordering: reference kind/category before immediate cv.
        if (l.ReferenceType.ReferenceKind != r.ReferenceType.ReferenceKind)
            return l.ReferenceType.ReferenceKind == ReferenceKind.RValue ? -1 : 1;
        if (l.TargetType.IntegerKind != r.TargetType.IntegerKind) return 0;
        TypeQualifiers lq = l.TargetType.Qualifiers, rq = r.TargetType.Qualifiers;
        if (lq == rq) return 0;
        if ((lq & ~rq) == 0) return -1;
        if ((rq & ~lq) == 0) return 1;
        return 0;
    }

    private static bool PatternDeduces(FunctionTemplateDeclaration pattern, FunctionTemplateDeclaration argument)
    {
        // Bounded partial ordering by deduction in both directions. Only direct
        // by-value type parameters and fixed integer types enter these patterns.
        var deductions = new Dictionary<int, SemanticType>();
        for (int i = 0; i < pattern.Type.ParameterTypes.Count; ++i)
        {
            SemanticType parameter = pattern.Type.ParameterTypes[i];
            SemanticType actual = argument.Type.ParameterTypes[i];
            if (parameter is TemplateParameterType p)
            {
                if (deductions.TryGetValue(p.Index, out SemanticType? previous) && !ReferenceEquals(previous, actual)) return false;
                deductions[p.Index] = actual;
            }
            else if (!ReferenceEquals(parameter, actual)) return false;
        }
        return true;
    }

    private static OverloadCandidate? SelectBestCandidate(OverloadCandidate[] candidates)
    {
        // EDG forms the intersection of per-argument best-match sets and then
        // requires a strictly better argument against every other candidate.
        // For this bounded scalar conversion relation, checking each pair
        // preserves that invariant. Do not sum ranks or prefer a declaration's
        // position, integer width, signedness, or return type.
        OverloadCandidate[] viable = candidates.Where(candidate => candidate.IsViable).ToArray();
        foreach (OverloadCandidate candidate in viable)
            if (viable.All(other => ReferenceEquals(candidate, other) || BetterCandidate(candidate, other)))
                return candidate;
        return null;
    }
}
