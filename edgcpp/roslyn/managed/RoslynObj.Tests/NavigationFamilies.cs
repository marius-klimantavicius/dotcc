using RoslynObj;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

internal static class NavigationFamilies
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture is not ("templates.roslynobj" or "template_edges.roslynobj" or "types.roslynobj" or "type_edges.roslynobj" or "objects.roslynobj" or "common.roslynobj" or "retained.roslynobj" or "cleanup_semantics.roslynobj" or "module_interface.roslynobj" or "module_import.roslynobj"))return;
        var pending=new Queue<SemanticEntity>(unit.Entities);
        var seen=new HashSet<SemanticEntity>();
        var counts=new SortedDictionary<string,(int Links,int Collections,int Items)>();
        while(pending.TryDequeue(out var entity))
            if(seen.Add(entity))Read(entity,0);
        check(seen.Count>0&&counts.Count>0,"Actual small-family public navigation must reach captured entities and typed relationships without unavailable payload errors.");
        foreach(var (name,count) in counts)
            Console.WriteLine($"navigation:{name},links={count.Links},collections={count.Collections},items={count.Items}");

        void Read(object owner,int depth)
        {
            foreach(var property in owner.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance).Where(p=>p.GetIndexParameters().Length==0))
            {
                if(!IsNavigation(property.PropertyType))continue;
                object? value;
                try { value=property.GetValue(owner); }
                catch(TargetInvocationException error)
                {
                    throw new InvalidOperationException($"Public navigation {owner.GetType().Name}.{property.Name} failed.",error.InnerException??error);
                }
                if(value is null)continue;
                var name=Label(owner.GetType())+"."+property.Name;
                counts.TryGetValue(name,out var count);
                if(value is IEnumerable list)
                {
                    var items=0;
                    foreach(var item in list)
                    {
                        items++;
                        if(item is SemanticEntity target)pending.Enqueue(target);
                        else if(item is SemanticDetail&&depth<2)Read(item,depth+1);
                        else if(item is not null&&item.GetType().Namespace=="RoslynObj"&&depth==0)Read(item,1);
                    }
                    counts[name]=(count.Links,count.Collections+1,count.Items+items);
                }
                else
                {
                    counts[name]=(count.Links+1,count.Collections,count.Items);
                    if(value is SemanticEntity target)pending.Enqueue(target);
                    else if(value is SemanticDetail&&(owner is SemanticEntity||owner is not SemanticDetail)&&depth<2)Read(value,depth+1);
                    else if(owner is SemanticEntity&&depth==0)Read(value,1);
                }
            }
        }
    }

    private static bool IsNavigation(Type type) => type!=typeof(string)&&type!=typeof(SemanticKind)&&(typeof(SemanticEntity).IsAssignableFrom(type)||typeof(SemanticDetail).IsAssignableFrom(type)||typeof(IEnumerable).IsAssignableFrom(type)||type.Namespace=="RoslynObj"&&!type.IsValueType);
    private static string Label(Type type)
    {
        var name=type.Name.Split('`')[0];
        if(!name.EndsWith("Details",StringComparison.Ordinal))return "entity:"+name;
        return Regex.Replace(name[..^7],"(?<=[a-z0-9])([A-Z])","_$1").ToLowerInvariant();
    }
}
