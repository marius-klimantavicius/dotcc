using Core = Managed.GhosttyVt.Native.GhosttyVt;

var count = Core.dotcc_upstream_count();
var expected = ulong.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture);
if (count != expected) throw new InvalidOperationException($"Expected {expected} actual upstream cases, observed {count}.");
var failed = false;
for (ulong index = 0; index < count; index++)
    if (Core.dotcc_upstream_run(index) != 0) failed = true;
var status = Core.dotcc_upstream_host_finish(count);
return failed ? 1 : status;
