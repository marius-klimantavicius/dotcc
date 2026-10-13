using RoslynObj;

internal static class Concurrency
{
    internal static void Verify(string path,Action<bool,string> check)
    {
        using(var snapshot=RoslynObject.Open(path,new(){PageSize=3,CachedRowCount=32,CachedPageCount=2}))
        {
            var unit=snapshot.TranslationUnit;
            var functions=unit.Functions;
            var expected=Project(functions);
            var count=functions.Count;
            var retained=functions[0];
            var scalar=retained.Details.IsVirtual;
            var name=retained.Name;
            Parallel.For(0,8,new ParallelOptions{MaxDegreeOfParallelism=4},_ =>
            {
                var actual=Project(functions);
                check(actual.SequenceEqual(expected)&&functions.Count==count,"Parallel signature/body navigation must retain one immutable snapshot under row/page cache eviction.");
                check(retained.Name==name&&retained.Details.IsVirtual==scalar,"Retained object navigation must rehydrate exact rows after concurrent cache pressure.");
            });
            check(snapshot.Statistics.CachedRows<=32&&snapshot.Statistics.CachedPages<=2,"Concurrent reads must preserve configured row and page cache bounds.");
        }
        var disposed=RoslynObject.Open(path,new(){PageSize=2,CachedRowCount=32,CachedPageCount=2});
        var list=disposed.TranslationUnit.Functions;
        var function=list[0];
        var detail=function.Details;
        var detachedFlag=detail.IsVirtual;
        _=list.Count;
        using var enumerator=list.GetEnumerator();
        check(enumerator.MoveNext(),"A valid object must provide a routine before disposal.");
        disposed.Dispose();disposed.Dispose();
        check(detail.IsVirtual==detachedFlag,"Already materialized immutable scalar data must remain an ordinary value after owner disposal.");
        check(Disposed(()=>_=list.Count)&&Disposed(()=>_=list[0])&&Disposed(()=>_=enumerator.MoveNext()),"Cached counts, random navigation and resumed lazy enumeration must reject reads after owner disposal.");
        check(Disposed(()=>_=function.Name)&&Disposed(()=>_=function.Signature),"Database-backed properties of retained entities must reject navigation after owner disposal.");
        check(disposed.Statistics.CachedRows==0&&disposed.Statistics.CachedPages==0,"Disposal must release both bounded snapshot caches.");

        using var racing=RoslynObject.Open(path,new(){PageSize=3,CachedRowCount=32,CachedPageCount=2});
        var racingFunctions=racing.TranslationUnit.Functions;
        using var readersStarted=new CountdownEvent(4);
        var errors=new System.Collections.Concurrent.ConcurrentQueue<Exception>();
        var readers=Enumerable.Range(0,4).Select(readerIndex=>Task.Run(()=>
        {
            try
            {
                try { _=racingFunctions[0].Name; }
                finally { readersStarted.Signal(); }
                while(true)_=Project(racingFunctions);
            }
            catch(ObjectDisposedException) { }
            catch(Exception error) { errors.Enqueue(error); }
        })).ToArray();
        readersStarted.Wait();
        racing.Dispose();
        Task.WaitAll(readers);
        check(errors.IsEmpty,"Disposal racing active signature/body reads must complete a read or throw ObjectDisposedException, never leak SQLite or cache errors.");
        check(racing.Statistics.CachedRows==0&&racing.Statistics.CachedPages==0&&Disposed(()=>_=racingFunctions.Count),"Concurrent disposal must release the snapshot and reject subsequent cached navigation.");
    }

    private static (string? Name,string ReturnKind,string Parameters,int Operations)[] Project(IReadOnlyList<FunctionSymbol> functions)
        => functions.Select(function=>(function.Name,function.ReturnType.Kind.Name,
            string.Join(';',function.Parameters.Select(parameter=>$"{parameter.Name}:{parameter.Type.Kind.Name}:{parameter.Role.Name}")),
            function.Body?.DescendantOperations().Count()??0)).ToArray();

    private static bool Disposed(Action read)
    {
        try { read();return false; }
        catch(ObjectDisposedException) { return true; }
    }
}
