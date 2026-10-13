using RoslynObj;
using System.Numerics;

internal static class RemainingRetainedProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        if(fixture=="retained_expressions.roslynobj")VerifyExpressions(unit,profile=="source_semantic",check);
        if(profile!="source_semantic")return;
        if(fixture=="native_attributes.roslynobj")
        {
            var attribute=unit.Entities.OfType<MicrosoftAttribute>().Single(a=>a.Name=="uuid");
            var targets=attribute.Targets;
            check(targets.Count==1&&targets[0].Family=="edg_ms_attribute_target"&&targets[0].Name=="msat_none"&&attribute.Target is RecordType {Name:"NativeUuid"},"The UUID descriptor must retain its actual singleton msat_none target-kind list independently from its concrete record application target.");
        }
        if(fixture=="source_order.roslynobj")
        {
            var space=unit.Namespaces.Single(n=>n.Name=="SourceOrder"&&!n.IsNamespaceAlias);var scope=space.Scope!;
            VerifyTypes(scope,new[]{"Integer","RepeatedAlias","Sequence","Choice"},check);
            var sequence=(RecordType)scope.Members.OfType<CppType>().Single(t=>t.Name=="Sequence");
            var nested=unit.Entities.OfType<Scope>().Single(s=>s.AssociatedEntity==sequence&&s.Kind.Name=="sck_class_struct_union");
            VerifyTypes(nested,new[]{"Nested","Alias"},check);
        }
    }

    private static void VerifyTypes(Scope scope,string[] names,Action<bool,string> check)
    {
        var types=scope.Members.OfType<CppType>().ToArray();
        var rows=scope.Details.ScopeMemberRows.Where(r=>r.Member is CppType).ToArray();
        check(types.Select(t=>t.Name).SequenceEqual(names)&&rows.Select(r=>r.Ordinal).SequenceEqual(Enumerable.Range(0,names.Length).Select(i=>(long)i))&&rows.All(r=>r.Owner==scope)&&rows.Select(r=>r.Member).SequenceEqual(types)&&types.All(t=>t.SourceCorrespondence!.ParentScope==scope),"The actual retained scope type chain must preserve its named canonical descriptors, exact type-list ordinals and owner links independently from other member families and source occurrence order.");
    }

    private static void VerifyExpressions(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var copies=unit.Entities.OfType<Initialization>().Where(i=>i.Kind.Name=="dik_bitwise_copy").ToArray();
        if(source)
        {
            var copy=copies.Single();var function=unit.Functions.Single(f=>f.Name=="array_binding_copy");
            var parameter=function.SourceParameters.Single().Symbol!;
            var container=(VariableSymbol)copy.Target!;
            var value=(OperatorOperation)copy.BitwiseCopySource!;
            var reference=(ReferenceOperation)value.Operands.Single();
            var operations=function.Body!.DescendantOperations().ToArray();
            check(copy.Expression is null&&container.IsStructuredBindingContainer&&container.Initializer==copy&&unit.Variables.Where(v=>v.BindingContainer==container).Select(v=>v.Name).SequenceEqual(new[]{"left","right"})&&value.OperatorKind.Name=="eok_ref_indirect"&&reference.Target==parameter,"The structured array binding must retain its actual bitwise-copy container and exact indirect source parameter expression independently from the binding aliases.");
            check(value==copy.Details.Source&&copy.Operation!.ChildRelations.Single(r=>r.Role.Name=="operand"&&r.Ordinal==0).Child==value&&value.RepresentationRole.Name=="source_semantic"&&operations.Count(o=>o==copy.Operation)==1&&operations.Count(o=>o==value)==1,"The source bitwise-copy recipe must expose its actual source expression and one evaluated operand edge at the initialization site.");
        }
        else check(copies.Length==0,"The lowered array binding retains no bitwise-copy recipe; the reader must not reconstruct the source initialization.");

        var size=unit.Entities.OfType<DependentOperation>().Single(d=>d.Kind.Name=="enk_sizeof_pack");
        var sizeOperand=(ReferenceOperation)size.WrappedExpression!;
        var sizeFunction=unit.Functions.Single(f=>f.Name=="expression_pack_size"&&f.SourceParameters.Any(p=>p.Symbol==sizeOperand.Target));
        check(sizeOperand.Target is VariableSymbol {Name:"values"}&&sizeFunction.SourceParameters.Single().Descriptor.IsPack&&size.References.Single(r=>r.Role.Name=="metadata"&&r.Ordinal==0).Target==sizeOperand&&sizeOperand.RepresentationRole.Name=="metadata"&&!size.Children.Contains(sizeOperand),"The expression sizeof-pack arm must retain its exact values parameter pack through a metadata wrapper reference rather than an evaluated operand edge.");

        var braced=unit.Entities.OfType<DependentOperation>().Single(d=>d.Kind.Name=="enk_braced_init_list");
        var arguments=braced.Operands;var rows=braced.OperandRelations;
        check(arguments.Count==2&&arguments.Cast<ConstantOperation>().Select(a=>((IntegerValue)a.Constant.Value).Value).SequenceEqual(new BigInteger[]{1,2})&&rows.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1})&&rows.All(r=>r.Owner==braced&&r.Role.Name=="operand")&&rows.Select(r=>r.Expression).SequenceEqual(arguments)&&arguments.All(a=>a.RepresentationRole.Name=="metadata")&&braced.Children.Count==0,"The dependent braced argument list must retain its ordered 1/2 expression descriptors and canonical metadata operand links without structural execution children.");

        var names=unit.Entities.OfType<DependentOperation>().Where(d=>d.Kind.Name=="enk_template_name").ToArray();
        var alias=unit.TemplateDescriptors.Single(t=>t.Name=="DeducibleAlias"&&t.TemplateKind.Name=="templk_class");
        check(names.Length==2&&names.All(n=>n.Template==alias&&n.RepresentationRole.Name=="metadata")&&alias.Pattern is CppType,"Both actual alias-deduction template-name expressions must retain the canonical class-template alias descriptor, separately from generated deduction-guide routines with the same name.");

        var pack=unit.Entities.OfType<DependentOperation>().Single(d=>d.Kind.Name=="enk_pack_index");
        var packOperand=(ReferenceOperation)pack.WrappedExpression!;var index=(ConstantOperation)pack.PackIndex!;
        var selection=unit.Functions.Single(f=>f.Name=="select_pack_value"&&f.SourceParameters.Any(p=>p.Symbol==packOperand.Target));
        var indexParameter=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Index"&&p.PlaceholderConstant==index.Constant);
        check(packOperand.Target is VariableSymbol {Name:"values"}&&selection.SourceParameters.Single().Descriptor.IsPack&&index.Constant.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_param"}&&indexParameter.PlaceholderConstant==index.Constant,"Pack indexing must retain the actual values parameter-pack expression and the distinct Index template-parameter constant, preserving both union pointers.");
        check(pack.References.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1})&&pack.References.All(r=>r.Role.Name=="metadata")&&pack.References.Select(r=>r.Target).SequenceEqual(new SemanticEntity[]{packOperand,index})&&packOperand.RepresentationRole.Name=="metadata"&&index.RepresentationRole.Name=="metadata"&&pack.Children.Count==0,"The pack expression and index must retain their exact ordered metadata references and no evaluated child edges.");
    }
}
