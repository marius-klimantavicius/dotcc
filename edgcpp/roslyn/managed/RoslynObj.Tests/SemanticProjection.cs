using RoslynObj;
using System.Numerics;
using System.Reflection;

internal static class SemanticProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture is "variables.roslynobj" or "cleanup_semantics.roslynobj" or "source_attributes.roslynobj")
        {
            Sweep(unit.Variables.Select(v=>v.Details),"variable_symbol",check);
            var attributes=unit.Entities.OfType<RoslynObj.Attribute>().ToArray();
            Sweep(attributes.Select(a=>a.Details),"attribute",check);
            Sweep(unit.Entities.OfType<AttributeArgument>().Select(a=>a.Details),"attribute_argument",check);
            Sweep(unit.Entities.OfType<Lifetime>().Select(l=>l.Details),"lifetime",check);
            Sweep(unit.Entities.OfType<Initialization>().Select(i=>i.Details),"initialization",check);
        }
        if(fixture=="reflection.roslynobj")
        {
            var values=unit.Variables.Where(v=>v.Name?.StartsWith("reflected_")==true&&v.ConstantInitializer?.Value is ReflectionValue)
                .ToDictionary(v=>v.Name!,v=>(ReflectionValue)v.ConstantInitializer!.Value);
            check(values["reflected_type"].Entity is ArithmeticType&&values["reflected_record"].Entity is RecordType {Name:"ReflectedRecord"},"Reflection values must navigate their actual arithmetic and record descriptors.");
            check(values["reflected_object"].Entity is VariableSymbol {Name:"reflected_variable"}&&values["reflected_routine"].Entity is FunctionSymbol {Name:"reflected_function"},"Object and routine reflection must preserve the actual symbol identities.");
            check(values["reflected_template"].Entity is TemplateDescriptor {Name:"ReflectedTemplate",DeclarationSymbol:TemplateSymbol}&&values["reflected_field"].Entity is FieldSymbol {Name:"member",ContainingType.Name:"ReflectedRecord"},"Template and field reflections must keep descriptor, declaration and containing-type relationships distinct.");
            var member=(DataMemberSpecification)values["reflected_member_spec"].Entity!;
            check(member.Type==values["reflected_type"].Entity&&member.RequestedAlignment==0&&member.BitWidth==0&&!member.NoUniqueAddress&&member.Name is null&&member.Annotations.Count==0,"A genuine prospective member must preserve its actual type and unset requests without inventing a name or natural layout.");
            check(values.Values.All(v=>v.LocalScopeNumber is not null)&&values["reflected_instantiated_parameter"].Entity==values["reflected_type"].Entity,"Reflection scope numbers and the actual instantiated parameter type must remain exact captured values.");
        }
        if(fixture=="source_attributes.roslynobj")
        {
            var routine=unit.Functions.Single(f=>f.Name=="attributed_routine");
            var primary=routine.Attributes.Single(a=>a.Kind.Name=="ak_deprecated");
            var argument=primary.Arguments.Single();
            if(source)
            {
                var secondary=unit.Declarations.Where(d=>d.Symbol==routine&&d.Secondary is not null).SelectMany(d=>d.Attributes).Single(a=>a.Kind.Name=="ak_deprecated");
                check(primary!=secondary&&secondary.Arguments.Single()==argument&&argument.OwningAttributes.Contains(primary)&&argument.OwningAttributes.Contains(secondary),"Distinct real routine/secondary attributes must preserve their shared actual argument identity and both reverse associations.");
                check(primary.Owner==routine&&secondary.Owner is Declaration&&primary.Group==secondary.Group&&primary.Group!.Members.Contains(primary)&&primary.Group.Members.Contains(secondary),"Canonical attribute ownership and group identity must follow actual descriptors independently from their shared argument.");
            }
            check(argument.Constant?.Value is StringValue text&&text.TargetBytes.SequenceEqual(System.Text.Encoding.ASCII.GetBytes("shared declaration argument").Append((byte)0)),"The shared actual attribute argument must expose its exact target string constant without copied payloads.");
            var alignment=unit.Variables.Single(v=>v.Name=="attributed_alignment").Attributes.Single(a=>a.Kind.Name=="ak_align");
            check(alignment.Arguments.Single() is {Kind.Name:"aak_type",Type:ArithmeticType {Kind.Name:"tk_integer"}},"A retained alignas type argument must navigate its actual type descriptor.");
            var allocation=unit.Functions.Single(f=>f.Name=="attributed_allocate").Attributes.Single(a=>a.Kind.Name=="ak_malloc");
            check(allocation.Arguments[0] is {Kind.Name:"aak_expression",Expression:ReferenceOperation {Target:FunctionSymbol {Name:"attributed_release"},RepresentationRole.Name:"metadata"}}&&allocation.Arguments[1].Constant?.Value is IntegerValue {Value:var index}&&index==1,"A genuine GNU malloc attribute must retain its selected routine expression and exact parameter index as separate argument kinds.");
        }
        if(fixture=="variables.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.Name is not null).GroupBy(v=>v.Name!).ToDictionary(g=>g.Key,g=>g.First());
            check(variables["standard_thread_value"] is {IsThreadLocal:true,StandardThreadLocalDeclared:true,StandardThreadLocalEffective:true,ExtensionThreadLocal:false}&&variables["constant_thread_value"].Details.IsConstinit,"Actual standard TLS declarations must retain their declared/effective flags and constinit state separately.");
            check(variables["extension_thread_value"] is {IsThreadLocal:true,ExtensionThreadLocal:true,StandardThreadLocalDeclared:false},"GNU TLS must preserve its extension discriminator independently from standard thread_local.");
            var ordinary=unit.Functions.Single(f=>f.Name=="variable_bindings").Body!;
            var first=ordinary.Locals.Single(v=>v.Name=="first");var second=ordinary.Locals.Single(v=>v.Name=="second");var container=first.BindingContainer!;
            check(first.IsStructuredBinding&&second.IsStructuredBinding&&second.BindingContainer==container&&container.IsStructuredBindingContainer&&container.Bindings.Select(b=>b.Variable).SequenceEqual(new[]{first,second}),"Structured bindings must preserve their actual container and retained ordered variable list.");
            if(source)
            {
                check(first is {InitializationKind.Name:"initk_binding",BoundExpression.RepresentationRole.Name:"metadata"}&&second is {InitializationKind.Name:"initk_binding",BoundExpression.RepresentationRole.Name:"metadata"},"Source ordinary decomposition must retain actual binding alias recipes as metadata.");
                var tuple=unit.Functions.Single(f=>f.Name=="variable_tuple_bindings").Body!;
                var bindings=tuple.Locals.Where(v=>v.IsStructuredBinding).ToArray();
                check(bindings.Length==2&&bindings.All(v=>v.InitializationKind.Name=="initk_dynamic"&&v.Initializer is not null),"Tuple-like decomposition must preserve its two actual dynamic initialization recipes.");
                var calls=tuple.DescendantOperations().OfType<CallOperation>().Where(c=>c.TargetMethod is {Name:"get",ContainingType.Name:"VariableTuple"}).ToArray();
                check(calls.Length==2&&calls.All(c=>c.RepresentationRole.Name=="source_semantic")&&calls.Select(c=>c.TargetMethod).Distinct().Count()==2&&calls.Select(c=>((IntegerValue)c.TargetMethod!.TemplateArguments.Single().Constant!.Value).Value).Order().SequenceEqual(new BigInteger[]{0,1}),"Actual tuple get<0>/get<1> calls must each occur exactly once in source executable containment with their selected specialization identities.");
            }
            check(variables["guarded"].CleanupRoutine?.Name=="variable_cleanup"&&variables["guarded"].Attributes.Any(a=>a.Kind.Name=="ak_cleanup"&&a.Arguments.Count>0),"GNU variable cleanup must expose both the selected cleanup routine and its canonical attribute arguments.");
            check(variables["variable_alias"].Details.AliasedVariable?.Name=="variable_alias_target"&&variables["variable_alias"].Details.IsGnuAlias&&variables["weak_variable"].Details.IsWeak,"Actual GNU aliases and weak definitions must retain their exact variable association and flags.");
            check(variables["hidden_variable"].Details.ElfVisibilityKind?.Name=="evk_hidden"&&variables["retained_variable"].Details.Section==".variable_test"&&variables["retained_variable"].Details.HasGnuUsedAttribute&&variables["tagged_variable"].Details.HasGnuAbiTagAttribute,"Visibility, section, used and ABI-tag facts must retain their recorded target metadata.");
            check(variables["extended"].Details.ExtendsLifetime&&variables["uninitialized"].Details.IsUninitialized&&variables["local_constant"].Details.IsConstexpr&&variables["local_constant"].Details.IsImmutable,"Lifetime extension and recorded local initialization/constant flags must remain independent facts.");
        }
        if(fixture=="cleanup_semantics.roslynobj")
        {
            var lifetimes=unit.Entities.OfType<Lifetime>().ToArray();
            check(lifetimes.Any(l=>l.BoundEntity is Scope)&&lifetimes.Any(l=>l.BoundEntity is TryDescriptor),"Genuine lifetimes must retain their actual scope and try bindings.");
            if(source)check(lifetimes.Any(l=>l.BoundEntity is LocalStaticInitialization),"Source local-static lifetime bindings must preserve their actual initialization descriptor.");
            else check(lifetimes.Any(l=>l.BoundEntity is BlockDescriptor)&&lifetimes.Any(l=>l.BoundEntity is null),"Lowered block bindings and cleared lifetime bindings must remain distinct without reconstructed ownership.");
            check(lifetimes.Any(l=>l.HasImplicitChild)&&lifetimes.Any(l=>l.Children.Count>0)&&lifetimes.Any(l=>l.ParentCheckpoint is not null),"Recorded lifetime children, implicit-child availability and parent destruction checkpoints must navigate through actual retained relationships.");
            var cleanups=lifetimes.SelectMany(l=>l.Cleanups).ToArray();
            check(cleanups.Length>0&&cleanups.Any(c=>c.Destructor?.ContainingType?.Name=="CleanupBase"),"Minimal lifetime obligations must project the actual initialization destructor without duplicate stored facts.");
            var local=unit.Functions.Single(f=>f.Name=="cleanup_local_static").Body!;
            check(local.Scope!.FunctionStaticLifetime is {Kind.Name:"olk_function_static"} root&&root.BoundEntity==local.Scope&&root!=local.Scope.Lifetime,"The function-static root must retain its exact scope binding separately from the ordinary scope lifetime.");
            check(local.Scope.LocalStaticInitializations.Count>0&&local.Scope.LocalStaticInitializations.All(i=>i.Variable.LocalStaticInitializations.Contains(i)),"Local-static storage lists must navigate canonical descriptors and their exact variable reverse associations.");
            var paths=unit.Functions.Single(f=>f.Name=="cleanup_paths").Body!;
            check(paths.Lifetimes.Count>0&&!paths.Lifetimes.Contains(unit.GlobalScope.Lifetime!),"Body lifetime navigation must remain rooted in captured body/scope/recipe facts without expanding through the file parent into unrelated routine children.");
            if(source)
            {
                var constructor=unit.Functions.Single(f=>f.Name=="CleanupVirtual"&&f.Body is not null);
                var initializers=constructor.Body!.Scope!.ConstructorInitializers;
                check(initializers.Select(i=>i.Kind.Name).SequenceEqual(new[]{"cik_virtual_base_class","cik_field"})&&initializers[0].BaseSubobject!.BaseType.Name=="CleanupBase"&&initializers[1].Field!.Name=="members","Actual constructor recipes must retain construction order even when the written initializer list places the member first.");
                var operations=constructor.Body.DescendantOperations().OfType<InitializationOperation>().ToArray();
                check(initializers.All(i=>i.EffectiveInitialization is not null&&operations.Count(o=>o.Initialization==i.EffectiveInitialization)==1),"Each actual source constructor recipe must occur exactly once in executable containment.");
                var destructor=unit.Functions.Single(f=>f.Name=="~CleanupVirtual"&&f.Body is not null);
                check(destructor.Body!.Scope!.ConstructorInitializers.Select(i=>i.Kind.Name).SequenceEqual(new[]{"cik_field","cik_virtual_base_class"}),"Actual destructor recipes must retain the recorded reversed field/base order.");
                var handlers=paths.DescendantOperations().OfType<TryOperation>().SelectMany(t=>t.Handlers).ToArray();
                check(handlers.Any(h=>h.ParameterInitialization is not null)&&handlers.All(h=>h.ParameterInitializationLifetime==h.ParameterInitialization?.Lifetime),"Handler parameter initialization lifetimes must follow the actual initialization rather than an invented handler lifetime.");
            }
            var arrays=unit.Functions.Where(f=>f.Body is not null).SelectMany(f=>f.Body!.ExceptionHandling.Arrays).ToArray();
            if(!source)
            {
                check(arrays.Any(a=>a.ElementCount==-1)&&arrays.Any(a=>a.ElementCount==2)&&arrays.All(a=>a.HandleConstant is not null),"Actual static and dynamic array cleanup tables must retain their raw handle constants and exact element counts.");
                var actions=unit.Functions.Where(f=>f.Body is not null).SelectMany(f=>f.Body!.ExceptionHandling.CleanupRegions).SelectMany(r=>r.Actions).ToArray();
                var release=actions.Single(a=>a.Storage?.Kind.Name=="no_object");
                check(release.TargetMethod is {Name:"__destroy_exception_object",Parameters.Count:0,ReturnType.Kind.Name:"tk_void"}&&release.Storage is {Offset:var zero,UsesThisParameterOffset:false}&&zero.IsZero,"The genuine object-free release must retain its void() signature and zero no-object locator.");
                var thrown=actions.Where(a=>a.Storage?.Kind.Name=="thrown_object_relative").ToArray();
                check(thrown.Length==2&&thrown.Select(a=>a.Storage!.Offset).Order().SequenceEqual(new BigInteger[]{0,8})&&thrown.All(a=>a.TargetMethod?.ContainingType?.Name=="CleanupBase"&&!a.Storage!.UsesThisParameterOffset),"Thrown aggregate destruction must address its actual exception object at offsets zero/eight independently from a routine receiver.");
                check(actions.All(a=>a.IsUnordered is null),"Lowered EH actions must expose unknown unordered state when their retained table has no such bit.");
                check(actions.Any(a=>a.Array is not null&&a.Storage?.Kind.Name=="direct_variable")&&actions.Any(a=>a.Array is not null&&a.Storage?.Kind.Name=="receiver_relative"),"Array actions must retain their actual per-action variable and receiver-relative storage contexts.");
                check(actions.Where(a=>a.Array is not null&&a.Storage is not null).All(a=>a.Array!.StorageLocations.Contains(a.Storage!))&&arrays.All(a=>a.StorageLocations.Count!=1||a.Storage==a.StorageLocations[0]),"Array storage conveniences must follow per-action associations and return a singular location only when unique.");
                check(actions.Any(a=>a.AbiDetail?.VlaCountLocation is not null),"A genuine VLA destructor obligation must retain its actual count-argument location.");
                check(actions.Where(a=>a.Array is not null&&a.AbiDetail?.VlaCountLocation is not null).All(a=>a.Array!.CountLocations.Contains(a.AbiDetail!.VlaCountLocation!))&&arrays.All(a=>a.CountLocations.Count!=1||a.CountLocation==a.CountLocations[0]),"Array count conveniences must follow actual using-action ABI locations and expose a singular count location only when unique.");
                check(actions.Any(a=>a.AbiDetail?.VttLocation is not null)&&actions.Any(a=>a.Storage?.UsesThisParameterOffset==true),"Virtual-base supplemental arguments and this-relative locations must retain their exact typed addressing facts.");
            }
        }
    }

    private static void Sweep<T>(IEnumerable<T> records,string family,Action<bool,string> check) where T : SemanticDetail
    {
        var properties=typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance)
            .Where(p=>IsScalar(Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType)).ToArray();
        var count=0;
        foreach(var record in records)
        {
            foreach(var property in properties)_=property.GetValue(record);
            count++;
        }
        if(count>0)
        {
            check(properties.Length>0,$"A genuine {family} family must expose its retained scalar fields through typed properties.");
            Console.WriteLine($"scalar:{family},{count} records,{string.Join(',',properties.Select(p=>p.Name))}");
        }
    }

    private static bool IsScalar(Type type) => type==typeof(bool)||type==typeof(long)||type==typeof(BigInteger)||type==typeof(string)||type==typeof(SemanticKind);
}
