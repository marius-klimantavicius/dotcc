namespace DotObj;

internal sealed record SourceTemplateReference(long Id, string SourceName, long? ParameterDepth, long? ParameterPosition,
    bool IsPack, long ParameterCount, long? SourceFileId, long Line, long Column, string Kind);
internal sealed record SourceTemplateFormal(long Ordinal, string Kind, string SourceName, long? TypeId, long? ConstantId,
    long? TemplateId, long? DefaultTypeId, long? DefaultConstantId, long? DefaultTemplateId, bool IsPack, bool IsAbbreviated,
    long? SourceFileId, long Line, long Column);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceTemplateReference> SourceTemplates { get; } = [];
    public Dictionary<long, List<SourceTemplateFormal>> SourceTemplateParameters { get; } = [];

    private bool SourceTemplatePayloadValid(string kind, long? type, long? constant, long? template) => kind switch
    {
        "type" => type is long id && SourceTypes.ContainsKey(id) && constant is null && template is null,
        "constant" => constant is long id && SourceSemanticConstants.ContainsKey(id) && type is null && template is null,
        "template" => template is long id && SourceTemplates.ContainsKey(id) && type is null && constant is null,
        _ => false
    };
    private void ReadSourceTemplateReferences()
    {
        using (var q = database.Prepare("SELECT id,kind,source_name,parameter_depth,parameter_position,is_pack,parameter_count,capture_proof,source_file_id,line,column FROM source_templates ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceTemplateReference(q.Integer(0), q.Text(2), q.OptionalInteger(3), q.OptionalInteger(4), q.Integer(5) == 1,
                    q.Integer(6), q.OptionalInteger(8), q.Integer(9), q.Integer(10),q.Text(1));
                bool projection = value.Kind switch
                {
                    "parameter" => value.ParameterDepth is >=0 && value.ParameterPosition is >0 && q.Text(7)=="original-template-parameter" && value.SourceFileId is >0 && value.Line>0,
                    "alias" => value.ParameterDepth is null && value.ParameterPosition is null && !value.IsPack && q.Text(7)=="original-alias-template" && value.SourceFileId is >0 && value.Line>0,
                    "edg-node" => q.Text(7)=="original-edg-template",
                    _ => false
                };
                if (value.Id <= 0 || !projection || value.ParameterCount < 0 || value.Line<0 || value.Column<0 ||
                    q.Integer(5) is not (0 or 1) || !SourceTemplates.TryAdd(value.Id,value))
                    throw new InvalidDataException("Template reference lacks its original template parameter declaration");
            }
        RejectRows("SELECT t.id FROM source_templates t LEFT JOIN edg_graph_roots r ON r.role='source-template' AND r.ordinal=t.id-1 LEFT JOIN edg_graph_nodes n ON n.id=r.node_id WHERE t.kind='edg-node' AND (n.id IS NULL OR n.entry_kind_name<>'template')",
            "Raw source template lacks its exact typed EDG template root");
        using (var q = database.Prepare("SELECT source_template_id,ordinal,kind,source_name,type_id,constant_id,template_id,default_type_id,default_constant_id,default_template_id,is_pack,is_abbreviated,source_file_id,line,column FROM source_template_parameters ORDER BY source_template_id,ordinal"))
            while (q.Read())
            {
                long owner = q.Integer(0);
                var value = new SourceTemplateFormal(q.Integer(1), q.Text(2), q.Text(3), q.OptionalInteger(4), q.OptionalInteger(5),
                    q.OptionalInteger(6), q.OptionalInteger(7), q.OptionalInteger(8), q.OptionalInteger(9), q.Integer(10) == 1,
                    q.Integer(11) == 1, q.OptionalInteger(12), q.Integer(13), q.Integer(14));
                if (!SourceTemplates.ContainsKey(owner) || q.Integer(10) is not (0 or 1) || q.Integer(11) is not (0 or 1) ||
                    value.Line < 0 || value.Column < 0 || !SourceTemplatePayloadValid(value.Kind, value.TypeId, value.ConstantId, value.TemplateId))
                    throw new InvalidDataException("Source template formal parameter lacks its exact typed payload");
                bool hasDefault = value.DefaultTypeId is not null || value.DefaultConstantId is not null || value.DefaultTemplateId is not null;
                if (hasDefault && (!SourceTemplatePayloadValid(value.Kind, value.DefaultTypeId, value.DefaultConstantId, value.DefaultTemplateId)))
                    throw new InvalidDataException("Source template default does not match its original parameter kind");
                if (!SourceTemplateParameters.TryGetValue(owner, out var list)) SourceTemplateParameters[owner] = list = [];
                if (value.Ordinal != list.Count) throw new InvalidDataException("Template formal parameter order is not contiguous");
                list.Add(value);
            }
    }
    private void VerifySourceTemplateReferences()
    {
        foreach (var value in SourceTemplates.Values)
            if (SourceTemplateParameters.GetValueOrDefault(value.Id, []).Count != value.ParameterCount)
                throw new InvalidDataException("Source template parameter list is incomplete");
        foreach (var formal in SourceTemplateParameters.Values.SelectMany(values => values))
            VerifySourceConstraintFormal(formal);
    }
}
