// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten scalar subset of EDG overload.c: add_function_to_candidate_functions_list,
// determine_arg_match_level, compare_standard_conversions, select_best_candidate_functions
// and match_is_better_on_at_least_one_arg. This does not port their class/reference,
// pointer, user-defined conversion, template or extension branches.
using System.Collections.ObjectModel;

namespace EdgSharp;

public enum StandardConversionRank { ExactMatch, Promotion, Conversion }
public enum CandidateRejectionReason { None, ArityMismatch, ArgumentConversion, DeductionFailure, InvalidTemplateDefinition }

/// <summary>
/// One by-value integer argument match. Lvalue transformation and top-level cv
/// removal do not worsen its rank. Equally ranked integer conversions are tied.
/// </summary>
public sealed class StandardConversionSequence
{
    public IntegerType SourceType { get; }
    public IntegerType TargetType { get; }
    public bool RequiresLValueToRValue { get; }
    public StandardConversionRank Rank { get; }
    internal StandardConversionSequence(IntegerType source, IntegerType target, bool lvalue, StandardConversionRank rank)
        => (SourceType, TargetType, RequiresLValueToRValue, Rank) = (source, target, lvalue, rank);
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
            if (argument.Type is not IntegerType source || type.ParameterTypes[i] is not IntegerType target)
                return new OverloadCandidate(function, CandidateRejectionReason.ArgumentConversion, []);
            conversions[i] = new StandardConversionSequence(source, target,
                argument.ValueCategory == ValueCategory.LValue, IntegerConversionRank(source, target));
        }
        return new OverloadCandidate(function, CandidateRejectionReason.None, conversions, type, templateArguments);
    }

    private static bool BetterCandidate(OverloadCandidate candidate, OverloadCandidate other)
    {
        bool strictlyBetter = false;
        for (int i = 0; i < candidate.ArgumentConversions.Count; ++i)
        {
            StandardConversionRank left = candidate.ArgumentConversions[i].Rank;
            StandardConversionRank right = other.ArgumentConversions[i].Rank;
            if (left > right) return false;
            if (left < right) strictlyBetter = true;
        }
        if (strictlyBetter) return true;
        if (candidate.Template == null && other.Template != null) return true;
        if (candidate.Template != null && other.Template != null)
            return PatternDeduces(other.Template, candidate.Template) && !PatternDeduces(candidate.Template, other.Template);
        return false;
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
        // For these totally ordered scalar ranks, checking each pair directly
        // preserves that invariant. Do not sum ranks or prefer a declaration's
        // position, integer width, signedness, or return type.
        OverloadCandidate[] viable = candidates.Where(candidate => candidate.IsViable).ToArray();
        foreach (OverloadCandidate candidate in viable)
            if (viable.All(other => ReferenceEquals(candidate, other) || BetterCandidate(candidate, other)))
                return candidate;
        return null;
    }
}
