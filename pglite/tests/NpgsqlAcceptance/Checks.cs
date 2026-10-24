using System.Collections;

namespace NpgsqlAcceptance;

internal sealed class Checks
{
    public int Assertions { get; private set; }

    public void True(bool actual, string message) => Equal(true, actual, message);
    public void False(bool actual, string message) => Equal(false, actual, message);

    public void Equal<T>(T expected, T actual, string message)
    {
        if (!Same(expected, actual))
            throw new InvalidOperationException($"{message}: expected {Display(expected)}, actual {Display(actual)}");
        Assertions++;
    }

    public T Throws<T>(Action action, string message) where T : Exception
    {
        try { action(); }
        catch (T exception) { Assertions++; return exception; }
        catch (Exception exception) { throw new InvalidOperationException($"{message}: expected {typeof(T).Name}, got {exception.GetType().Name}", exception); }
        throw new InvalidOperationException($"{message}: expected {typeof(T).Name}; operation succeeded");
    }

    public async Task<T> ThrowsAsync<T>(Func<Task> action, string message) where T : Exception
    {
        try { await action(); }
        catch (T exception) { Assertions++; return exception; }
        catch (Exception exception) { throw new InvalidOperationException($"{message}: expected {typeof(T).Name}, got {exception.GetType().Name}", exception); }
        throw new InvalidOperationException($"{message}: expected {typeof(T).Name}; operation succeeded");
    }

    static bool Same(object? expected, object? actual)
    {
        if (expected is Array left && actual is Array right)
        {
            if (left.Rank != right.Rank) return false;
            for (int dim = 0; dim < left.Rank; dim++)
                if (left.GetLength(dim) != right.GetLength(dim) || left.GetLowerBound(dim) != right.GetLowerBound(dim)) return false;
            return left.Cast<object?>().Zip(right.Cast<object?>()).All(pair => Same(pair.First, pair.Second));
        }
        return Equals(expected, actual);
    }

    static string Display(object? value) => value switch
    {
        null => "null",
        Array a => $"{value.GetType().Name}[{a.Length}]",
        string s when s.Length > 120 => $"string[{s.Length}]",
        _ => value.ToString() ?? "null"
    };
}

internal sealed record CaseResult(string Id, int Assertions, double Milliseconds, string? Error);
internal sealed record AcceptanceCase(string Id, Func<Task> Execute);
