using System.Globalization;
namespace DotObj;
internal sealed partial class ObjectFile
{
    private string SourceExpressionTypeKey(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Source expression type identity cycle/depth");
        var value=SourceTypes[id];
        if(value.Kind=="dependent" && SourceDependentTypes[id].Kind=="unknown")return "dependent-unknown";
        if(value.Kind is "typedef" or "qualified")
        {
            if(value.SourceForm!="plain")return SourceArgumentTypeIdentity(id,depth+1);
            string inner=SourceExpressionTypeKey(value.UnderlyingTypeId!.Value,depth+1);
            string qualifiers=(value.IsConst?"c":"")+(value.IsVolatile?"v":"")+(value.IsRestrict?"r":"")+(value.IsAtomic?"a":"")+
                string.Concat(SourceTypeQualifiers.GetValueOrDefault(id,[]).Select(SourceIdentityFrame));
            return qualifiers.Length==0?inner:"q"+SourceIdentityFrame(qualifiers)+SourceIdentityFrame(inner);
        }
        return SourceArgumentTypeIdentity(id,depth+1);
    }
    private string SourceStructuralArgumentKey(SourceTemplateArgument value,bool pack,int depth)=>(value.Kind=="pack-boundary"?"pack-boundary":value.TypeId is long type?"t"+SourceArgumentTypeIdentity(type,depth+1):
        value.ConstantId is long constant?"c"+SourceConstantIdentity(constant,depth+1):"m"+SourceTemplateIdentity(value.TemplateId!.Value,depth+1))+(pack?":pack":"");
    private string SourceStructuralFormalKey(SourceTemplateFormal value,int depth)=>SourceIdentityFrame(value.Kind)+(value.IsPack?"1":"0")+(value.IsAbbreviated?"1":"0")+
        SourceIdentityFrame(value.TypeId is long type?"t"+SourceArgumentTypeIdentity(type,depth+1):value.ConstantId is long constant?"c"+SourceConstantIdentity(constant,depth+1):"m"+SourceTemplateIdentity(value.TemplateId!.Value,depth+1))+
        SourceIdentityFrame(value.DefaultTypeId is long dt?"t"+SourceArgumentTypeIdentity(dt,depth+1):value.DefaultConstantId is long dc?"c"+SourceConstantIdentity(dc,depth+1):value.DefaultTemplateId is long dm?"m"+SourceTemplateIdentity(dm,depth+1):"");
    private string SourceStructuralDeclarationKey(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Source declaration identity cycle/depth");
        var value=SourceExpressionDeclarations[id];
        return SourceIdentityFrame(value.Kind)+SourceIdentityFrame(value.QualifiedName)+SourceIdentityFrame(SourceExpressionTypeKey(value.Type,depth+1))+
            SourceIdentityFrame(value.OwnerType is long owner?SourceArgumentTypeIdentity(owner,depth+1):"")+SourceIdentityFrame(value.TemplateOriginKind)+SourceIdentityFrame(value.RoutineKind)+SourceIdentityFrame(SourceDeclarationOperators.TryGetValue(id,out long originalOperator)?originalOperator.ToString(CultureInfo.InvariantCulture):"")+(value.Constexpr?"1":"0")+
            (value.HasTemplate?"1":"0")+(value.IsPrototype?"1":"0")+"F"+SourceIdentityFrame(SourceExpressionDeclarationFormals.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture))+
            string.Concat(SourceExpressionDeclarationFormals.GetValueOrDefault(id,[]).Select(f=>SourceIdentityFrame(SourceStructuralFormalKey(f,depth+1))))+"A"+SourceIdentityFrame(SourceExpressionDeclarationArguments.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture))+
            string.Concat(SourceExpressionDeclarationArguments.GetValueOrDefault(id,[]).Select(a=>SourceIdentityFrame(SourceStructuralArgumentKey(a,SourceConstraintArgumentDetails[("declaration",id,0,a.Ordinal)].Pack,depth+1))));
    }
    private string SourceStructuralExpressionKey(long id,int depth)
    {
        if(depth>128)throw new InvalidDataException("Source expression structural identity cycle/depth");
        var value=SourceExpressions[id];string result=SourceIdentityFrame(value.Kind)+SourceIdentityFrame(value.SourceTypeId is long type?SourceExpressionTypeKey(type,depth+1):"")+
            (value.Category=="lvalue"?"l":value.Category=="xvalue"?"x":"p")+SourceIdentityFrame(value.OriginalLvalueTypeId is long original?SourceExpressionTypeKey(original,depth+1):"")+
            (value.Parenthesized?"1":"0")+(value.CompilerGenerated?"1":"0")+(value.VolatileFetch?"1":"0")+(value.PackExpansion?"1":"0");
        if(SourceExpressionOperations.TryGetValue(id,out var operation))result+="O"+SourceIdentityFrame(operation.Opcode.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(operation.Name)+SourceIdentityFrame(operation.OriginalTypeKind?.ToString(CultureInfo.InvariantCulture)??"");
        result+="F"+SourceIdentityFrame(SourceExpressionFlags.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture))+string.Concat(SourceExpressionFlags.GetValueOrDefault(id,[]).Order(StringComparer.Ordinal).Select(SourceIdentityFrame));
        if(SourceExpressionTypeOperands.TryGetValue(id,out long operand))result+="T"+SourceIdentityFrame(SourceArgumentTypeIdentity(operand,depth+1));
        if(SourceExpressionConstants.TryGetValue(id,out long constant))result+="C"+SourceIdentityFrame(SourceConstantIdentity(constant,depth+1));
        if(SourceExpressionParameterReferences.TryGetValue(id,out var reference))result+="R"+SourceIdentityFrame((reference.Ordinal+1).ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(reference.Levels.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(SourceParameterReferenceContextKey(id));
        if(SourceExpressionRequirements.TryGetValue(id,out bool noex))result+=noex?"N1":"N0";
        if(SourceExpressionQueries.TryGetValue(id,out bool standard))result+=standard?"Q1":"Q0";
        result+="P"+SourceIdentityFrame(SourceExpressionParameters.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture));
        foreach(var parameter in SourceExpressionParameters.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceExpressionTypeKey(parameter.Type,depth+1))+
            SourceIdentityFrame(parameter.DeclaredType is long declared?SourceExpressionTypeKey(declared,depth+1):"")+SourceIdentityFrame(parameter.Qualifiers.ToString(CultureInfo.InvariantCulture))+(parameter.Pack?"1":"0");
        if(SourceExpressionDeclarationReferences.TryGetValue(id,out long declaration))result+="D"+SourceIdentityFrame(SourceStructuralDeclarationKey(declaration,depth+1));
        string argumentKind="concept";
        if(SourceDependentFunctionLookups.TryGetValue(id,out var lookup))
        {
            argumentKind="lookup";
            result+="L"+SourceIdentityFrame(lookup.Name)+SourceIdentityFrame(lookup.QualifiedName)+SourceIdentityFrame(lookup.Owner is long owner?SourceArgumentTypeIdentity(owner,depth+1):"")+
                SourceIdentityFrame(lookup.Conversion is long conversion?SourceArgumentTypeIdentity(conversion,depth+1):"")+SourceIdentityFrame(lookup.Operator.ToString(CultureInfo.InvariantCulture))+
                (lookup.ExplicitArguments?"1":"0")+(lookup.Qualified?"1":"0")+(lookup.Address?"1":"0")+(lookup.Symbol?"1":"0")+(lookup.UnknownBase?"1":"0")+(lookup.QualifiedUnknownBase?"1":"0");
            result+="C"+SourceIdentityFrame(SourceDependentFunctionCandidates.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture));
            foreach(long candidate in SourceDependentFunctionCandidates.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceStructuralDeclarationKey(candidate,depth+1));
        }
        if(SourceConceptUses.TryGetValue(id,out var conceptUse))
        {
            var concept=SourceConcepts[conceptUse.Concept];result+="K"+SourceIdentityFrame(concept.QualifiedName)+(conceptUse.TypeConstraint?"1":"0")+"F"+SourceIdentityFrame(SourceConceptFormals.GetValueOrDefault(concept.Id,[]).Count.ToString(CultureInfo.InvariantCulture));
            foreach(var formal in SourceConceptFormals.GetValueOrDefault(concept.Id,[]))result+=SourceIdentityFrame(SourceStructuralFormalKey(formal,depth+1));
        }
        result+="A"+SourceIdentityFrame((argumentKind=="lookup"?SourceDependentFunctionArguments:SourceConceptArguments).GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture));
        foreach(var argument in (argumentKind=="lookup"?SourceDependentFunctionArguments:SourceConceptArguments).GetValueOrDefault(id,[]))
            result+=SourceIdentityFrame(SourceStructuralArgumentKey(argument,SourceConstraintArgumentDetails[(argumentKind,id,0,argument.Ordinal)].Pack,depth+1));
        var substitutions=SourceExpressionSubstitutions.Where(p=>p.Key.Expression==id).OrderBy(p=>p.Key.Ordinal).ToArray();
        result+="S"+SourceIdentityFrame(substitutions.Length.ToString(CultureInfo.InvariantCulture));
        foreach(var substitution in substitutions)
        {
            var facts=substitution.Value;string group=(facts.Dependent?"1":"0")+(facts.Nondependent?"1":"0")+(facts.AdjustCoordinates?"1":"0")+(facts.AliasDeductionGuide?"1":"0");
            group+="F"+SourceIdentityFrame(SourceExpressionSubstitutionFormals.GetValueOrDefault(substitution.Key,[]).Count.ToString(CultureInfo.InvariantCulture));
            foreach(var formal in SourceExpressionSubstitutionFormals.GetValueOrDefault(substitution.Key,[]))group+=SourceIdentityFrame(SourceStructuralFormalKey(formal,depth+1));
            group+="A"+SourceIdentityFrame(SourceExpressionSubstitutionArguments.GetValueOrDefault(substitution.Key,[]).Count.ToString(CultureInfo.InvariantCulture));
            foreach(var argument in SourceExpressionSubstitutionArguments.GetValueOrDefault(substitution.Key,[]))group+=SourceIdentityFrame(SourceStructuralArgumentKey(argument,SourceConstraintArgumentDetails[("substitution",id,substitution.Key.Ordinal,argument.Ordinal)].Pack,depth+1));
            result+=SourceIdentityFrame(group);
        }
        if(SourceExpressionFieldReferences.ContainsKey(id))result+="H"+SourceFieldReferenceIdentity(id,depth+1);
        if(SourceExpressionFolds.ContainsKey(id))result+="J"+SourceFoldIdentity(id);
        if(SourceExpressionAllocations.ContainsKey(id))result+="B"+SourceAllocationIdentity(id,depth+1);
        if(SourceExpressionTemporaries.ContainsKey(id))result+="I"+SourceTemporaryIdentity(id,depth+1);
        result+="E"+SourceIdentityFrame(SourceExpressionEdges.GetValueOrDefault(id,[]).Count.ToString(CultureInfo.InvariantCulture));
        foreach(var edge in SourceExpressionEdges.GetValueOrDefault(id,[]).OrderBy(e=>e.Role,StringComparer.Ordinal).ThenBy(e=>e.Ordinal))
            result+=SourceIdentityFrame(edge.Role)+SourceIdentityFrame(SourceStructuralExpressionKey(edge.Child,depth+1));
        return result;
    }
}
