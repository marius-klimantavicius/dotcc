using RoslynObj;

internal static class SemanticActivationProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="separate_initialization.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.Name is not null).ToDictionary(v=>v.Name!);
            var rows=unit.FileScopeInitializationRoutines;
            check(unit.Capabilities.Any(c=>c.Name=="SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS"&&c.Enabled==true)&&rows.Count==3&&rows.All(r=>!r.IsThreadLocal),"The actual separate-init profile must expose its three retained non-thread startup routine entries.");
            check(variables["global_priority"].Details.InitializationPriority==201&&rows.Select(r=>r.Routine).SequenceEqual(new[]{variables["global_priority"].Details.DynamicInitializationRoutine,variables["global_first"].Details.DynamicInitializationRoutine,variables["global_second"].Details.DynamicInitializationRoutine}),"The actual priority-first startup list must preserve its retained order and exact per-variable initialization routine identities.");
            check(rows.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1,2})&&rows.All(r=>r.Owner==unit&&r.Routine.Body is not null),"Startup entries must retain their canonical translation-unit owner, ordered ordinals and actual routine bodies.");
            var thread=variables["thread_first"];
            check(thread is {IsThreadLocal:true,StandardThreadLocalDeclared:true}&&thread.Details.ThreadInitializationRoutine is not null&&thread.Details.ThreadWrapperRoutine is not null&&thread.Details.ThreadInitializationRoutine!=thread.Details.ThreadWrapperRoutine,"Actual dynamic TLS must navigate its distinct retained initialization and wrapper routines.");
            check(unit.Capabilities.Any(c=>c.Name=="USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES"&&c.Enabled==true)&&!rows.Any(r=>r.Routine==thread.Details.ThreadInitializationRoutine),"Lazy TLS must preserve its actual per-variable routines separately from the recorded file-startup list.");
        }
        if(fixture=="named_register.roslynobj")
        {
            var variable=unit.Variables.Single(v=>v.Name=="variable_named_register");
            check(unit.Capabilities.Any(c=>c.Name=="NAMED_REGISTERS_ALLOWED"&&c.Enabled==true)&&unit.Capabilities.Any(c=>c.Name=="INCLUDE_EDG_TEST_NAMED_REGISTERS"&&c.Enabled==true)&&variable.NamedRegisterNumber==2&&variable.RegisterKind is null,"The actual configured named-register variable must expose its exact numeric register without substituting a GNU register kind.");
        }
        if(fixture=="variable_semantics.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.Name is not null).ToDictionary(v=>v.Name!);
            check(variables["variable_assembly_name"].AssemblyName=="roslyn_assembly_variable"&&variables["variable_priority"].Details.InitializationPriority==200,"Actual GNU variables must retain their assembly spelling and numeric initialization priority.");
            var assigned=variables["assigned"];
            check(assigned.RegisterKind?.Name=="anr_a"&&assigned.NamedRegisterNumber is null,"An actual GNU register variable must preserve its retained register enum independently from an EDG named-register number.");
            var assembly=unit.Entities.OfType<AssemblyOperation>().Single();var operand=assembly.Operands.Single();
            check(operand.ModifierKind?.Name=="aom_modify"&&operand.Expression is ReferenceOperation reference&&reference.Target==assigned&&assembly.Children.Contains(reference),"The GNU register asm operand must retain its actual modifier, selected variable and evaluated expression edge.");
            var thread=variables["variable_dynamic_tls"];
            if(source)
            {
                check(thread.Details.ThreadInitializationRoutine is null&&thread.Details.ThreadWrapperRoutine is null,"Source TLS must leave not-yet-generated routine links absent.");
                check(unit.GlobalScope.DynamicInitializations.Select(i=>i.Target).SequenceEqual(new[]{thread,variables["variable_priority"]})&&unit.GlobalScope.Details.ScopeInitializationRows.All(r=>r.Owner==unit.GlobalScope)&&unit.GlobalScope.Details.ScopeInitializationRows.Select(r=>r.Initialization).SequenceEqual(unit.GlobalScope.DynamicInitializations),"The actual source scope initializer list must retain TLS-then-priority identities in recorded order, independently from the separate lowered startup list.");
            }
            else check(thread.Details.ThreadInitializationRoutine is not null&&thread.Details.ThreadWrapperRoutine is not null&&thread.Details.ThreadInitializationRoutine!=thread.Details.ThreadWrapperRoutine,"Lowered dynamic TLS must preserve its distinct actual initialization and wrapper links.");
            if(!source&&unit.Capabilities.Any(c=>c.Name=="MAINTAIN_NEEDED_FLAGS"&&c.Enabled==true)&&unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==false))
                check(unit.Variables.Any(v=>v.Details.EffectiveClassTypeinfoVariable is not null),"The actual tableless pointer-exception path must retain its effective-class typeinfo variable association.");
        }
        if(fixture=="constructor_semantics.roslynobj"&&source)
        {
            var constructors=unit.Entities.OfType<ConstructorInitializer>().ToArray();
            SemanticProjection.Sweep(constructors.Select(c=>c.Details),"constructor_initializer",check);
            var array=constructors.Single(c=>c.SourceExpression is not null);
            check(array is {Kind.Name:"cik_field",Field.Name:"values",Initialization.Kind.Name:"dik_nonconstant_aggregate"},"The actual GNU class-array initializer must retain its selected field, aggregate recipe and distinct source-expression link.");
            var delegation=constructors.Single(c=>c.Kind.Name=="cik_delegation");
            check(delegation is {Field:null,BaseSubobject:null,OriginalType.Name:"ConstructorDelegation",Initialization.Constructor.ContainingType.Name:"ConstructorDelegation"},"An actual delegating constructor must preserve its delegation discriminator, original type and selected constructor without a field/base substitution.");
            var function=unit.Functions.Single(f=>f.Details.IsDelegatingCtor&&f.Body?.Scope?.ConstructorInitializers.Contains(delegation)==true);
            check(function.Body!.DescendantOperations().OfType<InitializationOperation>().Count(i=>i.Initialization==delegation.Initialization)==1,"The actual delegating constructor recipe must remain executable exactly once in its recorded body.");
        }
    }
}
