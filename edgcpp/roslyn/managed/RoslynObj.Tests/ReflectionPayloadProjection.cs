using RoslynObj;
using System.Numerics;

internal static class ReflectionPayloadProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture=="reflection_payloads.roslynobj")VerifyAnnotations(unit,check);
        if(fixture=="retained_tokens.roslynobj")VerifyTokens(unit,check);
    }

    private static void VerifyAnnotations(TranslationUnit unit,Action<bool,string> check)
    {
        var specification=unit.Entities.OfType<DataMemberSpecification>().Single();
        check(specification.Type is ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"},"The reflected member specification must retain its actual int type.");
        var reflectedSpecification=unit.Entities.OfType<ReflectionValue>().Single(v=>v.Entity==specification);
        check(reflectedSpecification.Entity==specification&&unit.Constants.Any(c=>c.Value==reflectedSpecification),"The member reflection must target the canonical data-member specification descriptor.");
        var annotations=specification.Annotations;
        check(annotations.Count==2&&annotations.SequenceEqual(specification.Attributes)&&annotations[0]!=annotations[1],"The specification must retain two distinct annotation descriptors in actual list order.");
        var expected=new BigInteger[]{17,23};
        for(var ordinal=0;ordinal<annotations.Count;ordinal++)
        {
            var annotation=annotations[ordinal];var argument=annotation.Arguments.Single();var constant=argument.Constant!;
            check(annotation.Kind.Name=="ak_annotation"&&annotation.Owner==specification&&annotation.Owners.SequenceEqual(new SemanticEntity[]{specification}),"Each annotation must retain the exact prospective member as its actual canonical list owner.");
            var link=annotation.Details.AttributeArgumentLinkRows.Single();
            check(argument.Kind.Name=="aak_constant"&&link.Ordinal==0&&link.Argument==argument&&argument.OwningAttributes.SequenceEqual(new[]{annotation})&&constant.Value is IntegerValue integer&&integer.Value==expected[ordinal],"Each ordered annotation must retain its exact singleton constant argument, with values17 then23.");
            var reflection=unit.Entities.OfType<ReflectionValue>().Single(v=>v.Entity==constant);
            check(reflection.EntityKind.Name=="iek_constant"&&reflection.Entity==constant,"The annotation argument must be the same canonical constant retained by its value reflection.");
        }
    }

    private static void VerifyTokens(TranslationUnit unit,Action<bool,string> check)
    {
        var expression=unit.Entities.OfType<DependentOperation>().Single(e=>e.Kind.Name=="enk_token_sequence");
        var sequence=expression.TokenSequence!;var tokens=sequence.Tokens;
        check(sequence==expression.Dependency.TokenSequence&&unit.Entities.OfType<TokenSequence>().Single()==sequence&&tokens.All(t=>t.Owner==sequence),"The token expression and every token row must reference the same canonical token-sequence descriptor.");
        check(tokens.Select(t=>t.Ordinal).SequenceEqual(Enumerable.Range(0,8).Select(i=>(long)i)),"The actual source token list must preserve eight contiguous recorded ordinals.");
        check(tokens.Select(t=>t.Kind.Name).SequenceEqual(new[]{"tok_int","tok_identifier","tok_assign","tok_backslash","tok_identifier","tok_lparen","tok_rparen","tok_semicolon"}),"The token list must retain exact typed token kinds, including interpolation delimiters.");
        check(tokens.Select(t=>t.Spelling).SequenceEqual(new[]{"int","generated_value","=","\\","val","(",")",";"}),"The token list must retain independent spellings in source order without duplicating interpolation N as a token.");
        var operand=expression.OperandRelations.Single();var interpolation=(ConstantOperation)operand.Expression;
        var parameter=unit.TemplateDescriptors.Single(t=>t.Name=="retained_token_pattern").Parameters.Single(p=>p.Name=="N");
        check(operand.Owner==expression&&operand.Ordinal==0&&expression.Operands.Single()==interpolation&&interpolation.Constant.Value==parameter.PlaceholderConstant!.Value&&interpolation.Type==interpolation.Constant.Type,"The separate interpolation list must retain its exact ordered N constant expression and parameter value/type.");
        var reference=expression.References.Single();
        check(expression.RepresentationRole.Name=="metadata"&&interpolation.RepresentationRole.Name=="metadata"&&reference.Ordinal==0&&reference.Role.Name=="metadata"&&reference.Target==interpolation&&expression.Children.Count==0&&!interpolation.StructuralParents.Contains(expression),"The token interpolation must remain an actual metadata reference without an evaluated structural edge.");
        check(!unit.Functions.Where(f=>f.Body is not null).SelectMany(f=>f.Body!.DescendantOperations()).Any(o=>o==expression||o==interpolation),"Prototype token payloads and interpolation operands must remain outside evaluated function traversal.");
        check(expression.Source is {Range:null},"A token expression must retain its recorded source point without reading its unavailable optional expression range.");
    }
}
