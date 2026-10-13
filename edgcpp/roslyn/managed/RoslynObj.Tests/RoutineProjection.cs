using RoslynObj;
using System.Numerics;

internal static class RoutineProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture is "routines.roslynobj" or "routines_microsoft.roslynobj" or "routines_exception.roslynobj")
        {
            SemanticProjection.Sweep(unit.Functions.Select(f=>f.Details),"routine_symbol",check);
            SemanticProjection.Sweep(unit.Types.OfType<RoutineType>().Select(t=>t.Routine),"routine_type",check);
            SemanticProjection.Sweep(unit.Entities.OfType<GnuRoutineSupplement>().Select(g=>g.Details),"gnu_routine_supplement",check);
            SemanticProjection.Sweep(unit.Entities.OfType<ExceptionSpecification>().Select(e=>e.Details),"exception_specification",check);
        }
        if(fixture=="routines_exception.roslynobj")
        {
            ExceptionSpecification Spec(string name) => unit.Functions.Single(f=>f.Name==name).Signature.ExceptionSpecification!;
            check(Spec("routine_throw_list") is {Kind.Name:"dynamic",IsIndeterminate:false} listed&&listed.Types.Select(t=>t.Kind.Name).SequenceEqual(new[]{"tk_integer","tk_struct"})&&listed.Types[1].Name=="RoutineException","A genuine dynamic exception specification must retain its exact ordered type list.");
            check(Spec("routine_throw_empty") is {Kind.Name:"dynamic",Types.Count:0,ThrowAny:false,IsIndeterminate:false},"An explicit empty throw list must remain distinct from an unresolved generated exception specification.");
            check(Spec("routine_noexcept_true") is {NoexceptEffective:true,NoexceptArgument:not null,ThrowAny:false}&&Spec("routine_noexcept_false") is {NoexceptEffective:false,NoexceptArgument:not null,ThrowAny:true},"Concrete noexcept arguments must retain their opposite exact effective states and throw-any flags.");
            var dependent=unit.Functions.Where(f=>f.Name=="routine_noexcept_dependent").Select(f=>f.Signature.ExceptionSpecification!).ToArray();
            check(dependent.Length==3&&dependent.Count(e=>e.IsDependent&&e.NoexceptEffective is null&&e.NoexceptArgument is not null)==1&&dependent.Any(e=>e.NoexceptEffective==true)&&dependent.Any(e=>e.NoexceptEffective==false),"A dependent exception specification and its two actual instantiations must preserve unknown, true and false separately.");
            check(unit.Entities.OfType<ExceptionSpecification>().Any(e=>e.IsIndeterminate&&e.NoexceptEffective is null),"Generated unresolved special-member specifications must preserve their explicit indeterminate state.");
            var specifications=unit.Entities.OfType<ExceptionSpecification>().ToArray();
            check(specifications.Any(e=>e.ArgumentCached&&e.NoexceptArgument is null&&e.CopyFromPrototypeRoutine is null&&e.NoexceptEffective is null),"A genuinely deferred member exception specification must expose the cache-presence flag while excluding its frontend-only token-cache payload.");
            check(specifications.Any(e=>e.CopyFromPrototype&&e.CopyFromPrototypeRoutine is {Name:"subordinate"} routine&&routine.Details.IsPrototypeInstantiation&&e.NoexceptArgument is null&&e.NoexceptEffective is null),"A subordinate prototype exception specification must preserve its actual routine union target and direct copy flag without treating it as a noexcept constant.");
        }
        if(fixture=="routines.roslynobj")
        {
            FunctionSymbol Function(string name) => unit.Functions.Single(f=>f.Name==name);
            var typed=Function("routine_typed_declaration");
            check(typed.DeclaredType==typed.SymbolDetails.Type&&typed.DefinitionType==typed.Details.DeclaredType&&Function("routine_alias").DefinitionType is null,"Current common function type and optional retained defining type must follow their own exact authorities without a declaration-only source-type fallback.");
            if(source)check(typed.DefinitionType is RoutineType definition&&definition!=typed.Signature&&definition.ReturnType==typed.ReturnType,"This source definition must retain its distinct actual defining signature with the recorded return type.");
            else check(typed.DefinitionType is null,"The lowered profile must preserve unavailable source defining-type metadata as null.");
            check(typed.Signature.Parameters.Count==1&&typed.ReturnType.Kind.Name=="tk_integer"&&typed.Signature.LanguageLinkage?.Name=="nlk_external","A function declared through an extern-C typedef must navigate its actual callable signature and retained type linkage.");
            if(source)check(typed.SourceCorrespondence!.Linkage?.Name=="nlk_cplusplus_external"&&typed.Signature.IsLanguageLinkageExplicit,"Source symbol linkage and explicit type linkage must remain distinct recorded facts.");
            var pure=Function("routine_pure");
            check(pure.IsPureFunction==true&&!pure.IsPureVirtual&&unit.Functions.Any(f=>f.Name=="pure"&&f.IsPureVirtual&&f.IsPureFunction!=true),"GNU pure-function attributes must remain distinct from C++ pure virtual members.");
            var qualified=unit.Functions.Single(f=>f.Name=="qualified"&&f.ContainingType?.Name=="RoutineDerived");
            check(qualified.Signature.ObjectQualifiers.IsConst&&!qualified.Signature.ObjectQualifiers.IsRestrict&&qualified.Signature.ThisQualifiers.IsRestrict&&qualified.Signature.HasThisParameter,"Member cv-qualification and pointer restrict qualification must navigate their separate retained qualifier sets.");
            var result=Function("routine_class_result");
            check(result.Signature.ReturnsValueViaCopyConstructor&&result.Signature.ReturnsValueAsParameter==(source?null:true),"Class-result calling conventions must distinguish the copy-constructor fact from the current lowered return-slot parameter.");
            check(Function("routine_trailing").Signature.HasTrailingReturnType&&Function("routine_immediate") is {IsDefined:true,Body:not null} immediate&&immediate.Body.Root.RepresentationRole.Name=="metadata","Trailing return syntax and concrete consteval metadata bodies must preserve definition and representation facts independently.");
            check(unit.Functions.Any(f=>f.IsDefined&&f.Body is null&&f.BodyState.Name=="discarded")&&unit.Functions.Where(f=>f.BodyState.Name=="discarded").All(f=>f.IsDefined),"Completed definitions without retained bodies must remain discarded definitions rather than declaration-only routines.");
            check(unit.Functions.Any(f=>f.IsExplicitConstructor&&f.ContainingType?.Name=="RoutineValue")&&unit.Functions.Any(f=>f.SpecialFunctionKind?.Name=="sfk_operator"&&f.OperatorName is not null)&&unit.Functions.Any(f=>f.IsExplicitConversionFunction),"Constructor/operator/conversion facts must retain their actual special-function discriminators and active payloads.");
            check(unit.Functions.Any(f=>f.SpecialFunctionKind?.Name=="sfk_lambda_entry_point"&&f.LambdaCallOperator?.IsLambdaBody==true)&&unit.Functions.Any(f=>f.SpecialFunctionKind?.Name=="sfk_deduction_guide"&&f.DeductionGuideTemplate?.Name=="RoutineGuide"),"Lambda entry points and deduction guides must navigate their distinct active routine/template union targets.");
            check(Function("routine_friend").BefriendingClasses.Any(t=>t.Name=="RoutineFriend")&&unit.Functions.Any(f=>f.IsInheritingConstructor&&f.InheritedRoutine?.ContainingType?.Name=="RoutineValue"&&f.Details.GeneratingUsing is not null),"Friend lists and inheriting-constructor originators must follow their separately guarded retained relationships.");
            check(unit.Functions.Any(f=>f.TrailingRequiresClause?.Constraint is not null)&&unit.Functions.All(f=>f.IsIneligible is not false&&f.HasNoEffect is not false),"Original parameterized constraints must remain navigable while false uncached eligibility/effect predicates stay unknown.");
            var target=Function("routine_alias_target");
            check(Function("routine_alias").GnuSupplement?.AliasedRoutine==target&&Function("routine_ifunc").IfuncResolver?.Name=="routine_resolver","GNU aliases and ifunc resolvers must keep their exact selected routine identities and distinct purpose.");
            check(Function("routine_constructor_priority").GnuSupplement?.ConstructorPriority==201&&Function("routine_destructor_priority").GnuSupplement?.DestructorPriority==202&&Function("routine_section").GnuSupplement?.Section==".routine_test","Actual initialization/finalization priorities and section atoms must remain exact target metadata.");
            var partners=unit.Functions.Where(f=>f.Name=="routine_inline_partner").ToArray();
            check(partners.Length==2&&partners.All(f=>f.GnuSupplement?.InlinePartner is {} partner&&partners.Contains(partner)&&partner.GnuSupplement?.InlinePartner==f),"GNU inline and out-of-line routines must preserve two distinct identities and reciprocal retained partner links.");
            var representative=unit.Functions.Single(f=>f.Name=="routine_multiversion"&&f.GnuSupplement?.IsRepresentative==true);
            var versions=representative.GnuSupplement!.TargetedVersions;
            check(versions.Count==2&&versions.All(f=>f.GnuSupplement is {IsTargetSpecificVersion:true} version&&version.Representative==representative)&&versions[0].GnuSupplement!.TargetArchitectures.Any(k=>k.Name=="mvak_default_target")&&versions[1].GnuSupplement!.TargetArchitectures.Any(k=>k.Name=="mvak_isa_sse2"),"Multiversion descriptors must preserve representative/version arms, default-first dispatch order and exact target architecture members.");
            var bases=unit.Types.Single(t=>t.Name=="RoutineDerived").BaseSubobjects;
            check(bases.Any(b=>b.BaseType.Name=="RoutineBase"&&b.DirectBaseNumber==2),"The second direct base must preserve the numeric sequence value two without boolean coercion.");
            if(!source)check(unit.Functions.Any(f=>f.OverridingFunctionForWrapper is {Name:"clone"}&&f.OverriddenFunctionForWrapper is {Name:"clone"}),"A genuine nonprimary covariant base must retain its actual wrapper target identities.");
            if(!source)
            {
                var alternate=unit.Functions.Where(f=>f.PrimaryEntry is not null).ToArray();
                check(alternate.Length>0&&alternate.All(f=>f.PrimaryEntry!=f&&f.PrimaryEntry!.ContainingType==f.ContainingType&&f.PrimaryEntry.SpecialFunctionKind==f.SpecialFunctionKind)&&alternate.Any(f=>f.ContainingType?.Name=="RoutineValue"&&f.SpecialFunctionKind?.Name=="sfk_constructor"),"Actual alternate constructor/destructor entries must navigate their distinct retained primary entry of the same class and special-function kind without reconstructing a wrapper hierarchy.");
            }
            if(profile is "native_lowered" or "exported_templates_lowered")check(unit.Functions.Any(f=>f.OverridingFunctionForWrapper is {Name:"clone"}&&f.ReturnAdjustment is not null),"An Itanium ABI wrapper must retain its guarded exact return adjustment metadata.");
            if(profile=="cfront_lowered")check(unit.Functions.All(f=>f.ReturnAdjustment is null),"Cfront routines must preserve unknown adjustment metadata when the Itanium-only fields are not present.");
        }
        if(fixture=="routines_microsoft.roslynobj")
        {
            var selected=unit.Functions.Where(f=>f.SelectedOverrides.Count>0).ToArray();
            check(selected.Length==2&&selected.All(f=>f.SelectedOverrides.Single() is FunctionSymbol {Name:"call",IsPureVirtual:true}),"Native Microsoft selective overrides must navigate their actual selected pure-virtual base routines.");
            check(selected.Select(f=>f.ContainingType!.Name).Order().SequenceEqual(new[]{"RoutineNativeImplementation","RoutineSelectiveDerived"}),"Native selective declarations must retain their actual owning derived types.");
            check(unit.Functions.Single(f=>f.Name=="routine_native_convention").Signature.IsCallingConventionExplicit==true,"Native calling-convention spelling must retain its explicit marker separately from the target's current convention.");
        }
        if(fixture=="retained.roslynobj")
        {
            check(unit.Types.OfType<RecordType>().Any(t=>t.IsLambdaClosureClass)&&unit.Entities.OfType<LambdaDefinition>().All(l=>l.ClosureType is RecordType {IsLambdaClosureClass:true}),"Actual retained lambda closure descriptors must retain their direct class-supplement lambda classification.");
            var parameter=unit.Functions.Single(f=>f.Name=="coroutine_parameter").SourceParameters.Single(p=>p.Name=="input");
            check(parameter.Descriptor.Details.PassedViaCopyConstructor,"A real by-value coroutine parameter must retain its copy-constructor passing flag.");
            check(unit.Functions.Where(f=>f.Name=="CopyParameter").SelectMany(f=>f.SourceParameters).Any(p=>p.Descriptor.Details.CopyOrMoveConstructorParameter),"A real copy constructor parameter must retain its specific parameter role flag.");
            check(unit.Functions.Single(f=>f.Name=="default_parameter").Parameters.Single().Descriptor.Details.HasDefaultArgument,"A real default parameter must retain its direct presence marker independently from its captured expression.");
            if(unit.Types.Any(t=>t.Name=="ExplicitObject"))check(unit.Functions.Where(f=>f.Name=="read").SelectMany(f=>f.Parameters).Any(p=>p.Descriptor.IsExplicitThis),"A genuine explicit-object signature must retain its explicit-this descriptor flag even when lowering converts its routine into a nonmember.");
        }
        if(fixture=="template_edges.roslynobj")
        {
            check(unit.Entities.OfType<LambdaDefinition>().Any(l=>l.ClosureType is RecordType {IsLambdaClosureClass:true}),"A genuine pack-lambda closure must preserve its actual record-type lambda flag.");
            var prototype=unit.Functions.Single(f=>f.Name=="captured_pack"&&f.Instantiation?.IsPrototype==true);
            check(prototype.Parameters.Any(p=>p.Descriptor.IsPack),"A real prototype signature must retain its parameter-pack marker.");
            var instance=unit.Functions.Single(f=>f.Name=="captured_pack"&&f.Instantiation?.IsPrototype==false);
            var pack=instance.Parameters.Where(p=>p.Descriptor.IsPackElement).ToArray();
            check(pack.Length==3&&pack.Select(p=>p.Ordinal).SequenceEqual(new long[]{0,1,2})&&pack.All(p=>p.Descriptor.ParameterNumber==1),"Expanded pack elements must preserve distinct signature ordinals and their repeated original template parameter number one.");
        }
        if(fixture=="types.roslynobj")
        {
            var status=(EnumSymbol)unit.Types.Single(t=>t.Name=="Status").DeclarationSymbol!;
            var scope=unit.Entities.OfType<Scope>().Single(s=>s.AssociatedEntity==status.Type);
            check(scope.Members.OfType<EnumeratorSymbol>().Select(e=>e.Name).SequenceEqual(new[]{"ready","done"})&&scope.Members.OfType<EnumeratorSymbol>().All(e=>e.ContainingEntity==status.Type),"Scoped enum member navigation must retain its exact descriptor association and enumerator containing type.");
            check(scope.Parent!.ChildScopesByParent.Contains(scope),"Reverse child-scope navigation must follow the retained Parent relation independently from the actual ordered scope list.");
        }
    }
}
