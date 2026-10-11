using System.Globalization;

namespace DotObj;

internal sealed partial class ObjectFile
{
    private void VerifySourceConstraints()
    {
        VerifySourceParameterBinders();
        VerifySourceFunctionParameterDeclarations();
        VerifySourceLocalDeclarations();
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
                    Shape();break;
                case "dependent-function-reference":Shape();break;
                case "requires":Shape(("requirement",1,int.MaxValue));break;
                case "compound-requirement":
                    Shape(("expression",1,1),("constraint",0,1));
                    break;
                case "nested-requirement":Shape(("constraint",1,1));break;
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
                    if(declaration.Kind+("-reference")!=value.Kind)
                        throw new InvalidDataException("Source declaration reference has the wrong declaration kind");
                    break;
                case "constant":Shape();break;
                case "type-operand":Shape();break;
                case "sizeof":case "alignof":case "datasizeof":
                    Shape(("operand",0,1));
                    if(SourceExpressionQueries[value.Id] && value.Kind!="alignof")throw new InvalidDataException("Source query standard-alignof flag belongs only to alignof");
                    break;
                case "builtin-operation":
                    Shape(("operand",0,int.MaxValue));
                    if(SourceExpressionOperations[value.Id].OriginalTypeKind is not null)
                        throw new InvalidDataException("Source builtin operation has an invalid original-type-kind payload");
                    break;
                case "operation":
                    Shape(("operand",0,int.MaxValue));var operation=SourceExpressionOperations[value.Id];
                    if(operation.OriginalTypeKind is null || operation.OriginalTypeKind<0 || !SourceOperationCatalog.Valid(operation.Opcode,operation.Name))
                        throw new InvalidDataException("Source operation opcode and name do not agree");
                    VerifySourceOperationFlags(value.Id,operation.Name);break;
                default:throw new InvalidDataException("Unknown owned source expression family: "+value.Kind);
            }
            if(SourceExpressionBinders.TryGetValue(value.Id,out long binder) &&
                !SourceParameterBinders.ContainsKey(binder))
                throw new InvalidDataException("Source expression lexical owner is not an enclosing original parameter scope");

        }
        foreach(var concept in SourceConcepts.Values)
        {
            var formals=SourceConceptFormals.GetValueOrDefault(concept.Id,[]);
            if(formals.Count!=concept.ParameterCount)throw new InvalidDataException("Source concept formal list is incomplete");
        }
        foreach(var declaration in SourceExpressionDeclarations.Values)
        {
            var formals=SourceExpressionDeclarationFormals.GetValueOrDefault(declaration.Id,[]);
            var arguments=SourceExpressionDeclarationArguments.GetValueOrDefault(declaration.Id,[]);
            if(formals.Count!=declaration.FormalCount || arguments.Count!=declaration.ArgumentCount ||
                (Original(SourceTypeById(declaration.Type)).Kind=="routine")!=(declaration.Kind=="routine"))
                throw new InvalidDataException("Source declaration template/signature ownership is incomplete");
            VerifySourceConstraintInitializer(declaration);
            if(declaration.TemplatePrototype is long prototype &&
                (!SourceExpressionDeclarations.TryGetValue(prototype,out var original) || original.Kind!=declaration.Kind))
                throw new InvalidDataException("Source declaration prototype has an invalid declaration kind");

        }
        foreach(var pair in SourceExpressionSubstitutions)
        {
            var formals=SourceExpressionSubstitutionFormals.GetValueOrDefault(pair.Key,[]);var arguments=SourceExpressionSubstitutionArguments.GetValueOrDefault(pair.Key,[]);
            if(SourceExpressions[pair.Key.Expression].Kind!="requires" || formals.Count!=pair.Value.FormalCount || arguments.Count!=pair.Value.ArgumentCount)
                throw new InvalidDataException("Original requires substitution lists are incomplete");
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
        // Shared and cyclic original graphs are retained as recorded. No
        // cross-unit identities or concept satisfaction proofs are constructed.
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
    private static readonly HashSet<string> SourceOperationFlagNames=["returns-glvalue","reinterpret-cast","reinterpret-semantics","const-cast","reference-cast","rvalue-reference-cast","implicit-member-naming","implicit-cast-step","conversion-call","adl-suppressed","qualified-function-name","adl-only","address-of-overload-set","operator-call-syntax","pointer-second","virtual-call","left-to-right","right-to-left","consteval-call","gnu-omitted-middle","deferred-ampersand","suppress-top-level-parens"];
    private void VerifySourceOperationFlags(long id,string name)
    {
        var flags=SourceExpressionFlags.GetValueOrDefault(id,[]);
        if(flags.Any(flag=>!SourceOperationFlagNames.Contains(flag)))
            throw new InvalidDataException("Source operation contains inconsistent original semantic flags");
    }
}
