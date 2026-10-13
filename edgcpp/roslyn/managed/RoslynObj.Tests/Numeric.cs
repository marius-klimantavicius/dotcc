using System.Numerics;
using RoslynObj;

internal static class Numeric
{
    internal static void Verify(TranslationUnit unit,string filename,bool source,Action<bool,string> check)
    {
        if(filename is "types.roslynobj" or "type_edges.roslynobj" or "bit_precise.roslynobj" or "fixed_point.roslynobj" or "nonreal_float.roslynobj" or "gnu_nonreal_float.roslynobj" or "float_kinds.roslynobj" or "profile_numeric.roslynobj")
            SemanticProjection.Sweep(unit.Types.OfType<ArithmeticType>().Select(t=>t.Arithmetic),"arithmetic_type",check);
        if(filename=="fixed_point.roslynobj")
        {
            var values=unit.Entities.OfType<ConstantValue<FixedPointValueDetails>>().Select(v=>v.Payload).ToArray();
            check(values.Any(v=>v.ScaledInteger==16384&&v.Scale==15)&&values.Any(v=>v.ScaledInteger==49152&&v.Scale==15),"Actual fixed-point cache associations must retain distinct exact 0.5 and 1.5 scaled values.");
            if(source)
            {
                var expected=new[]{("short_fraction",new BigInteger(32),7L),("fraction",new BigInteger(-16384),15L),("long_fraction",new BigInteger(268435456),31L),("unsigned_fraction",new BigInteger(49152),16L),("accumulator",new BigInteger(73728),15L),("unsigned_accumulator",new BigInteger(15032385536L),32L),("saturated_accumulator",new BigInteger(49152),15L)};
                check(expected.All(item=>unit.Variables.Single(v=>v.Name==item.Item1).ConstantInitializer!.Value is ConstantValue<FixedPointValueDetails> value&&value.Payload.ScaledInteger==item.Item2&&value.Payload.Scale==item.Item3),"Real TR18037 literals must retain every exact signed scaled integer and its actual binary scale.");
                check(((ArithmeticType)unit.Variables.Single(v=>v.Name=="saturated_accumulator").Type).Arithmetic.IsSaturating&&((ArithmeticType)unit.Variables.Single(v=>v.Name=="unsigned_fraction").Type).Arithmetic.Signed==false,"Fixed-point saturating and unsigned descriptor facts must remain separate from value contents.");
            }
        }
        if(filename is "nonreal_float.roslynobj" or "gnu_nonreal_float.roslynobj")
        {
            var expected=new[]{("complex_float",BigInteger.Parse("10485760"),-23L,false,BigInteger.Parse("10485760"),-22L,false),("complex_double",BigInteger.Parse("7881299347898368"),-51L,true,BigInteger.Parse("4785074604081152"),-50L,false),("complex_long_double",BigInteger.Parse("9223372036854775808"),-66L,false,BigInteger.Parse("13835058055282163712"),-64L,true)};
            foreach(var item in expected)
            {
                var variable=unit.Variables.Single(v=>v.Name==item.Item1);
                var (real,imaginary)=Parts(variable.ConstantInitializer!.Value);
                var binary64Long=item.Item1=="complex_long_double"&&real.Details.RepresentationKind?.Name=="binary64";
                check(binary64Long ? Match(real,BigInteger.One<<52,-55,false)&&Match(imaginary,new BigInteger(3)<<51,-53,true) : Match(real,item.Item2,item.Item3,item.Item4)&&Match(imaginary,item.Item5,item.Item6,item.Item7),"Actual complex real/imaginary components must preserve their distinct exact significands, exponents and signs across stock lowering.");
                if(source)check(variable.Type.Kind.Name=="tk_complex"&&variable.ConstantInitializer.Value.Kind.Name=="ck_complex","An unlowered complex occurrence must retain its actual type and value subtype.");
                if(variable.ConstantInitializer.Value is ComplexValue complex)check(complex.Real==real&&complex.Imaginary==imaginary&&complex.Details.RealValue==real&&complex.Payload.ImaginaryValue==imaginary,"Ordinary complex component properties must return the canonical retained floating values.");
            }
            if(filename=="nonreal_float.roslynobj")
            {
                var imaginary=new[]{("imaginary_float",BigInteger.Parse("8388608"),-24L,false),("imaginary_double",BigInteger.Parse("4503599627370496"),-54L,true),("imaginary_long_double",BigInteger.Parse("13835058055282163712"),-64L,false)};
                check(imaginary.All(item=>
                {
                    var value=(FloatingValue)unit.Variables.Single(v=>v.Name==item.Item1).ConstantInitializer!.Value;
                    return item.Item1=="imaginary_long_double"&&value.Details.RepresentationKind?.Name=="binary64" ? Match(value,new BigInteger(3)<<51,-53,false) : Match(value,item.Item2,item.Item3,item.Item4);
                }),"C99 imaginary values must preserve exact scalar contents and signs independently from complex component cache entries.");
                if(source)check(imaginary.All(item=>unit.Variables.Single(v=>v.Name==item.Item1).Type.Kind.Name=="tk_imaginary"),"Unlowered imaginary occurrences must retain their actual imaginary type descriptors.");
            }
        }
        if(filename=="label_difference.roslynobj"&&!source)
        {
            var values=unit.Entities.OfType<LabelDifferenceValue>().ToArray();
            check(values.Length==2&&values.Select(v=>v.ToLabel?.Name).Order().SequenceEqual(new[]{"first","second"})&&values.All(v=>v.FromLabel?.Name=="base"&&v.LeftLabel==v.ToLabel&&v.RightLabel==v.FromLabel),"Actual GNU label differences must preserve &&to minus &&from operand order.");
            check(values.All(v=>((ConstantValue<AddressValueDetails>)v.FromValue).Payload.BaseEntity==v.FromLabel&&((ConstantValue<AddressValueDetails>)v.ToValue).Payload.BaseEntity==v.ToLabel),"Label-difference components must retain the exact corresponding address-value label relationships.");
        }
        if(filename=="member_pointers.roslynobj")
        {
            var values=unit.Entities.OfType<ConstantValue<MemberPointerValueDetails>>().Select(v=>v.Payload).ToArray();
            if(!source)
                check(values.Length==2&&values.All(v=>v.IsNull&&v.ContainingType is null&&v.Member is null)&&values.Any(v=>v.IsFunctionPtr)&&values.Any(v=>!v.IsFunctionPtr),"Lowered null member-pointer values must preserve their actual subtype without inventing a lost containing class.");
            else
            {
                MemberPointerValueDetails Value(string name)=>((ConstantValue<MemberPointerValueDetails>)unit.Variables.Single(v=>v.Name==name).ConstantInitializer!.Value).Payload;
                var baseData=Value("base_data");var convertedData=Value("converted_data");var baseFunction=Value("base_function");var convertedFunction=Value("converted_function");
                check(baseData.Member is FieldSymbol {Name:"data",ContainingType.Name:"PointerBase"}&&baseData.ContainingType?.Name=="PointerBase"&&convertedData.Member==baseData.Member&&convertedData.ContainingType?.Name=="PointerDerived","A converted data member pointer must preserve the actual member's declaring type separately from the converted containing class.");
                check(baseFunction.Member is FunctionSymbol {Name:"method",ContainingType.Name:"PointerBase"}&&baseFunction.ContainingType?.Name=="PointerBase"&&convertedFunction.Member==baseFunction.Member&&convertedFunction.ContainingType?.Name=="PointerDerived","A converted function member pointer must preserve its exact routine and actual containing class independently.");
                check(convertedData.Subobject==convertedFunction.Subobject&&convertedData.Subobject is {BaseType.Name:"PointerBase",MostDerivedType.Name:"PointerDerived"},"Both actual converted member-pointer values must retain the same exact base-subobject adjustment relationship.");
                check(Value("null_data") is {IsNull:true,IsFunctionPtr:false,Member:null,ContainingType.Name:"PointerDerived"}&&Value("null_function") is {IsNull:true,IsFunctionPtr:true,Member:null,ContainingType.Name:"PointerDerived"},"Unlowered null data/function member-pointer values must retain their actual class and distinct kind facts.");
            }
        }
    }

    private static (FloatingValue Real,FloatingValue Imaginary) Parts(ConstantValue value)
    {
        if(value is ComplexValue complex)return (complex.Real!,complex.Imaginary);
        var elements=((AggregateValue)value).Elements;
        if(elements.Count==1&&elements[0].Value is AggregateValue nested)elements=nested.Elements;
        return ((FloatingValue)elements[0].Value,(FloatingValue)elements[1].Value);
    }
    private static bool Match(FloatingValue value,BigInteger significand,long exponent,bool negative)
        => value.Value is {Class:FloatingPointClass.Finite,Radix:2} exact&&exact.Significand==significand&&exact.Exponent==exponent&&exact.IsNegative==negative;
}
