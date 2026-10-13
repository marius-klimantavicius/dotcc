using RoslynObj;
using System.Numerics;

internal static class ListControlIdentityProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture=="upc_affinity.roslynobj"&&source)VerifyLoop(unit,check);
        if(fixture=="source_statements.roslynobj"&&source)VerifyAssemblyAndLabels(unit,check);
        if(fixture=="source_vla.roslynobj"&&source)VerifyDimensions(unit,check);
        if(fixture=="cleanup_semantics.roslynobj"&&profile=="native_lowered"&&unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==true))VerifyHandler(unit,check);
        if(fixture=="native_microsoft.roslynobj"&&source)VerifyMicrosoftTry(unit,check);
        if(fixture=="native_attributes.roslynobj")VerifyMicrosoftArguments(unit,check);
        if(fixture=="layout.roslynobj"&&unit.Capabilities.Any(c=>c.Name=="IA64_ABI"&&c.Enabled==true))VerifyVcalls(unit,check);
        if(fixture is "module_partition.roslynobj" or "module_header.roslynobj" or "module_header_consumer.roslynobj")VerifyModules(unit,fixture,check);
    }

    private static void VerifyLoop(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="upc_affinity_loop");
        var executed=function.Body!.DescendantOperations().ToArray();
        var loop=executed.OfType<LoopOperation>().Single();
        var index=unit.Variables.Single(v=>v.Name=="index"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var initialization=(BlockOperation)loop.Initialization!;
        var declaration=initialization.Statements.OfType<DeclarationOperation>().Single();
        check(declaration.Entities.Single()==index&&initialization.Statements.OfType<InitializationStatementOperation>().Single().Initialization.Target==index&&loop.ChildRelations.Single(r=>r.Role.Name=="initialization"&&r.Ordinal==0).Child==initialization&&executed.Count(n=>n==initialization)==1,"The retained loop initializer must preserve its actual index declaration, initialization block and single evaluated header edge.");
        check(loop.Increment is OperatorOperation {OperatorKind.Name:"eok_pre_incr"} increment&&increment.Operands.Single() is ReferenceOperation {Target:var target}&&target==index&&loop.ChildRelations.Single(r=>r.Role.Name=="increment"&&r.Ordinal==0).Child==increment&&executed.Count(n=>n==increment)==1,"The recorded loop increment must retain the exact index operand and one evaluated increment edge.");
        check(loop.InitScope is null,"This stock loop records no dedicated for-init scope; the reader must preserve absence independently from its retained initialization statement.");
    }

    private static void VerifyAssemblyAndLabels(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="inline_assembly");
        var assembly=function.Body!.DescendantOperations().OfType<AssemblyOperation>().Single(a=>a.IsGoto==false);
        var output=unit.Variables.Single(v=>v.Name=="output"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var input=function.SourceParameters.Single(p=>p.Name=="value").Symbol!;
        check(assembly.Operands.Select(o=>o.Kind.Name).SequenceEqual(new[]{"output","input"})&&assembly.Operands.Select(o=>o.Ordinal).SequenceEqual(new long[]{0,1})&&assembly.Operands.All(o=>o.Owner==assembly.Definition),"The actual asm operand chain must preserve output/input descriptor order and canonical owner.");
        check(assembly.Operands[0].Expression is ReferenceOperation {Target:var first}&&first==output&&assembly.Operands[1].Expression is ReferenceOperation {Target:var second}&&second==input,"Ordered asm descriptors must retain the exact output local and input parameter expressions.");
        foreach(var entry in function.Body.DescendantOperations().OfType<AssemblyOperation>())
        {
            var clobber=entry.Clobbers.Single();
            check(clobber.Owner==entry.Definition&&clobber.Ordinal==0&&clobber.RegisterKind?.Name=="anr_memory","Each actual singleton clobber list must retain owner, ordinal zero and the exact memory-register discriminator.");
        }
        var labels=unit.Functions.Single(f=>f.Name=="indirect_jump").Body!.Scope!.Members.OfType<LabelSymbol>().ToArray();
        check(labels.Select(l=>l.Name).SequenceEqual(new[]{"first","second"})&&labels.All(l=>l.Target is LabelOperation statement&&statement.Label==l),"The actual scope label list must retain first/second order and exact defining targets; it is not a CFG execution schedule.");
    }

    private static void VerifyDimensions(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="source_vla");
        var scope=function.Body!.Scope!;
        var rows=unit.Variables.Single(v=>v.Name=="rows"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var outer=(ArrayType)rows.Type;
        var alias=scope.Members.OfType<TypeReference>().Single(t=>t.Name=="Row");
        var inner=(ArrayType)alias.UnderlyingType;
        var dimensions=scope.Members.OfType<VlaDimension>().ToArray();
        check(dimensions.SequenceEqual(new[]{inner.VlaDimension!,outer.VlaDimension!})&&dimensions[0]!=dimensions[1]&&dimensions[0].Type==inner&&dimensions[1].Type==outer,"The actual memory-scope dimension list must retain typedef Row then rows identities in recorded list order, without inferring an execution order.");
        var entries=scope.Details.ScopeMemberRows.Where(r=>r.Member is VlaDimension).ToArray();
        check(entries.Select(r=>r.Ordinal).SequenceEqual(new long[]{3,4})&&entries.All(r=>r.Owner==scope)&&entries.Select(r=>r.Member).SequenceEqual(dimensions),"Dimension list context must preserve its exact recorded owner and grouped-list ordinals.");
    }

    private static void VerifyHandler(TranslationUnit unit,Action<bool,string> check)
    {
        var handler=unit.Entities.OfType<ExceptionHandler>().Single(h=>h.Parameter?.Name=="caught");
        var descriptor=unit.Entities.OfType<TryDescriptor>().Single(t=>t.Handlers.Contains(handler));
        var entry=descriptor.Details.TryHandlerRows.Single();
        check(descriptor.Handlers.Single()==handler&&entry.Owner==descriptor&&entry.Ordinal==0&&entry.Handler==handler,"The actual singleton catch list must retain its canonical try descriptor, handler and terminating ordinal-zero entry.");
        var typeinfo=handler.Details.Typeinfo!;
        check(typeinfo==handler.CaughtType!.Details.TypeinfoVariable&&unit.Variables.Contains(typeinfo)&&typeinfo.ConstantInitializer is not null&&typeinfo.Details.LoweringGenerated,"The catch typeinfo pointer must retain the exact underlying caught type's generated storage variable and initializer.");
    }

    private static void VerifyMicrosoftTry(TranslationUnit unit,Action<bool,string> check)
    {
        var operations=unit.Functions.Single(f=>f.Name=="native_seh").Body!.DescendantOperations().OfType<MicrosoftTryOperation>().ToArray();
        var final=operations.Single(o=>o.IsFinally);var except=operations.Single(o=>!o.IsFinally);
        check(final.Filter is null&&final.Handler is null&&final.Finally is BlockOperation&&except.Finally is null&&except.Handler is BlockOperation&&except.Filter is ConstantOperation {Constant.Value:IntegerValue {Value:var filter}}&&filter==1,"Microsoft try-finally and try-except must preserve distinct guarded cleanup arms and the actual constant filter.");
        foreach(var operation in operations)
        {
            var cleanup=operation.IsFinally?operation.Finally!:operation.Handler!;
            check(operation.ChildRelations.Single(r=>r.Role.Name=="body"&&r.Ordinal==0).Child==operation.Body&&operation.ChildRelations.Single(r=>r.Role.Name=="else"&&r.Ordinal==0).Child==cleanup&&operation.Body.RepresentationRole==operation.RepresentationRole&&cleanup.RepresentationRole==operation.RepresentationRole,"SEH bodies and cleanup statements must retain their exact structural associations and current roles, not an unconditional execution-order interpretation.");
            if(operation.Filter is {} condition)check(operation.ChildRelations.Single(r=>r.Role.Name=="condition"&&r.Ordinal==0).Child==condition,"A try-except filter must retain its actual condition edge independently from guarded body and handler edges.");
        }
        check(Constants(final.Body).Contains(2)&&Constants(final.Finally!).Contains(3)&&Constants(except.Body).Contains(4)&&Constants(except.Handler!).Contains(5),"Each native SEH body/cleanup must retain the actual fixture's distinct update constant.");
    }

    private static IEnumerable<BigInteger> Constants(SemanticNode node)
        => node.DescendantOperations().OfType<ConstantOperation>().Select(o=>o.Constant.Value).OfType<IntegerValue>().Select(v=>v.Value);

    private static void VerifyMicrosoftArguments(TranslationUnit unit,Action<bool,string> check)
    {
        var module=unit.Entities.OfType<MicrosoftAttribute>().Single(a=>a.Name=="module");
        var expected=new[]{"type","name","uuid","version","lcid","control"};
        var links=module.Details.MsAttributeArgumentLinkRows;
        check(module.Arguments.Select(a=>a.ParameterName).SequenceEqual(expected)&&links.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1,2,3,4,5})&&links.All(r=>r.Owner==module)&&links.Select(r=>r.Argument).SequenceEqual(module.Arguments),"The actual native module attribute must preserve six ordered named arguments and their canonical link owner/argument identities.");
    }

    private static void VerifyVcalls(TranslationUnit unit,Action<bool,string> check)
    {
        var root=unit.Types.OfType<RecordType>().Single(t=>t.Name=="LayoutRoot");
        var copy=root.Methods.Single(f=>f.Name=="copy");
        var destructor=root.Methods.Single(f=>f.SpecialFunctionKind?.Name=="sfk_destructor"&&f.PrimaryEntry is null);
        var entries=root.VirtualCallOffsets;
        check(entries.Count==2&&entries.Select(e=>e.Ordinal).SequenceEqual(new long[]{0,1})&&entries.All(e=>e.Owner==root)&&entries.Select(e=>e.Routine).SequenceEqual(new[]{copy,destructor})&&entries.Select(e=>e.VirtualTableIndex).SequenceEqual(new BigInteger[]{-3,-4}),"The retained virtual-call-offset chain must preserve copy/destructor order, exact owner/routine identities and recorded signed indices without constructing a virtual-slot map.");
    }

    private static void VerifyModules(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture!="module_header_consumer.roslynobj")
        {
            check(unit.Modules.Count==0&&unit.ModuleImports.Count==0,"These genuine IFC producer snapshots retain no imported-module roots; the reader must not synthesize a partition or parent-unit relationship.");
            return;
        }
        var import=unit.ModuleImports.Single();var module=unit.Modules.Single();
        check(import.Module==module&&module.Kind.Name=="mk_header_unit"&&module.FileKind.Name=="mfk_edg_ifc"&&module.Name=="module_header.hpp"&&!module.UsesAngleBrackets&&module.Unit is null,"The actual quoted header import must retain its canonical imported module, header-unit/file-format kinds and absent unit association.");
        check(module.ResolvedHeader is {} header&&Path.GetFileName(header)=="module_header.hpp"&&Path.IsPathFullyQualified(header)&&module.ResolvedFile is {} file&&Path.GetFileName(file)=="Header.eifc"&&Path.IsPathFullyQualified(file)&&header!=file,"Resolved source header and consumed IFC file must preserve their distinct exact path roles.");
        var attribute=import.Attributes.Single();
        check(attribute.Name=="marker"&&attribute.NamespaceName=="roslyn_probe"&&attribute.Kind.Name=="ak_unrecognized"&&attribute.Owners.Contains(import)&&!module.Attributes.Contains(attribute),"The retained import marker must stay attached to the exact module-import occurrence, independently from the imported module descriptor.");
    }
}
