using RoslynObj;

internal static class ConstantReferences
{
    internal static void Verify(TranslationUnit unit,string path,bool source,Action<bool,string> check)
    => VerifySnapshot(unit,path,System.IO.Path.GetFileName(path),source,check);

    private static void VerifySnapshot(TranslationUnit unit,string path,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="constant_occurrences.roslynobj"&&source)
        {
            var aggregate=unit.Variables.Single(v=>v.Name=="constant_aggregate");
            var pointer=unit.Variables.Single(v=>v.Name=="constant_element_address").ConstantInitializer!;
            check(unit.SemanticModel.GetReferences(aggregate).Any(r=>r.Owner==pointer&&r.ReferencedSymbol==aggregate&&r.Role==SymbolReferenceRole.RetainedRelation),"A literal pointer initializer without an expression must expose its actual Constant occurrence as a reference owner.");
        }
        if(fixture=="member_pointers.roslynobj"&&source)
        {
            var data=unit.Variables.Single(v=>v.Name=="base_data").ConstantInitializer!;
            var target=(Symbol)((ConstantValue<MemberPointerValueDetails>)data.Value).Payload.Member!;
            check(unit.SemanticModel.GetReferences(target).Any(r=>r.Owner==data),"A retained member-pointer initializer must expose its actual member target and occurrence owner.");
            using var paged=RoslynObject.Open(path,new(){PageSize=1,CachedRowCount=32,CachedPageCount=2});
            var uses=paged.TranslationUnit.SemanticModel.GetReferences(target);
            check(uses.Count>=2&&uses.ToArray().Length==uses.Count&&uses.All(r=>r.ReferencedSymbol==target),"Literal-target reference UNIONs must enumerate all actual uses through multiple keyset pages.");
            var operations=unit.Entities.OfType<ConstantOperation>().Where(o=>o.Constant.Value is ConstantValue<MemberPointerValueDetails> {Payload.Member:Symbol}).ToArray();
            check(operations.Count(o=>o.RepresentationRole.Name=="source_semantic")==2&&operations.All(o=>unit.SemanticModel.GetSymbol(o)==((ConstantValue<MemberPointerValueDetails>)o.Constant.Value).Payload.Member),"The two executable member-pointer uses and every captured metadata operation must resolve their exact retained member symbols.");
        }
        if(fixture=="label_difference.roslynobj")
        {
            var operations=unit.Entities.OfType<ConstantOperation>().Where(o=>o.Constant.Value is AddressValue {BaseEntity:LabelSymbol}).ToArray();
            if(source)check(operations.Length==6&&operations.All(o=>unit.SemanticModel.GetSymbol(o)==((AddressValue)o.Constant.Value).BaseEntity),"Actual label-address constant operations must resolve their captured label symbols.");
            else
            {
                var constants=unit.Constants.Where(c=>c.Value is LabelDifferenceValue).ToArray();
                check(constants.Length==2&&constants.All(c=>c.Value is LabelDifferenceValue value&&unit.SemanticModel.GetReferences(value.FromLabel!).Any(r=>r.Owner==c||r.Owner is ConstantOperation o&&o.Constant==c)&&unit.SemanticModel.GetReferences(value.ToLabel!).Any(r=>r.Owner==c||r.Owner is ConstantOperation o&&o.Constant==c)),"Each actual label difference must expose both retained label targets through its occurrence or actual expression owner.");
                check(unit.Entities.OfType<ConstantOperation>().Where(o=>o.Constant.Value is LabelDifferenceValue).All(o=>unit.SemanticModel.GetSymbol(o) is null),"A two-label subtraction has no unambiguous single symbol.");
            }
        }
    }
}
