using RoslynObj;
using System.Text;

internal static class ControlAssemblyProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="control_assembly.roslynobj")
        {
            VerifyLoops(unit,source,check);
            VerifyAssembly(unit,check);
        }
        if(fixture=="control_microsoft.roslynobj")VerifyMicrosoft(unit,source,check);
    }

    private static void VerifyLoops(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="retained_loops");var executed=function.Body!.DescendantOperations().ToArray();
        var loops=executed.OfType<LoopOperation>().ToArray();
        check(loops.Select(l=>l.Kind.Name).SequenceEqual(new[]{"stmk_for","stmk_while","stmk_end_test_while"}),"The retained function must expose the actual for, while and end-test-while descriptors in structural traversal.");
        foreach(var loop in loops)
        {
            var body=loop.Body!;var relation=loop.ChildRelations.Single(r=>r.Role.Name=="body");
            check(loop.Loop.Body==body&&relation.Ordinal==0&&relation.Child==body&&body.StructuralParents.Contains(loop)&&body.RepresentationRole==loop.RepresentationRole&&executed.Count(o=>o==body)==1,"Each ordinary loop must retain its exact body descriptor through a single evaluated body edge at the loop's actual representation role.");
        }
        var forLoop=loops.Single(l=>l.Kind.Name=="stmk_for");
        if(source)
        {
            var scope=forLoop.InitScope!;var index=scope.Members.OfType<VariableSymbol>().Single(v=>v.Name=="index");
            check(scope==forLoop.Loop.InitScope&&scope.Kind.Name=="sck_block"&&index.SourceCorrespondence!.ParentScope==scope&&index.SourceCorrespondence.EnclosingRoutine==function,"The source for initializer must retain the exact block scope owning its index declaration and recorded enclosing routine.");
            check(forLoop.Initialization is not null&&forLoop.ChildRelations.Single(r=>r.Role.Name=="initialization").Child==forLoop.Initialization&&executed.Count(o=>o==forLoop.Initialization)==1,"The retained source for-initialization statement must remain an actual evaluated child, independent from its scope metadata.");
        }
        else check(forLoop.InitScope is null&&forLoop.Loop.InitScope is null,"Lowering clears the for-init scope pointer; the API must preserve absence without recovering it from an adjacent block.");
    }

    private static void VerifyAssembly(TranslationUnit unit,Action<bool,string> check)
    {
        var named=unit.Functions.Single(f=>f.Name=="named_routine");var supplement=named.GnuSupplement!;
        check(supplement==named.Details.GnuSupplement&&supplement.AssemblyName=="alternate_routine"&&supplement.Details.AssemblyName==supplement.AssemblyName,"The GNU routine supplement must retain its supplied assembly name independently from the routine's source/current name.");
        var call=unit.Functions.Single(f=>f.Name=="retained_loops").Body!.DescendantOperations().OfType<CallOperation>().Single();
        check(call.TargetMethod==named,"The executable call must retain the canonical routine carrying the assembly-name supplement.");
        var function=unit.Functions.Single(f=>f.Name=="retained_asm_function");var body=(AssemblyOperation)function.Body!.Root;
        check(body.Kind.Name=="stmk_asm_func_body"&&body.Text=="\n  nop\n"&&body.Assembly.Text==body.Text&&body.Definition is null&&body.AsmString is null,"An asm-function body must retain its opaque text and exact body kind without inventing a statement-asm descriptor.");
        check(body.RepresentationRole.Name!="metadata"&&function.Body.DescendantOperations().Single()==body&&unit.Functions.Single(f=>f.Name=="use_retained_asm_function").Body!.DescendantOperations().OfType<CallOperation>().Single().TargetMethod==function,"The asm-function root and its caller must retain their actual executable routine/body identities without interpreting instructions.");
        var entries=unit.GlobalScope.Members.OfType<AssemblyDefinition>().ToArray();var rows=unit.GlobalScope.Details.ScopeMemberRows.Where(r=>r.Member is AssemblyDefinition).ToArray();
        check(entries.Length==2&&rows.Select(r=>r.Member).SequenceEqual(entries)&&rows.All(r=>r.Owner==unit.GlobalScope)&&rows[1].Ordinal==rows[0].Ordinal+1,"The scope must retain its two canonical asm entries and their actual contiguous storage-list order.");
        var expected=new[]{"first_scope_asm","second_scope_asm"};
        for(var ordinal=0;ordinal<entries.Length;ordinal++)
        {
            var entry=entries[ordinal];var value=(StringValue)entry.AsmString.Value;
            check(value.TargetBytes.SequenceEqual(Encoding.UTF8.GetBytes(expected[ordinal]+"\0"))&&entry.Details.AsmString==entry.AsmString&&entry.SourceCorrespondence!.Entities.Contains(entry),"Each ordered scope asm entry must retain its exact canonical string constant, target bytes and source-correspondence owner association.");
        }
    }

    private static void VerifyMicrosoft(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="NativeFieldIterator");var field=record.Fields.Single(f=>f.Name=="current");
        var function=record.Methods.Single(f=>f.Name=="sum");var executed=function.Body!.DescendantOperations().ToArray();
        if(source)
        {
            var loop=executed.OfType<ForEachOperation>().Single();
            check(loop.UsesPreviousDeclarationIterator&&loop.PreviousField==field&&loop.ForEach.PreviousField==field&&loop.Iterator is null&&loop.PatternKind.Name=="sfepk_array_pattern","The previous-declaration foreach arm must retain the exact current FieldSymbol and absent variable iterator under its actual native array pattern.");
            check(loop.Body is not null&&loop.Assignment is not null&&loop.ChildRelations.Single(r=>r.Role.Name=="body").Child==loop.Body&&loop.ChildRelations.Single(r=>r.Child==loop.Assignment).Role.Name=="operand"&&executed.Count(o=>o==loop.Body)==1&&executed.Count(o=>o==loop.Assignment)==1,"The retained foreach body and assignment must remain exact evaluated children, distinct from the previous-field metadata pointer.");
        }
        else check(!unit.Entities.OfType<ForEachOperation>().Any()&&executed.OfType<LoopOperation>().Any(),"The native profile must expose the actual lowered loop without reconstructing a cleared foreach descriptor or previous-field association.");
        var target=unit.Types.OfType<RecordType>().Single(t=>t.Name=="AttributeBlock");
        var uuid=unit.Entities.OfType<MicrosoftAttribute>().Single(a=>a.Name=="uuid");var next=uuid.NextInBlock!;
        check(uuid.Kind.Name=="msak_uuid"&&next.Name=="no_injected_text"&&next.Kind.Name=="msak_no_injected_text"&&next.NextInBlock is null&&next==uuid.Details.NextInBlock&&uuid.Target==target&&next.Target==target,"The Microsoft bracket block must retain the physical uuid→no_injected_text chain and both exact application targets under recognized kinds.");
        var owner=unit.Entities.OfType<Scope>().Single(s=>s.MicrosoftAttributes.Contains(uuid));
        check(owner.MicrosoftAttributes.SequenceEqual(new[]{uuid,next})&&owner!=uuid.Target&&owner!=next.Target,"The actual scope owner list must preserve the same two canonical attributes in recorded order, independently from physical block links and application targets.");
    }
}
