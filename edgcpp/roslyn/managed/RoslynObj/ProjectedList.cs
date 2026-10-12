using System.Collections;
namespace RoslynObj;

internal sealed class ProjectedList<TSource, TResult>(IReadOnlyList<TSource> source, Func<TSource, TResult> project)
    : IReadOnlyList<TResult>
{
    public int Count => source.Count;
    public TResult this[int index] => project(source[index]);
    public IEnumerator<TResult> GetEnumerator() => source.Select(project).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
