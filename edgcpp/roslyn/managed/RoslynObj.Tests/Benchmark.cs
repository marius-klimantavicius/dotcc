using RoslynObj;
using System.Diagnostics;

internal static class Benchmark
{
    internal static void Run(IEnumerable<string> paths)
    {
        Console.WriteLine("object\tpass\tentities\toperations\tcommands\trows\twall_ms\trss_bytes\tcached_rows\tcached_pages");
        foreach(var path in paths)
        {
            using var snapshot=RoslynObject.Open(path,new(){PageSize=128,CachedRowCount=4096,CachedPageCount=128});
            for(var pass=0;pass<2;pass++)
            {
                var before=snapshot.Statistics;
                var elapsed=Stopwatch.StartNew();
                var counts=Sweep(snapshot.TranslationUnit);
                var after=snapshot.Statistics;
                using var process=Process.GetCurrentProcess();process.Refresh();
                Console.WriteLine($"{Path.GetFileName(path)}\t{(pass==0?"cold":"warm")}\t{counts.Entities}\t{counts.Operations}\t{after.Commands-before.Commands}\t{after.RowsRead-before.RowsRead}\t{elapsed.Elapsed.TotalMilliseconds:F2}\t{process.WorkingSet64}\t{after.CachedRows}\t{after.CachedPages}");
                if(after.CachedRows>4096||after.CachedPages>128)throw new Exception("Public traversal exceeded its configured cache bounds.");
            }
        }
    }

    private static (long Entities,long Operations) Sweep(TranslationUnit unit)
    {
        long entities=0,operations=0;
        foreach(var entity in unit.Entities)
        {
            entities++;
            switch(entity)
            {
                case FunctionSymbol function:
                    _=function.Name;_=function.ReturnType.Kind;
                    foreach(var parameter in function.Parameters) { _=parameter.Name;_=parameter.Type.Kind;_=parameter.Role; }
                    if(function.Body is {} body)
                    {
                        foreach(var operation in body.DescendantOperations())
                        {
                            operations++;_=operation.Type?.Kind;
                            if(operation is CallOperation call) { _=call.TargetMethod?.Name;foreach(var argument in call.Arguments)_=argument.Value.Type.Kind; }
                        }
                        foreach(var region in body.ExceptionHandling.CleanupRegions)
                            foreach(var action in region.Actions) { _=action.Kind;_=action.TargetMethod?.Name;_=action.Storage?.Offset; }
                    }
                    break;
                case VariableSymbol variable:_=variable.Name;_=variable.Type.Kind;_=variable.ConstantInitializer?.Value.Kind;break;
                case FieldSymbol field:_=field.Name;_=field.Type.Kind;break;
                case CppType type:
                    _=type.Name;_=type.Kind;_=type.Size;_=type.Alignment;
                    if(type is RecordType && type.Layout is {} layout)foreach(var field in layout.Fields)_=field.Offset;
                    break;
                case Constant constant:_=constant.Type?.Kind;_=constant.Value.Kind;break;
                case ConstantValue value:_=value.Kind;break;
                case TemplateSymbol template:
                    _=template.Name;foreach(var parameter in template.Parameters) { _=parameter.Name;_=parameter.Kind; }
                    foreach(var instance in template.Instances)foreach(var argument in instance.Arguments)_=argument.Kind;
                    break;
                case Declaration declaration:_=unit.SemanticModel.GetSymbol(declaration);_=unit.SemanticModel.GetType(declaration);break;
                case SemanticNode node:_=node.Kind;break;
                case Symbol symbol:_=symbol.Name;break;
            }
        }
        return (entities,operations);
    }
}
