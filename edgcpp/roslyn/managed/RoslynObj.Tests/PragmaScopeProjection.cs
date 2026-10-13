using RoslynObj;

internal static class PragmaScopeProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture is not ("pragma_scope_gnu.roslynobj" or "pragma_scope_upc.roslynobj" or "pragma_scope_microsoft.roslynobj"))return;
        var pragmas=unit.GlobalScope.Pragmas.ToArray();
        SemanticProjection.Sweep(pragmas.Select(p=>p.Details),"pragma",check);
        check(pragmas.Length==4&&pragmas.All(p=>unit.GlobalScope.Pragmas.Contains(p)),"Each genuine pragma fixture must retain its four canonical pragmas in the actual file-scope owner list.");
        if(fixture=="pragma_scope_gnu.roslynobj")
        {
            var visibility=pragmas.Where(p=>p.Gcc is not null).ToArray();
            check(visibility.Length==2&&visibility[0].Gcc is {Kind.Name:"gcc_pk_visibility_push",Visibility.Name:"evk_hidden"}&&visibility[1].Gcc is {Kind.Name:"gcc_pk_visibility_pop",Visibility:null},"The actual visibility push/pop arms must retain the selected hidden kind and absent pop value.");
            check(unit.Variables.Single(v=>v.Name=="pragma_hidden_variable").Details.ElfVisibilityKind?.Name=="evk_hidden"&&unit.Variables.Single(v=>v.Name=="pragma_visible_variable").Details.ElfVisibilityKind?.Name=="evk_unspecified","Recorded variable visibility must follow the retained hidden/ordinary facts without applying a guessed pragma stack.");
            var directive=pragmas.Single(p=>p.Kind.Name=="pk_ident_directive");var tokens=pragmas.Single(p=>p.Kind.Name=="pk_ident_pragma");
            check(directive.Ident!.StringConstant.Value is StringValue bytes&&bytes.TargetBytes.SequenceEqual("typed-ident\0"u8.ToArray())&&tokens.Ident is null&&tokens.Text is not null&&tokens.Text.Contains("plain_tokens"),"The genuine ident directive must retain its typed string constant while the ident pragma retains uninterpreted tokens without a manufactured string payload.");
            var assembly=unit.GlobalScope.Members.OfType<AssemblyDefinition>().Single();
            check(!assembly.IsGnu&&assembly.IsVolatile&&!assembly.Details.HasVolatileKeyword&&!assembly.Details.HasInlineKeyword&&!assembly.IsGoto&&assembly.Operands.Count==0&&assembly.Details.NumberOfConstraints==0&&assembly.AsmString.Value is StringValue asmBytes&&asmBytes.ByteLength==1,"The exact basic file-scope asm descriptor must remain a volatile non-GNU form with no constraint or evaluated operand inference from GNU mode.");
            var function=unit.Functions.Single(f=>f.Name=="pragma_named_result");var scope=function.Body!.Scope!;var result=scope.NrvoVariable!;
            check(result.Name=="result"&&result.SourceCorrespondence?.ParentScope==scope&&result.ContainingEntity==function&&scope.Members.Contains(result)&&scope.AssociatedEntity==function,"The actual named-return optimization slot must retain its exact parent scope and enclosing routine as distinct normalized relationships.");
        }
        if(fixture=="pragma_scope_upc.roslynobj")
        {
            var payloads=pragmas.Select(p=>p.Upc!).ToArray();
            check(payloads.Select(p=>p.Kind.Name).SequenceEqual(new[]{"upc_pk_access","upc_pk_coherence","upc_pk_access","upc_pk_coherence"})&&payloads[0].AccessMethod?.Name=="upc_access_strict"&&payloads[2].AccessMethod?.Name=="upc_access_relaxed"&&payloads[1].CoherenceOperation?.Name=="upc_coherence_stack_save"&&payloads[3].CoherenceOperation?.Name=="upc_coherence_stack_restore"&&payloads.Where(p=>p.Kind.Name=="upc_pk_access").All(p=>p.CoherenceOperation is null)&&payloads.Where(p=>p.Kind.Name=="upc_pk_coherence").All(p=>p.AccessMethod is null),"The retained UPC pragma list must preserve exact strict/save/relaxed/restore order and the disjoint active union payloads.");
        }
        if(fixture=="pragma_scope_microsoft.roslynobj")
        {
            var library=pragmas.Single(p=>p.Comment is not null);
            check(library.Comment is {Kind.Name:"mpct_lib",StringConstant.Value:StringValue bytes}&&bytes.TargetBytes.SequenceEqual("native_semantic.lib\0"u8.ToArray()),"The native Microsoft library pragma must retain its exact kind and target string constant.");
            var conformance=pragmas.Where(p=>p.Conformance is not null).Select(p=>p.Conformance!).ToArray();
            check(conformance.Length==2&&conformance[0] is {Kind.Name:"mpck_forScope",IsPush:true,IsOn:true,IsPop:false,Identifier:"semantic_marker"}&&conformance[1] is {Kind.Name:"mpck_forScope",IsPush:false,IsOn:false,IsPop:true,Identifier:"semantic_marker"},"The native conformance push/on and pop must retain their exact named stack marker and recorded bits.");
            var alias=pragmas.Single(p=>p.IncludeAlias is not null);
            check(alias.IgnoreInBackEnd&&alias.IncludeAlias is {LongFileName:null,ShortFileName:null},"The stock-retained ignored include-alias pragma must preserve both genuinely null payloads rather than reconstructing token filenames.");
            check(unit.Capabilities.Any(c=>c.Name=="ALL_TEMPLATE_INFO_IN_IL"&&c.Enabled==false)&&!unit.Entities.OfType<MicrosoftIfExists>().Any()&&unit.Entities.OfType<Scope>().All(s=>s.IfExistsEntries.Count==0),"The inactive source if-exists retention guard must remain explicit without fabricated dependent entries.");
        }
    }
}
