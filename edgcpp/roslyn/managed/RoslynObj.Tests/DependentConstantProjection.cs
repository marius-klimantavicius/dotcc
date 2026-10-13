using RoslynObj;
using System.Numerics;

internal static class DependentConstantProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="dependent_constants.roslynobj")VerifyDependentConstants(unit,check);
        if(fixture!="constant_storage.roslynobj")return;
        var body=unit.Functions.Single(f=>f.Name=="lowered_complex_constant").Body!;
        var returned=body.DescendantOperations().OfType<StatementOperation>().Single(s=>s.Kind.Name=="stmk_return").Expression!;
        if(source)
        {
            check(returned is ConstantOperation {RepresentationRole.Name:"source_semantic",Constant.AssociatedStorageVariable:null,Constant.Value:ComplexValue complex}&&complex.Real is not null&&Matches(complex.Real.Value,5,-2)&&Matches(complex.Imaginary.Value,5,-1)&&unit.Entities.OfType<Constant>().All(c=>c.AssociatedStorageVariable is null),"The source complex literal must retain its exact real/imaginary values without inventing lowered storage.");
        }
        else
        {
            var constant=unit.Entities.OfType<Constant>().Single(c=>c.AssociatedStorageVariable is not null);var storage=constant.AssociatedStorageVariable!;
            check(storage.ConstantInitializer==constant&&unit.Variables.Contains(storage)&&returned is ReferenceOperation {RepresentationRole.Name:"execution",Target:var target}&&target==storage&&body.DescendantOperations().Count(o=>o==returned)==1,"The actual lowered complex constant must retain reciprocal storage/initializer identities and one evaluated return reference to that exact storage variable.");
            var aggregate=(AggregateValue)constant.Value;var array=(AggregateValue)aggregate.Elements.Single().Value;var components=array.Elements.Select(e=>(FloatingValue)e.Value).ToArray();
            check(components.Length==2&&Matches(components[0].Value,5,-2)&&Matches(components[1].Value,5,-1),"The actual lowered complex struct's array element must retain real1.25 then imaginary2.5 as exact target floating components independently from its addressable storage identity.");
        }
    }

    private static void VerifyDependentConstants(TranslationUnit unit,Action<bool,string> check)
    {
        var constant=unit.Entities.OfType<Constant>().Single(c=>c.Value is ConstantValue<DependentValueDetails> v&&v.Payload.Kind.Name=="tpck_sizeof");
        var value=((ConstantValue<DependentValueDetails>)constant.Value).Payload;
        var prototype=unit.Functions.Single(f=>f.Name=="dependent_expression_size"&&f.Instantiation?.IsPrototype==true);
        check(value.Expression is OperatorOperation {OperatorKind.Name:"eok_ref_indirect",RepresentationRole.Name:"metadata"} expression&&expression.Operands.Single() is ReferenceOperation {Target:var parameter,RepresentationRole.Name:"metadata"}&&parameter==prototype.SourceParameters.Single(p=>p.Name=="value").Symbol&&value.Type==expression.Type&&constant.ContextualExpressions.Count==0,"The actual dependent sizeof expression must retain the exact metadata indirection of its reference parameter and operand type, with no fabricated local-expression context.");
        var count=unit.Entities.OfType<DependentOperation>().Single(o=>o.Kind.Name=="enk_sizeof_pack");var templateParameter=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Templates");
        check(count.RepresentationRole.Name=="metadata"&&count.PackTemplate==templateParameter.PlaceholderTemplate&&count.PackTemplate is {TemplateKind.Name:"templk_template_template_param"} descriptor&&descriptor.DeclaredParameters.Single()==templateParameter&&count.PackType is null&&count.WrappedExpression is null&&count.Children.Count==0,"The actual template-template sizeof-pack arm must retain its placeholder descriptor and exact parameter binding, without reading or evaluating inactive type/expression arms.");
        var written=(TypeReference)unit.Entities.OfType<AliasSymbol>().Single(a=>a.Name=="WrittenTypeOperand").UnderlyingType!;
        check(written.Reference.ReferenceKind.Name=="trk_is_typeof_with_type_operand"&&written.OperatorTypeArgument is QualifiedType {IsConst:true,UnderlyingType.Kind.Name:"tk_integer"} operand&&operand==written.UnderlyingType&&written.DecltypeExpression is null,"GNU typeof(const int) must preserve its actual qualified type operand independently from any expression-based typeref arm.");
        var underlying=(TypeReference)unit.Entities.OfType<AliasSymbol>().Single(a=>a.Name=="WrittenEnumOperand").UnderlyingType!;
        var enumeration=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="OperatorEnum");
        check(underlying.Reference.ReferenceKind.Name=="trk_is_underlying_type"&&underlying.OperatorTypeArgument==enumeration.Type&&underlying.UnderlyingType==enumeration.UnderlyingType&&underlying.OperatorTypeArgument!=underlying.UnderlyingType,"The underlying-type operator must retain the exact written enum argument separately from its resulting arithmetic descriptor.");
    }

    private static bool Matches(ExactFloatingPoint value,int significand,int exponent)
    {
        if(value.Class!=FloatingPointClass.Finite||value.IsNegative||value.Radix!=2)return false;
        var difference=checked((int)(value.Exponent-exponent));
        return difference>=0?(value.Significand<<difference)==significand:value.Significand==(new BigInteger(significand)<<-difference);
    }
}
