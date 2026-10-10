namespace DotObj;

internal sealed record SourceExpression(long Id, string Kind, long? SourceTypeId, string Category,
    long EdgeCount, long ParameterCount, long ArgumentCount, long SubstitutionCount, long? OriginalLvalueTypeId, bool Parenthesized, bool CompilerGenerated, bool VolatileFetch,bool PackExpansion);
internal sealed record SourceExpressionEdge(string Role, long Ordinal, long Child);
internal sealed record SourceConcept(long Id, string Name, string QualifiedName, long Definition, long? HeaderConstraint, long ParameterCount,bool HasNamespaceOwner);
internal sealed record SourceExpressionParameter(long Type, long? DeclaredType, string Name, long Qualifiers, bool Pack);
internal sealed record SourceExpressionDeclaration(long Id, string Kind, string Name, string QualifiedName, long Type,
    long? OwnerType, long? TemplatePrototype, bool HasTemplate, bool IsPrototype, bool Constexpr, long FormalCount, long ArgumentCount, long? OriginalInitKind, long? OriginalDynamicInitKind, string InitializerKind, long? InitializerConstant, long? InitializerExpression, string TemplateOriginKind,string RoutineKind,string ScopeKind);
internal sealed record SourceExpressionSubstitution(bool Dependent, bool Nondependent, bool AdjustCoordinates, bool AliasDeductionGuide, long FormalCount, long ArgumentCount);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceExpression> SourceExpressions { get; } = [];
    public Dictionary<long, List<SourceExpressionEdge>> SourceExpressionEdges { get; } = [];
    public Dictionary<long, SourceConcept> SourceConcepts { get; } = [];
    public Dictionary<long, long> SourceTypeConstraints { get; } = [];
    public Dictionary<long, List<SourceTemplateFormal>> SourceConceptFormals { get; } = [];
    public Dictionary<long, (long Concept, bool TypeConstraint)> SourceConceptUses { get; } = [];
    public Dictionary<long, List<SourceTemplateArgument>> SourceConceptArguments { get; } = [];
    public Dictionary<long, List<SourceExpressionParameter>> SourceExpressionParameters { get; } = [];
    public Dictionary<long, (long Binder, long Ordinal, long Levels)> SourceExpressionParameterReferences { get; } = [];
    public Dictionary<long, long> SourceExpressionBinders { get; } = [];
    public Dictionary<long, (long Opcode, string Name, long? OriginalTypeKind)> SourceExpressionOperations { get; } = [];
    public Dictionary<long, HashSet<string>> SourceExpressionFlags { get; } = [];
    public Dictionary<long, long> SourceExpressionTypeOperands { get; } = [];
    public Dictionary<long, long> SourceExpressionConstants { get; } = [];
    public Dictionary<long, bool> SourceExpressionRequirements { get; } = [];
    public Dictionary<long, bool> SourceExpressionQueries { get; } = [];
    public Dictionary<long, SourceExpressionDeclaration> SourceExpressionDeclarations { get; } = [];
    public Dictionary<long, long> SourceExpressionDeclarationReferences { get; } = [];
    public Dictionary<long, List<SourceTemplateFormal>> SourceExpressionDeclarationFormals { get; } = [];
    public Dictionary<long, List<SourceTemplateArgument>> SourceExpressionDeclarationArguments { get; } = [];
    public Dictionary<(long Expression, long Ordinal), SourceExpressionSubstitution> SourceExpressionSubstitutions { get; } = [];
    public Dictionary<(long Expression, long Ordinal), List<SourceTemplateFormal>> SourceExpressionSubstitutionFormals { get; } = [];
    public Dictionary<(long Expression, long Ordinal), List<SourceTemplateArgument>> SourceExpressionSubstitutionArguments { get; } = [];

    // Called after original types, template formals, constants and records have
    // been loaded. No ReadBody, execution entity or ABI completion is requested.
    private void ReadSourceConstraints()
    {
        using (var q = database.Prepare("SELECT id,kind,source_type_id,value_category,edge_count,parameter_count,argument_count,substitution_count,capture_proof,line,column,original_lvalue_type_id,is_parenthesized,is_compiler_generated,is_volatile_fetch,is_pack_expansion FROM source_expressions ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceExpression(q.Integer(0), q.Text(1), q.OptionalInteger(2), q.Text(3), q.Integer(4), q.Integer(5), q.Integer(6), q.Integer(7),q.OptionalInteger(11),q.Integer(12)==1,q.Integer(13)==1,q.Integer(14)==1,q.Integer(15)==1);
                if (value.Id <= 0 || value.SourceTypeId is long type && !SourceTypes.ContainsKey(type) ||
                    value.Category is not ("none" or "lvalue" or "xvalue" or "prvalue") || (value.SourceTypeId is null) != (value.Category == "none") ||
                    value.OriginalLvalueTypeId is long original && !SourceTypes.ContainsKey(original) || q.Integer(12) is not(0 or 1) || q.Integer(13) is not(0 or 1) || q.Integer(14) is not(0 or 1) || q.Integer(15) is not(0 or 1) ||
                    value.EdgeCount < 0 || value.ParameterCount < 0 || value.ArgumentCount < 0 || value.SubstitutionCount < 0 ||
                    q.Text(8) != "original-source-expression" || q.Integer(9) < 0 || q.Integer(10) < 0 || !SourceExpressions.TryAdd(value.Id, value))
                    throw new InvalidDataException("Invalid owned original source expression");
            }
        using (var q = database.Prepare("SELECT expression_id,role,ordinal,child_expression_id FROM source_expression_edges ORDER BY expression_id,role,ordinal"))
            while (q.Read())
            {
                long id=q.Integer(0);var edge=new SourceExpressionEdge(q.Text(1),q.Integer(2),q.Integer(3));
                if (!SourceExpressions.ContainsKey(id) || !SourceExpressions.ContainsKey(edge.Child)) throw new InvalidDataException("Source expression edge lacks its owned nodes");
                if (!SourceExpressionEdges.TryGetValue(id,out var edges)) SourceExpressionEdges[id]=edges=[];
                if (edge.Ordinal != edges.Count(item=>item.Role==edge.Role)) throw new InvalidDataException("Source expression operand order is not contiguous");
                edges.Add(edge);
            }
        using (var q = database.Prepare("SELECT id,source_name,qualified_name,definition_expression_id,header_constraint_expression_id,parameter_count,capture_proof,has_namespace_owner FROM source_concepts ORDER BY id"))
            while (q.Read())
            {
                var value=new SourceConcept(q.Integer(0),q.Text(1),q.Text(2),q.Integer(3),q.OptionalInteger(4),q.Integer(5),q.Integer(7)==1);
                if(value.Id<=0 || value.Name.Length==0 || value.QualifiedName.Length==0 || value.ParameterCount<=0 ||
                    !SourceExpressions.ContainsKey(value.Definition) || value.HeaderConstraint is long header && !SourceExpressions.ContainsKey(header) ||
                    q.Text(6)!="original-concept-definition" || q.Integer(7) is not(0 or 1) || !SourceConcepts.TryAdd(value.Id,value)) throw new InvalidDataException("Source concept lacks its original definition and formal header");
            }
        using (var q=database.Prepare("SELECT source_type_id,constraint_expression_id,capture_proof FROM source_type_constraints ORDER BY source_type_id"))
            while(q.Read())
            {
                if(!SourceDependentTypes.TryGetValue(q.Integer(0),out var type) || type.Kind is not("parameter" or "auto-placeholder") ||
                    !SourceExpressions.ContainsKey(q.Integer(1)) || q.Text(2)!="original-type-constraint" || !SourceTypeConstraints.TryAdd(q.Integer(0),q.Integer(1)))
                    throw new InvalidDataException("Constrained type lacks its original concept use");
            }
        using(var q=database.Prepare("SELECT expression_id,concept_id,is_type_constraint FROM source_concept_uses ORDER BY expression_id"))
            while(q.Read())
                if(!SourceExpressions.ContainsKey(q.Integer(0)) || !SourceConcepts.ContainsKey(q.Integer(1)) || q.Integer(2) is not(0 or 1) ||
                    !SourceConceptUses.TryAdd(q.Integer(0),(q.Integer(1),q.Integer(2)==1))) throw new InvalidDataException("Invalid source concept-id declaration reference");
        using(var q=database.Prepare("SELECT expression_id,ordinal,source_type_id,declared_source_type_id,source_name,qualifier_bits,is_pack FROM source_expression_parameters ORDER BY expression_id,ordinal"))
            while(q.Read())
            {
                long owner=q.Integer(0);var parameter=new SourceExpressionParameter(q.Integer(2),q.OptionalInteger(3),q.Text(4),q.Integer(5),q.Integer(6)==1);
                if(!SourceExpressions.ContainsKey(owner) || !SourceTypes.ContainsKey(parameter.Type) || parameter.DeclaredType is long declared && !SourceTypes.ContainsKey(declared) ||
                    parameter.Qualifiers<0 || q.Integer(6) is not(0 or 1)) throw new InvalidDataException("Invalid source requires parameter declaration");
                if(!SourceExpressionParameters.TryGetValue(owner,out var values)) SourceExpressionParameters[owner]=values=[];
                if(q.Integer(1)!=values.Count)throw new InvalidDataException("Requires parameter order is not contiguous");values.Add(parameter);
            }
        ReadSourceParameterBinders();
        ReadSourceExpressionLongRelation("source_expression_bindings","lexical_binder_id",SourceExpressionBinders,SourceParameterBinders.ContainsKey);
        ReadSourceExpressionLongRelation("source_expression_type_operands","source_type_id",SourceExpressionTypeOperands,SourceTypes.ContainsKey);
        ReadSourceExpressionLongRelation("source_expression_constants","constant_id",SourceExpressionConstants,SourceSemanticConstants.ContainsKey);
        using(var q=database.Prepare("SELECT expression_id,binder_id,parameter_ordinal,original_levels_up FROM source_expression_parameter_references ORDER BY expression_id"))
            while(q.Read())
                if(!SourceExpressions.ContainsKey(q.Integer(0)) || !SourceParameterBinders.ContainsKey(q.Integer(1)) || q.Integer(2)<0 || q.Integer(3)<0 ||
                    !SourceExpressionParameterReferences.TryAdd(q.Integer(0),(q.Integer(1),q.Integer(2),q.Integer(3)))) throw new InvalidDataException("Invalid original requires parameter reference");
        using(var q=database.Prepare("SELECT expression_id,opcode,operation_name,original_type_kind FROM source_expression_operations ORDER BY expression_id"))
            while(q.Read())
                if(!SourceExpressions.ContainsKey(q.Integer(0)) || q.Integer(1)<0 || !SourceExpressionOperations.TryAdd(q.Integer(0),(q.Integer(1),q.Text(2),q.OptionalInteger(3))))
                    throw new InvalidDataException("Invalid original source operation");
        using(var q=database.Prepare("SELECT expression_id,flag FROM source_expression_flags ORDER BY expression_id,flag"))
            while(q.Read())
            {
                long id=q.Integer(0);if(!SourceExpressions.ContainsKey(id))throw new InvalidDataException("Source operation flag lacks owned expression");
                if(!SourceExpressionFlags.TryGetValue(id,out var flags))SourceExpressionFlags[id]=flags=[];
                if(!flags.Add(q.Text(1)))throw new InvalidDataException("Duplicate source operation flag");
            }
        ReadSourceExpressionBoolRelation("source_expression_requirements","requires_noexcept",SourceExpressionRequirements);
        ReadSourceExpressionBoolRelation("source_expression_queries","is_standard_alignof",SourceExpressionQueries);
        ReadSourceConstraintDeclarations();
        ReadSourceFunctionParameterDeclarations();
        ReadSourceLocalDeclarations();
        ReadSourceExpressionLongRelation("source_expression_declaration_references","declaration_id",SourceExpressionDeclarationReferences,SourceExpressionDeclarations.ContainsKey);
        ReadSourceConstraintFormals("source_concept_parameters","concept_id",SourceConcepts.ContainsKey,SourceConceptFormals);
        ReadSourceConstraintArguments("source_concept_arguments","expression_id",SourceConceptUses.ContainsKey,SourceConceptArguments);
        ReadSourceConstraintFormals("source_expression_declaration_formals","declaration_id",SourceExpressionDeclarations.ContainsKey,SourceExpressionDeclarationFormals);
        ReadSourceConstraintArguments("source_expression_declaration_arguments","declaration_id",SourceExpressionDeclarations.ContainsKey,SourceExpressionDeclarationArguments);
        ReadSourceConstraintSubstitutions();
        ReadSourceConstraintExtensions();
        ReadSourceExpressionTemporaries();
        ReadSourceExpressionAllocations();
        ReadSourceExpressionFolds();
        ReadSourceClassTemplates();
    }
    private void ReadSourceExpressionLongRelation(string table,string column,Dictionary<long,long> target,Func<long,bool> childExists)
    {
        using var q=database.Prepare($"SELECT expression_id,{column} FROM {table} ORDER BY expression_id");
        while(q.Read())if(!SourceExpressions.ContainsKey(q.Integer(0)) || !childExists(q.Integer(1)) || !target.TryAdd(q.Integer(0),q.Integer(1)))
            throw new InvalidDataException("Source expression relation lacks its exact typed owner: "+table);
    }
    private void ReadSourceExpressionBoolRelation(string table,string column,Dictionary<long,bool> target)
    {
        using var q=database.Prepare($"SELECT expression_id,{column} FROM {table} ORDER BY expression_id");
        while(q.Read())if(!SourceExpressions.ContainsKey(q.Integer(0)) || q.Integer(1) is not(0 or 1) || !target.TryAdd(q.Integer(0),q.Integer(1)==1))
            throw new InvalidDataException("Source expression relation lacks its original boolean fact: "+table);
    }
    private void ReadSourceConstraintDeclarations()
    {
        using var q=database.Prepare("SELECT id,kind,source_name,qualified_name,source_type_id,owner_source_type_id,template_prototype_declaration_id,has_template_origin,is_template_prototype,is_constexpr,formal_count,argument_count,definition_status,capture_proof,original_init_kind,original_dynamic_init_kind,initializer_kind,initializer_constant_id,initializer_expression_id,template_origin_kind,routine_kind,scope_kind FROM source_expression_declarations ORDER BY id");
        while(q.Read())
        {
            var value=new SourceExpressionDeclaration(q.Integer(0),q.Text(1),q.Text(2),q.Text(3),q.Integer(4),q.OptionalInteger(5),q.OptionalInteger(6),q.Integer(7)==1,q.Integer(8)==1,q.Integer(9)==1,q.Integer(10),q.Integer(11),q.OptionalInteger(14),q.OptionalInteger(15),q.Text(16),q.OptionalInteger(17),q.OptionalInteger(18),q.Text(19),q.Text(20),q.Text(21));
            if(value.Id<=0 || value.Kind is not("variable" or "routine") || value.Name.Length==0 || value.QualifiedName.Length==0 || !SourceTypes.ContainsKey(value.Type) ||
                value.OwnerType is long owner && !SourceTypes.ContainsKey(owner) || q.Integer(7) is not(0 or 1) || q.Integer(8) is not(0 or 1) || q.Integer(9) is not(0 or 1) ||
                value.TemplateOriginKind is not("none" or "variable-template" or "function-template" or "static-data-member" or "class-member-function") ||
                value.HasTemplate != (value.TemplateOriginKind!="none") || value.ScopeKind is not("nonlocal" or "function-parameter" or "function-local" or "namespace") ||
                (value.Kind=="variable"?value.RoutineKind!="not-routine":value.RoutineKind is not("function" or "constructor" or "destructor" or "operator")) ||
                value.FormalCount<0 || value.ArgumentCount<0 || q.Text(12)!="reference-only" || q.Text(13)!="original-expression-declaration" ||
                !SourceExpressionDeclarations.TryAdd(value.Id,value))throw new InvalidDataException("Source expression declaration must retain its original semantic-only signature");
        }
    }
    private void ReadSourceConstraintFormals(string table,string ownerColumn,Func<long,bool> ownerExists,Dictionary<long,List<SourceTemplateFormal>> target)
    {
        using var q=database.Prepare($"SELECT {ownerColumn},ordinal,kind,source_name,type_id,constant_id,template_id,default_type_id,default_constant_id,default_template_id,is_pack,is_abbreviated,source_file_id,line,column FROM {table} ORDER BY {ownerColumn},ordinal");
        while(q.Read())
        {
            var value=new SourceTemplateFormal(q.Integer(1),q.Text(2),q.Text(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8),q.OptionalInteger(9),q.Integer(10)==1,q.Integer(11)==1,q.OptionalInteger(12),q.Integer(13),q.Integer(14));
            if(!ownerExists(q.Integer(0)) || q.Integer(10) is not(0 or 1) || q.Integer(11) is not(0 or 1))throw new InvalidDataException("Source constraint formal lacks its owner");
            VerifySourceConstraintFormal(value);
            if(!target.TryGetValue(q.Integer(0),out var values))target[q.Integer(0)]=values=[];
            if(value.Ordinal!=values.Count)throw new InvalidDataException("Source constraint formal order is not contiguous");values.Add(value);
        }
    }
    private void VerifySourceConstraintFormal(SourceTemplateFormal formal)
    {
        if(formal.Line<0 || formal.Column<0 || !SourceTemplatePayloadValid(formal.Kind,formal.TypeId,formal.ConstantId,formal.TemplateId))
            throw new InvalidDataException("Source constraint formal lacks its exact typed payload");
        bool hasDefault=formal.DefaultTypeId is not null || formal.DefaultConstantId is not null || formal.DefaultTemplateId is not null;
        if(hasDefault && (formal.IsPack || !SourceTemplatePayloadValid(formal.Kind,formal.DefaultTypeId,formal.DefaultConstantId,formal.DefaultTemplateId)))
            throw new InvalidDataException("Source constraint formal default has a different semantic kind");
        if(formal.Kind=="type" && (!SourceDependentTypes.TryGetValue(Original(SourceTypeById(formal.TypeId!.Value)).Id,out var parameter) || parameter.Kind!="parameter" || parameter.IsPack!=formal.IsPack) ||
            formal.Kind=="constant" && (SourceSemanticConstants[formal.ConstantId!.Value].Kind!="parameter" || SourceSemanticConstants[formal.ConstantId.Value].IsPack!=formal.IsPack))
            throw new InvalidDataException("Source constraint formal must reference its original parameter declaration");
    }
    private void ReadSourceConstraintArguments(string table,string ownerColumn,Func<long,bool> ownerExists,Dictionary<long,List<SourceTemplateArgument>> target)
    {
        using var q=database.Prepare($"SELECT {ownerColumn},ordinal,kind,source_type_id,constant_id,template_id FROM {table} ORDER BY {ownerColumn},ordinal");
        while(q.Read())
        {
            var value=new SourceTemplateArgument(q.Integer(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5));
            if(!ownerExists(q.Integer(0)) || !SourceConstraintPayloadValid(value.Kind,value.TypeId,value.ConstantId,value.TemplateId))throw new InvalidDataException("Source constraint argument lacks its typed semantic value");
            if(!target.TryGetValue(q.Integer(0),out var values))target[q.Integer(0)]=values=[];
            if(value.Ordinal!=values.Count)throw new InvalidDataException("Source constraint argument order is not contiguous");values.Add(value);
        }
    }
    private void ReadSourceConstraintSubstitutions()
    {
        using(var q=database.Prepare("SELECT expression_id,substitution_ordinal,arguments_known_dependent,arguments_known_nondependent,adjust_coordinates,alias_deduction_guide,formal_count,argument_count FROM source_expression_substitutions ORDER BY expression_id,substitution_ordinal"))
            while(q.Read())
            {
                var value=new SourceExpressionSubstitution(q.Integer(2)==1,q.Integer(3)==1,q.Integer(4)==1,q.Integer(5)==1,q.Integer(6),q.Integer(7));
                if(!SourceExpressions.ContainsKey(q.Integer(0)) || q.Integer(1)!=SourceExpressionSubstitutions.Keys.Count(key=>key.Expression==q.Integer(0)) ||
                    q.Integer(2) is not(0 or 1) || q.Integer(3) is not(0 or 1) || q.Integer(4) is not(0 or 1) || q.Integer(5) is not(0 or 1) ||
                    value.Dependent && value.Nondependent || value.FormalCount<0 || value.ArgumentCount<0 ||
                    !SourceExpressionSubstitutions.TryAdd((q.Integer(0),q.Integer(1)),value))throw new InvalidDataException("Invalid original requires substitution map");
            }
        using(var q=database.Prepare("SELECT expression_id,substitution_ordinal,ordinal,kind,source_name,type_id,constant_id,template_id,default_type_id,default_constant_id,default_template_id,is_pack,is_abbreviated,source_file_id,line,column FROM source_expression_substitution_formals ORDER BY expression_id,substitution_ordinal,ordinal"))
            while(q.Read())
            {
                var key=(q.Integer(0),q.Integer(1));
                var value=new SourceTemplateFormal(q.Integer(2),q.Text(3),q.Text(4),q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8),q.OptionalInteger(9),q.OptionalInteger(10),q.Integer(11)==1,q.Integer(12)==1,q.OptionalInteger(13),q.Integer(14),q.Integer(15));
                if(!SourceExpressionSubstitutions.ContainsKey(key) || q.Integer(11) is not(0 or 1) || q.Integer(12) is not(0 or 1))throw new InvalidDataException("Source substitution formal lacks its exact binder");
                VerifySourceConstraintFormal(value);
                if(!SourceExpressionSubstitutionFormals.TryGetValue(key,out var values))SourceExpressionSubstitutionFormals[key]=values=[];
                if(value.Ordinal!=values.Count)throw new InvalidDataException("Source substitution formal order is not contiguous");values.Add(value);
            }
        using(var q=database.Prepare("SELECT expression_id,substitution_ordinal,ordinal,kind,source_type_id,constant_id,template_id FROM source_expression_substitution_arguments ORDER BY expression_id,substitution_ordinal,ordinal"))
            while(q.Read())
            {
                var key=(q.Integer(0),q.Integer(1));var value=new SourceTemplateArgument(q.Integer(2),q.Text(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6));
                if(!SourceExpressionSubstitutions.ContainsKey(key) || !SourceConstraintPayloadValid(value.Kind,value.TypeId,value.ConstantId,value.TemplateId))throw new InvalidDataException("Source substitution argument lacks exact typed value");
                if(!SourceExpressionSubstitutionArguments.TryGetValue(key,out var values))SourceExpressionSubstitutionArguments[key]=values=[];
                if(value.Ordinal!=values.Count)throw new InvalidDataException("Source substitution argument order is not contiguous");values.Add(value);
            }
    }
}
