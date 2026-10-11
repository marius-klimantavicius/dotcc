using System.Globalization;
namespace DotObj;
internal sealed record SourceLocalScope(long Id,long Routine,long? Parent,string Kind,long Ordinal,long ChildCount,long VariableCount);
internal sealed record SourceLocalVariableDeclaration(long Scope,long Ordinal,long? DeclaredType);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceLocalScope> SourceLocalScopes {get;}=[];
    public Dictionary<long,SourceLocalVariableDeclaration> SourceLocalVariableDeclarations {get;}=[];
    private void ReadSourceLocalDeclarations()
    {
        using(var q=database.Prepare("SELECT id,routine_declaration_id,parent_id,kind,sibling_ordinal,child_scope_count,variable_count,line,column,capture_proof FROM source_local_scopes ORDER BY id"))
            while(q.Read())
            {
                var value=new SourceLocalScope(q.Integer(0),q.Integer(1),q.OptionalInteger(2),q.Text(3),q.Integer(4),q.Integer(5),q.Integer(6));
                if(value.Id<=0 || value.Parent==value.Id || value.Kind is not("function" or "block" or "condition") ||
                    (value.Kind=="function")!=(value.Parent is null) || value.Ordinal<0 || value.ChildCount<0 || value.VariableCount<0 ||
                    q.Integer(7)<0 || q.Integer(8)<0 || q.Text(9)!="original-local-scope-tree" ||
                    !SourceExpressionDeclarations.TryGetValue(value.Routine,out var routine) || routine.Kind!="routine" ||
                    routine.ScopeKind is not("nonlocal" or "namespace") || !SourceLocalScopes.TryAdd(value.Id,value))
                    throw new InvalidDataException("Original local scope lacks its exact routine/tree identity");
            }
        using(var q=database.Prepare("SELECT declaration_id,scope_id,declaration_ordinal,declared_source_type_id,capture_proof FROM source_local_variable_declarations ORDER BY declaration_id"))
            while(q.Read())
            {
                var value=new SourceLocalVariableDeclaration(q.Integer(1),q.Integer(2),q.OptionalInteger(3));
                if(!SourceExpressionDeclarations.ContainsKey(q.Integer(0)) || !SourceLocalScopes.ContainsKey(value.Scope) || value.Ordinal<0 ||
                    value.DeclaredType is long declared && !SourceTypes.ContainsKey(declared) || q.Text(4)!="original-nonstatic-variable-list" ||
                    !SourceLocalVariableDeclarations.TryAdd(q.Integer(0),value))
                    throw new InvalidDataException("Original local declaration lacks its exact nonstatic list membership");
            }
    }
    private void VerifySourceLocalDeclarations()
    {
        var roots=new HashSet<long>();var siblings=new HashSet<(long,long)>();var used=new HashSet<long>();
        foreach(var scope in SourceLocalScopes.Values)
        {
            if(scope.Parent is long parent)
            {
                if(!SourceLocalScopes.TryGetValue(parent,out var enclosing) || enclosing.Routine!=scope.Routine || scope.Ordinal>=enclosing.ChildCount ||
                    !siblings.Add((parent,scope.Ordinal)))throw new InvalidDataException("Original local scope differs from its parent routine or sibling list");
            }
            else if(scope.Ordinal!=0 || !roots.Add(scope.Routine))throw new InvalidDataException("Original function has conflicting local root scopes");
            var ancestors = new HashSet<long>();
            for (long? current=scope.Id; current is long id; current=SourceLocalScopes[id].Parent)
                if (!ancestors.Add(id)) throw new InvalidDataException("Original local scope parent graph is cyclic");
        }
        var ordinals=new HashSet<(long,long)>();
        foreach(var pair in SourceLocalVariableDeclarations)
        {
            var value=pair.Value;var declaration=SourceExpressionDeclarations[pair.Key];var scope=SourceLocalScopes[value.Scope];
            if(declaration.ScopeKind!="function-local" || declaration.Kind!="variable" || declaration.OwnerType is not null || declaration.HasTemplate ||
                declaration.OriginalInitKind is null || SourceFunctionParameterDeclarations.ContainsKey(pair.Key) ||
                value.Ordinal>=scope.VariableCount || !ordinals.Add((value.Scope,value.Ordinal)))
                throw new InvalidDataException("Original local variable differs from its owned scope/list declaration");
            for(long? current=value.Scope;current is long id;current=SourceLocalScopes[id].Parent)used.Add(id);
        }
        if(!used.SetEquals(SourceLocalScopes.Keys) || SourceExpressionDeclarations.Values.Any(value=>(value.ScopeKind=="function-local")!=SourceLocalVariableDeclarations.ContainsKey(value.Id)))
            throw new InvalidDataException("Original local scope/declaration ownership is missing or unreferenced");
    }
}
