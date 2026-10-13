using RoslynObj;

internal static class RetainedBranchesProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="retained_branches_c.roslynobj")
        {
            var enumeration=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="AffiliatedKind");var arithmetic=unit.Types.OfType<ArithmeticType>().Single(t=>t.Arithmetic.AffiliatedEnumType is not null);
            check(arithmetic.Arithmetic.AffiliatedEnumType==enumeration.Type&&arithmetic.Arithmetic.ArithmeticKind.Name=="signed_integer"&&enumeration.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"affiliated_first","affiliated_second"}),"The actual C affiliated arithmetic descriptor must point to its exact enum descriptor independently from the enumerator symbols.");
        }
        if(fixture=="retained_branches_microsoft.roslynobj")
        {
            var storage=unit.Variables.Single(v=>v.Name=="based_storage");var pointer=(PointerType)unit.Variables.Single(v=>v.Name=="based_pointer").Type;
            check(pointer.Pointer.BasedVariable==storage&&pointer.PointeeType.Kind.Name=="tk_integer"&&storage.Type is PointerType,"The actual native based pointer must retain its selected storage variable and integer pointee rather than inferring a base from its name.");
        }
        if(fixture!="retained_branches_cpp.roslynobj")return;
        var assertions=unit.Entities.OfType<StaticAssertion>().ToArray();
        check(assertions.Length==(source?2:1)&&assertions.All(a=>a.Condition.Value is IntegerValue {Value:var value}&&value==1&&a.Message?.Value is StringValue)&&assertions.Any(a=>((StringValue)a.Message!.Value).TargetBytes.SequenceEqual("retained local static-assert message\0"u8.ToArray())),"The genuine nonterse assertions must retain their true constants and local message bytes through named entities, preserving profile-specific retention counts.");
        if(source)check(assertions.Any(a=>((StringValue)a.Message!.Value).TargetBytes.SequenceEqual("retained static-assert message\0"u8.ToArray()))&&assertions.All(a=>unit.Entities.OfType<SourceEntry>().Any(e=>e.Target==a)),"The source global assertion must retain its distinct message and exact source occurrence without deriving one from diagnostics.");
        var template=unit.TemplateDescriptors.Single(t=>t.Name=="RequiredHeader");var header=template.Headers.Single();
        check(header.RequiresClause is {Constraint:var constraint}&&constraint is {RepresentationRole.Name:"metadata"}&&constraint==header.Constraint&&header.RequiresClause.Details.ConstraintExpression==constraint&&header.Template==template,"The actual template-header requires clause must preserve its canonical descriptor and metadata constraint independently from trailing routine clauses.");
        var field=unit.Types.OfType<RecordType>().Single(t=>t.Name=="FieldClosureOwner").Fields.Single(f=>f.Name=="value");var closure=(RecordType)field.InitializerEntities.Single();
        check(closure.IsLambdaClosureClass&&closure.Record.DefinedInFieldInitializer&&closure.LambdaParentEntity==field&&field.DefaultInitializer?.Operation?.RepresentationRole.Name=="metadata","The actual default-member lambda closure must retain its field-parent union, initializer entity membership and unlowered metadata recipe.");
        var references=unit.Entities.OfType<Scope>().SelectMany(s=>s.LocalExpressionReferences).Where(r=>r.Kind.Name=="lerk_array_bound").ToArray();
        check(references.Length==2&&references.All(r=>r.Referrer is ArrayType array&&array.Array.BoundExpression==r.Expression&&r.Owner.Kind.Name=="sck_function"&&r.Expression.RepresentationRole.Name=="metadata"&&r.Expression is ReferenceOperation {Target:VariableSymbol {Name:"bound"}}),"The actual ordinary local array-bound wrappers must preserve the exact array/scope/expression identities and bound variable references as metadata.");
        var dependentArrays=unit.Types.OfType<ArrayType>().Where(a=>a.Array.DependentBoundConstant is not null).ToArray();
        check(dependentArrays.Length==2&&dependentArrays.All(a=>a.Array.DependentBoundConstant is {Details.TemplateLocalExprRef:true,Value:ConstantValue<DependentValueDetails> value} constant&&value.Payload.Kind.Name=="tpck_expression"&&value.Payload.Expression is ReferenceOperation {Target:VariableSymbol {Name:"bound"},RepresentationRole.Name:"metadata"} expression&&a.Array.DependentBoundExpression is null&&constant.ContextualExpressions.Count==1&&constant.ContextualExpressions.Single() is {Kind.Name:"lerk_tpl_param_expr"} reference&&reference.Referrer==constant&&reference.Expression==expression&&reference.Owner.LocalExpressionReferences.Any(r=>r.Ordinal==reference.Ordinal&&r.Referrer==constant&&r.Expression==expression)),"The dependent array bounds must preserve their actual constant-owned template expression contexts and scope identities, with no fabricated array-specific expression link.");
    }
}
