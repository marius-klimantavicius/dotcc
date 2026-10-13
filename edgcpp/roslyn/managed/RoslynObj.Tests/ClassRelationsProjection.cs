using RoslynObj;

internal static class ClassRelationsProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture=="layout.roslynobj")
        {
            RecordType Record(string name)=>unit.Types.OfType<RecordType>().Single(t=>t.Name==name);
            var diamond=Record("LayoutDiamond");var left=diamond.BaseSubobjects.Single(b=>b.BaseType==Record("LayoutLeft"));var right=diamond.BaseSubobjects.Single(b=>b.BaseType==Record("LayoutRight"));var shared=diamond.BaseSubobjects.Single(b=>b.BaseType==Record("LayoutRoot"));
            check(diamond.Bases.Select(b=>b.BaseType).SequenceEqual(new[]{left.BaseType,right.BaseType})&&diamond.Bases.Select(b=>b.Subobject).SequenceEqual(new[]{left,right})&&diamond.Bases.Select(b=>b.Ordinal).SequenceEqual(new long[]{0,1}),"Direct bases must preserve declaration order Left/Right and their exact contextual subobjects independently from grouped base storage or postorder.");
            check(left.OriginalType==left.BaseType&&right.OriginalType==right.BaseType&&shared.OriginalType is null,"This written diamond must retain each direct base's original descriptor while the indirect shared base has no invented spelling descriptor.");
            var paths=shared.Derivations;
            check(paths.Count==2&&paths[0].Steps.Select(s=>s.Subobject).SequenceEqual(new[]{left,shared})&&paths[1].Steps.Select(s=>s.Subobject).SequenceEqual(new[]{right,shared})&&paths.All(p=>p.BaseSubobject==shared&&p.Details.PathTailOrdinal==1&&p.Steps.Select(s=>s.Ordinal).SequenceEqual(new long[]{0,1})),"The shared virtual base must retain the two actual ordered derivation paths and their terminal owner without expanding or merging path prefixes.");
            check(left.Derivations.Single().Steps.Single().Subobject==left&&right.Derivations.Single().Steps.Single().Subobject==right,"Each direct derivation must preserve its own one-step contextual identity.");
            if(source||profile=="cfront_lowered")
                check(shared.VirtualBasePointerBaseSubobject==left&&shared.VirtualBasePointerOffset==16&&shared.MostDerivedType==diamond,"The non-Itanium virtual-base pointer must retain its actual containing Left subobject and recorded displacement relative to the most-derived Diamond.");
            if(profile is "native_lowered" or "exported_templates_lowered")
            {
                check(diamond.PrimaryBaseSubobject==left&&diamond.VirtualFunctionInfoBaseSubobject==left,"The actual lowered primary and shared virtual-function-info base fields must independently identify this diamond's Left subobject.");
                var offsets=diamond.VirtualCallOffsets;
                check(offsets.Count==2&&offsets.Select(o=>o.Ordinal).SequenceEqual(new long[]{0,1})&&offsets[0] is {Routine.ContainingType.Name:"LayoutLeft",Routine.SpecialFunctionKind.Name:"sfk_destructor"}&&offsets[0].BaseSubobject==left&&offsets[1].Routine==unit.Functions.Single(f=>f.Name=="copy"&&f.ContainingType==diamond)&&offsets[1].BaseSubobject is null&&offsets.All(o=>o.Owner==diamond&&!o.IsPrimary),"Recorded vcall entries must preserve actual routine/base arms and ordinal order without an inferred virtual slot map.");
                check(diamond.VirtualFunctionTableVariable!.ConstantInitializer is not null&&diamond.VirtualTableTableVariable!.ConstantInitializer is not null&&diamond.VirtualFunctionTableVariable!=diamond.VirtualTableTableVariable,"Actual complete vtable and VTT variables must remain distinct canonical variables with their retained constant initializers.");
            }
            else check(diamond.PrimaryBaseSubobject is null&&diamond.VirtualFunctionInfoBaseSubobject==left,"An absent Itanium primary-base field must stay absent while the independent virtual-function-info base remains recorded.");
            if(profile=="cfront_lowered")
            {
                check(shared.VirtualFunctionTableVariable==unit.Variables.Single(v=>v.CurrentName=="__vtbl__10LayoutRoot__13LayoutDiamond")&&right.VirtualFunctionTableVariable==unit.Variables.Single(v=>v.CurrentName=="__vtbl__11LayoutRight__13LayoutDiamond")&&shared.VirtualFunctionTableVariable!=diamond.VirtualFunctionTableVariable&&right.VirtualFunctionTableVariable!.ConstantInitializer is not null,"Actual Cfront contextual Root/Right vtable variables must navigate their own retained identities independently from Diamond's complete table variable.");
            }
            var fields=Record("LayoutFields");var anonymous=unit.Types.OfType<RecordType>().Single(t=>t.Record.AnonymousUnionField is {ContainingType:var owner}&&owner==fields);
            check(anonymous.Record.AnonymousUnionKind?.Name=="auk_field"&&anonymous.Record.AnonymousUnionField is {Name:null} field&&Underlying(field.Type)==anonymous&&fields.Fields.Contains(field),"The active anonymous-union field arm must retain its actual unnamed field and exact union descriptor through any recorded qualifier wrapper.");
            check(Record("LayoutFriend").BefriendingClasses.SequenceEqual(new[]{Record("LayoutFriends")}),"The class's reverse befriending list must retain the actual LayoutFriends owner independently from the forward Friends list.");
            check(new[]{diamond,fields,Record("LayoutFriend")}.All(t=>t.Scope is not null&&t.Scope.AssociatedEntity==t),"Each actual class scope must preserve its exact descriptor association rather than creating a scope for an absent forward declaration.");
            var resolved=(RecordType)Underlying(unit.Variables.Single(v=>v.Name=="layout_resolved_width").Type);
            check(resolved.Instantiation?.OriginalDefinition==unit.TemplateDescriptors.Single(t=>t.Name=="LayoutDependentWidth")&&resolved.Instantiation!.Details.Template==resolved.Template,"The actual instantiated class must navigate its originating retained template descriptor without new lookup or selection.");
            if(source)
            {
                var deallocator=diamond.DeallocationRoutine!;
                check(deallocator==Record("LayoutRoot").DeallocationRoutine&&deallocator.Name=="operator delete"&&deallocator.ContainingType is null&&deallocator.ReturnType.Kind.Name=="tk_void"&&deallocator.Signature.Parameters[0].Type is PointerType pointer&&pointer.PointeeType.Kind.Name=="tk_void","The retained selected deallocation routine must preserve its canonical shared global identity and actual void-pointer signature.");
                check(diamond.SubobjectPartner is null&&fields.SubobjectPartner is null,"Unlowered source classes must preserve absent prelowering subobject partners.");
            }
            else check(fields.SubobjectPartner==fields&&diamond.SubobjectPartner is RecordType partner&&partner!=diamond&&partner.SubobjectPartner==diamond,"Lowered classes must preserve both self-partners and exact reciprocal complete/subobject descriptors without manufacturing extra types.");
        }
        if(fixture=="near_far.roslynobj")
        {
            var near=unit.Types.OfType<RecordType>().Single(t=>t.Name=="ConstantNearRecord");var far=unit.Types.OfType<RecordType>().Single(t=>t.Name=="ConstantFarRecord");
            check(unit.Capabilities.Any(c=>c.Name=="il_header.near_and_far_are_enabled"&&c.Enabled==true)&&near.Record.IsNear==true&&near.Record.IsFar==false&&far.Record.IsFar==true&&far.Record.IsNear==false,"The active near/far profile must preserve the two actual class qualifier bits independently from pointer size or target ABI inference.");
        }
        if(fixture=="variable_initializers.roslynobj")
        {
            var closures=unit.Types.OfType<RecordType>().Where(t=>t.LambdaParentEntity is VariableSymbol).ToArray();
            check(closures.Length>0&&closures.All(t=>t.Record.DefinedInVariableInitializer&&t.LambdaParentEntity is VariableSymbol variable&&variable.EntitiesDefinedInInitializer.Contains(t)),"Every actual variable-owned closure in this fixture must retain its active parent union and the exact reverse initializer-entity membership.");
        }
        if(fixture=="common_microsoft.roslynobj")
        {
            var prototype=unit.Types.OfType<RecordType>().Single(t=>t.Name=="CommonUnknownBase"&&t.Instantiation?.IsPrototype==true);
            var parameter=unit.TemplateDescriptors.Single(t=>t.Name=="CommonUnknownBase").Parameters.Single();
            check(prototype.Bases.Single().BaseType is RecordType {Record.IsProxyClass:true} proxy&&proxy.ProxyOfType==parameter.PlaceholderType&&proxy.DeclarationSymbol is null,"The actual dependent base proxy must retain the template parameter's exact placeholder descriptor without an invented class declaration symbol.");
        }
    }

    private static CppType Underlying(CppType type)
    {
        while(true)
        {
            if(type is TypeReference reference)type=reference.UnderlyingType;
            else if(type is QualifiedType qualified)type=qualified.Qualification.UnderlyingType;
            else return type;
        }
    }
}
