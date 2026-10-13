using RoslynObj;
using System.Numerics;

internal static class AvailabilityOrphanDerivationProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="routines.roslynobj")VerifyAvailability(unit,source,check);
        if(fixture=="layout.roslynobj")VerifyDerivations(unit,check);
        if(fixture is "templates.roslynobj" or "source_order.roslynobj" or "retained.roslynobj" or "common.roslynobj")VerifyOrphans(unit,fixture,source,check);
        if(fixture=="source_order.roslynobj"&&source)
        {
            var linkage=unit.Entities.OfType<LinkageSpecification>().Single();
            check(linkage.NameString.Value is StringValue name&&name.TargetBytes.SequenceEqual(new byte[]{67,0})&&linkage.NameString==linkage.Details.NameString,"The actual linkage block must retain its exact C-plus-terminator constant bytes independently from its linkage enum.");
        }
    }

    private static void VerifyAvailability(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        FactAvailability Fact(SemanticEntity entity,string name)=>unit.SemanticModel.GetAvailability(entity).Single(f=>f.Property==name);
        check(Fact(unit,"macro_arguments") is {State:AvailabilityState.Available,Reason:null}&&Fact(unit,"tokens") is {State:AvailabilityState.NotRetained,Reason:"Stock IL retains specialized token sequences, but no general source token stream."}&&Fact(unit,"trivia") is {State:AvailabilityState.NotRetained,Reason:"No complete source trivia stream is retained by this stock configuration."},"Translation-unit availability must preserve recorded fact names, states and exact reasons without promoting specialized token sequences into a general token/trivia stream.");
        if(source)check(unit.SemanticModel.GetAvailability(unit).All(f=>f.Property!="declaration_sequence"),"An omitted source-profile declaration-sequence fact must remain absent from the availability list rather than acquiring a manufactured default entry.");
        else check(Fact(unit,"declaration_sequence") is {State:AvailabilityState.Disabled,Reason:"GENERATE_SOURCE_SEQUENCE_LISTS is disabled in this resolved build."},"The lowered build must report its explicit disabled source-list fact and recorded build reason.");
        var function=unit.Functions.Single(f=>f.Name=="routine_typed_declaration");
        check(Fact(function,"body") is {State:AvailabilityState.Available,Reason:null}&&function.Body is not null&&Fact(function,"original_signature").State==(source?AvailabilityState.Available:AvailabilityState.NotRetained)&&Fact(function,"original_signature").Reason==(source?null:"Stock lowering changes the current signature; the unlowered routine type is not backend-readable."),"Routine availability must preserve its own retained-body fact and source/current-signature contrast independently from optional defining-type navigation.");
        var discarded=unit.Functions.Single(f=>f.BodyState.Name=="discarded");
        check(discarded.IsDefined&&discarded.Body is null&&Fact(discarded,"body") is {State:AvailabilityState.NotRetained,Reason:"EDG records a completed definition whose body is no longer retained (for example, a trivial special member)."},"The recorded discarded definition must expose a lost-body availability reason without becoming declaration-only.");
        var integer=function.ReturnType;var noObject=unit.Types.Single(t=>t.Kind.Name=="tk_void");
        check(Fact(integer,"size") is {State:AvailabilityState.Available,Reason:null}&&Fact(integer,"alignment") is {State:AvailabilityState.Available,Reason:null}&&integer.Size is not null&&integer.Alignment is not null&&noObject.IsIncomplete&&Fact(noObject,"size") is {State:AvailabilityState.Absent,Reason:"The retained type is incomplete."}&&Fact(noObject,"alignment") is {State:AvailabilityState.Absent,Reason:"The retained type is incomplete."}&&noObject.Size is null&&noObject.Alignment is null,"Availability must be queried for each exact type owner and preserve stock's incomplete-void state before the non-object rule.");
        check(Fact(function.Signature,"size") is {State:AvailabilityState.NotApplicable,Reason:"The descriptor is not an object type."}&&Fact(function.Signature,"alignment") is {State:AvailabilityState.NotApplicable,Reason:"The descriptor is not an object type."},"The actual non-object routine descriptor must retain its separately recorded not-applicable size/alignment state.");
        var placeholder=unit.TemplateDescriptors.Single(t=>t.Name=="RoutineGuide"&&t.TemplateKind.Name=="templk_class").Parameters.Single().PlaceholderType!;
        check(Fact(placeholder,"size") is {State:AvailabilityState.Dependent,Reason:"A placeholder or instantiation-dependent descriptor does not specify a concrete object size."}&&placeholder.Size is null,"The real class-template placeholder must preserve its recorded dependent size and reason without using the retained helper coefficient as an object size.");
        var call=unit.Functions.Single(f=>f.Name=="main").Body!.DescendantOperations().OfType<CallOperation>().Single(c=>c.TargetMethod==function);
        check(Fact(call,"source_target").State==(source?AvailabilityState.Available:AvailabilityState.NotRetained)&&Fact(call,"source_dispatch").State==(source?AvailabilityState.Available:AvailabilityState.NotRetained)&&call.SourceTargetMethod==(source?function:null)&&Fact(call,"source_target").Reason==(source?null:"Stock lowering rewrites calls; a current callee is not a retained source-selected target.")&&Fact(call,"source_dispatch").Reason==(source?null:"Stock lowering rewrites member and virtual dispatch; the current call form is retained separately.")&&unit.SemanticModel.GetAvailability(call).All(f=>f.Property!="body"),"Call availability must preserve exact source target/dispatch contrast and owner-specific reasons, independently from its still-retained current callee.");
    }

    private static void VerifyDerivations(TranslationUnit unit,Action<bool,string> check)
    {
        var diamond=unit.Types.OfType<RecordType>().Single(t=>t.Name=="LayoutDiamond");
        var left=diamond.BaseSubobjects.Single(b=>b.BaseType.Name=="LayoutLeft");var right=diamond.BaseSubobjects.Single(b=>b.BaseType.Name=="LayoutRight");var shared=diamond.BaseSubobjects.Single(b=>b.BaseType.Name=="LayoutRoot");var paths=shared.Derivations;
        SemanticProjection.Sweep(new[]{left,right,shared}.SelectMany(b=>b.Derivations).Select(d=>d.Details),"base_derivation",check);
        check(paths.Count==2&&paths.Select(p=>p.Details.Ordinal).SequenceEqual(new long[]{0,1})&&paths.All(p=>!p.IsDirect&&p.Access?.Name=="as_public")&&paths[0].IsPreferred&&!paths[1].IsPreferred,"The shared virtual base must retain its actual ordered alternatives and recorded preferred/public flags; no independent access or path selection is computed.");
        foreach(var (path,first) in new[]{(paths[0],left),(paths[1],right)})
            check(path.BaseSubobject==shared&&path.Steps.Select(s=>s.Subobject).SequenceEqual(new[]{first,shared})&&path.Steps.All(s=>s.Owner==path)&&path.PathTail is {Ordinal:1,Subobject:var terminal}&&terminal==shared&&path.ReversedSteps.Select(s=>s.Subobject).SequenceEqual(new[]{shared,first})&&path.Steps.Count==path.Details.PathTailOrdinal+1,"Forward and reverse navigation must use only the actual tail-bounded segment and exact contextual subobjects, preserving the wrapper owner despite potentially shared prefixes.");
        check(new[]{left,right}.All(b=>b.Derivations.Single() is {IsDirect:true,IsPreferred:true,Access.Name:"as_public"} direct&&direct.PathTail is {Ordinal:0,Subobject:var terminal}&&terminal==b&&direct.Steps.Count==1&&direct.ReversedSteps.Single().Subobject==b),"Each direct-base derivation must retain its singleton path, exact tail and copied direct/preferred/access fields separately from the indirect alternatives.");
    }

    private static void VerifyOrphans(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        var headers=unit.Entities.OfType<SemanticRecord<OrphanScopeDetails>>().Select(o=>o.Details).ToArray();
        SemanticProjection.Sweep(headers,"orphan_scope",check);
        if(fixture=="templates.roslynobj"&&!source)
        {
            var function=unit.Functions.Single(f=>f.Name=="scalar_static");var header=headers.Single(h=>h.Routine==function);var rows=header.OrphanScopeMemberRows.Where(r=>r.Kind.Name=="variable").ToArray();var variable=unit.Variables.Single(v=>v.Name=="retained_scalar");
            check(header.TranslationUnit==unit&&header.ScopeNumber==39&&rows.Length==1&&rows[0].Ordinal==0&&rows[0].Entity==variable&&rows[0].Owner.Routine==function&&variable.SourceCorrespondence?.EnclosingRoutine==header.Routine,"The native local-static orphan bucket must preserve its literal memory-scope number39 and exact routine/variable association without creating lexical or execution ownership from membership.");
        }
        if(fixture=="source_order.roslynobj"&&source||fixture=="common.roslynobj"&&source)
        {
            var function=unit.Functions.Single(f=>f.Name==(fixture=="source_order.roslynobj"?"local_entries":"common_locals"));var header=headers.Single(h=>h.Routine==function&&h.OrphanScopeMemberRows.Any(r=>r.Kind.Name=="type"));var rows=header.OrphanScopeMemberRows.Where(r=>r.Kind.Name=="type").ToArray();var name=fixture=="source_order.roslynobj"?"Local":"LocalRecord";
            check(header.TranslationUnit==unit&&header.ScopeNumber==30&&rows.Length==1&&rows[0].Ordinal==0&&rows[0].Entity is RecordType {Name:var typeName} type&&typeName==name&&unit.Types.Contains(type)&&type.SourceCorrespondence?.EnclosingRoutine==header.Routine,"The actual source orphan type bucket must preserve its scope number30, canonical local class and recorded routine association independently from its source sublist buckets.");
        }
        if(fixture=="retained.roslynobj"&&source)
        {
            var function=unit.Functions.Single(f=>f.Name=="lambda_uses");var header=headers.Single(h=>h.Routine==function);var rows=header.OrphanScopeMemberRows.Where(r=>r.Kind.Name=="type").ToArray();
            check(header.TranslationUnit==unit&&header.ScopeNumber==21&&rows.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1,2})&&rows.All(r=>r.Entity is RecordType {IsLambdaClosureClass:true} type&&unit.Types.Contains(type)&&type.SourceCorrespondence?.EnclosingRoutine==function)&&rows.Select(r=>unit.Entities.OfType<Initialization>().Single(i=>i.Lambda?.ClosureType==r.Entity).Target!.Name).SequenceEqual(new[]{"first","second","third"}),"The retained lambda orphan types must preserve actual bucket order and canonical initializer/type links, with scope number21 treated as metadata rather than an entity or execution scope.");
        }
    }
}
