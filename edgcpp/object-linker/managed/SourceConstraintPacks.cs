using System.Globalization;
namespace DotObj;
internal sealed record SourceConstraintArgumentDetails(bool Pack,bool PackElement,bool Ellipsis,bool IntegerPack,bool Explicit,bool InjectedClass,bool Auto,bool DecltypeAuto,long? Expansion,long? SubstitutedTemplate);
internal sealed record SourcePackExpansion(long Count,bool Ellipsis,bool EnclosingOnly,bool EnclosingAny,bool FunctionDeclarator,bool PackIndex,bool TemplateTemplate,long? ParameterType);
internal sealed record SourcePackReference(SourceTemplateArgument Argument,long? Depth,long? Position,bool Enclosing);
internal sealed record SourceDependentFunctionLookup(string Name,string QualifiedName,long? Owner,long? Conversion,long Operator,bool ExplicitArguments,bool Qualified,bool Address,bool Symbol,long CandidateCount,bool UnknownBase,bool QualifiedUnknownBase);
internal sealed partial class ObjectFile
{
    public Dictionary<(string Kind,long Owner,long Substitution,long Ordinal),SourceConstraintArgumentDetails> SourceConstraintArgumentDetails {get;}=[];
    public Dictionary<long,SourcePackExpansion> SourcePackExpansions {get;}=[];
    public Dictionary<long,List<SourcePackReference>> SourcePackReferences {get;}=[];
    public Dictionary<long,SourceDependentFunctionLookup> SourceDependentFunctionLookups {get;}=[];
    public Dictionary<long,List<long>> SourceDependentFunctionCandidates {get;}=[];
    public Dictionary<long,List<SourceTemplateArgument>> SourceDependentFunctionArguments {get;}=[];
    private bool SourceConstraintPayloadValid(string kind,long? type,long? constant,long? template)=>kind=="pack-boundary"
        ?type is null && constant is null && template is null:SourceTemplatePayloadValid(kind,type,constant,template);
    private void ReadSourceConstraintExtensions()
    {
        using(var q=database.Prepare("SELECT id,reference_count,ellipsis_seen,uses_only_enclosing_packs,uses_any_enclosing_packs,is_function_declarator,is_pack_index,parameter_is_template_template,parameter_source_type_id,capture_proof,line,column FROM source_pack_expansions ORDER BY id"))
            while(q.Read())
            {
                var value=new SourcePackExpansion(q.Integer(1),q.Integer(2)==1,q.Integer(3)==1,q.Integer(4)==1,q.Integer(5)==1,q.Integer(6)==1,q.Integer(7)==1,q.OptionalInteger(8));
                if(q.Integer(0)<=0 || value.Count<0 || Enumerable.Range(2,6).Any(index=>q.Integer(index) is not(0 or 1)) || value.Count>0 && value.EnclosingOnly && !value.EnclosingAny ||
                    value.ParameterType is long type && !SourceTypes.ContainsKey(type) || q.Text(9)!="original-pack-expansion" || q.Integer(10)<0 || q.Integer(11)<0 || !SourcePackExpansions.TryAdd(q.Integer(0),value))
                    throw new InvalidDataException("Invalid original source pack expansion descriptor");
            }
        using(var q=database.Prepare("SELECT expansion_id,ordinal,kind,source_type_id,constant_id,template_id,parameter_depth,parameter_position,uses_enclosing_pack FROM source_pack_references ORDER BY expansion_id,ordinal"))
            while(q.Read())
            {
                var argument=new SourceTemplateArgument(q.Integer(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5));
                var value=new SourcePackReference(argument,q.OptionalInteger(6),q.OptionalInteger(7),q.Integer(8)==1);
                if(!SourcePackExpansions.ContainsKey(q.Integer(0)) || !SourceTemplatePayloadValid(argument.Kind,argument.TypeId,argument.ConstantId,argument.TemplateId) ||
                    (value.Depth is null)!=(value.Position is null) || value.Depth<0 || value.Position<=0 || q.Integer(8) is not(0 or 1))throw new InvalidDataException("Source pack reference lacks exact original template parameter");
                if(!SourcePackReferences.TryGetValue(q.Integer(0),out var values))SourcePackReferences[q.Integer(0)]=values=[];
                if(argument.Ordinal!=values.Count)throw new InvalidDataException("Source pack reference order is not contiguous");values.Add(value);
            }
        using(var q=database.Prepare("SELECT expression_id,source_name,qualified_name,owner_source_type_id,conversion_source_type_id,operator_kind,has_explicit_template_arguments,is_qualified_name,has_address_of,has_lookup_symbol,candidate_count,member_of_unknown_base,qualified_unknown_base_member,capture_proof FROM source_dependent_function_lookups ORDER BY expression_id"))
            while(q.Read())
            {
                var value=new SourceDependentFunctionLookup(q.Text(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.Integer(5),q.Integer(6)==1,q.Integer(7)==1,q.Integer(8)==1,q.Integer(9)==1,q.Integer(10),q.Integer(11)==1,q.Integer(12)==1);
                if(!SourceExpressions.ContainsKey(q.Integer(0)) || value.Operator is <0 or >=48 || value.Owner is long owner && !SourceTypes.ContainsKey(owner) ||
                    value.Conversion is long conversion && !SourceTypes.ContainsKey(conversion) || Enumerable.Range(6,4).Any(index=>q.Integer(index) is not(0 or 1)) ||
                    q.Integer(11) is not(0 or 1) || q.Integer(12) is not(0 or 1) || value.CandidateCount<0 || value.QualifiedUnknownBase && !value.UnknownBase ||
                    q.Text(13)!="original-dependent-function-lookup" || !SourceDependentFunctionLookups.TryAdd(q.Integer(0),value))throw new InvalidDataException("Invalid original dependent function lookup");
            }
        using(var q=database.Prepare("SELECT expression_id,ordinal,declaration_id FROM source_dependent_function_candidates ORDER BY expression_id,ordinal"))
            while(q.Read())
            {
                if(!SourceDependentFunctionLookups.ContainsKey(q.Integer(0)) || !SourceExpressionDeclarations.TryGetValue(q.Integer(2),out var declaration) || declaration.Kind!="routine")
                    throw new InvalidDataException("Dependent function candidate lacks its exact source-only routine declaration");
                if(!SourceDependentFunctionCandidates.TryGetValue(q.Integer(0),out var values))SourceDependentFunctionCandidates[q.Integer(0)]=values=[];
                if(q.Integer(1)!=values.Count || values.Contains(q.Integer(2)))throw new InvalidDataException("Dependent function candidate order/identity is invalid");values.Add(q.Integer(2));
            }
        ReadSourceConstraintArguments("source_dependent_function_arguments","expression_id",SourceDependentFunctionLookups.ContainsKey,SourceDependentFunctionArguments);
        using(var q=database.Prepare("SELECT owner_kind,owner_id,substitution_ordinal,argument_ordinal,is_pack,is_pack_element,has_pack_ellipsis,is_integer_pack,is_explicit,is_injected_class_name,parameter_is_auto,parameter_is_decltype_auto,pack_expansion_id,substituted_template_id,capture_proof FROM source_constraint_argument_details ORDER BY owner_kind,owner_id,substitution_ordinal,argument_ordinal"))
            while(q.Read())
            {
                var key=(q.Text(0),q.Integer(1),q.Integer(2),q.Integer(3));
                var value=new SourceConstraintArgumentDetails(q.Integer(4)==1,q.Integer(5)==1,q.Integer(6)==1,q.Integer(7)==1,q.Integer(8)==1,q.Integer(9)==1,q.Integer(10)==1,q.Integer(11)==1,q.OptionalInteger(12),q.OptionalInteger(13));
                if(key.Item2<=0 || key.Item3<0 || key.Item4<0 || Enumerable.Range(4,8).Any(index=>q.Integer(index) is not(0 or 1)) ||
                    value.Expansion is long expansion && !SourcePackExpansions.ContainsKey(expansion) || value.SubstitutedTemplate is long template && !SourceTemplates.ContainsKey(template) ||
                    q.Text(14)!="original-template-argument" || !SourceConstraintArgumentDetails.TryAdd(key,value))throw new InvalidDataException("Invalid original source argument pack/substitution metadata");
            }
    }
    private void VerifySourceConstraintExtensions()
    {
        var used=new HashSet<(string,long,long,long)>();
        void Arguments(string kind,long owner,long substitution,List<SourceTemplateArgument> arguments)
        {
            foreach(var argument in arguments)
            {
                var key=(kind,owner,substitution,argument.Ordinal);
                if(!SourceConstraintArgumentDetails.TryGetValue(key,out var facts) || !used.Add(key) ||
                    kind=="class" && facts.Pack!=argument.IsPack || facts.IntegerPack && argument.Kind!="constant" || facts.SubstitutedTemplate is not null && argument.Kind!="template" ||
                    facts.InjectedClass && argument.Kind!="type" || facts.Auto && facts.DecltypeAuto)
                    throw new InvalidDataException("Original argument lacks exact pack, expansion and substitution facts");
            }
        }
        foreach(var pair in SourceTemplateArguments)Arguments("class",pair.Key,0,pair.Value);
        foreach(var pair in SourceConceptArguments)Arguments("concept",pair.Key,0,pair.Value);
        foreach(var pair in SourceExpressionDeclarationArguments)Arguments("declaration",pair.Key,0,pair.Value);
        foreach(var pair in SourceExpressionSubstitutionArguments)Arguments("substitution",pair.Key.Expression,pair.Key.Ordinal,pair.Value);
        foreach(var pair in SourceDependentFunctionArguments)Arguments("lookup",pair.Key,0,pair.Value);
        if(used.Count!=SourceConstraintArgumentDetails.Count)throw new InvalidDataException("Source argument metadata has no exact original argument owner");
        foreach(var pair in SourcePackExpansions)
        {
            var refs=SourcePackReferences.GetValueOrDefault(pair.Key,[]);
            if(refs.Count!=pair.Value.Count)throw new InvalidDataException("Original pack expansion reference list is incomplete");
            foreach(var reference in refs)
            {
                var argument=reference.Argument;long depth,position;bool pack;
                if(argument.Kind=="type")
                {
                    if(!SourceDependentTypes.TryGetValue(Original(SourceTypeById(argument.TypeId!.Value)).Id,out var parameter) || parameter.Kind!="parameter")throw new InvalidDataException("Pack type reference is not its original template parameter");
                    depth=parameter.NestingDepth!.Value;position=parameter.ParameterPosition!.Value;pack=parameter.IsPack;
                }
                else if(argument.Kind=="constant")
                {
                    var parameter=SourceSemanticConstants[argument.ConstantId!.Value];
                    if(parameter.Kind!="parameter")throw new InvalidDataException("Pack constant reference is not its original template parameter");
                    depth=parameter.ParameterDepth!.Value;position=parameter.ParameterPosition!.Value;pack=parameter.IsPack;
                }
                else {var parameter=SourceTemplates[argument.TemplateId!.Value];depth=parameter.ParameterDepth;position=parameter.ParameterPosition;pack=parameter.IsPack;}
                if(!pack || reference.Depth is long original && (original!=depth || reference.Position!=position))throw new InvalidDataException("Original pack reference coordinates disagree with selected parameter");
            }
        }
        foreach(var pair in SourceDependentFunctionLookups)
        {
            var value=pair.Value;
            if(SourceExpressions[pair.Key].Kind!="dependent-function-reference" || SourceDependentFunctionCandidates.GetValueOrDefault(pair.Key,[]).Count!=value.CandidateCount ||
                value.Symbol!=(value.CandidateCount!=0) || !value.ExplicitArguments && SourceDependentFunctionArguments.GetValueOrDefault(pair.Key,[]).Count!=0 ||
                value.Name.Length==0 && value.Operator==0 && value.Conversion is null || value.QualifiedName.Length==0 && value.Owner is null)
                throw new InvalidDataException("Dependent function lookup lost its original candidates/name or explicit argument boundary");
        }
    }
    private string SourceConstraintArgumentDetailsIdentity(string kind,long owner,long substitution,SourceTemplateArgument argument)
    {
        var value=SourceConstraintArgumentDetails[(kind,owner,substitution,argument.Ordinal)];
        return SourceIdentityFrame(argument.Kind=="pack-boundary"?"pack-boundary":SourceConstraintArgumentIdentity(argument))+
            (value.Pack?"1":"0")+(value.PackElement?"1":"0")+(value.Ellipsis?"1":"0")+(value.IntegerPack?"1":"0")+(value.Explicit?"1":"0")+(value.InjectedClass?"1":"0")+(value.Auto?"1":"0")+(value.DecltypeAuto?"1":"0")+
            SourceIdentityFrame(value.SubstitutedTemplate is long template?SourceTemplateIdentity(template,0):"")+SourceIdentityFrame(value.Expansion is long expansion?SourcePackExpansionIdentity(expansion):"");
    }
    private string SourcePackExpansionIdentity(long id)
    {
        var value=SourcePackExpansions[id];string result=(value.Ellipsis?"1":"0")+(value.EnclosingOnly?"1":"0")+(value.EnclosingAny?"1":"0")+(value.FunctionDeclarator?"1":"0")+(value.PackIndex?"1":"0")+(value.TemplateTemplate?"1":"0")+
            SourceIdentityFrame(value.ParameterType is long type?SourceTypeSignature(type):"");
        foreach(var reference in SourcePackReferences.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceConstraintArgumentIdentity(reference.Argument))+
            SourceIdentityFrame(reference.Depth?.ToString(CultureInfo.InvariantCulture)??"")+SourceIdentityFrame(reference.Position?.ToString(CultureInfo.InvariantCulture)??"")+(reference.Enclosing?"1":"0");
        return SourceGraphHash(result);
    }
    private string SourceDependentFunctionIdentity(long id,HashSet<long> activeConcepts)
    {
        var value=SourceDependentFunctionLookups[id];string result=SourceIdentityFrame(value.Name)+SourceIdentityFrame(value.QualifiedName)+
            SourceIdentityFrame(value.Owner is long owner?SourceTypeSignature(owner):"")+SourceIdentityFrame(value.Conversion is long conversion?SourceTypeSignature(conversion):"")+
            SourceIdentityFrame(value.Operator.ToString(CultureInfo.InvariantCulture))+(value.ExplicitArguments?"1":"0")+(value.Qualified?"1":"0")+(value.Address?"1":"0")+(value.Symbol?"1":"0")+(value.UnknownBase?"1":"0")+(value.QualifiedUnknownBase?"1":"0");
        foreach(long candidate in SourceDependentFunctionCandidates.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceConstraintDeclarationIdentity(candidate,sourceDeclarationIdentityActive,activeConcepts));
        foreach(var argument in SourceDependentFunctionArguments.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceConstraintArgumentDetailsIdentity("lookup",id,0,argument));
        return result;
    }
}
