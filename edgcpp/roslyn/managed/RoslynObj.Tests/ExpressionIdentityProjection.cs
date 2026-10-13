using RoslynObj;

internal static class ExpressionIdentityProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture is "cleanup_semantics.roslynobj" or "retained.roslynobj")
        {
            VerifyLifetimes(unit,check);
            if(!source&&unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==true))VerifyEh(unit,fixture,check);
        }
        if(fixture=="retained.roslynobj"&&source)VerifyReuseAndCaptures(unit,check);
        if(fixture=="member_pointers.roslynobj"&&source)VerifyConstantNames(unit,check);
        if(fixture=="source_statements.roslynobj"&&source)VerifyMemberAndAssembly(unit,check);
        if(fixture=="execution_c.roslynobj"&&source)VerifyChoose(unit,check);
        if(fixture=="constant_occurrences.roslynobj"&&profile=="cfront_lowered")VerifyFoldedInitialization(unit,check);
        if(fixture=="throw_accessible.roslynobj")VerifyAccessibleThrows(unit,check);
        if(fixture=="scoped_assume.roslynobj")VerifyScopedAttribute(unit,check);
        if(fixture is "retained.roslynobj" or "constant_occurrences.roslynobj")
        {
            var instances=unit.Entities.OfType<TemplateInstantiation>().ToArray();
            foreach(var instance in instances)check(instance.Details.State.Family=="instantiation_state","Actual template-instance descriptors must expose their retained typed state discriminator.");
            Console.WriteLine($"scalar:template_instance_state,{instances.Length} records,State");
        }
    }

    private static void VerifyLifetimes(TranslationUnit unit,Action<bool,string> check)
    {
        var wrappers=unit.Entities.OfType<LifetimeOperation>().ToArray();
        check(wrappers.Length>0,"The existing fixture must retain actual full-expression lifetime wrappers.");
        foreach(var wrapper in wrappers)
        {
            var edge=wrapper.ChildRelations.Single();
            check(wrapper.Lifetime is {Kind.Name:"olk_expr_temporary"} lifetime&&lifetime.BoundEntity==wrapper&&unit.Entities.OfType<Lifetime>().Contains(lifetime),"A lifetime expression must navigate the canonical temporary lifetime whose actual bound entity is that wrapper.");
            check(edge.Role.Name=="body"&&edge.Ordinal==0&&edge.Child==wrapper.Value&&wrapper.Value.StructuralParents.Contains(wrapper)&&wrapper.Value.RepresentationRole==wrapper.RepresentationRole,"The retained full-expression value must be the exact body child with the wrapper's current representation role.");
        }
    }

    private static void VerifyEh(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        var operations=unit.Entities.OfType<LoweredEhOperation>().ToArray();
        var states=operations.OfType<CleanupStateOperation>().ToArray();
        check(states.Length>0&&states.Any(s=>s.Region is not null)&&states.Any(s=>s.Region is null),"The genuine table representation must retain both concrete cleanup states and its no-region sentinel.");
        foreach(var state in states)
        {
            var eh=state.ExceptionOperation.FunctionEh!;
            check(state.StateNumber is not null&&state.StateNumber==state.ExceptionOperation.StateNumber&&state.Region==state.ExceptionOperation.Region&&state.IsUnreachable==(state.ExceptionKind.Name=="leck_unreachable_cleanup_state"),"Numeric cleanup-state facades must preserve the exact construct's state, region and unreachable discriminator.");
            check(state.Region is {} region?state.StateNumber==region.Ordinal&&region.Details.FunctionEh.Routine==eh.Routine&&eh.Routine.Body!.ExceptionHandling.CleanupRegions.Contains(region):state.StateNumber==eh.NullState,"Concrete states must resolve their own routine's decoded region; the recorded null-state sentinel must retain no region.");
        }
        var epilogues=operations.Where(o=>o.ExceptionKind.Name=="leck_function_epilogue").ToArray();
        check(epilogues.Length>0&&epilogues.All(o=>o.ExceptionOperation.Routine==o.ExceptionOperation.FunctionEh!.Routine&&o.ExceptionOperation.Routine!.Body!.ExceptionHandling.Epilogues.Contains(o)),"Each function epilogue must retain the actual routine pointer and that routine's exact EH epilogue association.");
        var actions=unit.Entities.OfType<CleanupAction>().ToArray();
        foreach(var action in actions)check(action.Details.Kind.Family=="cleanup_kind","Actual cleanup actions must expose their retained typed action discriminator.");
        Console.WriteLine($"scalar:cleanup_action_kind,{actions.Length} records,Kind");
        if(fixture=="cleanup_semantics.roslynobj")
        {
            var handler=unit.Entities.OfType<ExceptionHandler>().Single(h=>h.Parameter?.Name=="caught");
            var caught=operations.Single(o=>o.ExceptionKind.Name=="leck_caught_object_address");
            var ending=operations.Single(o=>o.ExceptionKind.Name=="leck_catch_epilogue");
            check(caught.ExceptionOperation.Handler==handler&&ending.ExceptionOperation.Handler==handler&&caught.ExceptionOperation.FunctionEh!.Routine.Name=="cleanup_paths"&&ending.ExceptionOperation.FunctionEh!.Routine==caught.ExceptionOperation.FunctionEh.Routine,"Caught-object and catch-epilogue union arms must point to the same canonical caught parameter's handler in cleanup_paths.");
            foreach(var endingTry in operations.Where(o=>o.ExceptionKind.Name=="leck_try_epilogue"))
                check(endingTry.ExceptionOperation.TryBlock is {} descriptor&&descriptor.Handlers.Contains(handler)&&descriptor.Body is BlockOperation&&endingTry.ExceptionOperation.FunctionEh!.Routine.Name=="cleanup_paths","A try epilogue must retain its canonical try descriptor and actual handler/body links independently from a statement ID.");
        }
        if(fixture=="retained.roslynobj")
        {
            var internalTries=operations.Where(o=>o.ExceptionKind.Name=="leck_internal_try").ToArray();
            check(internalTries.Length>0,"The lowered coroutine fixture must retain actual internal try/catch pairs.");
            foreach(var operation in internalTries)
            {
                var payload=operation.ExceptionOperation;
                check(payload.TryExpression is not null&&payload.CatchExpression is not null&&payload.TryExpression!=payload.CatchExpression&&operation.ChildRelations.Single(r=>r.Role.Name=="then"&&r.Ordinal==0).Child==payload.TryExpression&&operation.ChildRelations.Single(r=>r.Role.Name=="else"&&r.Ordinal==0).Child==payload.CatchExpression,"Internal EH pairs must preserve their actual first/second expression identities through distinct then/else edges; those edges do not imply unconditional catch execution.");
            }
        }
    }

    private static void VerifyReuseAndCaptures(TranslationUnit unit,Action<bool,string> check)
    {
        var reused=unit.Entities.OfType<ReuseOperation>().ToArray();
        check(reused.Length>0&&reused.GroupBy(r=>r.Initialization).Any(g=>g.Count()>1),"Retained coroutine value reuse must share the actual earlier initializer identity.");
        foreach(var operation in reused)
            check(operation.Initialization.Details.IsReusedValue&&operation.Initialization.Kind.Name=="dik_expression"&&operation.Initialization.Expression is not null&&operation.Children.All(c=>c!=operation.Initialization.Operation),"Reuse must retain its earlier expression recipe without adding an evaluated edge that repeats the initialization.");
        check(reused.Any(r=>r.Initialization.Operation is {RepresentationRole.Name:"source_semantic"} active&&active.StructuralParents.Count>0),"A reused recipe may independently be evaluated at its real initializer site; the reuse association must not demote that role.");
        var function=unit.Functions.Single(f=>f.Name=="lambda_uses");
        var parameter=function.SourceParameters.Single(p=>p.Name=="value").Symbol!;
        var first=unit.Variables.Single(v=>v.Name=="first"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var second=unit.Variables.Single(v=>v.Name=="second"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var third=unit.Variables.Single(v=>v.Name=="third"&&v.SourceCorrespondence?.EnclosingRoutine==function);
        var firstLambda=first.Initializer!.Lambda!;var secondLambda=second.Initializer!.Lambda!;var thirdLambda=third.Initializer!.Lambda!;
        var value=firstLambda.Captures.Single(c=>c.Field?.Name=="value");var reference=secondLambda.Captures.Single();var implicitValue=thirdLambda.Captures.Single();
        check(value.Variable==parameter&&value.Owner==firstLambda&&value.Field==firstLambda.ClosureType.Fields.Single(f=>f.Name=="value")&&value.Field!.ContainingType==firstLambda.ClosureType,"A copy capture must retain the exact source parameter and current closure field, not merge them by name.");
        check(reference.Variable==first&&reference.Owner==secondLambda&&reference.Field==secondLambda.ClosureType.Fields.Single(f=>f.Name=="first")&&reference.CaptureByReference&&implicitValue.Variable==parameter&&implicitValue.Owner==thirdLambda&&implicitValue.Field==thirdLambda.ClosureType.Fields.Single(f=>f.Name=="value"),"Reference and implicit copy captures must retain their actual distinct source-variable/current-field associations.");
    }

    private static void VerifyConstantNames(TranslationUnit unit,Action<bool,string> check)
    {
        var operations=unit.Entities.OfType<ConstantOperation>().Where(o=>o.NameReference is not null).ToArray();
        check(operations.Length==4&&operations.Select(o=>o.NameReference!).Distinct().Count()==4,"Member-pointer backing constants must retain four distinct occurrence name descriptors.");
        foreach(var operation in operations)
        {
            var member=((ConstantValue<MemberPointerValueDetails>)operation.Constant.Value).Payload.Member!;
            check(unit.Entities.OfType<NameReference>().Contains(operation.NameReference!)&&unit.SemanticModel.GetSymbol(operation)==member,"The actual constant occurrence name and exact member target must remain independently navigable.");
        }
        check(operations.Count(o=>o.NameReference!.Qualifier?.Target is CppType {Name:"PointerBase"})==2&&operations.Count(o=>o.NameReference!.Qualifier is null)==2,"Written PointerBase qualifiers must remain on their own source occurrence; converted backing occurrences retain their actual absent qualifier.");
    }

    private static void VerifyMemberAndAssembly(TranslationUnit unit,Action<bool,string> check)
    {
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="SourceValue");
        var field=record.Fields.Single(f=>f.Name=="value");
        var read=record.Methods.Single(f=>f.Name=="read");
        var member=read.Body!.DescendantOperations().OfType<OperatorOperation>().Single(o=>o.OperatorKind.Name=="eok_points_to_field");
        check(member.Details.OriginalLvalueType is QualifiedType {IsConst:true} original&&original.UnderlyingType==member.Type&&member.Type==field.Type&&member.ValueCategory.Name=="prvalue","The member fetch must retain its original const glvalue type separately from its current unqualified prvalue descriptor.");
        check(member.Operands.Count==2&&member.Operands[1] is ReferenceOperation {Target:var target}&&target==field&&member.OperandRelations.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1})&&member.OperandRelations.All(r=>r.Owner==member)&&member.OperandRelations.Select(r=>r.Expression).SequenceEqual(member.Operands),"Ordinary operator operand navigation must retain exact descriptor order and canonical field identity, independently from evaluation-direction flags.");
        var assembly=unit.Functions.Single(f=>f.Name=="inline_assembly").Body!.DescendantOperations().OfType<AssemblyOperation>().Single(a=>a.IsGoto==true);
        var label=assembly.Labels.Single();
        check(label.Owner==assembly.Definition&&label.Ordinal==0&&label.Label.Name=="reached"&&label.Label.Target is LabelOperation statement&&statement.Label==label.Label&&unit.Entities.OfType<LabelSymbol>().Contains(label.Label),"An asm-goto label must retain the exact canonical defining label target and its actual descriptor order.");
        check(assembly.Definition!.SourceCorrespondence is {} correspondence&&correspondence.Entities.Contains(assembly.Definition)&&correspondence.ParentScope is null&&correspondence.EnclosingRoutine is null,"The asm descriptor must navigate its own canonical common correspondence without substituting executable body membership for its actually absent parent/routine pointers.");
    }

    private static void VerifyChoose(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="execution_choose");
        var operation=function.Body!.DescendantOperations().OfType<BuiltinOperation>().Single(o=>o.Kind.Name=="enk_builtin_choose_expr");
        var operands=operation.Operands;
        check(operands.Count==3&&operands[0] is ConstantOperation {Constant.Value:IntegerValue {Value:var condition}}&&condition==1&&operation.SelectedExpression==operands[1]&&operands[2] is CallOperation {TargetMethod.Name:"execution_dimension"},"The genuine choose expression must retain condition, selected value+2 and unselected call in exact operand order, without recomputing source selection.");
        check(operation.OperandRelations.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1,2})&&operation.OperandRelations.Select(r=>r.Expression).SequenceEqual(operands)&&operation.ChildRelations.Single() is {Role.Name:"result",Child:var selected}&&selected==operands[1]&&operands[0].RepresentationRole.Name=="metadata"&&operands[2].RepresentationRole.Name=="metadata","All choose operands remain inspectable while only the actual recorded selected result has an evaluated structural edge.");
    }

    private static void VerifyFoldedInitialization(TranslationUnit unit,Action<bool,string> check)
    {
        var operation=unit.Entities.OfType<DependentOperation>().Single(o=>o.Kind.Name=="enk_initializer");
        var initialization=operation.Initialization!;
        check(initialization.Kind.Name=="dik_constructor"&&initialization.Constructor?.ContainingType?.Name=="ConstantPolymorphic"&&unit.Entities.OfType<Initialization>().Contains(initialization)&&operation.References.Single(r=>r.Role.Name=="metadata").Target==initialization&&operation.Children.Count==0,"A folded class initializer must retain its exact constructor recipe as metadata without adding another evaluated initialization site.");
    }

    private static void VerifyAccessibleThrows(TranslationUnit unit,Action<bool,string> check)
    {
        check(unit.Capabilities.Any(c=>c.Name is "cfront_2_1_mode" or "cfront_3_0_mode"&&c.Enabled==true),"The genuine throw fixture must preserve its actual USL runtime mode.");
        var type=unit.Types.OfType<RecordType>().Single(t=>t.Name=="ThrowObject");
        var first=type.BaseSubobjects.Single(b=>b.BaseType.Name=="ThrowFirst");var hidden=type.BaseSubobjects.Single(b=>b.BaseType.Name=="ThrowHidden");var second=type.BaseSubobjects.Single(b=>b.BaseType.Name=="ThrowSecond");
        check(new[]{first,hidden,second}.Select(b=>b.DirectBaseNumber).SequenceEqual(new long[]{1,2,3}),"The throw fixture's actual contextual direct bases must preserve declaration order separately from throw accessibility.");
        foreach(var name in new[]{"throw_public_object","throw_public_pointer","throw_member_object","throw_member_pointer"})
        {
            var function=unit.Functions.Single(f=>f.Name==name);
            var operation=function.Body!.DescendantOperations().OfType<ThrowOperation>().Single();
            var expected=name.StartsWith("throw_member_",StringComparison.Ordinal)?new[]{first,hidden,second}:new[]{first,second};
            check(operation.AccessibleBases.SequenceEqual(expected)&&operation.AccessibleBases.All(b=>type.BaseSubobjects.Contains(b)),"Each object/pointer throw must navigate its exact recorded ordered contextual bases, including the private base only where stock retained it; the reader does not recompute access.");
        }
    }

    private static void VerifyScopedAttribute(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="scoped_assume");
        var attribute=unit.Entities.OfType<RoslynObj.Attribute>().Single(a=>a.Name=="assume");
        var argument=attribute.Arguments.Single();var wrapper=argument.ScopedExpression!;
        check(argument.Kind.Name=="aak_expression"&&argument.IsLocalExpressionReference&&argument.Expression==wrapper.Expression&&argument.OwningAttributes.Single()==attribute&&unit.Entities.OfType<ScopedExpression>().Contains(wrapper),"The function-local assume argument must preserve its actual scoped-expression wrapper and canonical expression identity.");
        var reference=unit.Entities.OfType<Scope>().SelectMany(s=>s.LocalExpressionReferences).Single(r=>r.Referrer==wrapper);
        check(reference.Kind.Name=="lerk_scoped_expr"&&reference.Expression==argument.Expression&&reference.Owner==function.Body!.Scope&&reference.Owner.AssociatedEntity==function&&wrapper.SourceCorrespondence?.EnclosingRoutine==function&&wrapper.SourceCorrespondence.ParentScope is null,"The wrapper's exact scope-local reference and common enclosing-routine link must resolve the condition independently from its legitimately absent direct parent scope.");
        var subtree=CapturedSubtree(wrapper.Expression).ToArray();
        var executed=function.Body!.DescendantOperations().ToArray();
        check(subtree.Length>1&&subtree.All(n=>n.RepresentationRole.Name=="metadata")&&wrapper.Expression.StructuralParents.Count==0&&!subtree.Any(n=>executed.Contains(n)),"Every retained assume-condition node must remain metadata and outside evaluated function traversal.");
        check(subtree.OfType<ReferenceOperation>().Any(r=>r.Target==function.SourceParameters.Single(p=>p.Name=="x").Symbol)&&subtree.OfType<ConstantOperation>().Any(c=>c.Constant.Value is IntegerValue {Value:var value}&&value==0)&&executed.OfType<OperatorOperation>().Count(o=>o.OperatorKind.Name=="eok_add")==1,"The retained condition must navigate the actual x and zero operands while the ordinary return x+1 executes once.");
    }

    private static IEnumerable<SemanticNode> CapturedSubtree(SemanticNode root)
    {
        var visited=new HashSet<SemanticNode>();var pending=new Stack<SemanticNode>();pending.Push(root);
        while(pending.TryPop(out var node))
        {
            if(!visited.Add(node))continue;
            yield return node;
            foreach(var child in node.Children)pending.Push(child);
        }
    }
}
