namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase
{
    private void OnQueryResponse(PgliteQueryResult result, PgliteQueryOptions? queryOptions)
    {
        if (queryOptions?.OnNotice == null) { OnResponse(result); return; }
        var errors = new List<Exception>();
        bool previous = serviceDeliveringCallbacks;
        serviceDeliveringCallbacks = true;
        try
        {
            foreach (var notice in result.Notices)
            {
                try { queryOptions.OnNotice(notice); }
                catch (Exception error) { errors.Add(error); }
            }
        }
        finally { serviceDeliveringCallbacks = previous; }
        // Always update actual response state and deliver normal service events,
        // including when a query-specific notice callback failed.
        try { OnResponse(result); }
        catch (Exception error) { errors.Add(error); }
        if (errors.Count != 0)
            throw new AggregateException("A managed query notice callback failed after engine execution completed.", errors);
    }
}
