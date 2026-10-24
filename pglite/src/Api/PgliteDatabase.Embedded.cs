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
        // Configuration can emit initial GUC reports. Keep those bytes in an
        // owned, separate exchange; the connector obtains startup values from
        // actual current_setting queries and subsequent reports use raw I/O.
        transport.Begin([]);
        try
        {
            int status = Dispatch(operation);
            if (status != 0)
                throw new IOException($"Embedded backend configuration unexpectedly yielded {status}.");
        }
        finally { transport.ReleaseRequest(); }
    }
}
