namespace RoslynObj.CSharp;

internal static partial class ManagedProgramSelectionPlanner
{
    private sealed partial class Work
    {
        private const string TemplateComdatPolicy = "captured-template-comdat-manifest-order";

        private bool HasFunctionComdatSelectionPolicy
            => definitionSelection is InlineComdatPolicy or TemplateComdatPolicy;

        private bool IsSelectedComdatMember(FunctionSymbol function, bool requireDefinition)
            => HasFunctionComdatSelectionPolicy &&
                (IsInlineComdatMember(function, requireDefinition) ||
                    definitionSelection == TemplateComdatPolicy && IsExplicitClassComdatProvider(function));

        // A concrete explicitly instantiated class member supplies a captured
        // provider independently of source inline spelling. These associations
        // do not select template patterns, other members or related bodies.
        private static bool IsExplicitClassComdatProvider(FunctionSymbol function)
        {
            var details = function.Details;
            if (!HasComdatMemberShape(function, requireDefinition: true) ||
                !details.IsTemplateFunction || !details.ExplicitInstantiation || !details.ClassExplicitlyInstantiated ||
                details.ExplicitDoNotInstantiate || details.IsPrototypeInstantiation || details.IsSpecialized || details.IsDeleted ||
                function.Template is not { Kind.Name: "templk_member_function" } memberTemplate ||
                function.Instantiation is not { State.Name: "concrete", IsExplicit: true, IsDependent: false,
                    IsPrototype: false, IsSuppressed: false, IsSpecialized: false, SelectedSpecialization: null } memberInstance ||
                function.ContainingType is not RecordType record || record.IsIncomplete || record.IsDependent ||
                !record.Record.IsTemplateClass || !record.Record.ExplicitlyInstantiated || record.Record.DoNotInstantiate ||
                record.Record.IsPrototypeInstantiation || record.Record.IsNonrealClass || record.Record.IsSpecialized ||
                record.Template is not { Kind.Name: "templk_class" } classTemplate ||
                record.Record.Instance is not { State.Name: "concrete", IsExplicit: true, IsDependent: false,
                    IsPrototype: false, IsSuppressed: false, IsSpecialized: false, SelectedSpecialization: null } classInstance)
                return false;
            return memberInstance.Result.Equals(function) && memberInstance.OriginalDefinition?.Equals(memberTemplate) == true &&
                classInstance.Result.Equals(record) && classInstance.OriginalDefinition?.Equals(classTemplate) == true;
        }
    }
}
