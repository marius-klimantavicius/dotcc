using System;
using Managed.PGlite.Translated;
using Backend = Managed.PGlite.Translated.PGlite;
using static Managed.PGlite.Translated.PGlite;
using static Managed.PGlite.Translated.PGlite.Libc;

namespace Npgsql.Internal;

// Install only on the worker's engine owner, after startup. The engine globals
// are context-owned; the previous hook belongs to that same session. A utility
// hook sees actual CopyStmt nodes even when SQL originates inside a stored
// routine, so neither quoting nor dynamically constructed SQL can bypass this
// guard. PostgreSQL's own ERROR path performs ordinary protocol recovery.
internal static unsafe class EmbeddedCopyGuard
{
    [ThreadStatic] static nint _previous;

    internal static void Install()
    {
        _previous = Backend.Globals.ProcessUtility_hook;
        Backend.Globals.ProcessUtility_hook = (nint)(delegate*<PlannedStmt*, byte*, CBool,
            ProcessUtilityContext, ParamListInfoData*, QueryEnvironment*, _DestReceiver*, QueryCompletion*, void>)&Process;
    }

    static void Process(PlannedStmt* statement, byte* query, CBool readOnlyTree,
        ProcessUtilityContext context, ParamListInfoData* parameters,
        QueryEnvironment* environment, _DestReceiver* destination, QueryCompletion* completion)
    {
        if (statement->utilityStmt != null && statement->utilityStmt->type == NodeTag.T_CopyStmt &&
            ((CopyStmt*)statement->utilityStmt)->filename == null)
        {
            if ((int)Backend.errstart(Backend.ERROR, null) != 0)
            {
                Backend.errcode(Backend.ERRCODE_FEATURE_NOT_SUPPORTED);
                fixed (byte* message = "Embedded Npgsql does not support streaming COPY STDIN/STDOUT"u8)
                    Backend.errmsg_internal(message);
                fixed (byte* source = "Managed.PGlite.Npgsql"u8)
                    Backend.errfinish(source, 0, source);
            }
            throw new InvalidOperationException("PostgreSQL ERROR unexpectedly returned.");
        }
        if (_previous != 0)
            ((delegate*<PlannedStmt*, byte*, CBool, ProcessUtilityContext, ParamListInfoData*,
                QueryEnvironment*, _DestReceiver*, QueryCompletion*, void>)_previous)(statement,
                    query, readOnlyTree, context, parameters, environment, destination, completion);
        else
            Backend.standard_ProcessUtility(statement, query, readOnlyTree, context, parameters,
                environment, destination, completion);
    }
}
