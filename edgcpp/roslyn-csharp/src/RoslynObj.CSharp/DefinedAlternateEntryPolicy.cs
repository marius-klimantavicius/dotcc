namespace RoslynObj.CSharp;

internal static class DefinedAlternateEntryPolicy
{
    // A single captured unit already contains these exact executable entries.
    // Emission keeps separate bodies and identities; it performs no COMDAT
    // selection, GNU alias resolution or substitution of the primary body.
    internal static bool HasIndependentBody(FunctionSymbol entry, IReadOnlyCollection<FunctionSymbol> functions)
    {
        var primary = entry.PrimaryEntry;
        if (primary is null) return false;
        var simpleAliasEntry = entry.Details.IsAliasEntry == true
            && entry.ConstructorOrDestructorKind?.Name == "cdk_subobject"
            && primary.ConstructorOrDestructorKind?.Name == "cdk_complete";
        // Virtual-base lowering retains distinct complete/delegation bodies
        // around a subobject primary with its selected VTT execution argument.
        // These are separate executable entries, not aliases to substitute.
        var virtualBaseEntry = entry.Details.IsAliasEntry == false && primary.Details.IsAliasEntry == false
            && entry.ContainingType is RecordType { HasVirtualBases: true }
            && primary.ConstructorOrDestructorKind?.Name == "cdk_subobject"
            && (entry.ConstructorOrDestructorKind?.Name == "cdk_complete"
                || entry.ConstructorOrDestructorKind?.Name == "cdk_delegation" && entry.SpecialFunctionKind?.Name == "sfk_destructor")
            && primary.Signature.HasThisParameter && primary.Signature.Parameters.Count >= 2
            && primary.Signature.Parameters[0].Role.Name == "this_parameter"
            && primary.Signature.Parameters[1].Role.Name == "vtt_parameter";
        return functions.Contains(primary)
            && entry.IsDefined && entry.BodyState.Name == "definition" && entry.Body is not null
            && primary.IsDefined && primary.BodyState.Name == "definition" && primary.Body is not null
            && (simpleAliasEntry || virtualBaseEntry)
            && entry.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor"
            && entry.SpecialFunctionKind == primary.SpecialFunctionKind
            && entry.ContainingType is not null && entry.ContainingType.Equals(primary.ContainingType)
            && primary.PrimaryEntry is null && primary.AlternateEntryPoints.Contains(entry)
            && !HasUnresolvedBinding(entry) && !HasUnresolvedBinding(primary);
    }

    private static bool HasUnresolvedBinding(FunctionSymbol function)
    {
        var details = function.Details;
        var supplement = function.GnuSupplement;
        return details.IsWeak == true || details.IsWeakref == true || details.IsGnuAlias == true || details.IsIfunc == true
            || details.SupersededExternal || supplement?.AliasedRoutine is not null
            || supplement?.TargetedVersions.Count > 0 || supplement?.Representative is not null
            || supplement?.IsRepresentative == true || supplement?.IsTargetSpecificVersion == true
            || supplement?.ResolverRequired == true;
    }
}
