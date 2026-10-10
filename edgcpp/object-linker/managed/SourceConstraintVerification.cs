using System.Globalization;

namespace DotObj;

internal sealed partial class ObjectFile
{
    // Semantic contracts contain no object-local row IDs. The linker compares
    // these for repeated original concept and constrained parameter identities.
    public Dictionary<string,string> SourceConstraintContracts { get; } = new(StringComparer.Ordinal);

    private void VerifySourceConstraints()
    {
        VerifySourceParameterBinders();
        VerifySourceFunctionParameterDeclarations();
        VerifySourceConstraintExtensions();
        VerifySourceClassTemplates();
        VerifySourceAllocationPresence();
        VerifySourceFoldPresence();
        foreach(var expression in SourceExpressions.Values)
            if((expression.Kind=="temporary-initialization")!=SourceExpressionTemporaries.ContainsKey(expression.Id))throw new InvalidDataException("Original temporary expression is missing its initialization provenance");
        foreach(var value in SourceExpressions.Values)
        {
            var edges=SourceExpressionEdges.GetValueOrDefault(value.Id,[]);
            if(edges.Count!=value.EdgeCount || SourceExpressionParameters.GetValueOrDefault(value.Id,[]).Count!=value.ParameterCount ||
                (value.Kind=="dependent-function-reference"?SourceDependentFunctionArguments:SourceConceptArguments).GetValueOrDefault(value.Id,[]).Count!=value.ArgumentCount ||
                SourceExpressionSubstitutions.Keys.Count(key=>key.Expression==value.Id)!=value.SubstitutionCount ||
                value.Kind!="requires" && (value.ParameterCount!=0 || value.SubstitutionCount!=0) || value.Kind is not("concept-id" or "dependent-function-reference") && value.ArgumentCount!=0)
                throw new InvalidDataException("Original source expression lists are incomplete or have the wrong owner");
            bool query=value.Kind is "sizeof" or "alignof" or "datasizeof";
            if(SourceConceptUses.ContainsKey(value.Id)!=(value.Kind=="concept-id") ||
                SourceDependentFunctionLookups.ContainsKey(value.Id)!=(value.Kind=="dependent-function-reference") ||
                SourceExpressionParameterReferences.ContainsKey(value.Id)!=(value.Kind=="parameter-reference") ||
                SourceExpressionOperations.ContainsKey(value.Id)!=(value.Kind is "operation" or "builtin-operation") ||
                SourceExpressionConstants.ContainsKey(value.Id)!=(value.Kind=="constant") ||
                SourceExpressionRequirements.ContainsKey(value.Id)!=(value.Kind=="compound-requirement") ||
                SourceExpressionQueries.ContainsKey(value.Id)!=query ||
                SourceExpressionDeclarationReferences.ContainsKey(value.Id)!=(value.Kind is "variable-reference" or "routine-reference") ||
                SourceExpressionTypeOperands.ContainsKey(value.Id)!=(value.Kind=="type-operand" || query && edges.Count==0) ||
                SourceExpressionFlags.ContainsKey(value.Id) && value.Kind!="operation")
                throw new InvalidDataException("Original source expression has mismatched semantic payloads");
            void Shape(params (string Role,int Minimum,int Maximum)[] roles)
            {
                if(edges.Any(edge=>!roles.Any(role=>role.Role==edge.Role)) || roles.Any(role=>edges.Count(edge=>edge.Role==role.Role)<role.Minimum || edges.Count(edge=>edge.Role==role.Role)>role.Maximum))
                    throw new InvalidDataException("Original source expression operands do not match "+value.Kind);
            }
            switch(value.Kind)
            {
                case "concept-id":
                    Shape();var use=SourceConceptUses[value.Id];
                    var conceptFormals=SourceConceptFormals.GetValueOrDefault(use.Concept,[]);
                    if(use.TypeConstraint)
                    {
                        bool declarationBinding=SourceTypeConstraints.Values.Contains(value.Id);
                        bool requirement=SourceExpressionEdges.Any(pair=>SourceExpressionRequirements.ContainsKey(pair.Key) && pair.Value.Any(edge=>edge.Role=="constraint" && edge.Child==value.Id));
                        if(declarationBinding==requirement || conceptFormals.Count==0 || conceptFormals[0].Kind!="type" || conceptFormals[0].IsPack)
                            throw new InvalidDataException("Original type constraint lost its implicit first-type binding owner");
                        conceptFormals=conceptFormals.Skip(1).ToList();
                    }
                    MatchSourceConstraintArguments(conceptFormals,SourceConceptArguments.GetValueOrDefault(value.Id,[]),"concept",value.Id,0);
                    RequireSourceBoolean(value);break;
                case "dependent-function-reference":Shape();break;
                case "requires":Shape(("requirement",1,int.MaxValue));RequireSourceBoolean(value);break;
                case "compound-requirement":
                    Shape(("expression",1,1),("constraint",0,1));
                    foreach(var edge in edges.Where(edge=>edge.Role=="constraint"))
                        if(!SourceConceptUses.TryGetValue(edge.Child,out var constraint) || !constraint.TypeConstraint)
                            throw new InvalidDataException("Compound requirement return constraint must be its original type constraint");
                    break;
                case "nested-requirement":Shape(("constraint",1,1));RequireSourceConstraintOperand(SourceExpressions[edges.Single().Child]);break;
                case "expression-constant":Shape(("expression",1,1));break;
                case "field-reference":Shape();VerifySourceFieldReference(value);break;
                case "braced-initializer-list":Shape(("element",0,int.MaxValue));break;
                case "fold-expression":Shape(("operand",1,int.MaxValue));break;
                case "allocation-expression":VerifySourceExpressionAllocation(value,edges);break;
                case "temporary-initialization":VerifySourceExpressionTemporary(value,edges);break;
                case "parameter-reference":
                    Shape();VerifySourceParameterReference(value.Id);
                    break;
                case "variable-reference":case "routine-reference":
                    Shape();var declaration=SourceExpressionDeclarations[SourceExpressionDeclarationReferences[value.Id]];
                    if(declaration.Kind+("-reference")!=value.Kind || (value.OriginalLvalueTypeId??value.SourceTypeId) is not long sourceType ||
                        SourceTypeSignature(declaration.Kind=="routine" && Original(SourceTypeById(sourceType)).Kind=="pointer" ? Original(SourceTypeById(sourceType)).UnderlyingTypeId!.Value : sourceType)!=SourceTypeSignature(declaration.Type))
                        throw new InvalidDataException("Source declaration reference has a different original semantic signature");
                    break;
                case "constant":Shape();
                    if((value.OriginalLvalueTypeId??value.SourceTypeId) is not long constantType || SourceTypeSignature(constantType)!=SourceTypeSignature(SourceSemanticConstants[SourceExpressionConstants[value.Id]].SourceTypeId))
                        throw new InvalidDataException("Source constant expression lost its original result type");
                    break;
                case "type-operand":Shape();break;
                case "sizeof":case "alignof":case "datasizeof":
                    Shape(("operand",0,1));
                    if(SourceExpressionQueries[value.Id] && value.Kind!="alignof")throw new InvalidDataException("Source query standard-alignof flag belongs only to alignof");
                    break;
                case "builtin-operation":
                    Shape(("operand",1,int.MaxValue));var builtin=SourceExpressionOperations[value.Id];var trait=SourceBuiltinTrait(builtin.Opcode);
                    if(builtin.Name!=trait.Name || builtin.OriginalTypeKind is not null || trait.Arity>=0 && edges.Count!=trait.Arity ||
                        edges.Any(edge=>SourceExpressions[edge.Child].Kind!="type-operand"))throw new InvalidDataException("Source builtin trait lost exact opcode or typed operand roles");
                    break;
                case "operation":
                    Shape(("operand",0,int.MaxValue));var operation=SourceExpressionOperations[value.Id];
                    if(operation.OriginalTypeKind is null || operation.OriginalTypeKind<0 || !SourceOperationCatalog.Valid(operation.Opcode,operation.Name,edges.Count))
                        throw new InvalidDataException("Source operation opcode, name and operand count do not agree");
                    VerifySourceOperationFlags(value.Id,operation.Name);break;
                default:throw new InvalidDataException("Unknown owned source expression family: "+value.Kind);
            }
            if(SourceExpressionBinders.TryGetValue(value.Id,out long binder) &&
                (!SourceParameterBinders.ContainsKey(binder) || SourceParameterBinders[binder].RequiresExpression==value.Id))
                throw new InvalidDataException("Source expression lexical owner is not an enclosing original parameter scope");
            foreach(var edge in edges)
            {
                long expected=value.Kind=="requires"?SourceRequiresBodyBinders[value.Id]:SourceExpressionBinders.GetValueOrDefault(value.Id);
                if(SourceExpressionBinders.GetValueOrDefault(edge.Child)!=expected)
                    throw new InvalidDataException("Source operand changed its original lexical parameter scope");
            }
        }
        foreach(var concept in SourceConcepts.Values)
        {
            var formals=SourceConceptFormals.GetValueOrDefault(concept.Id,[]);
            if(formals.Count!=concept.ParameterCount)throw new InvalidDataException("Source concept formal list is incomplete");
            RequireSourceConstraintOperand(SourceExpressions[concept.Definition]);
            if(concept.HeaderConstraint is long header)RequireSourceConstraintOperand(SourceExpressions[header]);
        }
        foreach(var declaration in SourceExpressionDeclarations.Values)
        {
            var formals=SourceExpressionDeclarationFormals.GetValueOrDefault(declaration.Id,[]);
            var arguments=SourceExpressionDeclarationArguments.GetValueOrDefault(declaration.Id,[]);
            if(formals.Count!=declaration.FormalCount || arguments.Count!=declaration.ArgumentCount ||
                !declaration.HasTemplate && (declaration.IsPrototype || declaration.TemplatePrototype is not null || formals.Count!=0 || arguments.Count!=0) ||
                declaration.HasTemplate && declaration.TemplateOriginKind is not("static-data-member" or "class-member-function") && (declaration.IsPrototype== (declaration.TemplatePrototype is not null)) ||
                (Original(SourceTypeById(declaration.Type)).Kind=="routine")!=(declaration.Kind=="routine"))
                throw new InvalidDataException("Source declaration template/signature ownership is incomplete");
            VerifySourceConstraintInitializer(declaration);
            if(declaration.TemplateOriginKind=="static-data-member")
                VerifySourceStaticMemberDeclaration(declaration,formals,arguments);
            else if(declaration.TemplateOriginKind=="class-member-function")
                VerifySourceClassMemberRoutine(declaration,formals,arguments);
            else if(declaration.TemplatePrototype is long prototype)
            {
                if(!SourceExpressionDeclarations.TryGetValue(prototype,out var original) || !original.IsPrototype || !original.HasTemplate || original.TemplatePrototype is not null ||
                    original.Kind!=declaration.Kind || original.Name!=declaration.Name || original.QualifiedName!=declaration.QualifiedName)
                    throw new InvalidDataException("Source declaration instantiation lacks its exact original template prototype");
                MatchSourceConstraintArguments(SourceExpressionDeclarationFormals.GetValueOrDefault(prototype,[]),arguments,"declaration",declaration.Id,0);
            }
        }
        foreach(var pair in SourceExpressionSubstitutions)
        {
            var formals=SourceExpressionSubstitutionFormals.GetValueOrDefault(pair.Key,[]);var arguments=SourceExpressionSubstitutionArguments.GetValueOrDefault(pair.Key,[]);
            if(SourceExpressions[pair.Key.Expression].Kind!="requires" || formals.Count!=pair.Value.FormalCount || arguments.Count!=pair.Value.ArgumentCount)
                throw new InvalidDataException("Original requires substitution lists are incomplete");
            MatchSourceConstraintArguments(formals,arguments,"substitution",pair.Key.Expression,pair.Key.Ordinal);
        }
        // Presence marker is read independently of the relation, preventing an
        // omitted root from silently turning a constrained parameter into T.
        using(var q=database.Prepare("SELECT type_id,has_type_constraint FROM source_dependent_types ORDER BY type_id"))
            while(q.Read())
                if(q.Integer(1) is not(0 or 1) || SourceTypeConstraints.ContainsKey(q.Integer(0))!=(q.Integer(1)==1))
                    throw new InvalidDataException("Original constrained type lost its required source expression");
        foreach(var pair in SourceTypeConstraints)
        {
            if(!SourceConceptUses.TryGetValue(pair.Value,out var use) || !use.TypeConstraint)
                throw new InvalidDataException("Original type constraint is not an owned concept-id");
            // EDG retains only explicit arguments. The declared parameter is
            // the implicit first type via this exact owned relation; compound
            // requirements instead supply the type of their result expression.
            if(!SourceDependentTypes.TryGetValue(pair.Key,out var parameter) || parameter.Kind is not("parameter" or "auto-placeholder"))
                throw new InvalidDataException("Original type constraint does not bind its declared parameter");
        }
        var active=new HashSet<long>();var done=new HashSet<long>();
        void Visit(long id)
        {
            if(done.Contains(id))return;if(!active.Add(id))throw new InvalidDataException("Original source expression operand cycle");
            foreach(var edge in SourceExpressionEdges.GetValueOrDefault(id,[]))Visit(edge.Child);
            active.Remove(id);done.Add(id);
        }
        foreach(long id in SourceExpressions.Keys)Visit(id);
        VerifySourceConstantExpressionDeclarations();
        foreach(var concept in SourceConcepts.Values)
            AddSourceConstraintContract("concept:"+concept.QualifiedName,SourceConceptIdentity(concept.Id,new HashSet<long>()));
        foreach(var pair in SourceTypeConstraints.Where(pair=>SourceDependentTypes[pair.Key].Kind=="parameter"))
            AddSourceConstraintContract("parameter:"+SourceDependentTypes[pair.Key].IdentityKey,SourceExpressionIdentity(pair.Value,new HashSet<long>(),new HashSet<long>()));
    }
    private void VerifySourceConstraintInitializer(SourceExpressionDeclaration value)
    {
        bool variable=value.Kind=="variable";
        if(variable!=(value.OriginalInitKind is not null) || !variable && (value.InitializerKind!="not-variable" || value.OriginalDynamicInitKind is not null) ||
            (value.OriginalInitKind==2)!=(value.OriginalDynamicInitKind is not null) ||
            (value.InitializerKind=="constant")!=(value.InitializerConstant is not null) ||
            (value.InitializerKind=="expression")!=(value.InitializerExpression is not null) ||
            value.InitializerConstant is long constant && !SourceSemanticConstants.ContainsKey(constant) ||
            value.InitializerExpression is long expression && !SourceExpressions.ContainsKey(expression) ||
            value.OriginalInitKind==0 && value.InitializerKind!="none" || value.OriginalInitKind==3 && value.InitializerKind!="zero" ||
            value.OriginalInitKind==1 && value.InitializerKind is not("constant" or "expression") ||
            value.OriginalDynamicInitKind is long dynamicKind && (dynamicKind switch {
                1=>value.InitializerKind!="zero",2=>value.InitializerKind is not("constant" or "expression"),3=>value.InitializerKind!="expression",_=>true}) ||
            variable && value.InitializerKind is "not-variable" || value.OriginalInitKind is 4 or 5)
            throw new InvalidDataException("Source declaration initializer lost its exact original kind or semantic value");
    }
    private void RequireSourceConstraintOperand(SourceExpression expression)
    {
        // A concept definition or nested constraint is checked for bool after
        // substitution. Its original dependent result is deliberately retained.
        if(expression.SourceTypeId is long type && Original(SourceTypeById(type)).Kind=="dependent")return;
        RequireSourceBoolean(expression);
    }
    private void RequireSourceBoolean(SourceExpression expression)
    {
        if(expression.SourceTypeId is not long type || Original(SourceTypeById(type)).Kind!="bool")
            throw new InvalidDataException("Original constraint must retain its boolean semantic result type");
    }
    private void MatchSourceConstraintArguments(List<SourceTemplateFormal> formals,List<SourceTemplateArgument> arguments,string kind,long owner,long substitution)
    {
        int slot=0;
        foreach(var formal in formals)
        {
            if(formal.IsPack)
            {
                bool boundary=slot<arguments.Count && arguments[slot].Kind=="pack-boundary";
                if(boundary)slot++;
                // The original boundary plus is_pack_element identifies even
                // empty/nonterminal expansions. Nonreal lists instead carry a
                // single is_pack argument for the original formal pack.
                if(boundary)
                {
                    while(slot<arguments.Count && SourceConstraintArgumentDetails[(kind,owner,substitution,arguments[slot].Ordinal)].PackElement)
                        if(arguments[slot++].Kind!=formal.Kind)throw new InvalidDataException("Source constraint pack argument kind differs from its formal");
                }
                else if(slot<arguments.Count && SourceConstraintArgumentDetails[(kind,owner,substitution,arguments[slot].Ordinal)].Pack)
                {
                    if(arguments[slot++].Kind!=formal.Kind)throw new InvalidDataException("Source constraint dependent pack kind differs from its formal");
                }
                else throw new InvalidDataException("Source constraint pack lacks its original expansion boundary or dependent pack argument");
            }
            else if(slot>=arguments.Count || arguments[slot].Kind!=formal.Kind || SourceConstraintArgumentDetails[(kind,owner,substitution,arguments[slot].Ordinal)].PackElement)
                throw new InvalidDataException("Source constraint arguments do not match original formal kinds");
            else slot++;
        }
        if(slot!=arguments.Count)throw new InvalidDataException("Source constraint has excess original arguments");
    }
    private static readonly HashSet<string> SourceOperationFlagNames=["returns-glvalue","reinterpret-cast","reinterpret-semantics","const-cast","reference-cast","rvalue-reference-cast","implicit-member-naming","implicit-cast-step","conversion-call","adl-suppressed","qualified-function-name","adl-only","address-of-overload-set","operator-call-syntax","pointer-second","virtual-call","left-to-right","right-to-left","consteval-call","gnu-omitted-middle","deferred-ampersand","suppress-top-level-parens"];
    private void VerifySourceOperationFlags(long id,string name)
    {
        var flags=SourceExpressionFlags.GetValueOrDefault(id,[]);
        if(flags.Any(flag=>!SourceOperationFlagNames.Contains(flag)) || flags.Contains("left-to-right") && flags.Contains("right-to-left") ||
            flags.Contains("gnu-omitted-middle") && name!="eok_question" ||
            flags.Contains("rvalue-reference-cast") && !flags.Contains("reference-cast"))
            throw new InvalidDataException("Source operation contains inconsistent original semantic flags");
    }
    private void AddSourceConstraintContract(string key,string contract)
    {
        if(SourceConstraintContracts.TryGetValue(key,out string? old) && old!=contract)throw new InvalidDataException("Original constraint declaration has conflicting semantic definitions");
        SourceConstraintContracts[key]=contract;
    }
    private readonly HashSet<long> sourceDeclarationIdentityActive=[];
    private readonly Dictionary<long,string> sourceConceptIdentityCache=[];
    private readonly Dictionary<long,string> sourceExpressionIdentityCache=[];
    private static string SourceGraphHash(string value)=>Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
    private string SourceConceptIdentity(long id,HashSet<long> activeConcepts)
    {
        if(sourceConceptIdentityCache.TryGetValue(id,out string? cached))return cached;
        if(!activeConcepts.Add(id))throw new InvalidDataException("Original concept definition cycle");
        var concept=SourceConcepts[id];string value=SourceIdentityFrame(concept.QualifiedName)+
            string.Concat(SourceConceptFormals.GetValueOrDefault(id,[]).Select(SourceConstraintFormalIdentity))+
            SourceIdentityFrame(SourceExpressionIdentity(concept.Definition,[],activeConcepts))+
            (concept.HeaderConstraint is long header?SourceIdentityFrame(SourceExpressionIdentity(header,[],activeConcepts)):"0:");
        activeConcepts.Remove(id);return sourceConceptIdentityCache[id]=SourceGraphHash(value);
    }
    private string SourceConstraintFormalIdentity(SourceTemplateFormal formal)=>SourceIdentityFrame(formal.Kind)+
        SourceIdentityFrame(SourceConstraintArgumentIdentity(new(formal.Ordinal,formal.Kind,formal.TypeId,formal.ConstantId,formal.TemplateId)))+
        (formal.IsPack?"p":"-")+(formal.IsAbbreviated?"a":"-")+
        SourceIdentityFrame(formal.DefaultTypeId is not null || formal.DefaultConstantId is not null || formal.DefaultTemplateId is not null?
            SourceConstraintArgumentIdentity(new(0,formal.Kind,formal.DefaultTypeId,formal.DefaultConstantId,formal.DefaultTemplateId)):"");
    private string SourceConstraintArgumentIdentity(SourceTemplateArgument argument)=>argument.Kind switch
    {
        "type"=>"t"+SourceTypeSignature(argument.TypeId!.Value),
        "constant"=>"c"+SourceConstantIdentity(argument.ConstantId!.Value,0),
        "template"=>"m"+SourceTemplateIdentity(argument.TemplateId!.Value,0),
        _=>throw new InvalidDataException("Unknown source constraint argument family")
    };
    private string SourceExpressionIdentity(long id,HashSet<long> active,HashSet<long> activeConcepts)
    {
        if(sourceExpressionIdentityCache.TryGetValue(id,out string? cached))return cached;
        if(active.Count>256 || !active.Add(id))throw new InvalidDataException("Source expression identity cycle/depth");
        var value=SourceExpressions[id];string result=SourceIdentityFrame(value.Kind)+SourceIdentityFrame(value.SourceTypeId is long type?SourceTypeSignature(type):"")+SourceIdentityFrame(value.Category);
        result+=SourceIdentityFrame(value.OriginalLvalueTypeId is long original?SourceTypeSignature(original):"")+(value.Parenthesized?"1":"0")+(value.CompilerGenerated?"1":"0")+(value.VolatileFetch?"1":"0")+(value.PackExpansion?"1":"0");
        if(SourceConceptUses.TryGetValue(id,out var use))result+=SourceIdentityFrame(SourceConceptIdentity(use.Concept,activeConcepts))+(use.TypeConstraint?"1":"0")+
            string.Concat(SourceConceptArguments.GetValueOrDefault(id,[]).Select(argument=>SourceIdentityFrame(SourceConstraintArgumentDetailsIdentity("concept",id,0,argument))));
        if(SourceDependentFunctionLookups.ContainsKey(id))result+=SourceIdentityFrame(SourceDependentFunctionIdentity(id,activeConcepts));
        if(SourceExpressionOperations.TryGetValue(id,out var operation))result+=SourceIdentityFrame(operation.Opcode.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(operation.Name)+SourceIdentityFrame(operation.OriginalTypeKind?.ToString(CultureInfo.InvariantCulture)??"");
        result+=string.Concat(SourceExpressionFlags.GetValueOrDefault(id,[]).Order(StringComparer.Ordinal).Select(SourceIdentityFrame));
        if(SourceExpressionTypeOperands.TryGetValue(id,out long operand))result+=SourceIdentityFrame(SourceTypeSignature(operand));
        if(SourceExpressionConstants.TryGetValue(id,out long constant))result+=SourceIdentityFrame(SourceConstantIdentity(constant,0));
        if(SourceExpressionParameterReferences.TryGetValue(id,out var reference))result+=SourceIdentityFrame(reference.Ordinal.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(reference.Levels.ToString(CultureInfo.InvariantCulture))+SourceIdentityFrame(SourceParameterReferenceContextKey(id));
        if(SourceExpressionRequirements.TryGetValue(id,out bool noexceptRequired))result+=noexceptRequired?"1":"0";
        if(SourceExpressionQueries.TryGetValue(id,out bool standard))result+=standard?"1":"0";
        foreach(var parameter in SourceExpressionParameters.GetValueOrDefault(id,[]))result+=SourceIdentityFrame(SourceTypeSignature(parameter.Type))+SourceIdentityFrame(parameter.DeclaredType is long declared?SourceTypeSignature(declared):"")+SourceIdentityFrame(parameter.Qualifiers.ToString(CultureInfo.InvariantCulture))+(parameter.Pack?"1":"0");
        if(SourceExpressionDeclarationReferences.TryGetValue(id,out long declaration))result+=SourceIdentityFrame(SourceConstraintDeclarationIdentity(declaration,sourceDeclarationIdentityActive,activeConcepts));
        foreach(var substitution in SourceExpressionSubstitutions.Where(pair=>pair.Key.Expression==id).OrderBy(pair=>pair.Key.Ordinal))
        {
            var facts=substitution.Value;result+=(facts.Dependent?"1":"0")+(facts.Nondependent?"1":"0")+(facts.AdjustCoordinates?"1":"0")+(facts.AliasDeductionGuide?"1":"0")+
                string.Concat(SourceExpressionSubstitutionFormals.GetValueOrDefault(substitution.Key,[]).Select(SourceConstraintFormalIdentity))+
                string.Concat(SourceExpressionSubstitutionArguments.GetValueOrDefault(substitution.Key,[]).Select(argument=>SourceIdentityFrame(SourceConstraintArgumentDetailsIdentity("substitution",id,substitution.Key.Ordinal,argument))));
        }
        if(SourceExpressionFieldReferences.ContainsKey(id))result+="H"+SourceIdentityFrame(SourceFieldReferenceIdentity(id,0));
        if(SourceExpressionFolds.ContainsKey(id))result+="J"+SourceIdentityFrame(SourceFoldIdentity(id));
        if(SourceExpressionAllocations.ContainsKey(id))result+="B"+SourceIdentityFrame(SourceAllocationIdentity(id,0));
        if(SourceExpressionTemporaries.ContainsKey(id))result+="I"+SourceIdentityFrame(SourceTemporaryIdentity(id,0));
        foreach(var edge in SourceExpressionEdges.GetValueOrDefault(id,[]).OrderBy(edge=>edge.Role,StringComparer.Ordinal).ThenBy(edge=>edge.Ordinal))
            result+=SourceIdentityFrame(edge.Role)+SourceIdentityFrame(SourceExpressionIdentity(edge.Child,active,activeConcepts));
        active.Remove(id);return sourceExpressionIdentityCache[id]=SourceGraphHash(result);
    }
    private void VerifySourceStaticMemberDeclaration(SourceExpressionDeclaration value,List<SourceTemplateFormal> formals,List<SourceTemplateArgument> arguments)
    {
        if(value.Kind!="variable" || value.OwnerType is not long owner || value.TemplatePrototype is not long primary ||
            !SourceExpressionDeclarations.TryGetValue(primary,out var original) || original.Kind!="variable" ||
            original.TemplateOriginKind!="static-data-member" || original.TemplatePrototype!=original.Id ||
            original.OwnerType is not long originalOwner || original.Name!=value.Name || original.QualifiedName!=value.QualifiedName ||
            formals.Count!=0 || arguments.Count!=0)
            throw new InvalidDataException("Static template member lacks its exact original declaration and enclosing class");
        var type=Original(SourceTypeById(owner));var parent=Original(SourceTypeById(originalOwner));
        if(type.Kind!="record" || parent.Kind!="record" ||
            type.Id!=parent.Id && (!SourceRecordInstantiations.TryGetValue(SourceRecordsByType[type.Id].Id,out var instance) ||
                instance.PrototypeSourceRecordId!=SourceRecordsByType[parent.Id].Id))
            throw new InvalidDataException("Static template member owner does not instantiate its original class");
    }
    private void VerifySourceClassMemberRoutine(SourceExpressionDeclaration value,List<SourceTemplateFormal> formals,List<SourceTemplateArgument> arguments)
    {
        if(value.Kind!="routine" || value.OwnerType is not long owner || value.TemplatePrototype is not long primary ||
            !SourceExpressionDeclarations.TryGetValue(primary,out var original) || original.Kind!="routine" ||
            original.TemplateOriginKind!="class-member-function" || original.TemplatePrototype!=original.Id ||
            original.RoutineKind!=value.RoutineKind ||
            original.OwnerType is not long originalOwner || original.Name!=value.Name || original.QualifiedName!=value.QualifiedName ||
            formals.Count!=0 || arguments.Count!=0)
            throw new InvalidDataException("Class member routine lacks its exact original declaration and enclosing class");
        var type=Original(SourceTypeById(owner));var parent=Original(SourceTypeById(originalOwner));
        if(type.Kind!="record" || parent.Kind!="record" ||
            type.Id!=parent.Id && (!SourceRecordInstantiations.TryGetValue(SourceRecordsByType[type.Id].Id,out var instance) ||
                instance.PrototypeSourceRecordId!=SourceRecordsByType[parent.Id].Id))
            throw new InvalidDataException("Class member routine owner does not instantiate its original class");
    }
    private string SourceConstraintDeclarationIdentity(long id,HashSet<long> active,HashSet<long> activeConcepts)
    {
        if(active.Count>128 || !active.Add(id))throw new InvalidDataException("Source declaration template prototype cycle/depth");
        var value=SourceExpressionDeclarations[id];string result=SourceIdentityFrame(value.Kind)+SourceIdentityFrame(value.QualifiedName)+SourceIdentityFrame(SourceTypeSignature(value.Type))+
            SourceIdentityFrame(value.OwnerType is long owner?SourceTypeSignature(owner):"")+(value.Constexpr?"1":"0")+(value.HasTemplate?"1":"0")+(value.IsPrototype?"1":"0")+SourceIdentityFrame(value.TemplateOriginKind)+SourceIdentityFrame(value.RoutineKind)+SourceIdentityFrame(SourceDeclarationOperators.TryGetValue(id,out long originalOperator)?originalOperator.ToString(CultureInfo.InvariantCulture):"")+
            string.Concat(SourceExpressionDeclarationFormals.GetValueOrDefault(id,[]).Select(SourceConstraintFormalIdentity))+
            string.Concat(SourceExpressionDeclarationArguments.GetValueOrDefault(id,[]).Select(argument=>SourceIdentityFrame(SourceConstraintArgumentDetailsIdentity("declaration",id,0,argument))));
        if(SourceFunctionParameterDeclarations.TryGetValue(id,out var parameter))result+="B"+SourceIdentityFrame(SourceConstraintDeclarationIdentity(parameter.Routine,active,activeConcepts))+SourceIdentityFrame(parameter.Ordinal.ToString(CultureInfo.InvariantCulture));
        if(value.TemplatePrototype is long prototype && prototype!=id)result+=SourceIdentityFrame(SourceConstraintDeclarationIdentity(prototype,active,activeConcepts));
        result+=SourceIdentityFrame(value.InitializerKind)+SourceIdentityFrame(value.OriginalInitKind?.ToString(CultureInfo.InvariantCulture)??"")+SourceIdentityFrame(value.OriginalDynamicInitKind?.ToString(CultureInfo.InvariantCulture)??"");
        if(value.InitializerConstant is long constant)result+=SourceIdentityFrame(SourceConstantIdentity(constant,0));
        if(value.InitializerExpression is long expression)result+=SourceIdentityFrame(SourceExpressionIdentity(expression,[],activeConcepts));
        active.Remove(id);return result;
    }
}
