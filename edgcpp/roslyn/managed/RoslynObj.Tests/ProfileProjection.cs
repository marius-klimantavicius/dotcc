using System.Numerics;
using RoslynObj;

internal static class ProfileProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture is "basic.roslynobj" or "gnu_asm_modify.roslynobj" or "source_statements.roslynobj" or "native_microsoft.roslynobj" or "upc_dynamic.roslynobj" or "upc_static.roslynobj" or "constant_aggregates.roslynobj")
        {
            var capabilities=unit.Capabilities.ToDictionary(c=>c.Name);
            var headerNames=new[]{"plain_chars_are_signed","pcc_compatibility_mode","enum_type_is_integral","default_max_member_alignment","UCN_identifiers_used","vla_used","any_templates_seen","prototype_instantiations_in_il","il_has_all_prototype_instantiations","il_has_C_semantics","target_configuration_index","gcc_mode","gpp_mode","clang_mode","gnu_version","clang_version","short_enums","default_nocommon","gnu_c89_inlining","near_and_far_are_enabled","far_data_pointers","far_code_pointers","default_upc_strict_access"};
            foreach(var name in headerNames.Select(n=>"il_header."+n).Concat(new[]{"microsoft_mode","microsoft_version"}))
            {
                check(capabilities.TryGetValue(name,out var capability)&&BigInteger.TryParse(capability.Value,out var value)&&capability.Enabled==(value!=0),"Each recorded IL-header capability must expose its exact numeric value and matching enabled discriminator through the ordinary typed collection.");
                Console.WriteLine($"header-capability:{capability!.Name}={capability.Value},enabled={capability.Enabled}");
            }
            foreach(var name in new[]{"gcc_mode","gpp_mode","clang_mode","gnu_version","target_configuration_index"})
                check(capabilities["il_header."+name].Value==capabilities[name].Value,"Header and runtime copies of the same retained mode/version/target fact must agree without inferring values from the dialect label.");
            if(fixture is "gnu_asm_modify.roslynobj" or "source_statements.roslynobj")
                check(capabilities["il_header.gpp_mode"] is {Value:"1",Enabled:true}&&capabilities["il_header.gnu_version"].Value=="130000","The actual GNU C++ fixture must retain its recorded mode and selected compiler version.");
            if(fixture=="native_microsoft.roslynobj")
                check(capabilities["microsoft_mode"] is {Value:"1",Enabled:true}&&capabilities["microsoft_version"].Value=="1930"&&capabilities["il_header.target_configuration_index"].Value=="2","The actual Microsoft fixture must retain its recorded runtime mode, version and selected target index.");
            if(fixture is "upc_dynamic.roslynobj" or "upc_static.roslynobj" or "constant_aggregates.roslynobj")
                check(capabilities["il_header.il_has_C_semantics"] is {Value:"1",Enabled:true},"An actual C-family fixture must preserve the retained C-semantics header fact.");
        }
        if(unit.Capabilities.Any(c=>c.Name=="roslyn_build_profile"&&c.Value=="extern-inline"))
        {
            check(unit.Capabilities.Any(c=>c.Name=="INSTANTIATE_EXTERN_INLINE"&&c.Enabled==true)&&unit.Capabilities.Any(c=>c.Name=="instantiate_extern_inline"&&c.Enabled==true),"The extern-inline profile must retain both its active compile gate and actual runtime mode.");
            check(unit.Functions.Any(f=>f.Details.InlineInstanceRequired==true)&&unit.Functions.Any(f=>f.Details.InlineInstanceRequired==false)&&unit.Functions.All(f=>f.Details.InlineInstanceRequired is not null),"Actual extern-inline routines must preserve positive and negative required-instance flags under the enabled stock guard.");
            check(unit.Capabilities.Any(c=>c.Name=="DO_FULL_PORTABLE_EH_LOWERING"&&c.Enabled==false)&&unit.Functions.Any(f=>f.Body?.ExceptionHandling.CleanupRegions.Count>0),"Extern-inline instantiation must retain actual partial EH cleanup regions without enabling full portable EH lowering.");
        }
        bool Enabled(string name)=>unit.Capabilities.Single(c=>c.Name==name).Enabled==true;
        if(fixture=="instantiation_flags.roslynobj")
        {
            check(Enabled("AUTOMATIC_TEMPLATE_INSTANTIATION")&&Enabled("INSTANTIATE_INLINE_VARIABLES"),"The genuine instantiation profile must record the enabled guards for its retained variable facts.");
            var suppressed=unit.Variables.Single(v=>v.DoNotInstantiate==true);
            var required=unit.Variables.Single(v=>v.InstanceRequired==true);
            check(suppressed is {Name:"value",CanBeInstantiated:false,InstanceRequired:false}&&suppressed.Details.ExplicitDoNotInstantiate&&suppressed.Details.IsSpecialized&&suppressed.Details.SpecializedWithOldSyntax&&suppressed.ContainingEntity==unit.GlobalScope&&suppressed.Instantiation is not null&&suppressed.TemplateArguments.Count==0,"The actual suppressed member must retain its flags and lowered file scope while its discarded argument list remains empty.");
            check(required is {Name:"value",CanBeInstantiated:true,DoNotInstantiate:false,InlineInstanceRequired:true}&&required.Type.Kind.Name=="tk_integer"&&required.ContainingEntity==unit.GlobalScope&&required.Instantiation is not null&&required.TemplateArguments.Count==0,"The actual required member must retain its opposite flags, current integer type and lowered owner without recovering absent instance arguments.");
            check(unit.Variables.Single(v=>v.Name=="required_inline_variable").InlineInstanceRequired==true&&unit.Variables.Single(v=>v.Name=="value"&&v.Instantiation is null).InlineInstanceRequired==true,"Actual ordinary and lowered class-member inline variables must retain their enabled inline-instance requirement flags.");
            SemanticProjection.Sweep(unit.Variables.Select(v=>v.Details),"variable_symbol",check);
        }
        if(fixture=="profile_sun.roslynobj")
        {
            check(Enabled("SUN_EXTENSIONS_ALLOWED")&&Enabled("sun_mode")&&Enabled("sun_linker_scope_allowed"),"The genuine Sun object must record its actual compile and runtime extension configuration.");
            foreach(var (prefix,modifier) in new[]{("visible","dmt_global_link_scope"),("symbolic","dmt_symbolic_link_scope"),("hidden","dmt_hidden_link_scope")})
            {
                check(unit.Variables.Single(v=>v.Name==prefix+"_value").DeclarationModifiers.Any(k=>k.Name==modifier)&&unit.Functions.Single(f=>f.Name==prefix+"_function").DeclarationModifiers.Any(k=>k.Name==modifier),"Actual Sun variable and routine declarations must navigate their retained linker-scope modifiers.");
            }
            check(unit.Types.Single(t=>t.Name=="HiddenRecord").DeclarationModifiers.Any(k=>k.Name=="dmt_hidden_link_scope"),"An actual Sun class must share the normalized declaration-modifier projection.");
        }
        if(fixture=="profile_usl.roslynobj")
        {
            var legacy21=Enabled("cfront_2_1_mode");
            check(legacy21!=Enabled("cfront_3_0_mode")&&Enabled("CFRONT_OBJECT_CODE_COMPATIBILITY"),"The genuine USL profile must retain exactly its active 2.1 or 3.0 mode.");
            var nested=unit.Types.Single(t=>t.Name=="LegacyNested");
            check(nested.CurrentName==(legacy21?"LegacyNested":"__Q2_11LegacyOuter12LegacyNested")&&nested.SourceCorrespondence!.NameHasBeenMangled==!legacy21,"The USL modes must preserve their distinct actual nested-type mangling while ordinary names remain available.");
            var derived=unit.Types.Single(t=>t.Name=="LegacyDerived");
            check(derived.BaseSubobjects.Any(b=>b.IsVirtual&&b.BaseType.Name=="LegacyBase"),"An actual legacy virtual base must retain its canonical contextual base-subobject identity.");
            var parameters=unit.Functions.SelectMany(f=>f.Parameters).ToArray();
            check(parameters.Count(p=>p.Role.Name=="virtual_base_parameter")==1&&parameters.Where(p=>p.Role.Name=="virtual_base_parameter").All(p=>p.BaseSubobject?.BaseType.Name=="LegacyBase")&&parameters.Count(p=>p.Role.Name=="destructor_control_parameter")==2,"Actual USL signatures must expose their virtual-base and destructor-control parameter roles and base relationships.");
            check(unit.Variables.Single(v=>v.Name=="legacy_data_member").ConstantInitializer!.Value is IntegerValue&&unit.Variables.Single(v=>v.Name=="legacy_function_member").ConstantInitializer!.Value is AggregateValue,"Actual lowered USL data and function member-pointer constants must preserve their distinct integer and aggregate representations.");
        }
        if(fixture=="profile_numeric.roslynobj")
        {
            FloatingValue Value(string name)=>(FloatingValue)unit.Variables.Single(v=>v.Name==name).ConstantInitializer!.Value;
            check(EqualsRational(Value("host_float"),5,4)&&EqualsRational(Value("host_double"),-7,2)&&EqualsRational(Value("host_long_double"),5,4),"Each genuine numeric profile must preserve its finite literals as exact target values independent of host precision.");
            check(new[]{"host_float_negative_zero","host_double_negative_zero","host_long_double_negative_zero","host_narrow_negative_zero"}.All(n=>Value(n).Value is {IsZero:true,IsNegative:true}),"Each numeric profile must retain signed zero for every recorded target representation.");
            check(Value("host_infinity").Value is {Class:FloatingPointClass.Infinity,IsNegative:false}&&Value("host_nan").Value is {Class:FloatingPointClass.NaN,NaNPayload: {} payload,IsQuietNaN:true}&&payload==801,"Each numeric profile must retain actual infinity classification and the exact quiet NaN payload.");
            check(Value("host_narrow").Value is {Significand: {} narrow,Exponent: {} exponent}&&narrow==1025&&exponent==-10&&Value("host_narrow").Details.RepresentationKind?.Name=="binary16","Host floating storage must preserve the target binary16 value without narrowing through the host format.");
            if(Enabled("USE_DOUBLE_FOR_HOST_FP_VALUE"))check(Value("host_long_double").Details.RepresentationKind?.Name=="binary64","The actual host-double profile must retain its configured binary64 long-double target.");
            if(Enabled("USE_LONG_DOUBLE_FOR_HOST_FP_VALUE"))check(Value("host_long_double").Details.RepresentationKind?.Name=="binary80","The actual host-long-double profile must retain its configured binary80 long-double target.");
        }
    }

    internal static bool EqualsRational(FloatingValue value,BigInteger numerator,BigInteger denominator)
    {
        var exact=value.Value;
        if(exact.Class!=FloatingPointClass.Finite||exact.Radix!=2||exact.IsNegative!=(numerator.Sign<0))return false;
        var exponent=checked((int)exact.Exponent);
        return exponent>=0 ? (exact.Significand<<exponent)*denominator==BigInteger.Abs(numerator) : exact.Significand*denominator==(BigInteger.Abs(numerator)<<-exponent);
    }
}
