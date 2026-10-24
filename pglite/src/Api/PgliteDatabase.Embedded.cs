namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase
{
    // The copied Npgsql fork installs its utility policy in the existing engine
    // context. Merely being on the worker thread does not bind generated globals.
    // Keep this bridge internal: it supplies neither a second owner nor a public
    // arbitrary backend entrypoint, and uses the same reentry/failure rules as SQL.
    internal void InvokeBackend(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        RequireQueryEntry();
        int status = Dispatch(operation);
        if (status != 0)
            throw new IOException($"Embedded backend configuration unexpectedly yielded {status}.");
    }
}
