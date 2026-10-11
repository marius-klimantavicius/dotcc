using System.Globalization;
namespace DotObj;
internal sealed record SourceNamedTemplate(string QualifiedName,long? Owner,long Prototype,long? Constraint,bool HasNamespaceOwner);
internal sealed record SourceAliasInstance(long Template,bool Prototype,bool Nonreal,bool Dependent,long Count,bool OriginalPresent,long OriginalCount,long MinimumArguments);
internal sealed record SourceTypeOperator(long Opcode,string Name,long? Expression,long? Operand,bool Dependent,bool Unparenthesized);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceNamedTemplate> SourceNamedTemplates {get;}=[];
    public Dictionary<long,SourceAliasInstance> SourceAliasInstances {get;}=[];
    public Dictionary<long,List<SourceTemplateArgument>> SourceAliasSelectedArguments {get;}=[];
    public Dictionary<long,List<SourceTemplateArgument>> SourceAliasOriginalArguments {get;}=[];
    public Dictionary<long,SourceTypeOperator> SourceTypeOperators {get;}=[];
    private static string? SourceTypeOperatorName(long opcode)=>opcode switch
    {
        1=>"decltype",
        5=>"__underlying_type",
        6=>"typeof-expression",
        7=>"typeof-type",
        14=>"__add_lvalue_reference",
        15=>"__add_pointer",
        16=>"__add_rvalue_reference",
        17=>"__decay",
        18=>"__make_signed",
        19=>"__make_unsigned",
        20=>"__remove_all_extents",
        21=>"__remove_const",
        22=>"__remove_cv",
        23=>"__remove_cvref",
        24=>"__remove_extent",
        25=>"__remove_pointer",
        26=>"__remove_reference_t",
        27=>"__remove_restrict",
        28=>"__remove_volatile",
        29=>"__remove_reference",
        _=>null
    };
    private void ReadSourceTypeOrigins()
    {
        using(var q=database.Prepare("SELECT template_id,qualified_name,owner_source_type_id,prototype_source_type_id,constraint_expression_id,capture_proof,has_namespace_owner FROM source_named_templates ORDER BY template_id"))
            while(q.Read())
            {
                var value=new SourceNamedTemplate(q.Text(1),q.OptionalInteger(2),q.Integer(3),q.OptionalInteger(4),q.Integer(6)==1);
                if(!SourceTemplates.TryGetValue(q.Integer(0),out var template) || template.Kind!="alias" || value.QualifiedName.Length==0 ||
                    value.Owner is long owner && !SourceTypes.ContainsKey(owner) || !SourceTypes.ContainsKey(value.Prototype) ||
                    value.Constraint is long constraint && !SourceExpressions.ContainsKey(constraint) || q.Text(5)!="original-alias-template" || q.Integer(6) is not(0 or 1) ||
                    !SourceNamedTemplates.TryAdd(q.Integer(0),value))throw new InvalidDataException("Named alias template lost its original declaration/header");
            }
        using(var q=database.Prepare("SELECT type_id,template_id,is_prototype,is_nonreal,is_dependent,argument_count,has_original_arguments,original_argument_count,minimum_arguments,capture_proof FROM source_alias_instances ORDER BY type_id"))
            while(q.Read())
            {
                var value=new SourceAliasInstance(q.Integer(1),q.Integer(2)==1,q.Integer(3)==1,q.Integer(4)==1,q.Integer(5),q.Integer(6)==1,q.Integer(7),q.Integer(8));
                if(!SourceTypes.TryGetValue(q.Integer(0),out var type) || type.SourceForm!="template-alias" || type.Kind is not("typedef" or "qualified") ||
                    !SourceNamedTemplates.ContainsKey(value.Template) || Enumerable.Range(2,3).Any(i=>q.Integer(i) is not(0 or 1)) || q.Integer(6) is not(0 or 1) ||
                    value.Count<0 || value.OriginalCount<0 || value.MinimumArguments < -1 || !value.OriginalPresent && value.OriginalCount!=0 ||
                    q.Text(9)!="original-alias-instance" || !SourceAliasInstances.TryAdd(type.Id,value))
                    throw new InvalidDataException("Alias instance lost its original template or substitution boundary");
            }
        using(var q=database.Prepare("SELECT type_id,list_kind,ordinal,kind,source_type_id,constant_id,template_id FROM source_alias_arguments ORDER BY type_id,list_kind,ordinal"))
            while(q.Read())
            {
                var value=new SourceTemplateArgument(q.Integer(2),q.Text(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6));
                if(!SourceAliasInstances.ContainsKey(q.Integer(0)) || q.Text(1) is not("alias-selected" or "alias-original") ||
                    !SourceConstraintPayloadValid(value.Kind,value.TypeId,value.ConstantId,value.TemplateId))
                    throw new InvalidDataException("Alias argument lacks its exact original typed payload");
                var target=q.Text(1)=="alias-selected"?SourceAliasSelectedArguments:SourceAliasOriginalArguments;
                if(!target.TryGetValue(q.Integer(0),out var list))target[q.Integer(0)]=list=[];
                if(value.Ordinal!=list.Count)throw new InvalidDataException("Alias substitution argument order is not contiguous");list.Add(value);
            }
        using(var q=database.Prepare("SELECT type_id,opcode,expression_id,operand_source_type_id,is_dependent,is_unparenthesized_decltype,capture_proof,operator_name FROM source_type_operators ORDER BY type_id"))
            while(q.Read())
            {
                var value=new SourceTypeOperator(q.Integer(1),q.Text(7),q.OptionalInteger(2),q.OptionalInteger(3),q.Integer(4)==1,q.Integer(5)==1);
                bool expression=value.Opcode is 1 or 6;
                if(!SourceTypes.TryGetValue(q.Integer(0),out var type) || type.SourceForm!="type-operator" || type.Kind is not("typedef" or "qualified") ||
                    SourceTypeOperatorName(value.Opcode)!=value.Name || q.Integer(4) is not(0 or 1) || q.Integer(5) is not(0 or 1) ||
                    expression != (value.Expression is not null) || expression == (value.Operand is not null) ||
                    value.Expression is long e && !SourceExpressions.ContainsKey(e) || value.Operand is long operand && !SourceTypes.ContainsKey(operand) ||
                    value.Unparenthesized && value.Opcode!=1 || q.Text(6)!="original-type-operator" || !SourceTypeOperators.TryAdd(type.Id,value))
                    throw new InvalidDataException("Original source type operator lost its exact opcode/operand or decltype semantics");
            }
        foreach(var type in SourceTypes.Values)
            if(type.SourceForm is not("plain" or "template-alias" or "type-operator") ||
                (type.SourceForm=="template-alias")!=SourceAliasInstances.ContainsKey(type.Id) ||
                (type.SourceForm=="type-operator")!=SourceTypeOperators.ContainsKey(type.Id))
                throw new InvalidDataException("Original source type is missing its required alias/operator provenance");
        foreach(var template in SourceTemplates.Values)
            if((template.Kind=="alias")!=SourceNamedTemplates.ContainsKey(template.Id))
                throw new InvalidDataException("Original alias template is missing its required declaration");
        foreach(var pair in SourceNamedTemplates)
        {
            var value=pair.Value;var type=SourceTypes[value.Prototype];
            if(!SourceAliasInstances.TryGetValue(value.Prototype,out var prototype) || prototype.Template!=pair.Key || !prototype.Prototype ||
                type.SourceName!=SourceTemplates[pair.Key].SourceName)
                throw new InvalidDataException("Alias template does not own its exact original prototype type");
        }
        foreach(var pair in SourceAliasInstances)
        {
            var value=pair.Value;
            if(SourceAliasSelectedArguments.GetValueOrDefault(pair.Key,[]).Count!=value.Count ||
                SourceAliasOriginalArguments.GetValueOrDefault(pair.Key,[]).Count!=value.OriginalCount ||
                value.Prototype!=(SourceNamedTemplates[value.Template].Prototype==pair.Key))
                throw new InvalidDataException("Alias instance original argument lists/prototype ownership are incomplete");
        }
    }
}
