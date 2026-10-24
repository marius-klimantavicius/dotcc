using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Npgsql.Internal;

internal static class EmbeddedPolicy
{
    internal static void CheckCancellation(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (cancellationToken.CanBeCanceled)
            throw new NotSupportedException("Embedded PostgreSQL cannot interrupt executing translated code. Use CancellationToken.None.");
    }
    internal static NotSupportedException CopyUnsupported() => new(
        "Embedded Npgsql does not support streaming COPY STDIN/STDOUT. Use server file COPY or Managed.PGlite's buffered COPY API.");

}
