namespace DotObj;

internal sealed record SourceTemplateReference(long Id, string SourceName, long? ParameterDepth, long? ParameterPosition,
    bool IsPack, long ParameterCount, long SourceFileId, long Line, long Column, string Kind);
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
                    q.Integer(6), q.Integer(8), q.Integer(9), q.Integer(10),q.Text(1));
                if (value.Id <= 0 || value.Kind is not("parameter" or "alias") || value.SourceFileId <= 0 ||
                    value.ParameterCount < 0 || value.Line <= 0 || value.Column < 0 || q.Integer(5) is not (0 or 1) ||
                    (value.Kind=="parameter" ? value.ParameterDepth is not >=0 || value.ParameterPosition is not >0 || q.Text(7)!="original-template-parameter"
                      : value.ParameterDepth is not null || value.ParameterPosition is not null || value.IsPack || q.Text(7)!="original-alias-template") ||
                    !SourceTemplates.TryAdd(value.Id, value))
                    throw new InvalidDataException("Template reference lacks its original template parameter declaration");
            }
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
                if (hasDefault && (value.IsPack || !SourceTemplatePayloadValid(value.Kind, value.DefaultTypeId, value.DefaultConstantId, value.DefaultTemplateId)))
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
        foreach (var pair in SourceTemplateParameters)
            foreach (var formal in pair.Value)
            {
                if(SourceTemplates[pair.Key].Kind=="alias") {VerifySourceConstraintFormal(formal);continue;}
                if (formal.Kind == "type")
                {
                    var type = Original(SourceTypeById(formal.TypeId!.Value));
                    if (type.Kind != "dependent" || !SourceDependentTypes.TryGetValue(type.Id, out var parameter) ||
                        parameter.Kind != "parameter" || parameter.IsPack != formal.IsPack ||
                        parameter.NestingDepth != 0 || parameter.ParameterPosition != formal.Ordinal + 1)
                        throw new InvalidDataException("Template type formal must retain its original placeholder type");
                }
                else if (formal.Kind == "constant")
                {
                    var constant = SourceSemanticConstants[formal.ConstantId!.Value];
                    if (constant.Kind != "parameter" || constant.IsPack != formal.IsPack ||
                        constant.ParameterDepth != 0 || constant.ParameterPosition != formal.Ordinal + 1)
                        throw new InvalidDataException("Template non-type formal must retain its original placeholder value");
                }
                else if (SourceTemplates[formal.TemplateId!.Value].IsPack != formal.IsPack ||
                    SourceTemplates[formal.TemplateId.Value].ParameterDepth != 0 ||
                    SourceTemplates[formal.TemplateId.Value].ParameterPosition != formal.Ordinal + 1)
                    throw new InvalidDataException("Template formal pack differs from its exact template parameter declaration");
            }
        // EDG NO_NESTING_DEPTH belongs specifically to an original inner
        // template-template formal list; it is not an unbound source type.
        var formals = SourceTemplateParameters.Values.SelectMany(value => value).ToArray();
        foreach (var parameter in SourceDependentTypes.Values.Where(value => value.Kind == "parameter" && value.NestingDepth == 0))
            if (!formals.Any(formal => formal.TypeId is long type && Original(SourceTypeById(type)).Id == parameter.TypeId))
                throw new InvalidDataException("Depth-zero source type has no owned template formal declaration");
        foreach (var parameter in SourceSemanticConstants.Values.Where(value => value.Kind == "parameter" && value.ParameterDepth == 0))
            if (!formals.Any(formal => formal.ConstantId == parameter.Id))
                throw new InvalidDataException("Depth-zero source value has no owned template formal declaration");
        foreach (var parameter in SourceTemplates.Values.Where(value => value.Kind=="parameter" && value.ParameterDepth == 0))
            if (!formals.Any(formal => formal.TemplateId == parameter.Id))
                throw new InvalidDataException("Depth-zero source template has no owned enclosing formal declaration");
        foreach (long id in SourceTemplates.Keys) SourceTemplateIdentity(id, 0);
    }
    private string SourceTemplateIdentity(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source template identity cycle/depth");
        var value = SourceTemplates[id];
        using var file = database.Prepare($"SELECT path FROM source_files WHERE id={value.SourceFileId}");
        if (!file.Read()) throw new InvalidDataException("Template parameter declaration file is absent");
        string result = "template-parameter:" + SourceIdentityFrame(file.Text(0) + ":" + value.Line + ":" + value.Column + ":" +
            value.ParameterDepth + ":" + value.ParameterPosition) + ":" + (value.IsPack ? "1" : "0");
        if(value.Kind=="alias") {
            var declaration=SourceNamedTemplates[id];
            result="template-alias:"+SourceIdentityFrame(declaration.QualifiedName)+SourceIdentityFrame(declaration.Owner is long owner?SourceArgumentTypeIdentity(owner,depth+1):"")+
                SourceIdentityFrame(file.Text(0)+":"+value.Line+":"+value.Column);
        }
        foreach (var formal in SourceTemplateParameters.GetValueOrDefault(id, []))
        {
            string item = formal.Kind + ":" + (formal.IsPack ? "1" : "0") + ":" + (formal.IsAbbreviated ? "1" : "0");
            item += SourceIdentityFrame(formal.TypeId is long type ? SourceArgumentTypeIdentity(type, depth + 1)
                : formal.ConstantId is long constant ? SourceConstantIdentity(constant, depth + 1) : SourceTemplateIdentity(formal.TemplateId!.Value, depth + 1));
            item += SourceIdentityFrame(formal.DefaultTypeId is long defaultType ? "t" + SourceArgumentTypeIdentity(defaultType, depth + 1)
                : formal.DefaultConstantId is long defaultConstant ? "c" + SourceConstantIdentity(defaultConstant, depth + 1)
                : formal.DefaultTemplateId is long defaultTemplate ? "m" + SourceTemplateIdentity(defaultTemplate, depth + 1) : "");
            result += SourceIdentityFrame(item);
        }
        return result;
    }
}
