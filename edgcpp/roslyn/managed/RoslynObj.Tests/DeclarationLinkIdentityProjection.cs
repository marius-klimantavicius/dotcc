using RoslynObj;
using System.Numerics;

internal static class DeclarationLinkIdentityProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture=="source_order.roslynobj"&&source)VerifySourceEntries(unit,check);
        if(fixture=="retained.roslynobj"&&source)VerifyTypeDefault(unit,check);
        if(fixture=="types.roslynobj"&&source)VerifyEnum(unit,check);
        if(fixture=="template_edges.roslynobj"&&source)VerifyVariableArgumentsAndRequirement(unit,check);
        if(fixture=="native_attributes.roslynobj"&&source)VerifyUuid(unit,check);
        if(fixture=="layout.roslynobj"&&profile=="native_lowered"&&unit.Capabilities.Any(c=>c.Name=="IA64_ABI"&&c.Enabled==true))VerifyOverride(unit,check);
        if(fixture=="scoped_assume.roslynobj"&&source)VerifyStatementAttribute(unit,check);
        if(fixture=="template_parameter_defaults.roslynobj")VerifyTemplateAndParameterDefaults(unit,source,check);
        if(fixture=="parameter_attributes_microsoft.roslynobj")VerifyMicrosoftParameterAttributes(unit,source,check);
    }

    private static void VerifySourceEntries(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="repeated");
        var primary=function.Declarations.Single(d=>d.Secondary is null);
        var secondary=function.Declarations.Single(d=>d.Secondary is not null);
        var selected=function.SourceCorrespondence!.SourceSequenceEntry!;
        var secondaryEntry=unit.Entities.OfType<SourceEntry>().Single(e=>e.Target==secondary);
        check(selected.Tag.Name=="iek_routine"&&selected.Target==function&&selected.Declaration==primary&&selected.DeclaredSymbol==function&&primary.SourceCorrespondence==function.SourceCorrespondence&&secondaryEntry!=selected&&secondaryEntry.Declaration==secondary&&secondaryEntry.DeclaredSymbol==function,"The repeated routine must preserve the exact designated primary/definition source entry separately from its secondary occurrence, without choosing an entry by ID or list order.");
        var pragma=unit.Pragmas.Single(p=>p.SourceEntry is not null);
        var entry=pragma.SourceEntry!;
        check(entry.Tag.Name=="iek_pragma"&&entry.Target==pragma&&entry.SourceSequenceOwners.Count==1&&entry.SourceSequenceOwner!.SourceEntries.Contains(entry),"The retained pragma's designated source entry must point back to that exact pragma and occur in its actual scope sequence.");
    }

    private static void VerifyTypeDefault(TranslationUnit unit,Action<bool,string> check)
    {
        var parameter=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Promise"&&p.DefaultArgument is not null);
        var argument=parameter.DefaultArgument!;
        check(argument.Kind.Name=="tak_type"&&argument.Type is {Kind.Name:"tk_void"} actual&&unit.Types.Contains(actual)&&actual!=parameter.PlaceholderType&&parameter.PlaceholderType!.DeclaredParameters.Contains(parameter),"The Promise type parameter must retain its actual void default argument independently from the parameter's placeholder type.");
    }

    private static void VerifyEnum(TranslationUnit unit,Action<bool,string> check)
    {
        var status=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="Status");
        var colour=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="Colour");
        check(status.HasExplicitBase&&status.ExplicitUnderlyingType is ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_unsigned_short",Arithmetic.Bits:16} written&&unit.Types.Contains(written)&&!colour.HasExplicitBase&&colour.ExplicitUnderlyingType is null,"The explicitly written unsigned-short enum base must remain a canonical type link, while the enum without a written base preserves its absent explicit-type pointer.");
    }

    private static void VerifyVariableArgumentsAndRequirement(TranslationUnit unit,Action<bool,string> check)
    {
        var variable=unit.Variables.Single(v=>v.Name=="folded_sum"&&v.TemplateArguments.Count==1&&v.TemplateArguments[0].Elements.Count==3);
        var instance=variable.Instantiation!;var argument=variable.TemplateArguments.Single();
        check(instance.Result==variable&&instance.Arguments.Single()==argument&&instance.SelectedPartialArguments.Count==0&&instance.ArgumentMappings.Single().Argument==argument&&instance.ArgumentMappings.Single().Role.Name=="primary"&&argument.Kind.Name=="tak_start_of_pack_expansion"&&argument.Elements.Select(e=>((IntegerValue)e.Constant!.Value).Value).SequenceEqual(new BigInteger[]{1,2,3}),"The actual folded_sum variable instance must retain its primary argument and ordered 1/2/3 pack elements without substituting a selected-partial argument list.");
        var requirement=unit.Entities.OfType<DependentOperation>().Single(d=>d.Kind.Name=="enk_nested_req");
        var wrapped=requirement.WrappedExpression!;
        check(wrapped.RepresentationRole.Name=="metadata"&&requirement.References.Single(r=>r.Role.Name=="metadata"&&r.Ordinal==0).Target==wrapped&&!requirement.Children.Contains(wrapped),"The nested requirement must retain its exact wrapped expression as a metadata reference rather than an evaluated structural child.");
    }

    private static void VerifyUuid(TranslationUnit unit,Action<bool,string> check)
    {
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="NativeUuid");
        var attribute=unit.GlobalScope.MicrosoftAttributes.Single(a=>a.Name=="uuid");
        var correspondence=record.SourceCorrespondence!;
        check(attribute.Target==record&&record.DeclarationSymbol is TypeSymbol symbol&&symbol.SourceCorrespondence==correspondence&&symbol.Declarations.Single(d=>d.Secondary is null).SourceCorrespondence==correspondence&&correspondence.Entities.Contains(record)&&correspondence.Entities.Contains(symbol)&&!record.MicrosoftAttributes.Contains(attribute),"The UUID application must retain the actual record target and its shared symbol/declaration correspondence independently from the owning scope's Microsoft attribute list.");
    }

    private static void VerifyOverride(TranslationUnit unit,Action<bool,string> check)
    {
        var diamond=unit.Types.OfType<RecordType>().Single(t=>t.Name=="LayoutDiamond");
        var root=unit.Types.OfType<RecordType>().Single(t=>t.Name=="LayoutRoot");
        var copy=diamond.Methods.Single(f=>f.Name=="copy");var original=root.Methods.Single(f=>f.Name=="copy");
        var relation=copy.Overrides.Single();
        var contextual=diamond.BaseSubobjects.Single(b=>b.BaseType==root);
        check(relation.Owner==copy&&relation.Overridden==original&&relation.Ordinal==0&&relation.Subobject==contextual&&contextual.MostDerivedType==diamond&&contextual.Overrides.Any(r=>r.Owner==copy&&r.Overridden==original),"The retained virtual override must preserve the actual LayoutDiamond routine, LayoutRoot routine and contextual base-subobject identity in both navigation directions.");
    }

    private static void VerifyStatementAttribute(TranslationUnit unit,Action<bool,string> check)
    {
        var statement=unit.Entities.OfType<StatementOperation>().Single(s=>s.Kind.Name=="stmk_empty"&&s.Attributes.Count>0);
        var attribute=statement.Attributes.Single();
        check(attribute.Kind.Name=="ak_assume"&&attribute.Owners.Single()==statement&&attribute.Owner==statement,"The retained assume attribute must belong to its exact empty statement occurrence, independently from the attribute argument's scoped expression wrapper.");
    }

    private static void VerifyTemplateAndParameterDefaults(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var parameter=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Container"&&p.DefaultArgument is not null);
        var argument=parameter.DefaultArgument!;var defaultBox=unit.TemplateDescriptors.Single(t=>t.Name=="DefaultBox");
        check(argument.Kind.Name=="tak_template"&&argument.Template==defaultBox&&parameter.PlaceholderTemplate!=defaultBox&&parameter.PlaceholderTemplate!.DeclaredParameters.Contains(parameter),"The template-template parameter must retain DefaultBox as its actual default descriptor independently from the Container placeholder.");
        var concrete=unit.Variables.Where(v=>v.Name=="selected_value"&&v.Instantiation is {IsDependent:false,IsPrototype:false}).ToArray();
        var selected=concrete.Single(v=>v.Instantiation!.SelectedPartialArguments.SingleOrDefault()?.Type is ArithmeticType);
        var partial=selected.Instantiation!.SelectedPartialArguments.Single();var primary=selected.TemplateArguments.Single();
        check(primary.Type is PointerType pointer&&pointer.PointeeType==partial.Type&&partial.Type is ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"}&&selected.Instantiation.ArgumentMappings.Single(r=>r.Role.Name=="primary").Argument==primary&&selected.Instantiation.ArgumentMappings.Single(r=>r.Role.Name=="selected_partial").Argument==partial&&primary!=partial,"A concrete selected variable specialization must preserve pointer-to-int primary arguments separately from its deduced int partial-specialization argument.");
        var primaryOnly=concrete.Single(v=>v.TemplateArguments.SingleOrDefault()?.Type==partial.Type&&v.Instantiation!.SelectedPartialArguments.Count==0);
        check(primaryOnly.Instantiation!.Result==primaryOnly&&primaryOnly.TemplateArguments.Single().Type==partial.Type,"The primary-only int variable instance must retain its own result and genuinely empty selected-partial argument list.");
        var descriptor=unit.Entities.OfType<ParameterType>().Single(p=>p.EntitiesDefinedInDefault.Count>0);
        var closure=(RecordType)descriptor.EntitiesDefinedInDefault.Single();
        var member=unit.Functions.Single(f=>f.Name=="invoke"&&f.ContainingType?.Name=="DefaultLambdaHost");
        var link=descriptor.Details.ParameterDefaultEntityRows.Single();
        check(closure.IsLambdaClosureClass&&closure.LambdaParentEntity==member&&link.Owner==descriptor&&link.Ordinal==0&&link.Entity==closure&&member.Signature.Parameters.Any(p=>p.Descriptor==descriptor),"The called member's default argument must retain its ordered closure definition and actual routine parent through the exact parameter descriptor.");
        check(source?descriptor.DefaultExpression is {RepresentationRole.Name:"metadata"}:descriptor.DefaultExpression is null,"Default-expression availability must preserve the retained source expression and its legitimate absence after lowering.");
        var attributed=unit.Functions.Single(f=>f.Name=="parameter_attributes").Signature.Parameters.Single().Descriptor;
        var maybeUnused=attributed.Attributes.Single();
        check(maybeUnused.Kind.Name=="ak_maybe_unused"&&maybeUnused.Owners.Contains(attributed),"The declaration parameter must retain its actual maybe_unused attribute and canonical owner identity.");
        var directives=unit.Entities.OfType<InstantiationDirective>().ToArray();
        if(source)
        {
            var directive=directives.Single();var visibility=directive.Attributes.Single();
            check(visibility.Kind.Name=="ak_visibility"&&visibility.Owners.Single()==directive&&directive.Target is RecordType {Name:"AttributedInstantiation"}&&directive.DeclaredType==directive.Target,"The explicit instantiation's visibility attribute must remain attached to the directive occurrence and its exact retained type target.");
        }
        else check(directives.Length==0,"The lowered profile retains no source instantiation-directive occurrence; type attributes must not create one.");
    }

    private static void VerifyMicrosoftParameterAttributes(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="native_parameter_attributes");
        var formals=function.Signature.Parameters.Select(p=>p.Descriptor).ToArray();
        check(formals.Length==2&&formals.Select(p=>p.Name).SequenceEqual(new[]{"value","result"})&&formals.Select(p=>p.MicrosoftAttributes.Single().Name).SequenceEqual(new[]{"in","out"}),"The native Microsoft parameter application lists must retain exact in/out attributes on the two canonical formal descriptors.");
        var attributes=unit.Entities.OfType<MicrosoftAttribute>().ToArray();
        check(attributes.Length==(source?4:2)&&attributes.All(a=>a.Kind.Name==(source?"msak_misc":"msak_unrecognized")&&a.Target is ParameterType target&&target.MicrosoftAttributes.Contains(a)),"Copied source formals and suppressed lowered attributes must preserve their actual target descriptors and recorded kinds without inferring a common application owner.");
    }
}
