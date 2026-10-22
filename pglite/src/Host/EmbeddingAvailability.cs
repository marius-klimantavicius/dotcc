using System.Diagnostics.CodeAnalysis;

namespace Managed.PGlite.Hosting;

// Campaign-start diagnostic, not a database implementation. Replace this guard
// only when a complete translated core supplies and passes the lifecycle.
public static class EmbeddingAvailability
{
    [DoesNotReturn]
    public static void RequireSqlWorkflow()
    {
        throw new NotSupportedException(
            "TranslatedPGlite cannot yet perform embedded initialization, SQL, transactions, " +
            "shutdown and reopen persistence. The complete PostgreSQL core has not been " +
            "published, and PGlite's resumable exit/error, sigjmp, timer and instance-reset " +
            "boundaries remain unresolved. See pglite/docs/BOUNDARIES.md and the campaign " +
            "receipt. No SQL was executed.");
    }
}
