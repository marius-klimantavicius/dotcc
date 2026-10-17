namespace RoslynObj.CSharp;

/// <summary>Projection classification from the pinned EDG lowering contract, not execution of recipes.</summary>
internal static class ManagedAmbientPolicy
{
    internal static bool IsInertDescriptor(Initialization initialization)
    {
        var flags = initialization.Details;
        // c_gen_be.c dump_whole_variable_dynamic_init: dik_none has no
        // initialization. lower_init.c ignores its destructor association when
        // lifetime is null; InitRewrittenToAssignment alone supplies no action.
        return initialization.Kind.Name == "dik_none" && initialization.Target is VariableSymbol target &&
            !target.IsThreadLocal && !target.Details.IsPrototypeInstantiation && !target.Details.IsNonreal &&
            !target.Details.SuppressInlineDefinition && !target.Details.PromotedLocalStatic &&
            !target.Details.PromotedLocalStaticInitialization &&
            initialization.Expression is null && initialization.Constructor is null && initialization.Constant is null &&
            initialization.Lambda is null && initialization.BitwiseCopySource is null && flags.Source is null &&
            initialization.Operands.Count == 0 && initialization.Lifetime is null && initialization.InitExpressionLifetime is null &&
            initialization.OverlappingLifetime is null && initialization.Master is null && initialization.NextInDestructionList is null &&
            !flags.IsUnordered && !flags.IsConditional && !flags.IsOverlapping && !flags.IsClassResultMaster &&
            !flags.IsStaticTemporary && !flags.IsPartialAggregate && !flags.IsPartialInitialization &&
            !flags.FollowsExecutableStatement && !flags.HasTemporaryLifetime && !flags.IsConstructorInitialization &&
            !flags.IsConditionalGuard && !flags.IsVlaDeallocation && !flags.CreatesInitializerList && !flags.IsInitializerListArray &&
            !flags.IsReusedValue && !flags.IsQuestionResult && !flags.IsCommaResult && !flags.ConstructorCopyImpliedSource &&
            !flags.ConstructorImplicitCopy && !flags.ConstructorValueInitialization && !flags.ConstructorSequencedArguments &&
            !flags.ConstructorArrayCopy && !flags.FreesStorage && !flags.FreesArray && !flags.IsFreeingOfExceptionObject;
    }
}
