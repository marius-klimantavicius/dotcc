using RoslynObj;

namespace RoslynObj.CSharp;

/// <summary>Captured body availability is separate from an out-of-line definition.</summary>
internal static class ManagedFunctionDefinitionPolicy
{
    internal static bool RequiresOutOfLineProvider(FunctionSymbol function)
        => function.Details.SuppressInlineBody || function.Details.DefinitionForInliningOnly;

    // Binding, weak/alias selection and supported-body admission are separate.
    internal static bool IsProviderDefinition(FunctionSymbol function)
        => !RequiresOutOfLineProvider(function) && function.IsDefined && function.BodyState.Name == "definition" &&
            function.Body is { Root.RepresentationRole.Name: "execution" };
}
